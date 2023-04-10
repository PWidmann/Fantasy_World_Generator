using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FalloffGenerator
{
    public static float[,] GenerateFalloffMap(int width, int height, float falloffValue_a, float falloffValue_b, bool useBlur, int blurradius)
    {
        float[,] map = new float[width, height];

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                float x = i / (float)width * 2 - 1;
                float y = j / (float)height * 2 - 1;

                float value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
                map[i, j] = Evaluate(value, falloffValue_a, falloffValue_b);
            }
        }

        if (useBlur)
        {
            map = ApplyGaussianBlur(map, blurradius);
        }

        return map;
    }

    static float Evaluate(float value, float falloff_a, float falloff_b)
    {
        // FOR SMOOTH FALLOFF MAP
        float a = falloff_a;
        float b = falloff_b;

        return Mathf.Pow(value, a) / (Mathf.Pow(value, a) + Mathf.Pow(b - b * value, a));
    }

    public static float[,] ApplyGaussianBlur(float[,] map, int blurRadius)
    {
        int width = map.GetLength(0);
        int height = map.GetLength(1);
        float[,] blurredMap = new float[width, height];

        // Precompute Gaussian kernel weights
        float[] weights = new float[blurRadius + 1];
        for (int i = 0; i <= blurRadius; i++)
        {
            weights[i] = GaussianKernel(i, blurRadius);
        }

        // Apply horizontal blur
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                float sum = 0;
                float weightSum = 0;

                for (int k = -blurRadius; k <= blurRadius; k++)
                {
                    int x = Mathf.Clamp(i + k, 0, width - 1);
                    float weight = weights[Mathf.Abs(k)];
                    sum += map[x, j] * weight;
                    weightSum += weight;
                }

                blurredMap[i, j] = sum / weightSum;
            }
        }

        // Apply vertical blur
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                float sum = 0;
                float weightSum = 0;

                for (int k = -blurRadius; k <= blurRadius; k++)
                {
                    int y = Mathf.Clamp(j + k, 0, height - 1);
                    float weight = weights[Mathf.Abs(k)];
                    sum += blurredMap[i, y] * weight;
                    weightSum += weight;
                }

                blurredMap[i, j] = sum / weightSum;
            }
        }

        return blurredMap;
    }

    static float GaussianKernel(int x, int radius)
    {
        float sigma = radius / 3f;
        float weight = (1 / (2 * Mathf.PI * sigma * sigma)) * Mathf.Exp(-x * x / (2 * sigma * sigma));
        return weight;
    }
}