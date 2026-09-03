using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Takenoko
{
    /// <summary>
    /// Everything a target needs that is shared across texels.
    /// </summary>
    public struct BakeContext
    {
        public BakeScene Scene;
        public BvhRayCaster Caster;
        public int SampleCount;

        /// <summary>
        /// Diagonal of the combined bounds of every object in the bake, used as the
        /// reference length for distance settings.
        /// </summary>
        public float SceneDiagonal;

        /// <summary>
        /// Ray offset that keeps a ray from hitting the triangle it started on.
        /// </summary>
        public float RayBias;
    }

    /// <summary>
    /// A single bakeable quantity. Add a subclass and it shows up in the window's
    /// target list automatically.
    ///
    /// Note that this polymorphism is a CPU-only convenience: a GPU path will need
    /// one kernel per target rather than a virtual call, so keep Evaluate simple
    /// enough to translate.
    /// </summary>
    public abstract class BakeTarget
    {
        public abstract string DisplayName { get; }

        /// <summary>
        /// Suffix appended to the default output file name.
        /// </summary>
        public abstract string FileSuffix { get; }

        public virtual void DrawSettings()
        {
        }

        public virtual void LoadSettings(string prefix)
        {
        }

        public virtual void SaveSettings(string prefix)
        {
        }

        /// <summary>
        /// Called once before the texel loop starts.
        /// </summary>
        public virtual void Prepare(BakeContext context)
        {
        }

        /// <summary>
        /// Returns the value for one texel. Called from worker threads, so this must
        /// not touch Unity APIs or mutate shared state.
        /// </summary>
        public abstract float Evaluate(in BakeContext context, Vector3 position, Vector3 normal, uint seed);

        public static BakeTarget[] CreateAll()
        {
            List<BakeTarget> targets = new List<BakeTarget>();
            foreach (Type type in TypeCache.GetTypesDerivedFrom<BakeTarget>())
            {
                if (type.IsAbstract || type.GetConstructor(Type.EmptyTypes) == null)
                {
                    continue;
                }

                targets.Add((BakeTarget)Activator.CreateInstance(type));
            }

            targets.Sort((a, b) => string.CompareOrdinal(a.DisplayName, b.DisplayName));
            return targets.ToArray();
        }
    }

    public static class BakeSampling
    {
        /// <summary>
        /// Branchless orthonormal basis (Duff et al.), avoiding the degenerate case of
        /// picking a helper axis parallel to the normal.
        /// </summary>
        public static void BuildBasis(Vector3 normal, out Vector3 tangent, out Vector3 bitangent)
        {
            float sign = normal.z >= 0.0f ? 1.0f : -1.0f;
            float a = -1.0f / (sign + normal.z);
            float b = normal.x * normal.y * a;

            tangent = new Vector3(1.0f + sign * normal.x * normal.x * a, sign * b, -sign * normal.x);
            bitangent = new Vector3(b, sign + normal.y * normal.y * a, -normal.y);
        }

        /// <summary>
        /// Cosine weighted direction inside a cone of the given half angle around the
        /// axis. Cosine weighting on a hemisphere is a uniform disk projected onto it,
        /// so restricting the disk radius to sin(halfAngle) gives exactly the cone.
        /// </summary>
        public static Vector3 SampleConeCosine(Vector3 axis, float sinHalfAngle, float u1, float u2)
        {
            Vector3 tangent;
            Vector3 bitangent;
            BuildBasis(axis, out tangent, out bitangent);

            float radius = sinHalfAngle * Mathf.Sqrt(u1);
            float phi = 2.0f * Mathf.PI * u2;
            float x = radius * Mathf.Cos(phi);
            float y = radius * Mathf.Sin(phi);
            float z = Mathf.Sqrt(Mathf.Max(0.0f, 1.0f - radius * radius));

            return tangent * x + bitangent * y + axis * z;
        }

        public static uint Hash(uint value)
        {
            value ^= value >> 16;
            value *= 0x7feb352du;
            value ^= value >> 15;
            value *= 0x846ca68bu;
            value ^= value >> 16;
            return value;
        }

        public static float RadicalInverse2(uint bits)
        {
            bits = (bits << 16) | (bits >> 16);
            bits = ((bits & 0x55555555u) << 1) | ((bits & 0xAAAAAAAAu) >> 1);
            bits = ((bits & 0x33333333u) << 2) | ((bits & 0xCCCCCCCCu) >> 2);
            bits = ((bits & 0x0F0F0F0Fu) << 4) | ((bits & 0xF0F0F0F0u) >> 4);
            bits = ((bits & 0x00FF00FFu) << 8) | ((bits & 0xFF00FF00u) >> 8);
            return bits * 2.3283064365386963e-10f;
        }

        public static float Fract(float value)
        {
            return value - Mathf.Floor(value);
        }
    }
}
