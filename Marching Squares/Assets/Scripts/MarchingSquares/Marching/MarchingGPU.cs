using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace MarchingSquares
{
    public class MarchingGPU : Marching
    {
        // Compute shader
        private readonly ComputeShader _marchingShader;
        
        // Compute shaders variables
        private ComputeBuffer _voxelDataBuffer;
        private ComputeBuffer _trianglesBuffer;
        private ComputeBuffer _verticesBuffer;
        private ComputeBuffer _uvsBuffer;
        
        private int[] _trianglesArray;
        private Vector3[] _verticesArray;
        private Vector3[] _uvsArray;
        
        // Parameters and settings
        private int _resolution;
        
        // Thread params
        private int _threadGroups;
        private const int ThreadPerGroups = 128;

        // Chunk Material and Position
        private Material _material;
        private Vector3 _position;

        public MarchingGPU(ComputeShader marching)
        {
            _marchingShader = marching;
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
            _threadGroups = Mathf.CeilToInt(_resolution * _resolution / (float) ThreadPerGroups);
        }

        public void Refresh()
        {
            _trianglesBuffer?.Release();
            _verticesBuffer?.Release();
            _voxelDataBuffer?.Release();
            _uvsBuffer?.Release();

            int voxelNumber = _resolution * _resolution;
            
            // Setup the output arrays
            _trianglesArray = new int[voxelNumber * 9];
            _verticesArray = new Vector3[voxelNumber * 6];
            _uvsArray = new Vector3[voxelNumber * 6];
            
            // Calculate the compute buffer again
            _voxelDataBuffer = new ComputeBuffer(voxelNumber, Marshal.SizeOf(typeof(Vector3)) + Marshal.SizeOf(typeof(float)));
            _trianglesBuffer = new ComputeBuffer(voxelNumber * 9, Marshal.SizeOf(typeof(int)));
            _verticesBuffer = new ComputeBuffer(voxelNumber * 6, Marshal.SizeOf(typeof(Vector3)));
            _uvsBuffer = new ComputeBuffer(voxelNumber * 6, Marshal.SizeOf(typeof(Vector3)));
        }

        public override void Triangulate(ref VoxelData[] voxelData, float isoLevel, bool useUVMapping = false, bool useInterpolation = true)
        {
            // Compute
            int marchingKernel = _marchingShader.FindKernel("March");
            int cells = _resolution - 1;
            
            // Clear the mesh
            Clear();
            
            // Push our prepared data into Buffer
            _voxelDataBuffer.SetData(voxelData);
            _trianglesBuffer.SetData(_trianglesArray);
            _verticesBuffer.SetData(_verticesArray);
            _uvsBuffer.SetData(_uvsArray);
            
            // Set the buffers
            _marchingShader.SetBuffer(marchingKernel, "voxels", _voxelDataBuffer);
            _marchingShader.SetBuffer(marchingKernel, "triangles", _trianglesBuffer);
            _marchingShader.SetBuffer(marchingKernel, "vertices", _verticesBuffer);
            _marchingShader.SetBuffer(marchingKernel, "uvs", _uvsBuffer);

            // Update the parameters data and dispatch it
            _marchingShader.SetInt("cells", cells);
            _marchingShader.SetFloat("isoLevel", isoLevel);
            _marchingShader.SetBool("useInterpolation", useInterpolation);
            _marchingShader.SetBool("useUVMapping", useUVMapping);

            _marchingShader.Dispatch(marchingKernel, _threadGroups, 1, 1);

            // Get the data from the buffers
            _trianglesBuffer.GetData(_trianglesArray);
            _verticesBuffer.GetData(_verticesArray);
            _uvsBuffer.GetData(_uvsArray);

            // Compress the mesh
            // List<Vector3> verticesList = new List<Vector3>();
            // List<int> trianglesList = new List<int>();
            
            // CompressMesh(ref _trianglesArray, ref _verticesArray, ref trianglesList, ref verticesList);
            
            // Set the mesh vertices and triangles
            mesh.SetVertices(_verticesArray);
            mesh.SetTriangles(_trianglesArray, 0);

            if (useUVMapping)
            {
                mesh.SetUVs(0, _uvsArray);
            }

            // Mark mesh as dynamic
            mesh.MarkDynamic();
            
            // Draw the mesh GPU
            Graphics.DrawMesh(mesh, _position, Quaternion.identity, _material, 0);
        }

        public override void Clear() => mesh.Clear();

        public override void Destroy()
        {
            // Dispose the compute buffers
            _voxelDataBuffer?.Dispose();
            _trianglesBuffer?.Dispose();
            _verticesBuffer?.Dispose();
            _uvsBuffer?.Dispose();

            _voxelDataBuffer = null;
            _trianglesBuffer = null;
            _verticesBuffer = null;
            _uvsBuffer = null;
        }

        /**
         * Compress the mesh
         * Take in mesh arrays as inputs
         * put compressed mesh into out lists
         */
        private void CompressMesh(ref int[] inTriangles, ref Vector3[] inVertices, ref List<int> outTriangles, ref List<Vector3> outVertices)
        {
            for (int i = 0; i < inTriangles.Length; i += 3)
            {
                if (inTriangles[i] != 0 || inTriangles[i + 1] != 0 || inTriangles[i + 2] != 0)
                {
                    // Add the triangles
                    // Take the vertices from the array with the index inside the triangles array
                    outTriangles.Add(outVertices.Count);
                    outVertices.Add(inVertices[inTriangles[i]]);
                    
                    outTriangles.Add(outVertices.Count);
                    outVertices.Add(inVertices[inTriangles[i + 1]]);
                    
                    outTriangles.Add(outVertices.Count);
                    outVertices.Add(inVertices[inTriangles[i + 2]]);
                }
            }
        }
    }
}