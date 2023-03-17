using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    Mesh tempMesh;

    Vector3[] vertices;
    int[] triangles;

    [Header("Map Settings")]
    public bool useFlatShading = true;
    public int mapSize = 100; // Must be a multiple of chunkSize
    public int chunkSize = 20;
    public AnimationCurve heightCurve;
    public bool useFalloff = true;
    [SerializeField] GameObject terrain;
    [SerializeField] Material terrainMaterial;

    [SerializeField] Material terrainMat;
    [Header("Perlin Values")]
    public float heightScale = 2.0f;
    public int seed = 1337;
    public float frequency = 6.4f;
    public float amplitude = 3.8f;
    public float lacunarity = 1.8f;
    public float persistance = 0.5f;
    public int octaves = 4;

    private int xSize;
    private int zSize;
    private int numberOfJunks;
    private List<GameObject> chunks = new List<GameObject>();
    GameObject tempObj;
    private float fallOffValueA = 3;
    private float fallOffValueB = 2.2f;

    PerlinNoise noise;
    private float[,] noiseValues;
    private float[,] falloffMap;
    private Vector2[] uv;

    void Start()
    {
        //GenerateNewMap();
        
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            int terrainWidth = 512;
            int terrainLength = 512;
            int terrainHeight = 100;
            int terrainResolution = 513;
            float perlinScale = 0.01f;

            // Create new terrain data
            TerrainData terrainData = new TerrainData();
            terrainData.heightmapResolution = terrainResolution;
            terrainData.size = new Vector3(terrainWidth, terrainHeight, terrainLength);

            // Generate Perlin noise values
            float[,] heights = new float[terrainData.heightmapResolution, terrainData.heightmapResolution];
            for (int x = 0; x < terrainData.heightmapResolution; x++)
            {
                for (int z = 0; z < terrainData.heightmapResolution; z++)
                {
                    float xCoord = (float)x / terrainResolution * perlinScale;
                    float zCoord = (float)z / terrainResolution * perlinScale;
                    heights[x, z] = Mathf.PerlinNoise(xCoord, zCoord);
                }
            }

            // Set the terrain heights
            terrainData.SetHeights(0, 0, heights);


            AssignMaterialToTerrain(terrainData);

            // Create new terrain game object
            Terrain terrain = gameObject.AddComponent<Terrain>();
            terrain.terrainData = terrainData;
            terrain.transform.position = Vector3.zero;

            
        }
    }

    private void AssignMaterialToTerrain(TerrainData terrainData)
    {
        // Assign a material to the terrain
        float[,,] splatmapData = new float[terrainData.alphamapResolution, terrainData.alphamapResolution, 1];
        for (int y = 0; y < terrainData.alphamapResolution; y++)
        {
            for (int x = 0; x < terrainData.alphamapResolution; x++)
            {
                splatmapData[x, y, 0] = 1;
            }
        }
        terrainData.SetAlphamaps(0, 0, splatmapData);
        //terrainData.splatPrototypes = new SplatPrototype[] { new SplatPrototype() { texture = (Texture2D)terrainMat.mainTexture } };
    }

    private void GenerateNewMap()
    {
        noise = new PerlinNoise(seed, frequency, amplitude, lacunarity, persistance, octaves);

        xSize = mapSize;
        zSize = mapSize;
        numberOfJunks = (mapSize / chunkSize) * (mapSize / chunkSize);

        tempMesh = new Mesh();
        uv = new Vector2[(chunkSize + 1) * (chunkSize + 1)];
        noiseValues = noise.GetNoiseValues(mapSize + numberOfJunks, mapSize + numberOfJunks);
        falloffMap = FalloffGenerator.GenerateFalloffMap(mapSize + 1, mapSize + 1, fallOffValueA, fallOffValueB);

        if (useFalloff)
            noiseValues = SubtractingFalloff(noiseValues);

        CreateMeshObjects();

        int rowSize = mapSize / chunkSize;
        for (int i = 0, z = 0; z < rowSize; z++)
        {
            for (int x = 0; x < rowSize; x++)
            {
                CreateChunkData(new Vector2(x, z));
                UpdateChunkMesh(chunks[i], new Vector2(x, z));
                i++;
            }
        }
    }

    void CreateMeshObjects()
    {
        // Create terrain chunks and add them to list
        for (int x = 0; x < xSize / chunkSize; x++)
        {
            for (int z = 0; z < zSize / chunkSize; z++)
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

    void CreateChunkData(Vector2 terrainPosition)
    {
        // create mesh shape for one terrain chunk (size + 2 for normal border)
        vertices = new Vector3[(chunkSize + 1) * (chunkSize + 1)];

        int currentWorldPosX = (int)(terrainPosition.x * chunkSize);
        int currentWorldPosZ = (int)(terrainPosition.y * chunkSize);

        // create a bigger mesh than the final mesh, so we can later use the correct normals for mesh borders (1 quad border on every side)
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

    private void CreateUnityTerrain(float height, int chunkSize, Mesh chunkMeshData, Vector2 terrainPosition)
    {
        TerrainData terrainData = new TerrainData();
        terrainData.size = new Vector3(chunkSize, height, chunkSize);
        terrainData.heightmapResolution = chunkSize;

        int currentWorldPosX = (int)(terrainPosition.x * chunkSize);
        int currentWorldPosZ = (int)(terrainPosition.y * chunkSize);

        // create a bigger mesh than the final mesh, so we can later use the correct normals for mesh borders (1 quad border on every side)
        for (int z = 0; z <= chunkSize; z++)
        {
            for (int x = 0; x <= chunkSize; x++)
            {
                
                terrainData.SetHeights(x, z, noiseValues);
            }
        }

        GameObject unityTerrainObject = Terrain.CreateTerrainGameObject(terrainData);
        unityTerrainObject.transform.position = new Vector3(currentWorldPosX, 0, currentWorldPosZ);
    }

    void UpdateChunkMesh(GameObject terrainChunk, Vector2 pos)
    {

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



    float[,] SubtractingFalloff(float[,] _noiseValues)
    {
        float[,] newNoiseValues = _noiseValues;

        for (int x = 0; x <= mapSize; x++)
        {
            for (int y = 0; y <= mapSize; y++)
            {
                // SUBSTRACT FALLOFF MAP VALUES
                newNoiseValues[x, y] = Mathf.Clamp01(newNoiseValues[x, y] - falloffMap[x, y]);
            }
        }

        return newNoiseValues;
    }
}
