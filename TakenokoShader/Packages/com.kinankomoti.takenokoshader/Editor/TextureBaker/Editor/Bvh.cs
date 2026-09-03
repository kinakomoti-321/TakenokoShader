using System.Runtime.InteropServices;
using UnityEngine;

namespace Takenoko
{
    /// <summary>
    /// 32 bytes, laid out as two float4s so the same array can be uploaded to a
    /// StructuredBuffer without repacking. Count is zero for internal nodes, and
    /// LeftFirst is then the index of the left child; the right child always sits
    /// at LeftFirst + 1.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct BvhNode
    {
        public Vector3 BoundsMin;
        public int LeftFirst;
        public Vector3 BoundsMax;
        public int Count;
    }

    public struct BvhHit
    {
        public float Distance;
        public int TriangleIndex;
        public bool Backface;
    }

    /// <summary>
    /// Binned SAH BVH over a triangle soup. Traversal lives in BvhRayCaster so the
    /// per-ray stack is owned per thread rather than by the shared tree.
    /// </summary>
    public sealed class Bvh
    {
        private const int BinCount = 12;
        private const int MaxLeafSize = 4;

        /// <summary>
        /// Traversal needs one stack slot per level, and a GPU kernel will want the
        /// same bound as a compile time constant.
        /// </summary>
        public const int MaxDepth = 64;

        private readonly Vector3[] positions;
        private readonly Vector3[] centroids;
        private readonly Vector3[] triangleMin;
        private readonly Vector3[] triangleMax;

        private BvhNode[] nodes;
        private int[] triangleIndices;
        private int nodeCount;

        private readonly Vector3[] binMin = new Vector3[BinCount];
        private readonly Vector3[] binMax = new Vector3[BinCount];
        private readonly int[] binCount = new int[BinCount];
        private readonly float[] sweepLeftArea = new float[BinCount - 1];
        private readonly float[] sweepRightArea = new float[BinCount - 1];
        private readonly int[] sweepLeftCount = new int[BinCount - 1];
        private readonly int[] sweepRightCount = new int[BinCount - 1];

        public BvhNode[] Nodes { get { return nodes; } }
        public int[] TriangleIndices { get { return triangleIndices; } }
        public Vector3[] Positions { get { return positions; } }
        public int NodeCount { get { return nodeCount; } }

        public Bvh(Vector3[] positions, int triangleCount)
        {
            this.positions = positions;

            centroids = new Vector3[triangleCount];
            triangleMin = new Vector3[triangleCount];
            triangleMax = new Vector3[triangleCount];
            triangleIndices = new int[triangleCount];

            for (int tri = 0; tri < triangleCount; tri++)
            {
                int i = tri * 3;
                Vector3 v0 = positions[i];
                Vector3 v1 = positions[i + 1];
                Vector3 v2 = positions[i + 2];

                triangleMin[tri] = Vector3.Min(v0, Vector3.Min(v1, v2));
                triangleMax[tri] = Vector3.Max(v0, Vector3.Max(v1, v2));
                centroids[tri] = (v0 + v1 + v2) * (1.0f / 3.0f);
                triangleIndices[tri] = tri;
            }

            nodes = new BvhNode[Mathf.Max(2, triangleCount * 2)];
            nodeCount = 1;
            nodes[0].LeftFirst = 0;
            nodes[0].Count = triangleCount;
            UpdateNodeBounds(0);

            if (triangleCount > 0)
            {
                Subdivide(0, 0);
            }
        }

