using System;
using UnityEngine;

namespace MarchingSquares
{
    [Serializable]
    public struct VoxelData
    {
        public Vector3 position;
        public float value;
    }
    
    public abstract class Marching
    {
        public Mesh mesh;
        public abstract void Initialize(Material voxelMaterial, Vector3 chunkPosition, int chunkResolution);
        public abstract void Triangulate(VoxelData[] voxelData,  float isoLevel,  bool useUVMapping = false, bool useInterpolation = true);
        public abstract void Clear();
        public abstract void Destroy();
    }
}