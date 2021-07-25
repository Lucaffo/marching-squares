using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace MarchingSquares
{
    public class MarchingCPU : Marching
    {
        // Vertices and triangles of all the voxels square in chunk
        private List<Vector3> _vertices;
        private List<Vector2> _uvs;
        private List<int> _triangles;
        
        // Parameters and settings
        private bool _uvMapping;
        private bool _interpolation;
        private int _resolution;
        
        // Chunk Material and Position
        private Material _material;
        private Vector3 _position;

        public override void Initialize(Material voxelMaterial, Vector3 chunkPosition, int chunkResolution)
        {
            // Create the base mesh
            mesh = new Mesh
            {
                indexFormat = IndexFormat.UInt32,
                name = "VoxelGrid Mesh"
            };

            // Initialize vertices and triangles lists
            _vertices = new List<Vector3>();
            _triangles = new List<int>();
            _uvs = new List<Vector2>();
            
            // Assign the material and the position
            _material = voxelMaterial;
            _position = chunkPosition;

            // Set the chunk resolution
            _resolution = chunkResolution;
        }

        public override void Triangulate(ref VoxelData[] voxelData, float isoLevel, bool useUVMapping = false, bool useInterpolation = true)
        {
            // Setup settings
            _uvMapping = useUVMapping;
            _interpolation = useInterpolation;
            
            // Clear all
            Clear();
            
            int cells = _resolution - 1;
            int voxelIndex = 0;

            // Triangulate all the voxels inside the grid
            for (int y = 0; y < cells; y++, voxelIndex++)
            {
                for (int x = 0; x < cells; x++, voxelIndex++)
                {
                    TriangulateVoxel(
                        voxelData[voxelIndex],
                        voxelData[voxelIndex + 1],
                        voxelData[voxelIndex + _resolution],
                        voxelData[voxelIndex + _resolution + 1],
                        isoLevel);
                }
            }

            mesh.MarkDynamic();

            // Set the mesh vertices, uvs and triangles
            mesh.SetVertices(_vertices);
            mesh.SetTriangles(_triangles, 0);

            if (useUVMapping)
            {
                mesh.SetUVs(0, _uvs);
            }

            // Draw the mesh GPU
            Graphics.DrawMesh(mesh, _position, Quaternion.identity, _material, 0);
        }

        public override void Clear()
        {
            _vertices.Clear();
            _uvs.Clear();
            _triangles.Clear();
            mesh.Clear();
        }

        public override void Destroy()
        {
            mesh.Clear();
        }

        #region Triangulation functions

        private void TriangulateVoxel(VoxelData a, VoxelData b,
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
            float tTop;
            float tRight;
            float tBottom;
            float tLeft;

            if (_interpolation)
            {
                tTop = (isoLevel - c.value) / (d.value - c.value);
                tRight = (isoLevel - d.value) / (b.value - d.value);
                tBottom = (isoLevel - b.value) / (a.value - b.value);
                tLeft = (isoLevel - a.value) / (c.value - a.value);
            }
            else
            {
                // No, interpolation. By default are mid edge vertex.
                tTop = 0.5f;
                tRight = 0.5f;
                tBottom = 0.5f;
                tLeft = 0.5f;
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
            Vector2 top = Vector2.LerpUnclamped(c.position, d.position, tTop);
            Vector2 right = Vector2.LerpUnclamped(d.position, b.position, tRight);
            Vector2 bottom = Vector2.LerpUnclamped(b.position, a.position, tBottom);
            Vector2 left = Vector2.LerpUnclamped(a.position, c.position, tLeft);
            
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
            int vertexIndex = _vertices.Count;
            _vertices.Add(a);
            _vertices.Add(b);
            _vertices.Add(c);

            // Add uvs
            if (_uvMapping)
            {
                _uvs.Add(Vector2.right * a.x + Vector2.up * a.y);
                _uvs.Add(Vector2.right * b.x + Vector2.up * b.y);
                _uvs.Add(Vector2.right * c.x + Vector2.up * c.y);
            }

            _triangles.Add(vertexIndex);
            _triangles.Add(vertexIndex + 1);
            _triangles.Add(vertexIndex + 2);
        }

        private void AddQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            int vertexIndex = _vertices.Count;
            _vertices.Add(a);
            _vertices.Add(b);
            _vertices.Add(c);
            _vertices.Add(d);

            if (_uvMapping)
            {
                _uvs.Add(Vector2.right * a.x + Vector2.up * a.y);
                _uvs.Add(Vector2.right * b.x + Vector2.up * b.y);
                _uvs.Add(Vector2.right * c.x + Vector2.up * c.y);
                _uvs.Add(Vector2.right * d.x + Vector2.up * d.y);
            }

            _triangles.Add(vertexIndex);
            _triangles.Add(vertexIndex + 1);
            _triangles.Add(vertexIndex + 2);

            _triangles.Add(vertexIndex);
            _triangles.Add(vertexIndex + 2);
            _triangles.Add(vertexIndex + 3);
        }

        private void AddPentagon(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 e)
        {
            int vertexIndex = _vertices.Count;
            _vertices.Add(a);
            _vertices.Add(b);
            _vertices.Add(c);
            _vertices.Add(d);
            _vertices.Add(e);

            if (_uvMapping)
            {
                _uvs.Add(Vector2.right * a.x + Vector2.up * a.y);
                _uvs.Add(Vector2.right * b.x + Vector2.up * b.y);
                _uvs.Add(Vector2.right * c.x + Vector2.up * c.y);
                _uvs.Add(Vector2.right * d.x + Vector2.up * d.y);
                _uvs.Add(Vector2.right * e.x + Vector2.up * e.y);
            }

            _triangles.Add(vertexIndex);
            _triangles.Add(vertexIndex + 1);
            _triangles.Add(vertexIndex + 2);

            _triangles.Add(vertexIndex);
            _triangles.Add(vertexIndex + 2);
            _triangles.Add(vertexIndex + 3);

            _triangles.Add(vertexIndex);
            _triangles.Add(vertexIndex + 3);
            _triangles.Add(vertexIndex + 4);
        }

        #endregion
    }
}