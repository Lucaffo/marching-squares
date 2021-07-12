using System;
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

        private MeshRenderer meshRenderer;
        private MeshFilter meshFilter;

        private void Awake()
        {
           meshRenderer = GetComponent<MeshRenderer>();
           meshFilter = GetComponent<MeshFilter>();
        }

        private void OnDestroy()
        {
            // If an istance of a material is created,
            // you're responsible to destroy it,
            // altrought it remain in memory causing HUGE memory leaks.
            Destroy(meshRenderer.material);

            // If a mesh is created manually,
            // you're responsible to destroy it,
            // altrought it remain in memory causing HUGE memory leaks.
            Destroy(meshFilter.mesh);
        }

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
            this.transform.position += Vector3.forward * 10f;
        }

        public void SetUsedByMarching(bool isUsed)
        {
            isUsedByMarching = isUsed;
        }

        public void UpdateVoxelColor()
        {
            currentPointColor = isUsedByMarching ? isUsedColor : isNotUsedColor;
        }

        public void ShowVoxel(bool showVoxelPointGrid)
        {
            // Mesh renderer is getted from awake
            if(meshRenderer)
            {
                meshRenderer.enabled = showVoxelPointGrid;

                if (showVoxelPointGrid)
                {
                    // Eventually update the color 
                    UpdateVoxelColor();

                    // Apply the color to the material
                    meshRenderer.material.color = currentPointColor;
                }
            }
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