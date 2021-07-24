using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace MarchingSquares
{
    public class MarchingGPU : Marching
    {
        // Compute shader
        private readonly ComputeShader _computeShader;
        
        // Compute shaders variables
        private ComputeBuffer _voxelDataBuffer;
        private ComputeBuffer _trianglesBuffer;
        private ComputeBuffer _verticesBuffer;
        
        private int[] _trianglesArray;
        private Vector3[] _verticesArray;
        
        // Parameters and settings
        private int _resolution;
        
        // Thread params
        private int _threadGroups;
        private const int ThreadPerGroups = 64;

        // Chunk Material and Position
        private Material _material;
        private Vector3 _position;

        public MarchingGPU(ComputeShader shader)
        {
            _computeShader = shader;
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
            _material = voxelMaterial;
            _position = chunkPosition;

            // Set the chunk resolution
            _resolution = chunkResolution;
            
            // Setup the thread groups
            _threadGroups = Mathf.CeilToInt(_resolution * _resolution / (float)ThreadPerGroups);
        }

        public void Refresh()
        {
            _trianglesBuffer?.Release();
            _verticesBuffer?.Release();
            _voxelDataBuffer?.Release();

            int voxelNumber = _resolution * _resolution;

            // Setup the output arrays
            _trianglesArray = new int[voxelNumber * 9];
            _verticesArray = new Vector3[voxelNumber * 6];
            
            // Calculate the compute buffer again
            _voxelDataBuffer = new ComputeBuffer(voxelNumber, Marshal.SizeOf(typeof(Vector3)) + Marshal.SizeOf(typeof(float)));
            _trianglesBuffer = new ComputeBuffer(voxelNumber * 9, Marshal.SizeOf(typeof(int)));
            _verticesBuffer = new ComputeBuffer(voxelNumber * 6, Marshal.SizeOf(typeof(Vector3)));
        }

        public override void Triangulate(VoxelData[] voxelData, float isoLevel, bool useUVMapping = false, bool useInterpolation = true)
        {
            // Compute
            int marchingKernel = _computeShader.FindKernel("March");
            int cells = _resolution - 1;
            
            // Clear the mesh
            Clear();
            
            // Reset the counter of the buffers
            _voxelDataBuffer.SetCounterValue(0);
            _trianglesBuffer.SetCounterValue(0);
            _verticesBuffer.SetCounterValue(0);
            
            //Push our prepared data into Buffer
            _voxelDataBuffer.SetData(voxelData);
            _trianglesBuffer.SetData(_trianglesArray);
            _verticesBuffer.SetData(_verticesArray);

            // Set compute data and dispatch it
            _computeShader.SetInt("cells", cells);
            _computeShader.SetFloat("isoLevel", isoLevel);
            _computeShader.SetBool("useInterpolation", useInterpolation);

            _computeShader.SetBuffer(marchingKernel, "voxels", _voxelDataBuffer);
            _computeShader.SetBuffer(marchingKernel, "triangles", _trianglesBuffer);
            _computeShader.SetBuffer(marchingKernel, "vertices", _verticesBuffer);

            _computeShader.Dispatch(marchingKernel, _threadGroups, 1, 1);

            // Get the data from the buffers
            _trianglesBuffer.GetData(_trianglesArray);
            _verticesBuffer.GetData(_verticesArray);

            // Clear all the mesh and mark as dynamic
            mesh.MarkDynamic();

            // Set the mesh vertices and triangles
            mesh.SetVertices(_verticesArray);
            mesh.SetTriangles(_trianglesArray, 0);
            
            // Draw the mesh GPU
            Graphics.DrawMesh(mesh, _position, Quaternion.identity, _material, 0);
        }

        public override void Clear()
        {
            mesh.Clear();
        }

        public override void Destroy()
        {
            // Dispose the compute buffers
            _voxelDataBuffer?.Dispose();
            _trianglesBuffer?.Dispose();
            _verticesBuffer?.Dispose();

            _voxelDataBuffer = null;
            _trianglesBuffer = null;
            _verticesBuffer = null;
        }
    }
}