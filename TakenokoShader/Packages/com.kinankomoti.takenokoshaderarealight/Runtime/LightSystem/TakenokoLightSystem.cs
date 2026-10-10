
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDKBase;
using VRC.Udon;

namespace Kinankomoti.TakenokoShader
{
    public class TakenokoLightSystem : UdonSharpBehaviour
    {
        [SerializeField]
        [FormerlySerializedAs("lightSlot")]
        private TakenokoAreaLight[] lightSlots = new TakenokoAreaLight[8];

        private int enableLtcSystemId;
        private int[] lightVertexIds = new int[8];
        private int[] lightTextureIds = new int[8];
        private int[] lightEmissionIds = new int[8];

        [FormerlySerializedAs("LUT1")]
        public Texture2D ltcLut;

        private int ltcLutId;

        private void Start()
        {
            enableLtcSystemId = VRCShader.PropertyToID("_UdonEnableLtcSystem");
            lightTextureIds[0] = VRCShader.PropertyToID("_UdonLightTexture1");
            lightTextureIds[1] = VRCShader.PropertyToID("_UdonLightTexture2");
            lightTextureIds[2] = VRCShader.PropertyToID("_UdonLightTexture3");
            lightTextureIds[3] = VRCShader.PropertyToID("_UdonLightTexture4");
            lightTextureIds[4] = VRCShader.PropertyToID("_UdonLightTexture5");
            lightTextureIds[5] = VRCShader.PropertyToID("_UdonLightTexture6");
            lightTextureIds[6] = VRCShader.PropertyToID("_UdonLightTexture7");
            lightTextureIds[7] = VRCShader.PropertyToID("_UdonLightTexture8");
            lightVertexIds[0] = VRCShader.PropertyToID("_UdonLightVertex1");
            lightVertexIds[1] = VRCShader.PropertyToID("_UdonLightVertex2");
            lightVertexIds[2] = VRCShader.PropertyToID("_UdonLightVertex3");
            lightVertexIds[3] = VRCShader.PropertyToID("_UdonLightVertex4");
            lightVertexIds[4] = VRCShader.PropertyToID("_UdonLightVertex5");
            lightVertexIds[5] = VRCShader.PropertyToID("_UdonLightVertex6");
            lightVertexIds[6] = VRCShader.PropertyToID("_UdonLightVertex7");
            lightVertexIds[7] = VRCShader.PropertyToID("_UdonLightVertex8");
            lightEmissionIds[0] = VRCShader.PropertyToID("_UdonLightEmission1");
            lightEmissionIds[1] = VRCShader.PropertyToID("_UdonLightEmission2");
            lightEmissionIds[2] = VRCShader.PropertyToID("_UdonLightEmission3");
            lightEmissionIds[3] = VRCShader.PropertyToID("_UdonLightEmission4");
            lightEmissionIds[4] = VRCShader.PropertyToID("_UdonLightEmission5");
            lightEmissionIds[5] = VRCShader.PropertyToID("_UdonLightEmission6");
            lightEmissionIds[6] = VRCShader.PropertyToID("_UdonLightEmission7");
            lightEmissionIds[7] = VRCShader.PropertyToID("_UdonLightEmission8");
            ltcLutId = VRCShader.PropertyToID("_UdonLtcLut");

            VRCShader.SetGlobalFloat(enableLtcSystemId, 1.0f);

            VRCShader.SetGlobalTexture(ltcLutId, ltcLut);
        }

        private void Update()
        {
            for (int i = 0; i < lightSlots.Length; i++)
            {
                TakenokoAreaLight light = lightSlots[i];
                if (light == null || light.isStatic) continue;

                VRCShader.SetGlobalFloatArray(lightVertexIds[i], light.Vertex);
                VRCShader.SetGlobalTexture(lightTextureIds[i], light.texture);
                VRCShader.SetGlobalColor(lightEmissionIds[i], light.color);
            }
        }
    }
}
