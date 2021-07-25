using System;
using System.Collections;
using System.Threading;
using MarchingSquares;
using UnityEngine;

namespace Procedural.Marching.Squares.Utils
{
    public class NoiseOffsetMover : MonoBehaviour
    {
        [Header("Settings parameters")]
        public float scrollSpeed = 1.0f;
        public bool timeScroll = false;

        // Scrolling voxel map
        private VoxelMap map;

        // Get the input direction
        private Vector2 inputDirection;

        // Used by the condition time scroll
        private float time;
        private Vector2 timeDirection;

        private void OnEnable()
        {
            map = FindObjectOfType<VoxelMap>();
        }
        
        private void Update()
        {
            if(timeScroll)
            {
                time = Time.deltaTime;
                timeDirection = Vector2.one * time;
            }
            else
            {
                time = 0f;
                timeDirection = Vector2.zero;
            }
            
            inputDirection = (Vector2.right * Input.GetAxis("Horizontal") + Vector2.up * Input.GetAxis("Vertical")) * Time.deltaTime;
            inputDirection.Normalize();
            
            // Don't scroll if any direction is 0
            if(inputDirection.sqrMagnitude != 0 || timeDirection.sqrMagnitude != 0)
            {
                map.AddNoiseOffset(inputDirection * scrollSpeed + timeDirection * scrollSpeed);
            }
            
            map.Refresh();
        }
    }
}