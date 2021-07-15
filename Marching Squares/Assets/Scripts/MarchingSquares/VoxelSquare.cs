using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Procedural.Marching.Squares
{
    [Serializable, BurstCompatible]
    public struct VoxelSquareData
    {
        public float value;
        public bool isUsedByMarching;
        public float3 position;
    }

    public class VoxelSquare : MonoBehaviour
    {
        [Header("Voxel square settings")]
        public VoxelSquareData squareData;

        [Header("Points mesh materials")]
        public Mesh squareMesh;

        public Material maxIsoValueMaterial;

        [Range(0, 1)] public float midThreeshold = 0.5f;
        public Material midIsoValueMaterial;

        [Range(0, 1)] public float lowThreeshold = 0.2f;
        public Material lowIsoValueMaterial;

        public Material notUsedMaterial;

        private void Awake()
        {
            squareData = new VoxelSquareData();
        }

        public void Initialize(float x, float y, float size)
        {
            // Calculate its mainly 2 edge positions
            squareData.position.x = (x) * size;
            squareData.position.y = (y) * size;

            transform.position -= Vector3.forward * 20f;
        }

        public void SetUsedByMarching(bool isUsed)
        {
            squareData.isUsedByMarching = isUsed;
        }

        public void ShowVoxel(bool showVoxelPointGrid)
        {
            if(showVoxelPointGrid)
            {
                DrawSquare();
            }
        }

        public void DrawSquare()
        {
            Matrix4x4 meshMatrix = transform.localToWorldMatrix;

            /*if(isUsedByMarching)
            {
                Graphics.DrawMesh(squareMesh, meshMatrix, maxIsoValueMaterial, 0);
                return;
            }*/

            if(squareData.isUsedByMarching)
            {
                Graphics.DrawMesh(squareMesh, meshMatrix, maxIsoValueMaterial, 0);
                return;
            }

            // Color gradient features
            if (squareData.value >= midThreeshold)
            {
                Graphics.DrawMesh(squareMesh, meshMatrix, midIsoValueMaterial, 0);
                return;
            }

            if (squareData.value >= lowThreeshold)
            {
                Graphics.DrawMesh(squareMesh, meshMatrix, lowIsoValueMaterial, 0);
                return;
            } 

            // Low value
            Graphics.DrawMesh(squareMesh, meshMatrix, notUsedMaterial, 0);
        }
    }
}