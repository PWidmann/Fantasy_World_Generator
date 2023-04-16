using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class MaterialBlenderDemo : MonoBehaviour
{
    public Texture2D MaterialA;
    public Texture2D MaterialB;
    [Range(0f, 1f)]
    public float MaterialDistribution;
    [Range(0.01f, 1f)]
    public float Depth;
    [Header("Graph Settings")]
    [Range(0f, 1f)]
    public float SliceY = 0.5f;
    [Range(0f, 1f)]
    public float XScale;
    [Range(0f, 1f)]
    public float YScale;
    [Range(0f, 1f)]
    public float DepthMask;

    [HideInInspector]
    public Texture2D Result;
    private Color[] _colors;

    void OnValidate()
    {
        int width = MaterialA.width;
        int height = MaterialA.height;

        if (Result == null || _colors == null || _colors.Length == 0)
        {
            Result = new Texture2D(width * 4, height * 2, TextureFormat.ARGB32, false, true);
            _colors = new Color[width * 4 * height * 2];
        }
        Result.SetPixels(_colors);

        DrawSampleGraph(0, height, width * 4, height / 2);

        for (int tx = 0; tx < width * 4; tx++)
        {
            Result.SetPixel(tx, 10 + (int)(SliceY * height * 0.9f), Color.gray);
        }

        DrawExampleTextureLinear(width / 2, 10, width, height, 0.9f);
        DrawExampleTextureMasked(width / 2 + width + 10, 10, width, height, 0.9f);
        DrawExampleTextureMaskedBlended(width / 2 + width * 2 + 20, 10, width, height, 0.9f);

        Result.Apply(false, false);
    }

    private void DrawSampleGraph(int x, int y, int width, int height)
    {
        for (int tx = 0; tx < width; tx++)
        {
            var materialAAmount = MaterialDistribution;
            var materialBAmount = 1 - MaterialDistribution;

            var sX = tx * XScale / width;
            var pixelA = MaterialA.GetPixelBilinear(sX * XScale, SliceY);
            var pixelB = MaterialB.GetPixelBilinear(sX * XScale, SliceY);

            var scaleYA = (int)(pixelA.a * materialAAmount * height / YScale);
            var scaleYB = (int)(pixelB.a * materialBAmount * height / YScale);
            var scaleYMax = Mathf.Max(scaleYA, scaleYB);

            var colorA = new Color(pixelA.r, pixelA.g, pixelA.b, 0);
            var colorB = new Color(pixelB.r, pixelB.g, pixelB.b, 0);

            int px = (int)(sX * width);
            float depthMin = scaleYMax - Depth * height;
            for (int ty = 0; ty < scaleYMax; ty++)
            {
                var color = Color.black;
                bool materialAPresent = scaleYA >= ty;
                bool materialBPresent = scaleYB >= ty;
                if (materialAPresent)
                {
                    color += colorA;
                }
                if (materialBPresent)
                {
                    color += colorB;
                }
                if (materialAPresent && materialBPresent)
                {
                    color *= 0.5f;
                }
                if (ty < depthMin)
                {
                    color *= (1f - DepthMask);
                }
                Result.SetPixel(tx + x, ty + y, color);
            }
        }
    }

    private void DrawExampleTextureLinear(int x, int y, int width, int height, float scale)
    {
        float twidth = width * scale;
        float theight = height * scale;
        for (int tx = 0; tx < twidth; tx++)
        {
            for (int ty = 0; ty < theight; ty++)
            {
                var pixelA = MaterialA.GetPixelBilinear(tx / twidth, ty / theight);
                var pixelB = MaterialB.GetPixelBilinear(tx / twidth, ty / theight);

                var colorA = new Color(pixelA.r, pixelA.g, pixelA.b, 1);
                var colorB = new Color(pixelB.r, pixelB.g, pixelB.b, 1);

                Result.SetPixel(tx + x, ty + y, colorA * MaterialDistribution + colorB * (1f - MaterialDistribution));
            }
        }
    }

    private void DrawExampleTextureMasked(int x, int y, int width, int height, float scale)
    {
        float twidth = width * scale;
        float theight = height * scale;
        for (int tx = 0; tx < twidth; tx++)
        {
            for (int ty = 0; ty < theight; ty++)
            {
                var pixelA = MaterialA.GetPixelBilinear(tx / twidth, ty / theight);
                var pixelB = MaterialB.GetPixelBilinear(tx / twidth, ty / theight);

                if (pixelA.a * MaterialDistribution > pixelB.a * (1f - MaterialDistribution))
                {
                    Result.SetPixel(tx + x, ty + y, pixelA);
                }
                else
                {
                    Result.SetPixel(tx + x, ty + y, pixelB);
                }
            }
        }
    }

    private void DrawExampleTextureMaskedBlended(int x, int y, int width, int height, float scale)
    {
        float twidth = width * scale;
        float theight = height * scale;
        for (int tx = 0; tx < twidth; tx++)
        {
            for (int ty = 0; ty < theight; ty++)
            {
                var pixelA = MaterialA.GetPixelBilinear(tx / twidth, ty / theight);
                var pixelB = MaterialB.GetPixelBilinear(tx / twidth, ty / theight);

                var strengthA = pixelA.a * MaterialDistribution;
                var strengthB = pixelB.a * (1f - MaterialDistribution);
                var max = Math.Max(strengthA, strengthB);
                max = Math.Max(max - Depth, 0);

                var b1 = Math.Max(strengthA - max, 0);
                var b2 = Math.Max(strengthB - max, 0);

                var alphaSum = b1 + b2;
                var col = (
                    pixelA * b1 +
                    pixelB * b2
                ) / alphaSum;

                Result.SetPixel(tx + x, ty + y, col);
            }
        }
    }


}
