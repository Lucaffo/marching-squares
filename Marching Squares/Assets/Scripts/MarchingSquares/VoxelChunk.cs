using NoiseGenerator;
using UnityEngine;

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
        
        private VoxelData[] _voxelData;

        private MarchingGPU _marchingGPU;
        private MarchingCPU _marchingCPU;

        private int _chunkResolution;
        private float _chunkSize;
        private float _voxelSize;
        private Vector3 _chunkPosition;

        private void Awake()
        {
            _marchingCPU = new MarchingCPU();
            _marchingGPU = new MarchingGPU(marchingCompute);
        }

        private void OnDestroy()
        {
            _marchingCPU.Destroy();
            _marchingGPU.Destroy();
        }

        public void Initialize(float chunkSize, int chunkResolution)
        {
            gameObject.name = "Chunk(" + chunkX + "," + chunkY + ")";

            _chunkSize = chunkSize;
            _chunkResolution = chunkResolution;
            _voxelSize = chunkSize / (chunkResolution - 1);

            // Create the voxels arrays
            _voxelData = new VoxelData[chunkResolution * chunkResolution];

            // Caching the transform position
            _chunkPosition = transform.localPosition;

            // Create all the voxels
            int voxelIndex = 0;
            for (int y = 0; y < chunkResolution; y++)
            {
                for (int x = 0; x < chunkResolution; x++)
                {
                    // Set the voxel position
                    _voxelData[voxelIndex].position = Vector3.right * ( x * _voxelSize ) + Vector3.up * ( y * _voxelSize );
                    voxelIndex++;
                }
            }
            
            // Initialize the current used marching method
            _marchingGPU.Initialize(voxelMaterial, _chunkPosition, chunkResolution);
            _marchingCPU.Initialize(voxelMaterial, _chunkPosition, chunkResolution);
        }

        public void Refresh(int chunkRes)
        {
            if (_chunkResolution != chunkRes)
            {
                // Re-initialize the chunk
                Initialize(_chunkSize, chunkRes);
                Debug.Log(("Reinitialize chunks"));
            }

            for(int i = 0; i < _voxelData.Length; i++)
            {
                _voxelData[i].value = noiseGenerator.Generate(_voxelData[i].position.x + _chunkPosition.x, _voxelData[i].position.y + _chunkPosition.y);    
            }
            
            // Refresh the triangulation
            if(useComputeShader)
            {
                _marchingGPU.Refresh();
                _marchingGPU.Triangulate(ref _voxelData, noiseGenerator.isoLevel, useUVMapping, useInterpolation);
            }
            else
            {
                _marchingCPU.Triangulate(ref _voxelData, noiseGenerator.isoLevel, useUVMapping, useInterpolation);
            }
        }

        public Mesh GetMesh()
        {
            // Return the mesh used by the current used method
            return useComputeShader ? _marchingGPU.mesh : _marchingCPU.mesh;
        }
    }
}