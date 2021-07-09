using UnityEngine;

namespace NoiseGenerator
{
    [CreateAssetMenu(fileName = "Perlin", menuName = "Noises/Perlin noise")]
    public class Perlin : Noise
    {
        public Vector3 frequence;
        public Vector3 offset;
        public float amplitude;

        [Header("Debug settings")]
        public bool useTime;
        public float timeScale;
        private float time;
        
        public override float Generate(float x, float y)
        {
            if(useTime)
            {
                time = Time.time * timeScale;
            }
            else
            {
                time = 0f;
            }

            return amplitude * Mathf.PerlinNoise(x / frequence.x + offset.x + time, y / frequence.y + offset.y + time);
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