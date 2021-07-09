using Procedural.Marching.Squares;
using UnityEngine;

namespace MarchingSquare.Utils
{
    public class NoiseOffsetMover : MonoBehaviour
    {
        [Header("Settings parameters")]
        public float scrollSpeed = 1.0f;

        // Scrolling voxel map
        private VoxelMap map;

        // Get the input direction
        private Vector2 inputDirection;

        private void Start()
        {
            map = FindObjectOfType<VoxelMap>();
        }

        private void Update()
        {
            inputDirection = Vector2.right * Input.GetAxis("Horizontal") + Vector2.up * Input.GetAxis("Vertical");
            inputDirection.Normalize();

            map.AddNoiseOffset(inputDirection * scrollSpeed);
        }
    }
}