        private void Subdivide(int nodeIndex, int depth)
        {
            int count = nodes[nodeIndex].Count;
            int first = nodes[nodeIndex].LeftFirst;

            if (count <= MaxLeafSize || depth >= MaxDepth - 1)
            {
                return;
            }

            int axis;
            float splitPosition;
            float splitCost;
            if (!FindBestSplit(nodeIndex, out axis, out splitPosition, out splitCost))
            {
                return;
            }

            float leafCost = SurfaceArea(nodes[nodeIndex].BoundsMin, nodes[nodeIndex].BoundsMax) * count;
            if (splitCost >= leafCost)
            {
                return;
            }

            int i = first;
            int j = first + count - 1;
            while (i <= j)
            {
                int tri = triangleIndices[i];
                if (Component(centroids[tri], axis) < splitPosition)
                {
                    i++;
                }
                else
                {
                    triangleIndices[i] = triangleIndices[j];
                    triangleIndices[j] = tri;
                    j--;
                }
            }

            int leftCount = i - first;
            if (leftCount == 0 || leftCount == count)
            {
                return;
            }

            int leftChild = nodeCount;
            nodeCount += 2;
            EnsureNodeCapacity(nodeCount);

            nodes[leftChild].LeftFirst = first;
            nodes[leftChild].Count = leftCount;
            nodes[leftChild + 1].LeftFirst = i;
            nodes[leftChild + 1].Count = count - leftCount;

            nodes[nodeIndex].LeftFirst = leftChild;
            nodes[nodeIndex].Count = 0;

            UpdateNodeBounds(leftChild);
            UpdateNodeBounds(leftChild + 1);

            Subdivide(leftChild, depth + 1);
            Subdivide(leftChild + 1, depth + 1);
        }

        private bool FindBestSplit(int nodeIndex, out int bestAxis, out float bestPosition, out float bestCost)
        {
            bestAxis = -1;
            bestPosition = 0.0f;
            bestCost = float.MaxValue;

            int first = nodes[nodeIndex].LeftFirst;
            int count = nodes[nodeIndex].Count;

            for (int axis = 0; axis < 3; axis++)
            {
                float centroidMin = float.MaxValue;
                float centroidMax = float.MinValue;
                for (int i = 0; i < count; i++)
                {
                    float c = Component(centroids[triangleIndices[first + i]], axis);
                    centroidMin = Mathf.Min(centroidMin, c);
                    centroidMax = Mathf.Max(centroidMax, c);
                }

                float extent = centroidMax - centroidMin;
                if (extent < 1.0e-9f)
                {
                    continue;
                }

                for (int b = 0; b < BinCount; b++)
                {
                    binMin[b] = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
                    binMax[b] = new Vector3(float.MinValue, float.MinValue, float.MinValue);
                    binCount[b] = 0;
                }

                float scale = BinCount / extent;
                for (int i = 0; i < count; i++)
                {
                    int tri = triangleIndices[first + i];
                    int bin = Mathf.Min(BinCount - 1, (int)((Component(centroids[tri], axis) - centroidMin) * scale));
                    binCount[bin]++;
                    binMin[bin] = Vector3.Min(binMin[bin], triangleMin[tri]);
                    binMax[bin] = Vector3.Max(binMax[bin], triangleMax[tri]);
                }

                Vector3 accumulateMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
                Vector3 accumulateMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);
                int accumulateCount = 0;
                for (int b = 0; b < BinCount - 1; b++)
                {
                    accumulateCount += binCount[b];
                    accumulateMin = Vector3.Min(accumulateMin, binMin[b]);
                    accumulateMax = Vector3.Max(accumulateMax, binMax[b]);
                    sweepLeftCount[b] = accumulateCount;
                    sweepLeftArea[b] = SurfaceArea(accumulateMin, accumulateMax);
                }

                accumulateMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
                accumulateMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);
                accumulateCount = 0;
                for (int b = BinCount - 1; b > 0; b--)
                {
                    accumulateCount += binCount[b];
                    accumulateMin = Vector3.Min(accumulateMin, binMin[b]);
                    accumulateMax = Vector3.Max(accumulateMax, binMax[b]);
                    sweepRightCount[b - 1] = accumulateCount;
                    sweepRightArea[b - 1] = SurfaceArea(accumulateMin, accumulateMax);
                }

