using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    [Header("Base Refs")]
    [SerializeField] private Transform level;
    [SerializeField] private Material landMaterial;

    [Header("Noise Settings")]
    [SerializeField] public int depth = 20;
    [SerializeField] public int width = 256;
    [SerializeField] public int height = 256;
    [SerializeField] public float scale = 20;
    [SerializeField] public float terrainUpdateRate = 0.01f;
    
    private float gameTimer = 0;
    private TerrainData chunkData;
    private GameObject terrainChunk;

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
        terrainData.heightmapResolution = width + 1;
        terrainData.size = new Vector3(width, depth, height);
        terrainData.SetHeights(0, 0, GenerateHeights());
        return terrainData;
    }

    float[,] GenerateHeights()
    {
        float[,] heights = new float[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                heights[x, y] = CalculateHeight(x, y);
            }
        }

        return heights;
    }

    float CalculateHeight(int x, int y)
    {
        float xCoord = (float)x / width * scale;
        float yCoord = (float)y / height * scale;

        return Mathf.PerlinNoise(xCoord, yCoord);
    }
}
