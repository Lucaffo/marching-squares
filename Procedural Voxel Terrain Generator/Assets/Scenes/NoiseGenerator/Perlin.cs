using UnityEngine;

namespace NoiseGenerator
{
    [CreateAssetMenu(fileName = "Perlin", menuName = "Noises/Perlin noise")]
    public class Perlin : Noise
    {
        public Vector3 frequence;
        public Vector3 offset;
        public float amplitude;

        public override float Generate(float x, float y)
        {
            return amplitude * Mathf.PerlinNoise(x / frequence.x + offset.x, y / frequence.y + offset.y);
        }

        public override float Generate(float x, float y, float z)
        {
            throw new System.NotImplementedException();
        }
    }
}