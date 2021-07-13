using NoiseGenerator;
using System.Collections.Generic;
using UnityEngine;

namespace Procedural.Marching.Squares
{
    public class VoxelMap : MonoBehaviour
    {
        [Header("Noise")]
        public Noise noiseGenerator;

        [Header("Map settings")]
        public int mapScale = 2;
        public bool useInterpolation = false;

        [Header("Chunk settings")]
        public int chunkResolution = 2;
        public VoxelChunk chunkPrefab;

        [Header("Voxel settings")]
        public int voxelResolution = 8;
        public bool showVoxelPointGrid = false;
        [Range(0f, 1f)] public float voxelScale = 0.1f;
        
        // Chunks array
        private List<VoxelChunk> chunks;

        private float chunkSize, voxelSize;

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
            float chunkSize = mapScale / chunkResolution;
            float voxelSize = chunkSize / voxelResolution;

            if (chunkSize != this.chunkSize)
            {
                foreach (VoxelChunk chunk in chunks)
                {
                    Destroy(chunk.gameObject);
                }

                Initialize();
            }

            // Refresh all the chunks
            foreach (VoxelChunk chunk in chunks)
            {
                chunk.SetShowVoxelPointGrid(showVoxelPointGrid);
                chunk.SetVoxelScale(voxelScale);
                chunk.Initialize(voxelResolution, chunkSize, useInterpolation);

                // First chunk case
                if (chunk.x == 0 && chunk.y == 0)
                {
                    chunk.transform.localPosition = Vector3.zero;
                    continue;
                }

                // Other chunk cases
                if (chunk.x == chunk.y)
                {
                    // chunk.transform.localPosition = new Vector3(x * (chunkSize) - voxelSize, y * (chunkSize) - voxelSize);
                    chunk.transform.localPosition = Vector3.right * (chunk.x * (chunkSize - voxelSize)) + Vector3.up * (chunk.y * (chunkSize - voxelSize));
                    continue;
                }

                if (chunk.x > chunk.y)
                {
                    chunk.transform.localPosition = Vector3.right * (chunk.x * (chunkSize - voxelSize)) + Vector3.up * (chunk.y * (chunkSize - voxelSize));
                    continue;
                }

                if (chunk.x < chunk.y)
                {
                    chunk.transform.localPosition = Vector3.right * (chunk.x * (chunkSize - voxelSize)) + Vector3.up * (chunk.y * (chunkSize - voxelSize));
                }
            }
        }

        public void CreateChunkAt(int x, int y)
        {
            VoxelChunk chunk = Instantiate(chunkPrefab);
            chunk.SetNoiseGenerator(noiseGenerator);
            chunk.SetShowVoxelPointGrid(showVoxelPointGrid);
            chunk.SetVoxelScale(voxelScale);
            chunk.transform.parent = transform;

            chunk.x = x;
            chunk.y = y;

            chunk.Initialize(voxelResolution, chunkSize, useInterpolation);
            chunks.Add(chunk);

            // First chunk case
            if (x == 0 && y == 0)
            {
                return;
            }

            // Other chunk cases
            if(x == y)
            {
                // chunk.transform.localPosition = new Vector3(x * (chunkSize) - voxelSize, y * (chunkSize) - voxelSize);
                chunk.transform.localPosition = Vector3.right * (x * (chunkSize - voxelSize)) + Vector3.up * (y * (chunkSize - voxelSize));
                return;
            }
            
            if(x > y)
            {
                chunk.transform.localPosition = Vector3.right * (x * (chunkSize - voxelSize)) + Vector3.up * (y * (chunkSize - voxelSize));
                return;
            }
            
            if(x < y)
            {
                chunk.transform.localPosition = Vector3.right * (x * (chunkSize - voxelSize)) + Vector3.up * (y * (chunkSize - voxelSize));
            }
        }

        public void AddNoiseOffset(Vector3 offset)
        {
            noiseGenerator.AddOffset((Vector2) offset);
            Refresh();
        }

        public void SetNoiseOffset(Vector2 offset)
        {
            noiseGenerator.SetOffset(offset);
        }

        private void Initialize()
        {
            chunkSize = mapScale / chunkResolution;
            voxelSize = chunkSize / voxelResolution;

            chunks = new List<VoxelChunk>();

            for (int y = 0; y < chunkResolution; y++)
            {
                for (int x = 0; x < chunkResolution; x++)
                {
                    CreateChunkAt(x, y);
                }
            }
        }
    }
}
