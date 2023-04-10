using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinNoise
{
    int seed;

    float frequency;
    float lacunarity; // gaps between patterns / lakes
    float persistance;
    int octaves;

    public PerlinNoise(int seed, float frequency, float lacunarity, float persistance, int octaves)
    {
        this.seed = seed;
        this.frequency = frequency;
        this.lacunarity = lacunarity;
        this.persistance = persistance;
        this.octaves = octaves;
    }

    public float[,] GetNoiseValues(int width, int height)
    {
        float[,] noiseValues = new float[width, height];
        float max = 0f;
        float min = float.MaxValue;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float frequency = this.frequency;
                float noise = 0;

                for (int k = 0; k < octaves; k++)
                {
                    noise += Mathf.PerlinNoise((x + seed) / (float)width * frequency, (y + seed) / (float)height * frequency);
                    frequency *= lacunarity;
                }

                noiseValues[x, y] = noise;

                if (noise > max)
                {
                    max = noise;
                }

                if (noise < min)
                {
                    min = noise;
                }
            }
        }

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                noiseValues[i, j] = Mathf.InverseLerp(max, min, noiseValues[i, j]);
            }
        }

        return noiseValues;
    }
}