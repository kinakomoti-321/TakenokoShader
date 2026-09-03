using UnityEditor;
using UnityEngine;

namespace Takenoko
{
    /// <summary>
    /// Distance through the volume behind each texel.
    ///
    /// Rays go into the -N hemisphere and the hit distance is normalized against a
    /// maximum distance. Output is thickness, not transmission: thick reads white.
    /// That matches Takenoko_StandardFragment, which does the 1 - x inversion itself
    /// after dividing by NdotV. Maps baked in Substance and similar tools use the
    /// opposite convention and need inverting before use here.
    /// </summary>
    public sealed class ThicknessBakeTarget : BakeTarget
    {
        private enum DistanceMode
        {
            RelativeToBounds,
            Absolute
        }

        private DistanceMode distanceMode = DistanceMode.RelativeToBounds;
        private float boundsPercent = 25.0f;
        private float absoluteDistance = 1.0f;
        private float coneAngle = 75.0f;

        private float maxDistance;
        private float sinHalfAngle;

        public override string DisplayName
        {
            get { return "Thickness"; }
        }

        public override string FileSuffix
        {
            get { return "Thickness"; }
        }

        public override void DrawSettings()
        {
            distanceMode = (DistanceMode)EditorGUILayout.EnumPopup(
                new GUIContent("Distance Mode", "How the maximum thickness is defined."),
                distanceMode);

            if (distanceMode == DistanceMode.RelativeToBounds)
            {
                boundsPercent = EditorGUILayout.Slider(
                    new GUIContent("Bounds Percent", "Maximum thickness as a percentage of the combined bounding box diagonal."),
                    boundsPercent, 1.0f, 100.0f);
            }
            else
            {
                absoluteDistance = EditorGUILayout.FloatField(
                    new GUIContent("Max Distance", "Maximum thickness in world units."),
                    absoluteDistance);
                absoluteDistance = Mathf.Max(1.0e-4f, absoluteDistance);
            }

            coneAngle = EditorGUILayout.Slider(
                new GUIContent("Cone Angle", "Half angle of the sampling cone. Narrower avoids grazing rays that skim along the surface."),
                coneAngle, 5.0f, 90.0f);
        }

        public override void LoadSettings(string prefix)
        {
            string mode = EditorUserSettings.GetConfigValue(prefix + "distanceMode");
            DistanceMode parsed;
            if (!string.IsNullOrEmpty(mode) && System.Enum.TryParse(mode, out parsed))
            {
                distanceMode = parsed;
            }

            boundsPercent = LoadFloat(prefix + "boundsPercent", boundsPercent);
            absoluteDistance = LoadFloat(prefix + "absoluteDistance", absoluteDistance);
            coneAngle = LoadFloat(prefix + "coneAngle", coneAngle);
        }

        public override void SaveSettings(string prefix)
        {
            EditorUserSettings.SetConfigValue(prefix + "distanceMode", distanceMode.ToString());
            EditorUserSettings.SetConfigValue(prefix + "boundsPercent", boundsPercent.ToString("R"));
            EditorUserSettings.SetConfigValue(prefix + "absoluteDistance", absoluteDistance.ToString("R"));
            EditorUserSettings.SetConfigValue(prefix + "coneAngle", coneAngle.ToString("R"));
        }

        public override void Prepare(BakeContext context)
        {
            maxDistance = distanceMode == DistanceMode.RelativeToBounds
                ? context.SceneDiagonal * boundsPercent * 0.01f
                : absoluteDistance;

            maxDistance = Mathf.Max(1.0e-5f, maxDistance);
            sinHalfAngle = Mathf.Sin(coneAngle * Mathf.Deg2Rad);
        }

        public override float Evaluate(in BakeContext context, Vector3 position, Vector3 normal, uint seed)
        {
            Vector3 axis = -normal;

            // Cranley-Patterson rotation, so neighbouring texels do not share the
            // same sample directions and the noise stays uncorrelated.
            float rotation1 = BakeSampling.Hash(seed) * 2.3283064365386963e-10f;
            float rotation2 = BakeSampling.Hash(seed ^ 0x9e3779b9u) * 2.3283064365386963e-10f;

            int sampleCount = context.SampleCount;
            float total = 0.0f;

            for (int i = 0; i < sampleCount; i++)
            {
                float u1 = BakeSampling.Fract((i + 0.5f) / sampleCount + rotation1);
                float u2 = BakeSampling.Fract(BakeSampling.RadicalInverse2((uint)i) + rotation2);

                Vector3 direction = BakeSampling.SampleConeCosine(axis, sinHalfAngle, u1, u2);

                BvhHit hit;
                if (context.Caster.Raycast(position, direction, context.RayBias, maxDistance, out hit))
                {
                    total += Mathf.Clamp01(hit.Distance / maxDistance);
                }
                else
                {
                    // Nothing within range: treat as fully thick. An open surface such
                    // as cloth or a leaf therefore bakes solid white, which is correct
                    // for a volume thickness map but is why thin walls want their own
                    // target rather than this one.
                    total += 1.0f;
                }
            }

            // Averaging the normalized value rather than the raw distance keeps missed
            // rays from needing a substitute distance that would skew the mean.
            return total / sampleCount;
        }

        private static float LoadFloat(string key, float defaultValue)
        {
            string value = EditorUserSettings.GetConfigValue(key);
            float result;
            return float.TryParse(value, out result) ? result : defaultValue;
        }
    }
}
