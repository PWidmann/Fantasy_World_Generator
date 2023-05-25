﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinNoise
{
    int seed;

    float frequency;
    float amplitude;

    float lacunarity; // gaps between patterns / lakes
    float persistance;

    int octaves;

    public PerlinNoise(int seed, float frequency, float amplitude, float lacunarity, float persistance, int octaves)
    {
        this.seed = seed;
        this.frequency = frequency;
        this.amplitude = amplitude;
        this.lacunarity = lacunarity;
        this.persistance = persistance;
        this.octaves = octaves;
    }

    public float[,] GetNoiseValues(int width, int height)
    {
        float[,] noiseValues = new float[width, height];

        float max = 0f;
        float min = float.MaxValue;
        float tempAmplitude = 0;
        float tempFrequency = 0;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                noiseValues[x, y] = 0;

                tempAmplitude = amplitude;
                tempFrequency = frequency;

                for (int k = 0; k < octaves; k++)
                {
                    noiseValues[x, y] += Mathf.PerlinNoise((x + seed) / (float)width * frequency, (y + seed) / (float)height * frequency) * amplitude;
                    frequency *= lacunarity;
                    amplitude *= persistance;
                }

                amplitude = tempAmplitude;
                frequency = tempFrequency;

                if (noiseValues[x, y] > max)
                {
                    max = noiseValues[x, y];
                }

                if (noiseValues[x, y] < min)
                {
                    min = noiseValues[x, y];
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
