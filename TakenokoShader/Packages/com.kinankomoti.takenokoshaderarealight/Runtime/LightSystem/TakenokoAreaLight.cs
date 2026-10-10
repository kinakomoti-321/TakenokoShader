
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDKBase;
using VRC.Udon;

namespace Kinankomoti.TakenokoShader
{
    public class TakenokoAreaLight : UdonSharpBehaviour
    {
        public bool isStatic = false;

        [ColorUsage(false, true)]
        [FormerlySerializedAs("_color")]
        public Color color = new Color(1.0f, 1.0f, 1.0f);

        [FormerlySerializedAs("_texture")]
        public Texture2D texture;

        private float[] vertexData = new float[12];

        public float[] Vertex => vertexData;


        private void Start()
        {
            Mesh mesh = GetComponent<MeshFilter>().mesh;
            Vector3[] vertices = mesh.vertices;
            if (vertices.Length != 4) Debug.LogError("This mesh is invalid. a area light must have 4 vertices");

            Material material = GetComponent<MeshRenderer>().material;
            material.SetColor("_EmissionColor", color);
            material.SetTexture("_EmissionMap", texture);
        }

        private void Update()
        {
            Mesh mesh = GetComponent<MeshFilter>().mesh;
            Vector3[] vertices = mesh.vertices;
            if (vertices.Length != 4) return;

            Vector3[] worldVertices = new Vector3[vertices.Length];
            for (int i = 0; i < 4; i++)
            {
                worldVertices[i] = transform.TransformPoint(vertices[i]);
            }

            vertexData[0] = worldVertices[0].x;
            vertexData[1] = worldVertices[0].y;
            vertexData[2] = worldVertices[0].z;
            vertexData[3] = worldVertices[1].x;
            vertexData[4] = worldVertices[1].y;
            vertexData[5] = worldVertices[1].z;
            vertexData[6] = worldVertices[3].x;
            vertexData[7] = worldVertices[3].y;
            vertexData[8] = worldVertices[3].z;
            vertexData[9] = worldVertices[2].x;
            vertexData[10] = worldVertices[2].y;
            vertexData[11] = worldVertices[2].z;
        }
    }
}
