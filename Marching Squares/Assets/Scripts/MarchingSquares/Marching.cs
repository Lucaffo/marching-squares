using UnityEngine;

namespace MarchingSquares
{
    public abstract class Marching
    {
        public Mesh mesh;
        public abstract void Initialize(Material voxelMaterial, Vector3 chunkPosition, int chunkResolution);
        public abstract void Triangulate(VoxelData[] voxelData,  float isoLevel,  bool useUVMapping = false, bool useInterpolation = true);
        public abstract void Clear();
        public abstract void Destroy();
    }
}