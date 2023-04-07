using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    [Header("Base Refs")]
    [SerializeField] private Transform level;
    [SerializeField] private Material landMaterial;

    [Header("Noise Settings")]
    [SerializeField] private int terrainHeighScale = 20;
    [SerializeField] private int noiseScale = 20;
    private int mapSize = 256;
    private float terrainUpdateRate = 0.01f;
    
    private float gameTimer = 0;
    private TerrainData chunkData;
    private GameObject terrainChunk;


    private float[,] mapValues;

    private void Start()
    {

        GenerateChunkTerrainObject();
    }

    private void Update()
    {
        UpdateTerrain();
    }

    private void UpdateTerrain()
    {
        gameTimer += Time.deltaTime;
        if (gameTimer > terrainUpdateRate)
        {
            terrainChunk.GetComponent<Terrain>().terrainData = GenerateTerrain();
            gameTimer = 0;
        }
    }

    private void GenerateChunkTerrainObject()
    {
        terrainChunk = new GameObject("Terrainchunk");
        terrainChunk.AddComponent<Terrain>();
        terrainChunk.GetComponent<Terrain>().terrainData = GenerateTerrain();
        chunkData = terrainChunk.GetComponent<Terrain>().terrainData;
        terrainChunk.GetComponent<Terrain>().materialTemplate = landMaterial;
        terrainChunk.transform.SetParent(level);
    }

    TerrainData GenerateTerrain()
    {
        TerrainData terrainData = new TerrainData();
        terrainData.heightmapResolution = mapSize + 1;
        terrainData.size = new Vector3(mapSize, terrainHeighScale, mapSize);
        terrainData.SetHeights(0, 0, GenerateHeights());
        return terrainData;
    }

    float[,] GenerateHeights()
    {
        float[,] heights = new float[mapSize, mapSize];
        for (int x = 0; x < mapSize; x++)
        {
            for (int y = 0; y < mapSize; y++)
            {
                heights[x, y] = CalculateHeight(x, y);
            }
        }

        return heights;
    }

    float CalculateHeight(int x, int y)
    {
        float xCoord = (float)x / mapSize * noiseScale;
        float yCoord = (float)y / mapSize * noiseScale;

        return Mathf.PerlinNoise(xCoord, yCoord);
    }
}
