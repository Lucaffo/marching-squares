using UnityEngine;

namespace NoiseGenerator
{
    public abstract class Noise : ScriptableObject
    {
        [Range(0, 1)] public float threshold;
        public abstract float Generate(float x, float y);
        public abstract float Generate(float x, float y, float z);

        public abstract void AddOffset(Vector2 offset);
        public abstract void AddOffset(Vector3 offset);
    }
}