using UnityEngine;

namespace Procedural.Marching.Squares
{
    public class VoxelSquare : MonoBehaviour
    {
        [Header("Voxel square settings")]
        public float value;
        public bool isUsedByMarching;
        public Vector2 position, xEdgePosition, yEdgePosition;
        private float size;

        [Header("Points clouds colors")]
        public bool drawGizmos;
        public Color isUsedColor;
        public Color isNotUsedColor;
        private Color currentPointColor;
        
        public void Initialize(float x, float y, float size)
        {
            // Calculate its mainly 2 edge positions
            position.x = (x) * size;
            position.y = (y) * size;

            xEdgePosition = position;
            xEdgePosition.x += size * 0.5f;
            yEdgePosition = position;
            yEdgePosition.y += size * 0.5f;
            
            this.size = size;
        }

        public void SetUsedByMarching(bool isUsed)
        {
            isUsedByMarching = isUsed;
            UpdateVoxelColor();
        }

        public void UpdateVoxelColor()
        {
            currentPointColor = isUsedByMarching ? isUsedColor : isNotUsedColor;
        }

        private void OnDrawGizmos()
        {
            if(drawGizmos)
            {
                Gizmos.color = currentPointColor;
                Gizmos.DrawCube(position, Vector2.one * size * 0.5f);
            }
        }
    }
}