                for (int b = 0; b < BinCount - 1; b++)
                {
                    if (sweepLeftCount[b] == 0 || sweepRightCount[b] == 0)
                    {
                        continue;
                    }

                    float cost = sweepLeftArea[b] * sweepLeftCount[b] + sweepRightArea[b] * sweepRightCount[b];
                    if (cost < bestCost)
                    {
                        bestCost = cost;
                        bestAxis = axis;
                        bestPosition = centroidMin + (b + 1) / scale;
                    }
                }
            }

            return bestAxis >= 0;
        }

        private void UpdateNodeBounds(int nodeIndex)
        {
            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            int first = nodes[nodeIndex].LeftFirst;
            int count = nodes[nodeIndex].Count;
            for (int i = 0; i < count; i++)
            {
                int tri = triangleIndices[first + i];
                min = Vector3.Min(min, triangleMin[tri]);
                max = Vector3.Max(max, triangleMax[tri]);
            }

            nodes[nodeIndex].BoundsMin = min;
            nodes[nodeIndex].BoundsMax = max;
        }

        private void EnsureNodeCapacity(int required)
        {
            if (nodes.Length >= required)
            {
                return;
            }

            int capacity = nodes.Length;
            while (capacity < required)
            {
                capacity *= 2;
            }

            BvhNode[] grown = new BvhNode[capacity];
            System.Array.Copy(nodes, grown, nodeCount);
            nodes = grown;
        }

        private static float SurfaceArea(Vector3 min, Vector3 max)
        {
            Vector3 extent = max - min;
            if (extent.x < 0.0f)
            {
                return 0.0f;
            }

            return 2.0f * (extent.x * extent.y + extent.y * extent.z + extent.z * extent.x);
        }

        private static float Component(Vector3 value, int axis)
        {
            return axis == 0 ? value.x : (axis == 1 ? value.y : value.z);
        }
    }

    /// <summary>
    /// One caster per thread. Holds the traversal stack so Raycast allocates nothing.
    /// </summary>
    public sealed class BvhRayCaster
    {
        private const float DirectionEpsilon = 1.0e-8f;

        private readonly Bvh bvh;
        private readonly int[] stack = new int[Bvh.MaxDepth];

        public BvhRayCaster(Bvh bvh)
        {
            this.bvh = bvh;
        }

        public bool Raycast(Vector3 origin, Vector3 direction, float tMin, float tMax, out BvhHit hit)
        {
            hit = new BvhHit();
            hit.Distance = tMax;
            hit.TriangleIndex = -1;

            // A zero component would produce 0 * infinity in the slab test.
            direction.x = AvoidZero(direction.x);
            direction.y = AvoidZero(direction.y);
            direction.z = AvoidZero(direction.z);

            Vector3 inverseDirection = new Vector3(1.0f / direction.x, 1.0f / direction.y, 1.0f / direction.z);

            BvhNode[] nodes = bvh.Nodes;
            int[] indices = bvh.TriangleIndices;
            Vector3[] positions = bvh.Positions;

            if (float.IsPositiveInfinity(NodeDistance(nodes[0], origin, inverseDirection, tMin, hit.Distance)))
            {
                return false;
            }

            bool found = false;
            int stackPointer = 0;
            int nodeIndex = 0;

            while (true)
            {
                int count = nodes[nodeIndex].Count;
                if (count > 0)
                {
                    int first = nodes[nodeIndex].LeftFirst;
                    for (int i = 0; i < count; i++)
                    {
                        int triangle = indices[first + i];
                        float distance;
                        bool backface;
                        if (IntersectTriangle(positions, triangle, origin, direction, tMin, hit.Distance, out distance, out backface))
                        {
                            hit.Distance = distance;
                            hit.TriangleIndex = triangle;
                            hit.Backface = backface;
                            found = true;
                        }
                    }

                    if (stackPointer == 0)
                    {
                        break;
                    }

                    nodeIndex = stack[--stackPointer];
                    continue;
                }

                int left = nodes[nodeIndex].LeftFirst;
                int right = left + 1;
                float leftDistance = NodeDistance(nodes[left], origin, inverseDirection, tMin, hit.Distance);
                float rightDistance = NodeDistance(nodes[right], origin, inverseDirection, tMin, hit.Distance);

                if (leftDistance > rightDistance)
                {
                    int swapIndex = left;
                    left = right;
                    right = swapIndex;

                    float swapDistance = leftDistance;
                    leftDistance = rightDistance;
                    rightDistance = swapDistance;
                }

                if (float.IsPositiveInfinity(leftDistance))
                {
                    if (stackPointer == 0)
                    {
                        break;
                    }

                    nodeIndex = stack[--stackPointer];
                    continue;
                }

                nodeIndex = left;
                if (!float.IsPositiveInfinity(rightDistance))
                {
                    stack[stackPointer++] = right;
                }
            }

            return found;
        }

        private static float AvoidZero(float value)
        {
            if (value > DirectionEpsilon || value < -DirectionEpsilon)
            {
                return value;
            }

            return value < 0.0f ? -DirectionEpsilon : DirectionEpsilon;
        }

        private static float NodeDistance(BvhNode node, Vector3 origin, Vector3 inverseDirection, float tMin, float tMax)
        {
            float x1 = (node.BoundsMin.x - origin.x) * inverseDirection.x;
            float x2 = (node.BoundsMax.x - origin.x) * inverseDirection.x;
            float enter = Mathf.Min(x1, x2);
            float exit = Mathf.Max(x1, x2);

            float y1 = (node.BoundsMin.y - origin.y) * inverseDirection.y;
            float y2 = (node.BoundsMax.y - origin.y) * inverseDirection.y;
            enter = Mathf.Max(enter, Mathf.Min(y1, y2));
            exit = Mathf.Min(exit, Mathf.Max(y1, y2));

            float z1 = (node.BoundsMin.z - origin.z) * inverseDirection.z;
            float z2 = (node.BoundsMax.z - origin.z) * inverseDirection.z;
            enter = Mathf.Max(enter, Mathf.Min(z1, z2));
            exit = Mathf.Min(exit, Mathf.Max(z1, z2));

            enter = Mathf.Max(enter, tMin);
            exit = Mathf.Min(exit, tMax);

            return exit >= enter ? enter : float.PositiveInfinity;
        }

        private static bool IntersectTriangle(
            Vector3[] positions,
            int triangle,
            Vector3 origin,
            Vector3 direction,
            float tMin,
            float tMax,
            out float distance,
            out bool backface)
        {
            distance = 0.0f;
            backface = false;

            int i = triangle * 3;
            Vector3 v0 = positions[i];
            Vector3 edge1 = positions[i + 1] - v0;
            Vector3 edge2 = positions[i + 2] - v0;

            Vector3 pv = Vector3.Cross(direction, edge2);
            float determinant = Vector3.Dot(edge1, pv);
            if (determinant > -1.0e-12f && determinant < 1.0e-12f)
            {
                return false;
            }

            float inverseDeterminant = 1.0f / determinant;
            Vector3 tv = origin - v0;

            float u = Vector3.Dot(tv, pv) * inverseDeterminant;
            if (u < 0.0f || u > 1.0f)
            {
                return false;
            }

            Vector3 qv = Vector3.Cross(tv, edge1);
            float v = Vector3.Dot(direction, qv) * inverseDeterminant;
            if (v < 0.0f || u + v > 1.0f)
            {
                return false;
            }

            float t = Vector3.Dot(edge2, qv) * inverseDeterminant;
            if (t < tMin || t > tMax)
            {
                return false;
            }

            distance = t;
            // Read the facing from the geometric normal rather than the determinant
            // sign, which depends on winding conventions.
            backface = Vector3.Dot(Vector3.Cross(edge1, edge2), direction) > 0.0f;
            return true;
        }
    }
}
