using NoiseGenerator;
using System.Collections.Generic;
using UnityEngine;

namespace Procedural.Marching.Squares
{
    public class VoxelMap : MonoBehaviour
    {
        [Header("Noise")]
        public Noise noiseGenerator;
        public float refreshTime = 1f; private float timer = 0f;

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
            Initialize();
        }

        private void Update()
        {
            /* timer += Time.deltaTime;

            if(timer >= refreshTime && refreshTime != 0)
            {
                Refresh();
                timer = 0;
            }*/ 
        }

        [ContextMenu("Refresh voxel map")]
        public void Refresh()
        {
            if(chunks != null)
            {
                chunks.Clear();
            }

            foreach (VoxelChunk chunk in transform.GetComponentsInChildren<VoxelChunk>())
            {
                Destroy(chunk.gameObject);
            }

            Initialize();
        }

        public void CreateChunkAt(int chunkIndex, int x, int y)
        {
            VoxelChunk chunk = Instantiate(chunkPrefab);
            chunk.SetNoiseGenerator(noiseGenerator);
            chunk.transform.parent = transform;
            chunk.transform.localPosition = new Vector3(x * (chunkSize - voxelSize), y * (chunkSize - voxelSize));
            chunk.Initialize(voxelResolution, chunkSize);
            chunks.Add(chunk);
        }

        public void AddNoiseOffset(Vector3 offset)
        {
            noiseGenerator.AddOffset((Vector2) offset);
            Refresh();
        }

        private void Initialize()
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
    }
}
