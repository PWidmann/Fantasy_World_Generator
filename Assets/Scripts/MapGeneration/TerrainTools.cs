using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using Unity.VisualScripting;
using UnityEngine;

public static class TerrainTools
{
    public static float[,] ApplyFalloffMap(float[,] currentMap, float[,] falloffMap)
    {
        float[,] outputHeightMap = new float[currentMap.GetLength(1), currentMap.GetLength(0)];

        for (int x = 0; x < currentMap.GetLength(1); x++)
        {
            for (int y = 0; y < currentMap.GetLength(0); y++)
            {
                outputHeightMap[x, y] = Mathf.Clamp01(currentMap[x, y] - falloffMap[x, y]);
            }
        }

        return outputHeightMap;
    }

    public static float[,] SmoothToPlateaus(float[,] inputMap, float plateau1, float plateau2, float plateau3, float plateau4, float smoothness)
    {
        int width = inputMap.GetLength(0);
        int height = inputMap.GetLength(1);

        float[,] outputMap = new float[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float value = inputMap[x, y];

                if (value <= plateau1)
                {
                    outputMap[x, y] = Mathf.SmoothStep(value, plateau1, smoothness);
                }
                else if (value > plateau1 && value <= plateau2)
                {
                    outputMap[x, y] = Mathf.SmoothStep(value, plateau2, smoothness);
                }
                else if (value > plateau2 && value <= plateau3)
                {
                    outputMap[x, y] = Mathf.SmoothStep(value, plateau3, smoothness);
                }
                else if (value > plateau3 && value <= plateau4)
                {
                    outputMap[x, y] = Mathf.SmoothStep(value, plateau4, smoothness);
                }
                else
                {
                    outputMap[x, y] = value;
                }
            }
        }

        return outputMap;
    }

    public static float[,] ApplyNoiseScale(float[,] mapHeightValues, float noiseScale)
    {
        float[,] map = mapHeightValues;

        for (int x = 0; x < map.GetLength(1); x++)
        {
            for (int y = 0; y < map.GetLength(0); y++)
            {
                map[x, y] = map[x, y] * noiseScale;
            }
        }

        return map;
    }
}