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
        public int mapResolution = 2;
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
        
        private void Awake()
        {
            Initialize();
        }

        [ContextMenu("Refresh voxel map")]
        public void Refresh()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                if(Application.isEditor)
                {
                    DestroyImmediate(transform.GetChild(i).gameObject);
                }
                else
                {
                    Destroy(transform.GetChild(i).gameObject);
                }
            }
            
            if (chunks != null)
            {
                chunks.Clear();
            }

            Initialize();
        }

        public void CreateChunkAt(int chunkIndex, int x, int y)
        {
            VoxelChunk chunk = Instantiate(chunkPrefab);
            chunk.SetNoiseGenerator(noiseGenerator);
            chunk.SetShowVoxelPointGrid(showVoxelPointGrid);
            chunk.SetVoxelScale(voxelScale);

            chunk.transform.parent = transform;
            chunk.transform.localPosition = new Vector3(x * (chunkSize - voxelSize), y * (chunkSize - voxelSize));
            chunk.Initialize(voxelResolution, chunkSize, useInterpolation);
            chunks.Add(chunk);
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
    }
}
