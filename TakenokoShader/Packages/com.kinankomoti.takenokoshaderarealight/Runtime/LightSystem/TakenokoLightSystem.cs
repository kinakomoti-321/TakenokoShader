
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDKBase;
using VRC.Udon;

namespace Kinankomoti.TakenokoShader
{
    [DisallowMultipleComponent]
    public class TakenokoLightSystem : UdonSharpBehaviour
    {
        [SerializeField]
        [FormerlySerializedAs("lightSlot")]
        private TakenokoAreaLight[] lightSlots = new TakenokoAreaLight[8];

        private int enableLtcSystemId;
        private int[] lightVertexIds = new int[8];
        private int[] lightTextureIds = new int[8];
        private int[] lightEmissionIds = new int[8];
        private int[] lightActiveIds = new int[8];
        private TakenokoAreaLight[] previousLights = new TakenokoAreaLight[8];
        private float[] previousActive = new float[8];
        private bool[] uploaded = new bool[8];
        private float[] emptyVertices = new float[12];
        private bool initialized;

        [HideInInspector, FormerlySerializedAs("LUT1")]
        public Texture2D ltcLut;
        [HideInInspector, FormerlySerializedAs("LUT2")]
        public Texture2D fresnelLut;

        private int ltcLutId;
        private int fresnelLutId;

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        private void Reset()
        {
            LoadLuts();
        }

        private void OnValidate()
        {
            UnityEditor.EditorApplication.update -= LoadLuts;
            UnityEditor.EditorApplication.update += LoadLuts;
        }

        private void LoadLuts()
        {
            UnityEditor.EditorApplication.update -= LoadLuts;
            if (this == null || UnityEditor.EditorApplication.isPlaying) return;

            Texture2D matrix = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(
                "Packages/com.kinankomoti.takenokoshaderarealight/Resource/LUT_Matrix.exr");
            Texture2D fresnel = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(
                "Packages/com.kinankomoti.takenokoshaderarealight/Resource/LUT_Fresnel.exr");
            if (matrix == null)
                matrix = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/TakenokoShaderAreaLight/Resource/LUT_Matrix.exr");
            if (fresnel == null)
                fresnel = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/TakenokoShaderAreaLight/Resource/LUT_Fresnel.exr");
            if (ltcLut == matrix && fresnelLut == fresnel) return;

            ltcLut = matrix;
            fresnelLut = fresnel;
            UnityEditor.EditorUtility.SetDirty(this);
            if (UnityEditor.PrefabUtility.IsPartOfPrefabInstance(this))
                UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(this);
            if (!UnityEditor.EditorUtility.IsPersistent(this) && gameObject.scene.IsValid())
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }
#endif

        private void Start()
        {
            Initialize();
        }

        private void Initialize()
        {
            if (initialized) return;
            if (lightSlots == null || lightSlots.Length != 8)
            {
                TakenokoAreaLight[] slots = new TakenokoAreaLight[8];
                if (lightSlots != null)
                    for (int i = 0; i < lightSlots.Length && i < 8; i++) slots[i] = lightSlots[i];
                lightSlots = slots;
            }
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
            fresnelLutId = VRCShader.PropertyToID("_UdonFresnelLut");
            for (int i = 0; i < 8; i++)
            {
                lightActiveIds[i] = VRCShader.PropertyToID("_UdonIsActiveAreaLight" + (i + 1));
                VRCShader.SetGlobalFloat(lightActiveIds[i], 0.0f);
            }
            initialized = true;

            VRCShader.SetGlobalFloat(enableLtcSystemId, enabled && gameObject.activeInHierarchy ? 1.0f : 0.0f);

            VRCShader.SetGlobalTexture(ltcLutId, ltcLut);
            VRCShader.SetGlobalTexture(fresnelLutId, fresnelLut);
        }

        private void OnEnable()
        {
            if (initialized) VRCShader.SetGlobalFloat(enableLtcSystemId, 1.0f);
        }

        private void OnDisable()
        {
            if (!initialized) return;
            for (int i = 0; i < 8; i++)
            {
                if (previousActive[i] == 0.0f) continue;
                VRCShader.SetGlobalFloat(lightActiveIds[i], 0.0f);
                previousActive[i] = 0.0f;
            }
            VRCShader.SetGlobalFloat(enableLtcSystemId, 0.0f);
        }

        private void LateUpdate()
        {
            Initialize();
            for (int i = 0; i < 8; i++) UpdateSlot(i, false);
            for (int i = 0; i < 8; i++)
            {
                TakenokoAreaLight light = lightSlots[i];
                if (light != null && uploaded[i] && previousActive[i] == 1.0f) light.ClearUpdateFlags();
            }
        }

        public void SetAreaLight(TakenokoAreaLight light, int index)
        {
            if (index < 0 || index >= 8)
            {
                Debug.LogError("Area light index must be between 0 and 7");
                return;
            }
            Initialize();
            lightSlots[index] = light;
            if (light != null) light.RefreshLight();
            UpdateSlot(index, true);
        }

        public void ClearAreaLight(int index)
        {
            SetAreaLight(null, index);
        }

        private void UpdateSlot(int index, bool forceUpdate)
        {
            TakenokoAreaLight light = lightSlots[index];
            if (previousLights[index] != light)
            {
                previousLights[index] = light;
                uploaded[index] = false;
            }

            float active = enabled && gameObject.activeInHierarchy && light != null && light.enabled &&
                light.gameObject.activeInHierarchy && light.IsInitialized ? 1.0f : 0.0f;
            if (light != null && light.IsInitialized && (forceUpdate || active == 1.0f))
            {
                bool uploadAll = forceUpdate || !uploaded[index] || previousActive[index] == 0.0f;
                if (uploadAll || light.IsVertexUpdated)
                {
                    VRCShader.SetGlobalFloatArray(lightVertexIds[index], light.Vertex);
                }
                if (uploadAll || light.IsLightTextureUpdated)
                {
                    VRCShader.SetGlobalTexture(lightTextureIds[index], light.texture);
                }
                if (uploadAll || light.IsLightColorUpdated)
                {
                    VRCShader.SetGlobalColor(lightEmissionIds[index], light.color);
                }
                uploaded[index] = true;
            }
            else if (forceUpdate)
            {
                VRCShader.SetGlobalFloatArray(lightVertexIds[index], emptyVertices);
                VRCShader.SetGlobalTexture(lightTextureIds[index], null);
                VRCShader.SetGlobalColor(lightEmissionIds[index], Color.clear);
                uploaded[index] = false;
            }
            if (forceUpdate || previousActive[index] != active)
            {
                VRCShader.SetGlobalFloat(lightActiveIds[index], active);
                previousActive[index] = active;
            }
        }
    }
}
