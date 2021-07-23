using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace MarchingSquares
{
    public class MarchingGPU : Marching
    {
        // Compute shader
        private readonly ComputeShader computeShader;
        
        // Compute shaders variables
        private ComputeBuffer voxelDataBuffer;
        private ComputeBuffer trianglesBuffer;
        private ComputeBuffer verticesBuffer;
        private ComputeBuffer triCountBuffer;
        
        private int[] trianglesArray;
        private Vector3[] verticesArray;
        
        // Parameters and settings
        private int resolution;
        
        // Thread params
        private int threadGroups;
        private const int ThreadPerGroups = 64;

        // Chunk Material and Position
        private Material material;
        private Vector3 position;

        public MarchingGPU(ComputeShader shader)
        {
            computeShader = shader;
        }

        public override void Initialize(Material voxelMaterial, Vector3 chunkPosition, int chunkResolution)
        {
            // Create the base mesh
            mesh = new Mesh
            {
                indexFormat = IndexFormat.UInt32,
                name = "VoxelGrid Mesh"
            };
            
            // Assign the material and the position
            material = voxelMaterial;
            position = chunkPosition;

            // Set the chunk resolution
            resolution = chunkResolution;
            
            // Setup the thread groups
            threadGroups = Mathf.CeilToInt(chunkResolution * chunkResolution / (float)ThreadPerGroups);

            // Setup the output arrays
            trianglesArray = new int[chunkResolution * chunkResolution * 9];
            verticesArray = new Vector3[chunkResolution * chunkResolution * 9];
        }

        public void Refresh()
        {
            trianglesBuffer?.Release();
            verticesBuffer?.Release();
            voxelDataBuffer?.Release();
            triCountBuffer?.Release();

            // Calculate the compute buffer again
            int voxelDataStride = Marshal.SizeOf(typeof(Vector3)) + Marshal.SizeOf(typeof(float));
            voxelDataBuffer = new ComputeBuffer(resolution * resolution, voxelDataStride);

            int triangleDataStride = Marshal.SizeOf(typeof(int));
            trianglesBuffer = new ComputeBuffer(resolution * resolution * 9, triangleDataStride);

            int verticesDataStride = Marshal.SizeOf(typeof(Vector3));
            verticesBuffer = new ComputeBuffer(resolution * resolution * 9, verticesDataStride);

            triCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
        }

        public override void Triangulate(VoxelData[] voxelData, float isoLevel, bool useUVMapping = false, bool useInterpolation = true)
        {
            // Compute
            int marchingKernel = computeShader.FindKernel("March");
            int cells = resolution - 1;

            // Clear all the arrays
            Clear();
            
            //Push our prepared data into Buffer
            voxelDataBuffer.SetData(voxelData);

            // Reset the counter of the buffers
            voxelDataBuffer.SetCounterValue(0);
            trianglesBuffer.SetCounterValue(0);
            verticesBuffer.SetCounterValue(0);

            // Set compute data and dispatch it
            computeShader.SetInt("cells", cells);
            computeShader.SetFloat("isoLevel", isoLevel);
            computeShader.SetBool("useInterpolation", useInterpolation);

            computeShader.SetBuffer(marchingKernel, "voxels", voxelDataBuffer);
            computeShader.SetBuffer(marchingKernel, "triangles", trianglesBuffer);
            computeShader.SetBuffer(marchingKernel, "vertices", verticesBuffer);

            computeShader.Dispatch(marchingKernel, threadGroups, 1, 1);

            // Get the data from the buffers
            trianglesBuffer.GetData(trianglesArray);
            verticesBuffer.GetData(verticesArray);

            // Clear all the mesh and mark as dynamic
            mesh.MarkDynamic();

            // Set the mesh vertices and triangles
            mesh.SetVertices(verticesArray);
            mesh.SetTriangles(trianglesArray, 0);

            // Draw the mesh GPU
            Graphics.DrawMesh(mesh, position, Quaternion.identity, material, 0);
        }

        public override void Clear()
        {
            mesh.Clear();
            Array.Clear(trianglesArray, 0, trianglesArray.Length);
            Array.Clear(verticesArray, 0, verticesArray.Length);
        }

        public override void Destroy()
        {
            // Dispose the compute buffers
            voxelDataBuffer?.Dispose();
            trianglesBuffer?.Dispose();
            verticesBuffer?.Dispose();
            triCountBuffer?.Dispose();

            voxelDataBuffer = null;
            trianglesBuffer = null;
            verticesBuffer = null;
            triCountBuffer = null;
        }
    }
}