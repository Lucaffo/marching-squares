﻿using UnityEngine;

namespace NoiseGenerator
{
    public abstract class Noise : ScriptableObject
    {
        [Range(0, 1)] public float isoLevel;
        public Vector3 offset;
        public Vector3 frequence;

        public abstract float Generate(float x, float y);
        public abstract float Generate(float x, float y, float z);

        public abstract void AddOffset(Vector2 offset);
        public abstract void AddOffset(Vector3 offset);

        public abstract void SetOffset(Vector2 offset);
        public abstract void SetOffset(Vector3 offset);
    }
}