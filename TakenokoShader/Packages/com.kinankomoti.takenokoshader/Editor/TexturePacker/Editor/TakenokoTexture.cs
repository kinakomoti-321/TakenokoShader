using System;
using UnityEngine;
using UnityTexture = UnityEngine.Texture;
using UnityTexture2D = UnityEngine.Texture2D;

namespace Takenoko
{
    public sealed class TakenokoTexture : IDisposable
    {
        private readonly Color[] pixels;

        public Vector2Int Resolution { get; }

        private TakenokoTexture(Vector2Int resolution, Color[] pixels)
        {
            Resolution = resolution;
            this.pixels = pixels;
        }

        public static bool TryCreate(UnityTexture source, out TakenokoTexture texture, out string error)
        {
            texture = null;
            error = string.Empty;

            if (source == null)
            {
                error = "Texture is null.";
                return false;
            }

            if (TryReadTexture2D(source as UnityTexture2D, out Color[] directPixels))
            {
                texture = new TakenokoTexture(new Vector2Int(source.width, source.height), directPixels);
                return true;
            }

            if (TryReadByRenderTexture(source, out Color[] renderPixels, out error))
            {
                texture = new TakenokoTexture(new Vector2Int(source.width, source.height), renderPixels);
                return true;
            }

            return false;
        }

        public Color Sample(Vector2 uv)
        {
            if (pixels.Length == 0)
            {
                return Color.clear;
            }

            float u = Mathf.Clamp01(uv.x) * Resolution.x - 0.5f;
            float v = Mathf.Clamp01(uv.y) * Resolution.y - 0.5f;

            int xMin = Mathf.FloorToInt(u);
            int yMin = Mathf.FloorToInt(v);
            int xMax = Mathf.Min(xMin + 1, Resolution.x - 1);
            int yMax = Mathf.Min(yMin + 1, Resolution.y - 1);

            float tx = u - xMin;
            float ty = v - yMin;

            Color bottomLeft = Get(new Vector2Int(xMin, yMin));
            Color bottomRight = Get(new Vector2Int(xMax, yMin));
            Color topLeft = Get(new Vector2Int(xMin, yMax));
            Color topRight = Get(new Vector2Int(xMax, yMax));

            Color bottom = Color.LerpUnclamped(bottomLeft, bottomRight, tx);
            Color top = Color.LerpUnclamped(topLeft, topRight, tx);
            return Color.LerpUnclamped(bottom, top, ty);
        }

        public Color Get(Vector2Int index)
        {
            int x = Mathf.Clamp(index.x, 0, Resolution.x - 1);
            int y = Mathf.Clamp(index.y, 0, Resolution.y - 1);
            return pixels[y * Resolution.x + x];
        }

        public void Dispose()
        {
        }

        private static bool TryReadTexture2D(UnityTexture2D source, out Color[] sourcePixels)
        {
            sourcePixels = null;
            if (source == null || !source.isReadable)
            {
                return false;
            }

            try
            {
                sourcePixels = source.GetPixels();
                return true;
            }
            catch
            {
                sourcePixels = null;
                return false;
            }
        }

        private static bool TryReadByRenderTexture(UnityTexture source, out Color[] sourcePixels, out string error)
        {
            sourcePixels = null;
            error = string.Empty;

            RenderTexture temporary = null;
            UnityTexture2D readable = null;
            RenderTexture previousActive = RenderTexture.active;

            try
            {
                temporary = RenderTexture.GetTemporary(
                    source.width,
                    source.height,
                    0,
                    RenderTextureFormat.ARGBFloat,
                    RenderTextureReadWrite.Default);

                Graphics.Blit(source, temporary);
                RenderTexture.active = temporary;

                readable = new UnityTexture2D(source.width, source.height, TextureFormat.RGBAFloat, false, true);
                readable.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
                readable.Apply(false, false);

                sourcePixels = readable.GetPixels();
                return true;
            }
            catch (Exception exception)
            {
                error = exception.Message;
                return false;
            }
            finally
            {
                RenderTexture.active = previousActive;

                if (readable != null)
                {
                    UnityEngine.Object.DestroyImmediate(readable);
                }

                if (temporary != null)
                {
                    RenderTexture.ReleaseTemporary(temporary);
                }
            }
        }
    }
}
