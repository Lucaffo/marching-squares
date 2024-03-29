﻿using UnityEngine;

namespace NoiseGenerator
{
    [CreateAssetMenu(fileName = "Perlin", menuName = "Noises/Perlin noise")]
    public class Perlin : Noise
    {
        public float amplitude;
        
        public override float Generate(float x, float y)
        {
            return amplitude * Mathf.PerlinNoise(x / frequence.x + offset.x, y / frequence.y + offset.y);
        }

        public override float Generate(float x, float y, float z)
        {
            throw new System.NotImplementedException();
        }

        public override void AddOffset(Vector2 offset)
        {
            this.offset += (Vector3) offset;
        }

        public override void AddOffset(Vector3 offset)
        {
            this.offset += offset;
        }

        public override void SetOffset(Vector2 offset)
        {
            this.offset = offset;
        }

        public override void SetOffset(Vector3 offset)
        {
            this.offset = offset;
        }
    }
}