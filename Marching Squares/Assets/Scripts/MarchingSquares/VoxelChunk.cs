using NoiseGenerator;
using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
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
        public Material voxelMaterial;
        public Noise noiseGenerator;

        public float voxelSize;
        public float voxelScale;
        public bool showVoxelPointGrid;
        public bool useInterpolation;
        public bool useMultithreading;

        public int chunkX, chunkY;

        // Private attributes
        private VoxelSquare[] voxels;
        private NativeArray<VoxelSquareData> voxelsData;

        private Mesh mesh;

        private int chunkResolution;
        private float chunkSize;

        // Vertices and triangles of all the voxels square in chunk
        private NativeArray<float3> vertices;
        private NativeArray<int> triangles;

        private void Awake()
        {
            // Setup the mesh on filter
            if (mesh == null)
            {
                mesh = new Mesh();
                mesh.indexFormat = IndexFormat.UInt32;
                mesh.name = "VoxelGrid Mesh";
            }
        }

        private void OnDestroy()
        {
            mesh.Clear();
            Destroy(mesh);

            // Dispose the native arrays
            vertices.Dispose();
            triangles.Dispose();
            voxelsData.Dispose();
        }

        public void Initialize(int chunkRes, float chunkSize)
        {
            if (chunkRes != this.chunkResolution)
            {
                gameObject.name = "Chunk(" + chunkX + "," + chunkY + ")";

                this.chunkResolution = chunkRes;
                this.chunkSize = chunkSize;

                if (voxels != null)
                {
                    for (int i = 0; i < voxels.Length; i++)
                    {
                        Destroy(voxels[i].gameObject);
                    }
                }

                // Create the array of voxels
                voxels = new VoxelSquare[chunkResolution * chunkResolution];
                voxelsData = new NativeArray<VoxelSquareData>(chunkResolution * chunkResolution, Allocator.Persistent);

                // Initialize vertices and triangles lists
                vertices = new NativeArray<float3>(chunkResolution * chunkResolution * 9 * 6, Allocator.Persistent);
                triangles = new NativeArray<int>(chunkResolution * chunkResolution * 9 * 6, Allocator.Persistent);

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
            VoxelSquare voxelSquare = Instantiate(voxelQuadPrefab);
            voxelSquare.transform.parent = transform;
            voxelSquare.transform.localScale = Vector3.one * voxelSize * voxelScale;
            voxelSquare.transform.localPosition = new Vector3((x) * voxelSize, (y) * voxelSize);
            voxelSquare.Initialize(x, y, voxelSize);

            // Assign the noise value to the voxel square
            voxelSquare.squareData.value = noiseGenerator.Generate(x, y);
            voxelSquare.SetUsedByMarching(voxelSquare.squareData.value > noiseGenerator.isoLevel);

            // Debug option
            voxelSquare.ShowVoxel(showVoxelPointGrid);

            voxels[voxelIndex] = voxelSquare;
            voxelsData[voxelIndex] = voxelSquare.squareData;
        }

        public void Refresh()
        {
            for(int i = 0; i < 0; i++) 
            {
                VoxelSquare voxel = voxels[i];
                voxel.transform.localScale = Vector3.one * voxelSize * voxelScale;
                voxel.squareData.value = noiseGenerator.Generate(voxel.squareData.position.x + transform.position.x, voxel.squareData.position.y + transform.position.y);
                voxel.SetUsedByMarching(voxel.squareData.value > noiseGenerator.isoLevel);
                voxel.ShowVoxel(showVoxelPointGrid);
            }

            if (useMultithreading)
            {
                TriangulateVoxelsMultithread();
            }
            else
            {
                TriangulateVoxels();
            }
        }

        public void TriangulateVoxels()
        {
            // Clear the mesh
            mesh.Clear();

            int cells = chunkResolution - 1;
            int voxelIndex = 0;

            // Clear the native array
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = 0;
            }

            for (int i = 0; i < triangles.Length; i++)
            {
                triangles[i] = 0;
            }

            for (int y = 0; y < cells; y++, voxelIndex++)
            {
                for (int x = 0; x < cells; x++, voxelIndex++)
                {
                    TriangulateVoxel(voxelIndex,
                        voxels[voxelIndex].squareData,
                        voxels[voxelIndex + 1].squareData,
                        voxels[voxelIndex + chunkResolution].squareData,
                        voxels[voxelIndex + chunkResolution + 1].squareData);
                }
            }

            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles.ToArray(), 0);

            // Draw the mesh GPU
            Graphics.DrawMesh(mesh, transform.localToWorldMatrix, voxelMaterial, 0);
        }

        public void TriangulateVoxelsMultithread()
        {
            // Clear all
            mesh.Clear();

            int cells = chunkResolution - 1;
            int voxelIndex = 0;

            // Clear the native array
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = 0;
            }

            for (int i = 0; i < triangles.Length; i++)
            {
                triangles[i] = 0;
            }

            TriangulationJob job = new TriangulationJob()
            {
                squareData = voxelsData,
                chunkResolution = chunkResolution,

                triangles = triangles,
                vertices = vertices,

                isoLevel = noiseGenerator.isoLevel,
                useInterpolation = useInterpolation
            };

            JobHandle jobHandle = job.Schedule(cells * cells, 128);
            jobHandle.Complete();

            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles.ToArray(), 0);

            // Draw the mesh GPU
            Graphics.DrawMesh(mesh, transform.localToWorldMatrix, voxelMaterial, 0);
        }

        #region Triangulation functions

        public void TriangulateVoxel(int voxelIndex, 
                                     VoxelSquareData a, VoxelSquareData b,
                                     VoxelSquareData c, VoxelSquareData d)
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

            // Intepolations t values
            float t_top;
            float t_right;
            float t_bottom;
            float t_left;

            if (useInterpolation)
            {
                t_top = (noiseGenerator.isoLevel - c.value) / (d.value - c.value);
                t_right = (noiseGenerator.isoLevel - d.value) / (b.value - d.value);
                t_bottom = (noiseGenerator.isoLevel - b.value) / (a.value - b.value);
                t_left = (noiseGenerator.isoLevel - a.value) / (c.value - a.value);
            }
            else
            {
                // No, interpolation. By default are mid edge vertex.
                t_top = 0.5f;
                t_right = 0.5f;
                t_bottom = 0.5f;
                t_left = 0.5f;
            }

            float3 top = math.lerp(c.position, d.position, t_top);
            float3 right = math.lerp(d.position, b.position, t_right);
            float3 bottom = math.lerp(b.position, a.position, t_bottom);
            float3 left = math.lerp(a.position, c.position, t_left);

            int index = voxelIndex * 6 * 9;

            switch (cellType)
            {
                case 0:
                    return;
                case 1:
                    AddTriangle(index, a.position, left, bottom);
                    break;
                case 2:
                    AddTriangle(index, b.position, bottom, right);
                    break;
                case 4:
                    AddTriangle(index, c.position, top, left);
                    break;
                case 8:
                    AddTriangle(index, d.position, right, top);
                    break;
                case 3:
                    AddQuad(index, a.position, left, right, b.position);
                    break;
                case 5:
                    AddQuad(index, a.position, c.position, top, bottom);
                    break;
                case 10:
                    AddQuad(index, bottom, top, d.position, b.position);
                    break;
                case 12:
                    AddQuad(index, left, c.position, d.position, right);
                    break;
                case 15:
                    AddQuad(index, a.position, c.position, d.position, b.position);
                    break;
                case 7:
                    AddPentagon(index, a.position, c.position, top, right, b.position);
                    break;
                case 11:
                    AddPentagon(index, b.position, a.position, left, top, d.position);
                    break;
                case 13:
                    AddPentagon(index, c.position, d.position, right, bottom, a.position);
                    break;
                case 14:
                    AddPentagon(index, d.position, b.position, bottom, left, c.position);
                    break;
                case 6:
                    AddTwoTriangles(index, b.position, bottom, right, c.position, top, left);
                    break;
                case 9:
                    AddTwoTriangles(index, a.position, left, bottom, d.position, right, top);
                    break;
            }
        }

        private void AddTwoTriangles(int i, float3 a, float3 b, float3 c, float3 a1, float3 b1, float3 c1)
        {
            vertices[i] = a;
            vertices[i + 1] = b;
            vertices[i + 2] = c;

            triangles[i] = i;
            triangles[i + 1] = i + 1;
            triangles[i + 2] = i + 2;

            vertices[i + 3] = a1;
            vertices[i + 4] = b1;
            vertices[i + 5] = c1;

            triangles[i + 3] = i + 3;
            triangles[i + 4] = i + 4;
            triangles[i + 5] = i + 5;
        }

        private void AddTriangle(int i, float3 a, float3 b, float3 c)
        {
            vertices[i] = a;
            vertices[i + 1] = b;
            vertices[i + 2] = c;

            triangles[i] = i;
            triangles[i + 1] = i + 1;
            triangles[i + 2] = i + 2;
        }

        private void AddQuad(int i, float3 a, float3 b, float3 c, float3 d)
        {
            vertices[i] = a;
            vertices[i + 1] = b;
            vertices[i + 2] = c;
            vertices[i + 3] = d;

            triangles[i] = i;
            triangles[i + 1] = i + 1;
            triangles[i + 2] = i + 2;

            triangles[i + 3] = i;
            triangles[i + 4] = i + 2;
            triangles[i + 5] = i + 3;
        }

        private void AddPentagon(int i, float3 a, float3 b, float3 c, float3 d, float3 e)
        {
            vertices[i + 0] = a;
            vertices[i + 1] = b;
            vertices[i + 2] = c;
            vertices[i + 3] = d;
            vertices[i + 4] = e;

            triangles[i] = i;
            triangles[i + 1] = i + 1;
            triangles[i + 2] = i + 2;

            triangles[i + 3] = i;
            triangles[i + 4] = i + 2;
            triangles[i + 5] = i + 3;

            triangles[i + 6] = i;
            triangles[i + 7] = i + 3;
            triangles[i + 8] = i + 4;
        }

        #endregion
    }

    [BurstCompatible]
    public struct TriangulationJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<VoxelSquareData> squareData;
        public int chunkResolution;

        [NativeDisableContainerSafetyRestriction]
        public NativeArray<float3> vertices;

        [NativeDisableContainerSafetyRestriction]
        public NativeArray<int> triangles;

        public float isoLevel;
        public bool useInterpolation;

        public void Execute(int voxelIndex)
        {
            VoxelSquareData a = squareData[voxelIndex];
            VoxelSquareData b = squareData[voxelIndex + 1];
            VoxelSquareData c = squareData[voxelIndex + chunkResolution];
            VoxelSquareData d = squareData[voxelIndex + chunkResolution + 1];

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

            float3 top = math.lerp(c.position, d.position, t_top);
            float3 right = math.lerp(d.position, b.position, t_right);
            float3 bottom = math.lerp(b.position, a.position, t_bottom);
            float3 left = math.lerp(a.position, c.position, t_left);

            int index = voxelIndex * 6 * 9;

            switch (cellType)
            {
                case 0:
                    return;
                case 1:
                    AddTriangle(index, a.position, left, bottom);
                    break;
                case 2:
                    AddTriangle(index, b.position, bottom, right);
                    break;
                case 4:
                    AddTriangle(index, c.position, top, left);
                    break;
                case 8:
                    AddTriangle(index, d.position, right, top);
                    break;
                case 3:
                    AddQuad(index, a.position, left, right, b.position);
                    break;
                case 5:
                    AddQuad(index, a.position, c.position, top, bottom);
                    break;
                case 10:
                    AddQuad(index, bottom, top, d.position, b.position);
                    break;
                case 12:
                    AddQuad(index, left, c.position, d.position, right);
                    break;
                case 15:
                    AddQuad(index, a.position, c.position, d.position, b.position);
                    break;
                case 7:
                    AddPentagon(index, a.position, c.position, top, right, b.position);
                    break;
                case 11:
                    AddPentagon(index, b.position, a.position, left, top, d.position);
                    break;
                case 13:
                    AddPentagon(index, c.position, d.position, right, bottom, a.position);
                    break;
                case 14:
                    AddPentagon(index, d.position, b.position, bottom, left, c.position);
                    break;
                case 6:
                    AddTwoTriangles(index, b.position, bottom, right, c.position, top, left);
                    break;
                case 9:
                    AddTwoTriangles(index, a.position, left, bottom, d.position, right, top);
                    break;
            }
        }
        private void AddTwoTriangles(int i, float3 a, float3 b, float3 c, float3 a1, float3 b1, float3 c1)
        {
            vertices[i] = a;
            vertices[i + 1] = b;
            vertices[i + 2] = c;

            triangles[i] = i;
            triangles[i + 1] = i + 1;
            triangles[i + 2] = i + 2;

            vertices[i + 3] = a1;
            vertices[i + 4] = b1;
            vertices[i + 5] = c1;

            triangles[i + 3] = i + 3;
            triangles[i + 4] = i + 4;
            triangles[i + 5] = i + 5;
        }

        private void AddTriangle(int i, float3 a, float3 b, float3 c)
        {
            vertices[i] = a;
            vertices[i + 1] = b;
            vertices[i + 2] = c;

            triangles[i] = i;
            triangles[i + 1] = i + 1;
            triangles[i + 2] = i + 2;
        }

        private void AddQuad(int i, float3 a, float3 b, float3 c, float3 d)
        {
            vertices[i] = a;
            vertices[i + 1] = b;
            vertices[i + 2] = c;
            vertices[i + 3] = d;

            triangles[i] = i;
            triangles[i + 1] = i + 1;
            triangles[i + 2] = i + 2;

            triangles[i + 3] = i;
            triangles[i + 4] = i + 2;
            triangles[i + 5] = i + 3;
        }

        private void AddPentagon(int i, float3 a, float3 b, float3 c, float3 d, float3 e)
        {
            vertices[i + 0] = a;
            vertices[i + 1] = b;
            vertices[i + 2] = c;
            vertices[i + 3] = d;
            vertices[i + 4] = e;

            triangles[i] = i;
            triangles[i + 1] = i + 1;
            triangles[i + 2] = i + 2;

            triangles[i + 3] = i;
            triangles[i + 4] = i + 2;
            triangles[i + 5] = i + 3;

            triangles[i + 6] = i;
            triangles[i + 7] = i + 3;
            triangles[i + 8] = i + 4;
        }
    }
}