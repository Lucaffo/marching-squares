﻿using System;
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
        public Material midIsoValueMaterial;
        public Material lowIsoValueMaterial;

        public Material notUsedMaterial;

        public void Initialize(float x, float y, float size)
        {
            // Calculate its mainly 2 edge positions
            position.x = (x) * size;
            position.y = (y) * size;

            transform.position -= Vector3.forward * 20f;
        }

        public void SetUsedByMarching(bool isUsed)
        {
            isUsedByMarching = isUsed;
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

            if(isUsedByMarching)
            {
                Graphics.DrawMesh(squareMesh, meshMatrix, maxIsoValueMaterial, 0);
                return;
            }

            /*if(value >= 0.9f)
            {
                Graphics.DrawMesh(squareMesh, meshMatrix, maxIsoValueMaterial, 0);
                return;
            }

            // Color gradient features
            if (value >= 0.5f)
            {
                Graphics.DrawMesh(squareMesh, meshMatrix, midIsoValueMaterial, 0);
                return;
            }

            if (value >= 0.15f)
            {
                Graphics.DrawMesh(squareMesh, meshMatrix, lowIsoValueMaterial, 0);
                return;
            }*/ 

            // Low value
            Graphics.DrawMesh(squareMesh, meshMatrix, notUsedMaterial, 0);
        }
    }
}