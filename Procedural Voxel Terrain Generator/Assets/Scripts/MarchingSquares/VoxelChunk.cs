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
        [Range(0f, 1f)] public float voxelScale = 0.1f;

        private bool useInterpolation;

        private int chunkResolution;
        private Vector3 chunkOffset;

        private VoxelSquare[] voxels;
        private float voxelSize, chunkSize;
        private float halfSize;
        
        // The entire chunk mesh
        private Mesh chunkMesh;
        private MeshFilter chunkMeshFilter;
        private MeshCollider chunkMeshCollider;

        // Vertices and triangles of all the voxels square in chunk
        private List<Vector3> vertices;
        private List<Vector2> uvs;
        private List<int> triangles;
        
        private Noise noiseGenerator;
        
        public void Initialize(int chunkRes, float chunkSize, bool useInterpolation)
        {
            this.useInterpolation = useInterpolation;
            this.chunkResolution = chunkRes;
            this.chunkSize = chunkSize;
            
            chunkOffset = new Vector3(transform.localPosition.x * chunkResolution, transform.localPosition.y * chunkResolution);

            // Greater the resolution, less is the size of the voxel
            voxelSize = chunkSize / chunkResolution;

            // Used to center the voxel into grid
            halfSize = chunkSize * 0.5f;

            // Create the array of voxels
            voxels = new VoxelSquare[chunkResolution * chunkResolution];

            int voxelIndex = 0;

            for (int y = 0; y < chunkResolution; y++)
            {
                for (int x = 0; x < chunkResolution; x++)
                {
                    CreateVoxel(voxelIndex, x, y);
                    voxelIndex++;
                }
            }
            
            // Get the chunk mesh component
            chunkMeshFilter = GetComponent<MeshFilter>();
            chunkMeshCollider = GetComponent<MeshCollider>();
            
            chunkMesh = chunkMeshFilter.mesh;
            chunkMesh.name = "VoxelGrid Mesh";

            // Initialize vertices and triangles lists
            vertices = new List<Vector3>();
            triangles = new List<int>();
            uvs = new List<Vector2>();

            Refresh();
        }

        private void CreateVoxel(int voxelIndex, float x, float y)
        {
            VoxelSquare voxelSquare = Instantiate(voxelQuadPrefab);
            voxelSquare.transform.parent = transform;
            voxelSquare.transform.localScale = Vector3.one * voxelSize * voxelScale;
            voxelSquare.transform.localPosition = new Vector3((x) * voxelSize, (y) * voxelSize);
            voxelSquare.Initialize(x, y, voxelSize);

            voxelSquare.value = noiseGenerator.Generate(x, y);
            voxelSquare.SetUsedByMarching(voxelSquare.value > noiseGenerator.threshold);
            voxels[voxelIndex] = voxelSquare;
        }

        public void Refresh()
        {
            foreach(VoxelSquare voxel in voxels)
            {
                voxel.UpdateVoxelColor();
            }

            TriangulateVoxels();

            // Apply mesh to mesh collider
            Mesh colliderMesh = new Mesh();
            
            colliderMesh.indexFormat = IndexFormat.UInt32;

            colliderMesh.vertices = vertices.ToArray();
            colliderMesh.triangles = triangles.ToArray();
            
            chunkMeshCollider.sharedMesh = colliderMesh;
        }

        public void SetNoiseGenerator(Noise noiseGenerator)
        {
            this.noiseGenerator = noiseGenerator;
        }

        public void TriangulateVoxels()
        {
            // Clear all
            vertices.Clear();
            uvs.Clear();
            triangles.Clear();
            chunkMesh.Clear();
            
            int cells = chunkResolution - 1;
            int voxelIndex = 0;

            for (int y = 0; y < cells; y++, voxelIndex++)
            {
                for (int x = 0; x < cells; x++, voxelIndex++)
                {
                    TriangulateVoxel(
                        voxels[voxelIndex],
                        voxels[voxelIndex + 1],
                        voxels[voxelIndex + chunkResolution],
                        voxels[voxelIndex + chunkResolution + 1]);
                }
            }

            chunkMesh.SetVertices(vertices);
            chunkMesh.SetUVs(0, uvs);
            chunkMesh.triangles = triangles.ToArray();
        }

        #region Triangulation functions

        public void TriangulateVoxel(VoxelSquare a, VoxelSquare b, 
                                     VoxelSquare c, VoxelSquare d)
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
            
            // Instead of top you lerp between A and B to get the position.
            // Instead of right you lerp between B and C, etc.
            // 
            //          top
            //       C-------D
            //  left |       |  right
            //       |       |
            //       A-------B
            //         bottom

            float t_top = (noiseGenerator.isoLevel - c.value) / (d.value - c.value);
            Vector2 top = Vector2.Lerp(c.position, d.position, t_top);

            float t_right = (noiseGenerator.isoLevel - d.value) / (b.value - d.value);
            Vector2 right = Vector2.Lerp(d.position, b.position, t_right);

            float t_bottom = (noiseGenerator.isoLevel - b.value) / (a.value - b.value);
            Vector2 bottom = Vector2.Lerp(b.position, a.position, t_bottom);

            float t_left = (noiseGenerator.isoLevel - a.value) / (c.value - a.value);
            Vector2 left = Vector2.Lerp(a.position, c.position, t_left);
            
            switch (cellType)
            {
                case 0:
                    return;
                case 1:
                    if(useInterpolation)
                    {
                        AddTriangle(a.position, left, bottom);
                        break;
                    }
                    AddTriangle(a.position, a.yEdgePosition, a.xEdgePosition);
                    break;
                case 2:
                    if (useInterpolation)
                    {
                        AddTriangle(b.position, bottom, right);
                        break;
                    }
                    AddTriangle(b.position, a.xEdgePosition, b.yEdgePosition);
                    break;
                case 4:
                    if (useInterpolation)
                    {
                        AddTriangle(c.position, top, left);
                        break;
                    }
                    AddTriangle(c.position, c.xEdgePosition, a.yEdgePosition);
                    break;
                case 8:
                    if (useInterpolation)
                    {
                        AddTriangle(d.position, right, top);
                        break;
                    }
                    AddTriangle(d.position, b.yEdgePosition, c.xEdgePosition);
                    break;
                case 3:
                    if (useInterpolation)
                    {
                        AddQuad(a.position, left, right, b.position);
                        break;
                    }
                    AddQuad(a.position, a.yEdgePosition, b.yEdgePosition, b.position);
                    break;
                case 5:
                    if (useInterpolation)
                    {
                        AddQuad(a.position, c.position, top, bottom);
                        break;
                    }
                    AddQuad(a.position, c.position, c.xEdgePosition, a.xEdgePosition);
                    break;
                case 10:
                    if (useInterpolation)
                    {
                        AddQuad(bottom, top, d.position, b.position);
                        break;
                    }
                    AddQuad(a.xEdgePosition, c.xEdgePosition, d.position, b.position);
                    break;
                case 12:
                    if (useInterpolation)
                    {
                        AddQuad(left, c.position, d.position, right);
                        break;
                    }
                    AddQuad(a.yEdgePosition, c.position, d.position, b.yEdgePosition);
                    break;
                case 15:
                    if (useInterpolation)
                    {
                        AddQuad(a.position, c.position, d.position, b.position);
                        break;
                    }
                    AddQuad(a.position, c.position, d.position, b.position);
                    break;
                case 7:
                    if (useInterpolation)
                    {
                        AddPentagon(a.position, c.position, top, right, b.position);
                        break;
                    }
                    AddPentagon(a.position, c.position, c.xEdgePosition, b.yEdgePosition, b.position);
                    break;
                case 11:
                    if (useInterpolation)
                    {
                        AddPentagon(b.position, a.position, left, top, d.position);
                        break;
                    }
                    AddPentagon(b.position, a.position, a.yEdgePosition, c.xEdgePosition, d.position);
                    break;
                case 13:
                    if (useInterpolation)
                    {
                        AddPentagon(c.position, d.position, right, bottom, a.position);
                        break;
                    }
                    AddPentagon(c.position, d.position, b.yEdgePosition, a.xEdgePosition, a.position);
                    break;
                case 14:
                    if (useInterpolation)
                    {
                        AddPentagon(d.position, b.position, bottom, left, c.position);
                        break;
                    }
                    AddPentagon(d.position, b.position, a.xEdgePosition, a.yEdgePosition, c.position);
                    break;
                case 6:
                    if (useInterpolation)
                    {
                        AddTriangle(b.position, bottom, right);
                        AddTriangle(c.position, top, left);
                        break;
                    }
                    AddTriangle(b.position, a.xEdgePosition, b.yEdgePosition);
                    AddTriangle(c.position, c.xEdgePosition, a.yEdgePosition);
                    break;
                case 9:
                    if (useInterpolation)
                    {
                        AddTriangle(a.position, left, bottom);
                        AddTriangle(d.position, right, top);
                        break;
                    }
                    AddTriangle(a.position, a.yEdgePosition, a.xEdgePosition);
                    AddTriangle(d.position, b.yEdgePosition, c.xEdgePosition);
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
            uvs.Add(Vector2.right * vertices[vertexIndex].x + Vector2.up * vertices[vertexIndex].y);
            uvs.Add(Vector2.right * vertices[vertexIndex + 1].x + Vector2.up * vertices[vertexIndex + 1].y);
            uvs.Add(Vector2.right * vertices[vertexIndex + 2].x + Vector2.up * vertices[vertexIndex + 2].y);

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
            
            uvs.Add(Vector2.right * vertices[vertexIndex].x + Vector2.up * vertices[vertexIndex].y);
            uvs.Add(Vector2.right * vertices[vertexIndex + 1].x + Vector2.up * vertices[vertexIndex + 1].y);
            uvs.Add(Vector2.right * vertices[vertexIndex + 2].x + Vector2.up * vertices[vertexIndex + 2].y);
            uvs.Add(Vector2.right * vertices[vertexIndex + 3].x + Vector2.up * vertices[vertexIndex + 3].y);

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
            
            uvs.Add(Vector2.right * vertices[vertexIndex].x + Vector2.up * vertices[vertexIndex].y);
            uvs.Add(Vector2.right * vertices[vertexIndex + 1].x + Vector2.up * vertices[vertexIndex + 1].y);
            uvs.Add(Vector2.right * vertices[vertexIndex + 2].x + Vector2.up * vertices[vertexIndex + 2].y);
            uvs.Add(Vector2.right * vertices[vertexIndex + 3].x + Vector2.up * vertices[vertexIndex + 3].y);
            uvs.Add(Vector2.right * vertices[vertexIndex + 4].x + Vector2.up * vertices[vertexIndex + 4].y);

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