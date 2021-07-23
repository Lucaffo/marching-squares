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

    public class VoxelSquare : MonoBehaviour
    {
        // Caching the bool check in order to save computation
        public bool isUsedByMarching;

        [Header("Points mesh materials")]
        public Mesh squareMesh;
        public Material maxIsoValueMaterial;
        [Range(0, 1)] public float midThreeshold = 0.5f;
        public Material midIsoValueMaterial;
        [Range(0, 1)] public float lowThreeshold = 0.2f;
        public Material lowIsoValueMaterial;
        public Material notUsedMaterial;

        private Matrix4x4 meshMatrix;

        public void DrawSquare(float value)
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