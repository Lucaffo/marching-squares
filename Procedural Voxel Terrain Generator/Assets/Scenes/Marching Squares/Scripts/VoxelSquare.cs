using UnityEngine;

namespace Procedural.Marching.Squares
{
    public class VoxelSquare : MonoBehaviour
    {
        // Is regarded by the marching square algorithm?
        [SerializeField]
        [Range(0, 100)] public float isRegardedPercentage = 50;
        public bool isRegarded;

        // Voxel material and mesh
        private Material voxelMaterial;

        public Vector2 position, xEdgePosition, yEdgePosition;

        public void Initialize(int x, int y, float size)
        {
            // Calculate its mainly 2 edge positions
            position.x = (x + 0.5f) * size;
            position.y = (y + 0.5f) * size;

            xEdgePosition = position;
            xEdgePosition.x += size * 0.5f;
            yEdgePosition = position;
            yEdgePosition.y += size * 0.5f;

            // Is regarded by the marching square algorithm
            isRegarded = Random.Range(0, 100) < isRegardedPercentage;

            // Get necessary components
            voxelMaterial = GetComponent<MeshRenderer>().material;

            // Update color and triangulate the mesh
            UpdateVoxelColor();
        }

        public void UpdateVoxelColor()
        {
            voxelMaterial.color = isRegarded ? Color.black : Color.white;
        }
    }
}