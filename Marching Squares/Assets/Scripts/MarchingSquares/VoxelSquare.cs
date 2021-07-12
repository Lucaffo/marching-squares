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

        [Header("Points mesh materials")]
        public Mesh squareMesh;
        public Material usedMaterial;
        public Material notUsedMaterial;

        public void Initialize(float x, float y, float size)
        {
            // Calculate its mainly 2 edge positions
            position.x = (x) * size;
            position.y = (y) * size;

            xEdgePosition = position;
            xEdgePosition.x += size * 0.5f;
            yEdgePosition = position;
            yEdgePosition.y += size * 0.5f;

            this.transform.position += Vector3.forward * 10f;
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
                Graphics.DrawMesh(squareMesh, meshMatrix, usedMaterial, 0);
            }
            else
            {
                Graphics.DrawMesh(squareMesh, meshMatrix, notUsedMaterial, 0);
            }
        }
    }
}