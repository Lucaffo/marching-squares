using System;
using UnityEngine;

namespace Procedural.Marching.Squares
{
    public class VoxelSquare : MonoBehaviour
    {
        [Header("Voxel square settings")]
        public float value;
        public bool isUsedByMarching;
        
        public Vector2 position;

        [Header("Points mesh materials")]
        public Mesh squareMesh;

        public Material maxIsoValueMaterial;

        [Range(0, 1)] public float midThreeshold = 0.5f;
        public Material midIsoValueMaterial;

        [Range(0, 1)] public float lowThreeshold = 0.2f;
        public Material lowIsoValueMaterial;

        public Material notUsedMaterial;

        private Matrix4x4 meshMatrix;

        public void Initialize(float x, float y, float size)
        {
            // Calculate its mainly 2 edge positions and cache it
            position.x = (x) * size;
            position.y = (y) * size;
        }

        public void DrawSquare()
        {
            // Cache the mesh matrix
            meshMatrix = transform.localToWorldMatrix;

            if (isUsedByMarching)
            {
                Graphics.DrawMesh(squareMesh, meshMatrix, maxIsoValueMaterial, 0);
                return;
            }

            // Color gradient features
            if (value >= midThreeshold)
            {
                Graphics.DrawMesh(squareMesh, meshMatrix, midIsoValueMaterial, 0);
                return;
            }

            if (value >= lowThreeshold)
            {
                Graphics.DrawMesh(squareMesh, meshMatrix, lowIsoValueMaterial, 0);
                return;
            } 

            // Low value
            Graphics.DrawMesh(squareMesh, meshMatrix, notUsedMaterial, 0);
        }
    }
}