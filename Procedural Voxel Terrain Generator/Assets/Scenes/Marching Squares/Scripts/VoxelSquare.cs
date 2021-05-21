using UnityEngine;

namespace Procedural.Marching.Squares
{
    public class VoxelSquare : MonoBehaviour
    {
        public bool isUsedByMarching;
        
        private Material voxelMaterial;

        public Vector2 position, xEdgePosition, yEdgePosition;
        
        public void Initialize(float x, float y, float size)
        {
            // Calculate its mainly 2 edge positions
            position.x = (x) * size;
            position.y = (y) * size;

            xEdgePosition = position;
            xEdgePosition.x += size * 0.5f;
            yEdgePosition = position;
            yEdgePosition.y += size * 0.5f;

            // Get necessary components
            voxelMaterial = GetComponent<MeshRenderer>().sharedMaterial;
        }

        public void SetUsedByMarching(bool isUsed)
        {
            isUsedByMarching = isUsed;
            UpdateVoxelColor();
        }

        public void UpdateVoxelColor()
        {
            voxelMaterial.color = isUsedByMarching ? Color.black : Color.white;
        }
    }
}