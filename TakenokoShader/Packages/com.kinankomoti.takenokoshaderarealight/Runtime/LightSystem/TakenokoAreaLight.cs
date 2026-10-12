
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDKBase;
using VRC.Udon;

namespace Kinankomoti.TakenokoShader
{
    public class TakenokoAreaLight : UdonSharpBehaviour
    {
        [ColorUsage(false, true)]
        [FormerlySerializedAs("_color")]
        public Color color = new Color(1.0f, 1.0f, 1.0f);

        [FormerlySerializedAs("_texture")]
        public Texture2D texture;

        private float[] vertexData = new float[12];
        private Vector3[] localVertices;
        private Matrix4x4 previousMatrix;
        private Material lightMaterial;
        private Color previousColor;
        private Texture2D previousTexture;
        private bool materialInitialized;
        private bool initialized;

        [System.NonSerialized] public bool IsVertexUpdated;
        [System.NonSerialized] public bool IsLightColorUpdated;
        [System.NonSerialized] public bool IsLightTextureUpdated;

        public float[] Vertex => vertexData;
        public bool IsInitialized => materialInitialized;


        private void Start()
        {
            RefreshLight();
        }

        private void Update()
        {
            RefreshLight();
        }

        public void RefreshLight()
        {
            if (!initialized)
            {
                initialized = true;
                MeshFilter meshFilter = GetComponent<MeshFilter>();
                MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
                if (meshFilter == null || meshRenderer == null)
                {
                    Debug.LogError("An area light requires a MeshFilter and MeshRenderer");
                    return;
                }
                localVertices = meshFilter.mesh.vertices;
                if (localVertices.Length != 4)
                {
                    Debug.LogError("This mesh is invalid. a area light must have 4 vertices");
                    return;
                }
                lightMaterial = meshRenderer.material;
            }
            if (localVertices == null || localVertices.Length != 4) return;

            Matrix4x4 matrix = transform.localToWorldMatrix;
            if (!materialInitialized || matrix != previousMatrix)
            {
                for (int i = 0; i < 4; i++)
                {
                    int index = i < 2 ? i : 5 - i;
                    Vector3 vertex = matrix.MultiplyPoint3x4(localVertices[index]);
                    vertexData[i * 3] = vertex.x;
                    vertexData[i * 3 + 1] = vertex.y;
                    vertexData[i * 3 + 2] = vertex.z;
                }
                previousMatrix = matrix;
                IsVertexUpdated = true;
            }

            if (!materialInitialized || color != previousColor)
            {
                lightMaterial.SetColor("_EmissionColor", color);
                previousColor = color;
                IsLightColorUpdated = true;
            }
            if (!materialInitialized || texture != previousTexture)
            {
                lightMaterial.SetTexture("_EmissionMap", texture);
                previousTexture = texture;
                IsLightTextureUpdated = true;
            }
            materialInitialized = true;
        }

        public void ClearUpdateFlags()
        {
            IsVertexUpdated = false;
            IsLightColorUpdated = false;
            IsLightTextureUpdated = false;
        }
    }
}
