using System.Collections.Generic;
using UnityEngine;

namespace Procedural.Marching.Squares
{
    // Select only parent
    [SelectionBase]
    public class VoxelChunk : MonoBehaviour
    {
        [Header("Single Voxel settings")]
        public VoxelSquare voxelQuadPrefab;
        [Range(0f, 1f)] public float voxelScale = 0.1f;
        
        [HideInInspector] public VoxelChunk topChunk;
        [HideInInspector] public VoxelChunk rightChunk;
        [HideInInspector] public VoxelChunk toprightChunk;

        private int chunkResolution;

        private VoxelSquare[] voxels;
        private float voxelSize, chunkSize;
        private float halfSize;
        
        // The entire chunk mesh
        private Mesh chunkMesh;

        // Vertices and triangles of all the voxels square in chunk
        private List<Vector3> vertices;
        private List<int> triangles;

        public void Initialize(int chunkResolution, float chunkSize)
        {
            this.chunkResolution = chunkResolution;
            this.chunkSize = chunkSize;

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
            chunkMesh = GetComponent<MeshFilter>().mesh;
            chunkMesh.name = "VoxelGrid Mesh";
            
            // Initialize vertices and triangles lists
            vertices = new List<Vector3>();
            triangles = new List<int>();

            Refresh();
        }

        private void CreateVoxel(int voxelIndex, int x, int y)
        {
            VoxelSquare voxelSquare = Instantiate(voxelQuadPrefab);
            voxelSquare.transform.parent = transform;
            voxelSquare.transform.localScale = Vector3.one * voxelSize * voxelScale;
            voxelSquare.transform.localPosition = new Vector3((x + 0.5f) * voxelSize, (y + 0.5f) * voxelSize);
            voxelSquare.Initialize(x, y, voxelSize);
            voxels[voxelIndex] = voxelSquare;
        }

        private void Refresh()
        {
            foreach(VoxelSquare voxel in voxels)
            {
                voxel.UpdateVoxelColor();
            }
            TriangulateVoxels();
        }

        public void TriangulateVoxels()
        {
            // Clear all
            vertices.Clear();
            triangles.Clear();
            chunkMesh.Clear();
            
            int cells = chunkResolution - 1;
            for (int i = 0, y = 0; y < cells; y++, i++)
            {
                for (int x = 0; x < cells; x++, i++)
                {
                    TriangulateVoxel(
                        voxels[i],
                        voxels[i + 1],
                        voxels[i + chunkResolution],
                        voxels[i + chunkResolution + 1]);
                }
            }

            chunkMesh.SetVertices(vertices);
            chunkMesh.triangles = triangles.ToArray();
        }
        

        public void TriangulateVoxel(VoxelSquare a, VoxelSquare b, VoxelSquare c, VoxelSquare d)
        {
            // Triangulation table
            int cellType = 0; // Cell type may vary from 0 to 15
            if (a.isRegarded)
            {
                cellType |= 1;
            }
            if (b.isRegarded)
            {
                cellType |= 2;
            }
            if (c.isRegarded)
            {
                cellType |= 4;
            }
            if (d.isRegarded)
            {
                cellType |= 8;
            }

            switch (cellType)
            {
                case 0:
                    return;
                case 1:
                    AddTriangle(a.position, a.yEdgePosition, a.xEdgePosition);
                    break;
                case 2:
                    AddTriangle(b.position, a.xEdgePosition, b.yEdgePosition);
                    break;
                case 4:
                    AddTriangle(c.position, c.xEdgePosition, a.yEdgePosition);
                    break;
                case 8:
                    AddTriangle(d.position, b.yEdgePosition, c.xEdgePosition);
                    break;
                case 3:
                    AddQuad(a.position, a.yEdgePosition, b.yEdgePosition, b.position);
                    break;
                case 5:
                    AddQuad(a.position, c.position, c.xEdgePosition, a.xEdgePosition);
                    break;
                case 10:
                    AddQuad(a.xEdgePosition, c.xEdgePosition, d.position, b.position);
                    break;
                case 12:
                    AddQuad(a.yEdgePosition, c.position, d.position, b.yEdgePosition);
                    break;
                case 15:
                    AddQuad(a.position, c.position, d.position, b.position);
                    break;
                case 7:
                    AddPentagon(a.position, c.position, c.xEdgePosition, b.yEdgePosition, b.position);
                    break;
                case 11:
                    AddPentagon(b.position, a.position, a.yEdgePosition, c.xEdgePosition, d.position);
                    break;
                case 13:
                    AddPentagon(c.position, d.position, b.yEdgePosition, a.xEdgePosition, a.position);
                    break;
                case 14:
                    AddPentagon(d.position, b.position, a.xEdgePosition, a.yEdgePosition, c.position);
                    break;
                case 6:
                    AddTriangle(b.position, a.xEdgePosition, b.yEdgePosition);
                    AddTriangle(c.position, c.xEdgePosition, a.yEdgePosition);
                    break;
                case 9:
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
    }
}