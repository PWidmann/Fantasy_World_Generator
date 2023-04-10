using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    [Header("Base Refs")]
    [SerializeField] private Transform level;
    [SerializeField] private Material landMaterial;

    [Header("Map Settings")]
    [SerializeField] private int chunksPerSide = 2;
    [SerializeField] private int chunkSize = 256;
    [SerializeField] private int terrainHeighScale = 20;
    [SerializeField] private float noiseScale = 1;
    [SerializeField] private float xOffset = 0;
    [SerializeField] private float yOffset = 0;

    [Header("Noise Settings")]
    [SerializeField] private int seed = 1337;
    [SerializeField] private float frequency = 1.5f;
    [SerializeField] private int octaves = 3;
    [SerializeField] private float lacunarity = 2;
    [SerializeField] private float persistance = 0.5f;



    [Header("Falloff Map")]
    [SerializeField] private float fallOffValueA = 3;
    [SerializeField] private float fallOffValueB = 2.2f;
    [SerializeField] private bool useBlur = true;
    [SerializeField] private int blurRadius = 10;

    [Header("Plateau Values")]
    [SerializeField] private float plateau1 = 0.2f;
    [SerializeField] private float plateau2 = 0.4f;
    [SerializeField] private float plateau3 = 0.6f;
    [SerializeField] private float plateau4 = 0.9f;
    [SerializeField] private float plateauSmoothing = 0.5f;
    [SerializeField] private int nrRamps = 2;
    [SerializeField] private int rampWidth = 5;


    private float gameTimer = 0;
    private TerrainData chunkData;
    private GameObject terrainChunk;
    private bool terrainChunkGenerated = false;

    private float[,] mapHeightValues;
    private float[,] fallOffMap;

    private void Start()
    {
        fallOffMap = FalloffGenerator.GenerateFalloffMap(chunkSize + 1, chunkSize + 1, fallOffValueA, fallOffValueB, useBlur, blurRadius);
        GenerateChunkTerrainObject();
        UpdateTerrain();
    }

    private void OnValidate()
    {
        UpdateTerrain();
    }

    private void UpdateTerrain()
    {
        if (terrainChunkGenerated)
        {
            fallOffMap = FalloffGenerator.GenerateFalloffMap(chunkSize + 1, chunkSize + 1, fallOffValueA, fallOffValueB, useBlur, blurRadius);
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
        terrainChunkGenerated = true;
    }

    TerrainData GenerateTerrain()
    {
        TerrainData terrainData = new TerrainData();
        terrainData.heightmapResolution = chunkSize + 1;
        terrainData.size = new Vector3(chunkSize, terrainHeighScale, chunkSize);

        PerlinNoise noise = new PerlinNoise(seed, frequency, lacunarity, persistance, octaves);
        mapHeightValues = noise.GetNoiseValues(chunkSize + 1, chunkSize + 1);

        mapHeightValues = TerrainTools.ApplyFalloffMap(mapHeightValues, fallOffMap);

        mapHeightValues = TerrainTools.SmoothToPlateaus(mapHeightValues, plateau1, plateau2, plateau3, plateau4, plateauSmoothing);

        //mapHeightValues = TerrainTools.GeneratePlateauRamps(mapHeightValues, 0.3f, nrRamps, rampWidth);

        mapHeightValues = TerrainTools.ApplyNoiseScale(mapHeightValues, noiseScale);

        terrainData.SetHeights(0, 0, mapHeightValues);
        return terrainData;
    }
}
