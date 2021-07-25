using System;
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
            // Re-initialize the array
            if (Math.Abs((mapScale / mapResolution) - chunkSize) > Mathf.Epsilon)
            {
                Initialize();
                Debug.Log(("Reinitialize map"));
            }

            // Refresh all the chunks
            for(int i = 0; i < chunks.Count; i++)
            {
                chunks[i].showVoxelPointGrid = showVoxelPointGrid;
                chunks[i].voxelScale = voxelScale;
                chunks[i].useInterpolation = useInterpolation;
                chunks[i].useUVMapping = useUvMapping;
                chunks[i].useComputeShader = useComputeShader;
                chunks[i].noiseGenerator = noiseGenerator;
                chunks[i].Refresh(chunkResolution);
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

        public void AddNoiseOffset(Vector2 offset)
        {
            noiseGenerator.AddOffset(offset);
        }
    }
}
