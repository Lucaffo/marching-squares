using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using NoiseGenerator;
using Procedural.Marching.Squares;
using UnityEngine;
using UnityEngine.Rendering;

namespace MarchingSquares
{
    // Select only parent
    [SelectionBase]
    public class VoxelChunk : MonoBehaviour
    {
        [Header("Single Voxel settings")]
        
        // Voxel prefab and compute shader
        public ComputeShader marchingCompute;
        public Material voxelMaterial;

        // Can't touch these
        [HideInInspector] public int chunkX, chunkY;
        [HideInInspector] public Noise noiseGenerator;
        [HideInInspector] public float voxelScale;
        [HideInInspector] public bool showVoxelPointGrid;
        [HideInInspector] public bool useInterpolation;
        [HideInInspector] public bool useUVMapping;
        [HideInInspector] public bool useComputeShader;
        
        [SerializeField] private VoxelData[] voxelData;

        private MarchingGPU marchingGPU;
        private MarchingCPU marchingCPU;

        private int _chunkResolution;
        private float _chunkSize;
        private float _voxelSize;
        private Vector3 _chunkPosition;

        private void Awake()
        {
            marchingCPU = new MarchingCPU();
            marchingGPU = new MarchingGPU(marchingCompute);
        }

        private void OnDestroy()
        {
            marchingCPU.Destroy();
            marchingGPU.Destroy();
        }

        public void Initialize(float chunkSize, int chunkResolution)
        {
            gameObject.name = "Chunk(" + chunkX + "," + chunkY + ")";

            _chunkSize = chunkSize;
            _chunkResolution = chunkResolution;
            _voxelSize = chunkSize / (chunkResolution - 1);

            // Create the voxels arrays
            voxelData = new VoxelData[chunkResolution * chunkResolution];

            // Caching the transform position
            _chunkPosition = transform.localPosition;

            // Create all the voxels
            int voxelIndex = 0;
            for (int y = 0; y < chunkResolution; y++)
            {
                for (int x = 0; x < chunkResolution; x++)
                {
                    // Set the voxel position
                    voxelData[voxelIndex].position = Vector3.right * ( x * _voxelSize ) + Vector3.up * ( y * _voxelSize );
                    voxelIndex++;
                }
            }
            
            // Initialize the current used marching method
            marchingGPU.Initialize(voxelMaterial, _chunkPosition, chunkResolution);
            marchingCPU.Initialize(voxelMaterial, _chunkPosition, chunkResolution);
        }

        public void Refresh(int chunkRes)
        {
            if (_chunkResolution != chunkRes)
            {
                Initialize(_chunkSize, chunkRes);
            }

            for(int i = 0; i < voxelData.Length; i++)
            {
                voxelData[i].value = noiseGenerator.Generate(voxelData[i].position.x + _chunkPosition.x, voxelData[i].position.y + _chunkPosition.y);    
            }

            if (showVoxelPointGrid)
            {
                // Draw the square on grid
            }
            
            // Refresh the triangulation
            if(useComputeShader)
            {
                marchingGPU.Refresh();
                marchingGPU.Triangulate(voxelData, noiseGenerator.isoLevel, useUVMapping, useInterpolation);
            }
            else
            {
                marchingCPU.Triangulate(voxelData, noiseGenerator.isoLevel, useUVMapping, useInterpolation);
            }
        }

        public Mesh GetMesh()
        {
            // Return the mesh used by the current used method
            return useComputeShader ? marchingGPU.mesh : marchingCPU.mesh;
        }
    }
}