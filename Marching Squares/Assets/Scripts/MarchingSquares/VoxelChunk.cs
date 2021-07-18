using NoiseGenerator;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace Procedural.Marching.Squares
{
    struct Triangle
    {
        // Vertices
        public Vector2 a;
        public Vector2 b;
        public Vector2 c;

        // UVS
        public Vector2 aUV;
        public Vector2 bUV;
        public Vector2 cUV;
    }

    // Select only parent
    [SelectionBase]
    public class VoxelChunk : MonoBehaviour
    {
        [Header("Single Voxel settings")]
        public VoxelSquare voxelQuadPrefab;
        private const int threadPerGroups = 64;

        public ComputeShader marchingCompute;
        private ComputeBuffer voxelDataBuffer;
        private ComputeBuffer triangleDataBuffer;
        private ComputeBuffer triCountBuffer;
        private int cells;
        private int threadGroups;

        // Materials
        public Material voxelMaterial;

        public int chunkX, chunkY;

        public Noise noiseGenerator;
        public bool showVoxelPointGrid;
        public float voxelScale;
        public bool useInterpolation;
        public bool useUVMapping;
        public bool useComputeShader;

        private VoxelSquare[] voxels;
        private VoxelData[] voxelsData;

        private int chunkResolution;
        private float voxelSize;

        public Mesh mesh;

        // Vertices and triangles of all the voxels square in chunk
        private List<Vector3> vertices;
        private List<Vector2> uvs;
        private List<int> triangles;

        private void Awake()
        {
            // Setup the mesh on filter
            if (mesh == null)
            {
                mesh = new Mesh();
                mesh.indexFormat = IndexFormat.UInt32;
                mesh.name = "VoxelGrid Mesh";
            }

            // Initialize vertices and triangles lists
            vertices = new List<Vector3>();
            triangles = new List<int>();
            uvs = new List<Vector2>();
        }

        private void OnDestroy()
        {
            mesh.Clear();
            Destroy(mesh);

            // Dispose the compute buffers
            voxelDataBuffer?.Dispose();
            triangleDataBuffer?.Dispose();
            triCountBuffer?.Dispose();

            voxelDataBuffer = null;
            triangleDataBuffer = null;
            triCountBuffer = null;
        }

        public void Initialize(int chunkRes, float chunkSize)
        {
            if (chunkRes != this.chunkResolution)
            {
                gameObject.name = "Chunk(" + chunkX + "," + chunkY + ")";

                this.chunkResolution = chunkRes;
                this.cells = chunkRes - 1;

                threadGroups = Mathf.CeilToInt(chunkResolution * chunkResolution / (float)threadPerGroups);

                // Create the array of voxels
                if (voxels != null)
                {
                    for (int i = 0; i < voxels.Length; i++)
                    {
                        Destroy(voxels[i].gameObject);
                    }
                }

                voxels = new VoxelSquare[chunkResolution * chunkResolution]; 
                voxelsData = new VoxelData[chunkResolution * chunkResolution];

                // Greater the resolution, less is the size of the voxel
                voxelSize = chunkSize / cells;

                int voxelIndex = 0;

                for (int y = 0; y < chunkResolution; y++)
                {
                    for (int x = 0; x < chunkResolution; x++)
                    {
                        CreateVoxel(voxelIndex, x, y);
                        voxelIndex++;
                    }
                }
            }

            if(useComputeShader)
            {
                triangleDataBuffer?.Release();
                voxelDataBuffer?.Release();
                triCountBuffer?.Release();

                // Calculate the compute buffer again
                int voxelDataStride = Marshal.SizeOf(typeof(Vector2)) + Marshal.SizeOf(typeof(float));
                voxelDataBuffer = new ComputeBuffer(chunkResolution * chunkResolution, voxelDataStride);

                int triangleDataStride = Marshal.SizeOf(typeof(Vector2)) * 6;
                triangleDataBuffer = new ComputeBuffer(chunkResolution * chunkResolution * 5, triangleDataStride, ComputeBufferType.Append);

                triCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
            }

            Refresh();
        }

        private void CreateVoxel(int voxelIndex, float x, float y)
        {
            // Create the array of voxels
            VoxelSquare voxel = Instantiate(voxelQuadPrefab, transform);
            voxel.transform.localPosition = Vector3.right * ((x * voxelSize)) + Vector3.up * ((y * voxelSize));

            voxel.transform.localScale = Vector3.one * voxelSize * voxelScale;

            // Marching evaluation for the grid
            voxel.isUsedByMarching = voxelsData[voxelIndex].value > noiseGenerator.isoLevel;

            // Set the voxel to the voxel
            voxels[voxelIndex] = voxel;

            voxelsData[voxelIndex].value = noiseGenerator.Generate(x, y);
            voxelsData[voxelIndex].position = new Vector2(x * voxelSize, y * voxelSize);
        }

        public void Refresh()
        {
            // Caching the transform position
            Vector3 pos = transform.position;

            for(int i = 0; i < voxelsData.Length; i++)
            {
                voxelsData[i].value = noiseGenerator.Generate(voxelsData[i].position.x + pos.x, voxelsData[i].position.y + pos.y);    
            }

            // Optimize if the grid is not showed
            if (showVoxelPointGrid)
            {
                for (int i = 0; i < voxels.Length; i++)
                {
                    voxels[i].transform.localScale = Vector3.one * voxelSize * voxelScale;
                    voxels[i].isUsedByMarching = voxelsData[i].value > noiseGenerator.isoLevel;
                    voxels[i].DrawSquare(voxelsData[i].value);
                }
            }
            
            if(useComputeShader)
            {
                TriangulateVoxelsCompute();
            }
            else
            {
                TriangulateVoxels();
            }
        }

        public void TriangulateVoxels()
        {
            // Clear all
            vertices.Clear();
            uvs.Clear();
            triangles.Clear();
            mesh.Clear();
            
            int cells = chunkResolution - 1;
            int voxelIndex = 0;

            // Triangulate all the voxels inside the grid
            for (int y = 0; y < cells; y++, voxelIndex++)
            {
                for (int x = 0; x < cells; x++, voxelIndex++)
                {
                    TriangulateVoxel(
                        voxelsData[voxelIndex],
                        voxelsData[voxelIndex + 1],
                        voxelsData[voxelIndex + chunkResolution],
                        voxelsData[voxelIndex + chunkResolution + 1],
                        noiseGenerator.isoLevel);
                }
            }

            mesh.MarkDynamic();

            // Set the mesh vertices, uvs and triangles
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);

            if (useUVMapping)
            {
                mesh.SetUVs(0, uvs);
            }

            // Draw the mesh GPU
            Graphics.DrawMesh(mesh, transform.localToWorldMatrix, voxelMaterial, 0);
        }

        public void TriangulateVoxelsCompute()
        {
            //Push our prepared data into Buffer
            voxelDataBuffer.SetData(voxelsData);

            // Compute
            int marchingKernel = marchingCompute.FindKernel("March");

            // Reset the counter of the buffers
            voxelDataBuffer.SetCounterValue(0);
            triangleDataBuffer.SetCounterValue(0);

            marchingCompute.SetInt("cells", cells);
            marchingCompute.SetFloat("isoLevel", noiseGenerator.isoLevel);
            marchingCompute.SetBool("useInterpolation", useInterpolation);
            marchingCompute.SetBuffer(marchingKernel, "voxels", voxelDataBuffer);
            marchingCompute.SetBuffer(marchingKernel, "triangles", triangleDataBuffer);

            marchingCompute.Dispatch(marchingKernel, threadGroups, 1, 1);

            // Get number of triangles in the triangle buffer
            ComputeBuffer.CopyCount(triangleDataBuffer, triCountBuffer, 0);
            int[] triCountArray = { 0 };
            triCountBuffer.GetData(triCountArray);
            int numTris = triCountArray[0];

            // Get the triangles
            Triangle[] tris = new Triangle[numTris];
            triangleDataBuffer.GetData(tris);

            // Clear all the mesh
            mesh.Clear();
            uvs.Clear();
            vertices.Clear();
            triangles.Clear();

            for(int i = 0, t = 0; i < numTris; i++, t = i * 3)
            {
                vertices.Add(tris[i].a);
                vertices.Add(tris[i].b);
                vertices.Add(tris[i].c);

                triangles.Add(t);
                triangles.Add(t + 1);
                triangles.Add(t + 2);

                if (useUVMapping)
                {
                    uvs.Add(tris[i].aUV);
                    uvs.Add(tris[i].bUV);
                    uvs.Add(tris[i].cUV);
                }
            }

            mesh.MarkDynamic();

            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);

            if (useUVMapping)
            {
                mesh.SetUVs(0, uvs);
            }

            // Draw the mesh GPU
            Graphics.DrawMesh(mesh, transform.localToWorldMatrix, voxelMaterial, 0);
        }

        #region Triangulation functions

        public void TriangulateVoxel(VoxelData a, VoxelData b,
                                     VoxelData c, VoxelData d, float isoLevel)
        {
            // Triangulation table
            int cellType = 0; // Cell type may vary from 0 to 15

            if (a.value > isoLevel)
            {
                cellType |= 1;
            }
            if (b.value > isoLevel)
            {
                cellType |= 2;
            }
            if (c.value > isoLevel)
            {
                cellType |= 4;
            }
            if (d.value > isoLevel)
            {
                cellType |= 8;
            }

            // Instead of top you lerp between A and B to get the position.
            // Instead of right you lerp between B and C, etc.
            // 
            //          top
            //       C-------D
            //  left |       |  right
            //       |       |
            //       A-------B
            //         bottom

            // Intepolations t values
            float t_top;
            float t_right;
            float t_bottom;
            float t_left;

            if (useInterpolation)
            {
                t_top = (isoLevel - c.value) / (d.value - c.value);
                t_right = (isoLevel - d.value) / (b.value - d.value);
                t_bottom = (isoLevel - b.value) / (a.value - b.value);
                t_left = (isoLevel - a.value) / (c.value - a.value);
            }
            else
            {
                // No, interpolation. By default are mid edge vertex.
                t_top = 0.5f;
                t_right = 0.5f;
                t_bottom = 0.5f;
                t_left = 0.5f;
            }

            // Lerp unclamped is better in this case in order to avoid not used moltiplication
            // My value is already between 0 and 1, no needed to over safety
            /*
                Lerp:

                public static float Lerp(float a, float b, float t)
                {
                return a + (b - a) * Mathf.Clamp01(t);
                }

                LerpUnclamped:

                public static float LerpUnclamped(float a, float b, float t)
                {
                return a + (b - a) * t;
                }
             */
            Vector2 top = Vector2.LerpUnclamped(c.position, d.position, t_top);
            Vector2 right = Vector2.LerpUnclamped(d.position, b.position, t_right);
            Vector2 bottom = Vector2.LerpUnclamped(b.position, a.position, t_bottom);
            Vector2 left = Vector2.LerpUnclamped(a.position, c.position, t_left);
            
            switch (cellType)
            {
                case 0:
                    return;
                case 1:
                    AddTriangle(a.position, left, bottom);
                    break;
                case 2:
                    AddTriangle(b.position, bottom, right);
                    break;
                case 4:
                    AddTriangle(c.position, top, left);
                    break;
                case 8:
                    AddTriangle(d.position, right, top);
                    break;
                case 3:
                    AddQuad(a.position, left, right, b.position);
                    break;
                case 5:
                    AddQuad(a.position, c.position, top, bottom); 
                    break;
                case 10:
                    AddQuad(bottom, top, d.position, b.position);
                    break;
                case 12:
                    AddQuad(left, c.position, d.position, right);
                    break;
                case 15:
                    AddQuad(a.position, c.position, d.position, b.position);
                    break;
                case 7:
                    AddPentagon(a.position, c.position, top, right, b.position);
                    break;
                case 11:
                    AddPentagon(b.position, a.position, left, top, d.position);
                    break;
                case 13:
                    AddPentagon(c.position, d.position, right, bottom, a.position);
                    break;
                case 14:
                    AddPentagon(d.position, b.position, bottom, left, c.position);
                    break;
                case 6:
                    AddTriangle(b.position, bottom, right);
                    AddTriangle(c.position, top, left);
                    break;
                case 9:
                    AddTriangle(a.position, left, bottom);
                    AddTriangle(d.position, right, top);
                    break;
            }
        }

        private void AddTriangle(Vector3 a, Vector3 b, Vector3 c)
        {
            int vertexIndex = vertices.Count;
            vertices.Add(a);
            vertices.Add(b);
            vertices.Add(c);

            // Add uvs
            if (useUVMapping)
            {
                uvs.Add(Vector2.right * a.x + Vector2.up * a.y);
                uvs.Add(Vector2.right * b.x + Vector2.up * b.y);
                uvs.Add(Vector2.right * c.x + Vector2.up * c.y);
            }

            triangles.Add(vertexIndex);
            triangles.Add(vertexIndex + 1);
            triangles.Add(vertexIndex + 2);
        }

        private void AddQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            int vertexIndex = vertices.Count;
            vertices.Add(a);
            vertices.Add(b);
            vertices.Add(c);
            vertices.Add(d);

            if (useUVMapping)
            {
                uvs.Add(Vector2.right * a.x + Vector2.up * a.y);
                uvs.Add(Vector2.right * b.x + Vector2.up * b.y);
                uvs.Add(Vector2.right * c.x + Vector2.up * c.y);
                uvs.Add(Vector2.right * d.x + Vector2.up * d.y);
            }

            triangles.Add(vertexIndex);
            triangles.Add(vertexIndex + 1);
            triangles.Add(vertexIndex + 2);

            triangles.Add(vertexIndex);
            triangles.Add(vertexIndex + 2);
            triangles.Add(vertexIndex + 3);
        }

        private void AddPentagon(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 e)
        {
            int vertexIndex = vertices.Count;
            vertices.Add(a);
            vertices.Add(b);
            vertices.Add(c);
            vertices.Add(d);
            vertices.Add(e);

            if (useUVMapping)
            {
                uvs.Add(Vector2.right * a.x + Vector2.up * a.y);
                uvs.Add(Vector2.right * b.x + Vector2.up * b.y);
                uvs.Add(Vector2.right * c.x + Vector2.up * c.y);
                uvs.Add(Vector2.right * d.x + Vector2.up * d.y);
                uvs.Add(Vector2.right * e.x + Vector2.up * e.y);
            }

            triangles.Add(vertexIndex);
            triangles.Add(vertexIndex + 1);
            triangles.Add(vertexIndex + 2);

            triangles.Add(vertexIndex);
            triangles.Add(vertexIndex + 2);
            triangles.Add(vertexIndex + 3);

            triangles.Add(vertexIndex);
            triangles.Add(vertexIndex + 3);
            triangles.Add(vertexIndex + 4);
        }

        #endregion
    }
}