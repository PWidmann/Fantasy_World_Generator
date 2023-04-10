using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    Mesh tempMesh;

    Vector3[] vertices;
    int[] triangles;

    [Header("Map Settings")]
    public bool useFlatShading = true;
    public int mapSize = 100; // Must be a multiple of chunkSize
    public int chunkSize = 20;
    public AnimationCurve heightCurve;
    
    [SerializeField] GameObject terrain;
    [SerializeField] Material terrainMaterial;


    [Header("Perlin Values")]
    public float heightScale = 2.0f;
    public int seed = 1337;
    public float frequency = 6.4f;
    private float amplitude = 1f;
    public float lacunarity = 1.8f;
    private float persistance = 0.5f;
    public int octaves = 3;

    private int rowSize;
    private int numberOfJunks;
    private List<GameObject> chunks = new List<GameObject>();
    GameObject tempObj;

    [Header("FalloffGenerator")]
    [SerializeField] private bool useFalloff = true;
    [SerializeField] private float fallOffValueA = 3;
    [SerializeField] private float fallOffValueB = 2.2f;
    [SerializeField] private bool useBlur = true;
    [SerializeField] private int blurRadius = 10;

    [Header("Plateau Values")]
    [SerializeField] private bool usePlateaus = true;
    [SerializeField] private float plateau1 = 0.2f;
    [SerializeField] private float plateau2 = 0.4f;
    [SerializeField] private float plateau3 = 0.6f;
    [SerializeField] private float plateau4 = 0.9f;
    [SerializeField] private float plateauSmoothing = 0.5f;

    PerlinNoise noise;
    private float[,] noiseValues;
    private float[,] falloffMap;
    private Vector2[] uv;

    void Start()
    {
        InitializeMap();
        GenerateTerrain();
    }

    private void OnValidate()
    {
        GenerateTerrain();
    }

    

    private void InitializeMap()
    {
        // Don't change mapsize/chunksize while running
        numberOfJunks = (mapSize / chunkSize) * (mapSize / chunkSize);
        tempMesh = new Mesh();
        rowSize = mapSize / chunkSize;
        uv = new Vector2[(chunkSize + 1) * (chunkSize + 1)];

        GenerateChunkObjects();
    }

    private void GenerateTerrain()
    {
        noise = new PerlinNoise(seed, frequency, lacunarity, persistance, octaves);
        noiseValues = noise.GetNoiseValues(mapSize + 1, mapSize + 1);
        falloffMap = FalloffGenerator.GenerateFalloffMap(mapSize + 1, mapSize + 1, fallOffValueA, fallOffValueB, useBlur, blurRadius);
        
        if(useFalloff)
            noiseValues = TerrainTools.ApplyFalloffMap(noiseValues, falloffMap);

        if(usePlateaus)
            noiseValues = TerrainTools.SmoothToPlateaus(noiseValues, plateau1, plateau2, plateau3, plateau4, plateauSmoothing);

        for (int i = 0, z = 0; z < rowSize; z++)
        {
            for (int x = 0; x < rowSize; x++)
            {
                CreateMeshData(new Vector2(x, z));
                UpdateMesh(chunks[i], new Vector2(x, z));
                i++;
            }
        }
    }

    

    void GenerateChunkObjects()
    {
        chunks.Clear();

        for (int x = 0; x < mapSize / chunkSize; x++)
        {
            for (int z = 0; z < mapSize / chunkSize; z++)
            {
                tempObj = new GameObject("TerrainChunk");
                tempObj.AddComponent(typeof(MeshRenderer));
                tempObj.AddComponent(typeof(MeshFilter));
                tempObj.AddComponent(typeof(MeshCollider));
                tempObj.transform.SetParent(terrain.transform);
                chunks.Add(tempObj);
            }
        }
    }

    void CreateMeshData(Vector2 terrainPosition)
    {
        // create mesh shape for one terrain chunk (size + 2 for normal border)
        vertices = new Vector3[(chunkSize + 1) * (chunkSize + 1)];

        int currentWorldPosX = (int)(terrainPosition.x * chunkSize);
        int currentWorldPosZ = (int)(terrainPosition.y * chunkSize);

        
        for (int i = 0, z = 0; z <= chunkSize; z++)
        {
            for (int x = 0; x <= chunkSize; x++)
            {
                float y = heightCurve.Evaluate(noiseValues[currentWorldPosZ + z, currentWorldPosX + x]) * heightScale;
                vertices[i] = new Vector3(x, y, z);
                uv[i] = new Vector2(x / (float)chunkSize, z / (float)chunkSize);
                i++;
            }
        }

        triangles = new int[chunkSize * chunkSize * 6];

        int vert = 0;
        int tris = 0;

        for (int z = 0; z < chunkSize; z++)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + chunkSize + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + chunkSize + 1;
                triangles[tris + 5] = vert + chunkSize + 2;

                vert++;
                tris += 6;
            }

            vert++;
        }

        tempMesh.vertices = vertices;
        tempMesh.triangles = triangles;
    }

    void UpdateMesh(GameObject terrainChunk, Vector2 pos)
    {
        //FlatShading(triangles, vertices, uv);
        terrainChunk.GetComponent<MeshFilter>().mesh.Clear();
        terrainChunk.GetComponent<MeshFilter>().mesh.vertices = vertices;
        terrainChunk.GetComponent<MeshFilter>().mesh.triangles = triangles;
        terrainChunk.GetComponent<MeshFilter>().mesh.uv = uv;
        terrainChunk.GetComponent<MeshFilter>().mesh.RecalculateNormals();
        terrainChunk.GetComponent<MeshCollider>().sharedMesh = terrainChunk.GetComponent<MeshFilter>().sharedMesh;
        terrainChunk.GetComponent<MeshCollider>().enabled = true;
        terrainChunk.GetComponent<MeshRenderer>().material = terrainMaterial;
        
        terrainChunk.transform.position = new Vector3(pos.x * chunkSize, 0, pos.y * chunkSize);
    }

    void FlatShading(int[] triangles, Vector3[] vertices, Vector2[] uv)
    {
        // Duplicate vertices for each triangle to prevent smooth edges
        Vector3[] flatShadedVertices = new Vector3[triangles.Length];
        Vector2[] flatShadedUvs = new Vector2[triangles.Length];

        for (int i = 0; i < triangles.Length; i++)
        {
            flatShadedVertices[i] = vertices[triangles[i]];
            flatShadedUvs[i] = uv[triangles[i]];
            triangles[i] = i;
        }

        vertices = flatShadedVertices;
        uv = flatShadedUvs;
    }
}