using System.Collections.Generic;
using NoiseGenerator;
using UnityEngine;

namespace MarchingSquares
{
    public class VoxelMap : MonoBehaviour
    {
        [Header("Noise")]
        public Noise noiseGenerator;

        [Header("Map settings")]
        public float mapScale = 2;
        public bool useInterpolation = false;
        public bool useUvMapping = false;
        public bool useComputeShader = false;

        [Header("Chunk settings")]
        public int mapResolution = 2;
        public VoxelChunk chunkPrefab;

        [Header("Voxel settings")]
        public int chunkResolution = 8;
        public bool showVoxelPointGrid = false;
        [Range(0f, 1f)] public float voxelScale = 0.1f;
        
        // Chunks array
        public List<VoxelChunk> chunks;

        private float chunkSize;

        private void OnApplicationQuit()
        {
            Destroy(gameObject);
        }

        private void Awake()
        {   
            Initialize();
        }

        [ContextMenu("Refresh voxel map")]
        public void Refresh()
        {       
            // Calculate the chunk size
            float chunkSize = mapScale / mapResolution;

            // Re-initialize the array
            if (chunkSize != this.chunkSize)
            {
                Initialize();
            }

            // Refresh all the chunks
            foreach (VoxelChunk chunk in chunks)
            {
                chunk.showVoxelPointGrid = showVoxelPointGrid;
                chunk.voxelScale = voxelScale;
                chunk.useInterpolation = useInterpolation;
                chunk.useUVMapping = useUvMapping;
                chunk.useComputeShader = useComputeShader;
                chunk.noiseGenerator = noiseGenerator;
                
                chunk.Refresh(chunkResolution);
                
                // First chunk case
                if (chunk.chunkX == 0 && chunk.chunkY == 0)
                {
                    chunk.transform.localPosition = Vector3.zero;
                    continue;
                }
                
                // Other chunk cases
                if (chunk.chunkX == chunk.chunkY)
                {
                    // chunk.transform.localPosition = new Vector3(x * (chunkSize) - voxelSize, y * (chunkSize) - voxelSize);
                    chunk.transform.localPosition = Vector3.right * (chunk.chunkX * (chunkSize)) + Vector3.up * (chunk.chunkY * (chunkSize));
                    continue;
                }

                if (chunk.chunkX > chunk.chunkY)
                {
                    chunk.transform.localPosition = Vector3.right * (chunk.chunkX * (chunkSize)) + Vector3.up * (chunk.chunkY * (chunkSize));
                    continue;
                }

                if (chunk.chunkX < chunk.chunkY)
                {
                    chunk.transform.localPosition = Vector3.right * (chunk.chunkX * (chunkSize)) + Vector3.up * (chunk.chunkY * (chunkSize));
                }
            }
        }

        private void Initialize()
        {
            // Calculate the chunk size
            chunkSize = mapScale / mapResolution;

            // Initialize the chunks list
            foreach (VoxelChunk chunk in chunks)
            {
                Destroy(chunk.gameObject);
            }

            chunks = new List<VoxelChunk>();
            
            for (int y = 0; y < mapResolution; y++)
            {
                for (int x = 0; x < mapResolution; x++)
                {
                    CreateChunkAt(x, y);
                }
            }
        }
        
        private void CreateChunkAt(int x, int y)
        {
            VoxelChunk chunk = Instantiate(chunkPrefab, transform);

            chunk.chunkX = x;
            chunk.chunkY = y;

            // First chunk case
            if (chunk.chunkX == 0 && chunk.chunkY == 0)
            {
                chunk.transform.localPosition = Vector3.zero;
            }
            // Every other cases
            else
            {
                chunk.transform.localPosition = Vector3.right * (chunk.chunkX * chunkSize) + Vector3.up * (chunk.chunkY * chunkSize);
            }
            
            chunk.Initialize(chunkSize, chunkResolution);
            chunks.Add(chunk); 
        }

        public void AddNoiseOffset(Vector3 offset)
        {
            noiseGenerator.AddOffset((Vector2) offset);
            Refresh();
        }
    }
}
