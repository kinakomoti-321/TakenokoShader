using System.Collections.Generic;
using UnityEngine;

namespace Takenoko
{
    public enum BakeUvSet
    {
        Uv0 = 0,
        Uv1 = 1,
        Uv2 = 2,
        Uv3 = 3
    }

    /// <summary>
    /// World space triangle soup gathered from a renderer hierarchy.
    /// The vertex arrays hold three consecutive entries per triangle, so triangle
    /// i owns indices 3i, 3i+1 and 3i+2. This layout is what the BVH indexes into
    /// and maps directly onto a StructuredBuffer when the GPU path lands.
    /// </summary>
    public sealed class BakeScene
    {
        public Vector3[] Positions { get; private set; }
        public Vector3[] Normals { get; private set; }
        public Vector2[] Uvs { get; private set; }
        public int TriangleCount { get; private set; }
        public Bounds Bounds { get; private set; }
        public int RendererCount { get; private set; }

        /// <summary>
        /// Diagonal of the combined bounds. Every object baked into one map shares
        /// this value, so distance based settings stay continuous across objects.
        /// </summary>
        public float Diagonal
        {
            get { return Bounds.size.magnitude; }
        }

        private BakeScene()
        {
        }

        public static BakeScene Collect(GameObject root, BakeUvSet uvSet, out string error)
        {
            error = null;

            if (root == null)
            {
                error = "Bake target is not set.";
                return null;
            }

            List<Vector3> positions = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<Vector2> meshUvs = new List<Vector2>();
            int rendererCount = 0;

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                Mesh mesh;
                bool temporaryMesh = false;

                SkinnedMeshRenderer skinned = renderer as SkinnedMeshRenderer;
                if (skinned != null)
                {
                    if (skinned.sharedMesh == null)
                    {
                        continue;
                    }

                    // Bakes the current pose, including blend shapes. The result is in
                    // the renderer's local space without scale, so localToWorldMatrix
                    // below still applies the full transform.
                    mesh = new Mesh();
                    skinned.BakeMesh(mesh);
                    temporaryMesh = true;

                    // BakeMesh produces its own readable mesh, so the source importer
                    // flag is only worth reporting when nothing came back.
                    if (mesh.vertexCount == 0)
                    {
                        error = NotReadableMessage(skinned.sharedMesh);
                        Object.DestroyImmediate(mesh);
                        return null;
                    }
                }
                else
                {
                    MeshFilter filter = renderer.GetComponent<MeshFilter>();
                    if (filter == null || filter.sharedMesh == null)
                    {
                        continue;
                    }

                    if (!filter.sharedMesh.isReadable)
                    {
                        error = NotReadableMessage(filter.sharedMesh);
                        return null;
                    }

                    mesh = filter.sharedMesh;
                }

                try
                {
                    mesh.GetUVs((int)uvSet, meshUvs);
                    if (meshUvs.Count == 0)
                    {
                        error = string.Format("'{0}' has no {1}.", mesh.name, uvSet);
                        return null;
                    }

                    Matrix4x4 localToWorld = renderer.transform.localToWorldMatrix;
                    Matrix4x4 normalMatrix = localToWorld.inverse.transpose;
                    bool mirrored = localToWorld.determinant < 0.0f;

                    Vector3[] meshVertices = mesh.vertices;
                    Vector3[] meshNormals = mesh.normals;
                    bool hasNormals = meshNormals != null && meshNormals.Length == meshVertices.Length;

                    for (int subMesh = 0; subMesh < mesh.subMeshCount; subMesh++)
                    {
                        int[] indices = mesh.GetTriangles(subMesh);
                        for (int i = 0; i < indices.Length; i += 3)
                        {
                            int i0 = indices[i];
                            // A mirrored transform reverses the winding, which would
                            // invert every facing test downstream.
                            int i1 = mirrored ? indices[i + 2] : indices[i + 1];
                            int i2 = mirrored ? indices[i + 1] : indices[i + 2];

                            Vector3 p0 = localToWorld.MultiplyPoint3x4(meshVertices[i0]);
                            Vector3 p1 = localToWorld.MultiplyPoint3x4(meshVertices[i1]);
                            Vector3 p2 = localToWorld.MultiplyPoint3x4(meshVertices[i2]);

                            positions.Add(p0);
                            positions.Add(p1);
                            positions.Add(p2);

                            if (hasNormals)
                            {
                                normals.Add(normalMatrix.MultiplyVector(meshNormals[i0]).normalized);
                                normals.Add(normalMatrix.MultiplyVector(meshNormals[i1]).normalized);
                                normals.Add(normalMatrix.MultiplyVector(meshNormals[i2]).normalized);
                            }
                            else
                            {
                                Vector3 faceNormal = Vector3.Cross(p1 - p0, p2 - p0).normalized;
                                normals.Add(faceNormal);
                                normals.Add(faceNormal);
                                normals.Add(faceNormal);
                            }

                            uvs.Add(meshUvs[i0]);
                            uvs.Add(meshUvs[i1]);
                            uvs.Add(meshUvs[i2]);
                        }
                    }

                    rendererCount++;
                }
                finally
                {
                    if (temporaryMesh)
                    {
                        Object.DestroyImmediate(mesh);
                    }
                }
            }

            if (positions.Count == 0)
            {
                error = "No readable mesh was found under the bake target.";
                return null;
            }

            BakeScene scene = new BakeScene
            {
                Positions = positions.ToArray(),
                Normals = normals.ToArray(),
                Uvs = uvs.ToArray(),
                TriangleCount = positions.Count / 3,
                RendererCount = rendererCount
            };

            Bounds bounds = new Bounds(scene.Positions[0], Vector3.zero);
            for (int i = 1; i < scene.Positions.Length; i++)
            {
                bounds.Encapsulate(scene.Positions[i]);
            }

            scene.Bounds = bounds;
            return scene;
        }

        private static string NotReadableMessage(Mesh mesh)
        {
            return string.Format(
                "'{0}' is not readable. Enable Read/Write in its model importer before baking.",
                mesh.name);
        }
    }
}
