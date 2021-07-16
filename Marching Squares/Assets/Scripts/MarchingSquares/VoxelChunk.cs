using NoiseGenerator;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Procedural.Marching.Squares
{
    // Select only parent
    [SelectionBase]
    public class VoxelChunk : MonoBehaviour
    {
        [Header("Single Voxel settings")]
        public VoxelSquare voxelQuadPrefab;

        // Materials
        public Material voxelMaterial;

        public int chunkX, chunkY;

        public Noise noiseGenerator;
        public bool showVoxelPointGrid;
        public float voxelScale;
        public bool useInterpolation;
        public bool useUVMapping;

        private VoxelSquare[] voxels;

        private int chunkResolution;
        private float voxelSize;

        private Mesh mesh;

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
                mesh.MarkDynamic();
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
        }

        public void Initialize(int chunkRes, float chunkSize)
        {
            if(chunkRes != this.chunkResolution)
            {
                gameObject.name = "Chunk(" + chunkX + "," + chunkY + ")";

                this.chunkResolution = chunkRes;
                
                if(voxels != null)
                {
                    for (int i = 0; i < voxels.Length; i++)
                    {
                        Destroy(voxels[i].gameObject);
                    }
                }

                // Create the array of voxels
                voxels = new VoxelSquare[chunkResolution * chunkResolution];

                // Greater the resolution, less is the size of the voxel
                this.voxelSize = chunkSize / chunkResolution;

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

            Refresh();
        }

        private void CreateVoxel(int voxelIndex, float x, float y)
        {
            VoxelSquare voxel = Instantiate(voxelQuadPrefab);
            voxel.transform.parent = transform;
            voxel.transform.localScale = Vector3.one * voxelSize * voxelScale;
            voxel.transform.localPosition = new Vector3((x) * voxelSize, (y) * voxelSize);
            voxel.Initialize(x, y, voxelSize);

            // Marching evaluation
            voxel.value = noiseGenerator.Generate(x, y);
            voxel.isUsedByMarching = voxel.value > noiseGenerator.isoLevel;

            // Set the voxel to the voxel
            voxels[voxelIndex] = voxel;
        }

        public void Refresh()
        {
            // Caching the transform position
            Vector3 pos = transform.position;

            foreach(VoxelSquare voxel in voxels)
            {
                voxel.transform.localScale = Vector3.one * voxelSize * voxelScale;
                voxel.value = noiseGenerator.Generate(voxel.position.x + pos.x, voxel.position.y + pos.y);
                voxel.isUsedByMarching = voxel.value > noiseGenerator.isoLevel;
               
                if(showVoxelPointGrid)
                {
                    voxel.DrawSquare();
                }
            }

            TriangulateVoxels();
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
                        voxels[voxelIndex],
                        voxels[voxelIndex + 1],
                        voxels[voxelIndex + chunkResolution],
                        voxels[voxelIndex + chunkResolution + 1],
                        noiseGenerator.isoLevel);
                }
            }

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

        #region Triangulation functions

        public void TriangulateVoxel(VoxelSquare a, VoxelSquare b, 
                                     VoxelSquare c, VoxelSquare d, float isoLevel)
        {
            // Triangulation table
            int cellType = 0; // Cell type may vary from 0 to 15

            if (a.isUsedByMarching)
            {
                cellType |= 1;
            }
            if (b.isUsedByMarching)
            {
                cellType |= 2;
            }
            if (c.isUsedByMarching)
            {
                cellType |= 4;
            }
            if (d.isUsedByMarching)
            {
                cellType |= 8;
            }

            if(cellType == 0)
            {
                return;
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