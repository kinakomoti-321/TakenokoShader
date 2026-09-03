using UnityEngine;

namespace Takenoko
{
    /// <summary>
    /// Per texel surface point produced by rasterizing the mesh into UV space.
    /// </summary>
    public sealed class UvSampleMap
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public Vector3[] Positions { get; private set; }
        public Vector3[] Normals { get; private set; }
        public bool[] Valid { get; private set; }
        public int ValidCount { get; private set; }

        public UvSampleMap(int width, int height)
        {
            Width = width;
            Height = height;
            Positions = new Vector3[width * height];
            Normals = new Vector3[width * height];
            Valid = new bool[width * height];
        }

        public void MarkValid(int index, Vector3 position, Vector3 normal)
        {
            Positions[index] = position;
            Normals[index] = normal;
            Valid[index] = true;
            ValidCount++;
        }
    }

    public static class UvRasterizer
    {
        /// <summary>
        /// Rasterizes every triangle into texel space, writing the interpolated world
        /// position and normal of each covered texel.
        ///
        /// Coverage is conservative: a texel counts as covered when the triangle comes
        /// within half a texel diagonal of its center, not only when the center falls
        /// inside the triangle. Without that, texels straddling an island border stay
        /// empty and leave a dashed seam along every edge.
        ///
        /// UV overlap is resolved first-write-wins by design; overlapping layouts are
        /// out of scope for this baker.
        /// </summary>
        public static UvSampleMap Rasterize(BakeScene scene, int width, int height)
        {
            UvSampleMap map = new UvSampleMap(width, height);

            // Half the diagonal of a texel square, in texel units.
            const float coverageRadius = 0.70711f;
            const float coverageRadiusSquared = coverageRadius * coverageRadius;

            Vector2[] uvs = scene.Uvs;
            Vector3[] positions = scene.Positions;
            Vector3[] normals = scene.Normals;

            for (int triangle = 0; triangle < scene.TriangleCount; triangle++)
            {
                int i = triangle * 3;

                Vector2 uv0 = new Vector2(uvs[i].x * width, uvs[i].y * height);
                Vector2 uv1 = new Vector2(uvs[i + 1].x * width, uvs[i + 1].y * height);
                Vector2 uv2 = new Vector2(uvs[i + 2].x * width, uvs[i + 2].y * height);

                float doubleArea = Cross(uv1 - uv0, uv2 - uv0);
                if (doubleArea > -1.0e-9f && doubleArea < 1.0e-9f)
                {
                    continue;
                }

                float inverseDoubleArea = 1.0f / doubleArea;

                int minX = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(uv0.x, Mathf.Min(uv1.x, uv2.x)) - 1.0f));
                int maxX = Mathf.Min(width - 1, Mathf.CeilToInt(Mathf.Max(uv0.x, Mathf.Max(uv1.x, uv2.x)) + 1.0f));
                int minY = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(uv0.y, Mathf.Min(uv1.y, uv2.y)) - 1.0f));
                int maxY = Mathf.Min(height - 1, Mathf.CeilToInt(Mathf.Max(uv0.y, Mathf.Max(uv1.y, uv2.y)) + 1.0f));

                for (int y = minY; y <= maxY; y++)
                {
                    for (int x = minX; x <= maxX; x++)
                    {
                        int index = y * width + x;
                        if (map.Valid[index])
                        {
                            continue;
                        }

                        Vector2 center = new Vector2(x + 0.5f, y + 0.5f);
                        Vector3 barycentric = Barycentric(center, uv0, uv1, uv2, inverseDoubleArea);

                        if (barycentric.x < 0.0f || barycentric.y < 0.0f || barycentric.z < 0.0f)
                        {
                            Vector2 closest = ClosestPointOnTriangle(center, uv0, uv1, uv2);
                            if ((closest - center).sqrMagnitude > coverageRadiusSquared)
                            {
                                continue;
                            }

                            barycentric = Barycentric(closest, uv0, uv1, uv2, inverseDoubleArea);
                            barycentric.x = Mathf.Clamp01(barycentric.x);
                            barycentric.y = Mathf.Clamp01(barycentric.y);
                            barycentric.z = Mathf.Clamp01(barycentric.z);

                            float sum = barycentric.x + barycentric.y + barycentric.z;
                            if (sum < 1.0e-6f)
                            {
                                continue;
                            }

                            barycentric /= sum;
                        }

                        Vector3 position =
                            positions[i] * barycentric.x +
                            positions[i + 1] * barycentric.y +
                            positions[i + 2] * barycentric.z;

                        Vector3 normal =
                            normals[i] * barycentric.x +
                            normals[i + 1] * barycentric.y +
                            normals[i + 2] * barycentric.z;

                        if (normal.sqrMagnitude < 1.0e-12f)
                        {
                            continue;
                        }

                        map.MarkValid(index, position, normal.normalized);
                    }
                }
            }

            return map;
        }

        private static Vector3 Barycentric(Vector2 point, Vector2 a, Vector2 b, Vector2 c, float inverseDoubleArea)
        {
            float w0 = Cross(b - point, c - point) * inverseDoubleArea;
            float w1 = Cross(c - point, a - point) * inverseDoubleArea;
            float w2 = Cross(a - point, b - point) * inverseDoubleArea;
            return new Vector3(w0, w1, w2);
        }

        private static Vector2 ClosestPointOnTriangle(Vector2 point, Vector2 a, Vector2 b, Vector2 c)
        {
            Vector2 best = ClosestPointOnSegment(point, a, b);
            float bestDistance = (best - point).sqrMagnitude;

            Vector2 candidate = ClosestPointOnSegment(point, b, c);
            float candidateDistance = (candidate - point).sqrMagnitude;
            if (candidateDistance < bestDistance)
            {
                best = candidate;
                bestDistance = candidateDistance;
            }

            candidate = ClosestPointOnSegment(point, c, a);
            candidateDistance = (candidate - point).sqrMagnitude;
            if (candidateDistance < bestDistance)
            {
                best = candidate;
            }

            return best;
        }

        private static Vector2 ClosestPointOnSegment(Vector2 point, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            float lengthSquared = Vector2.Dot(ab, ab);
            if (lengthSquared < 1.0e-12f)
            {
                return a;
            }

            float t = Mathf.Clamp01(Vector2.Dot(point - a, ab) / lengthSquared);
            return a + ab * t;
        }

        private static float Cross(Vector2 a, Vector2 b)
        {
            return a.x * b.y - a.y * b.x;
        }
    }
}
