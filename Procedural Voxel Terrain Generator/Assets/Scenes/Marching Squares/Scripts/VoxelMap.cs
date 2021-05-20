using System.Collections.Generic;
using UnityEngine;

namespace Procedural.Marching.Squares
{
    public class VoxelMap : MonoBehaviour
    {
        [Header("Map settings")]
        public int mapResolution = 2;

        [Header("Chunk settings")]
        public int chunkResolution = 2;
        public VoxelChunk chunkPrefab;

        [Header("Voxel settings")]
        public int voxelResolution = 8;

        // Chunks array
        private List<VoxelChunk> chunks;

        private float chunkSize, voxelSize, halfSize;

        private void Awake()
        {
            halfSize = mapResolution * 0.5f;
            chunkSize = mapResolution / chunkResolution;
            voxelSize = chunkSize / voxelResolution;

            chunks = new List<VoxelChunk>();
            
            int chunkIndex = 0;

            for (int y = 0; y < chunkResolution; y++)
            {
                for (int x = 0; x < chunkResolution; x++)
                {
                    CreateChunkAt(chunkIndex, x, y);
                    chunkIndex++;
                }
            }
        }

        public void CreateChunkAt(int chunkIndex, int x, int y)
        {
            VoxelChunk chunk = Instantiate(chunkPrefab);
            chunk.Initialize(voxelResolution, chunkSize);
            chunk.transform.parent = transform;
            chunk.transform.localPosition = new Vector3(x * chunkSize - halfSize, y * chunkSize - halfSize);
            chunks.Add(chunk);
        }
    }
}
