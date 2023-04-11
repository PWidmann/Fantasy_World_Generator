using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.AccessControl;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using static UnityEditor.PlayerSettings;
using Random = UnityEngine.Random;

public class TerrainGenerator : MonoBehaviour
{
    [SerializeField] private Material terrainMat;
    [SerializeField] private Transform terrainTransform;


    [SerializeField] private int chunkSize = 10; // half of this are nr of quads
    [SerializeField] private int mapSize = 20; // needs to be a multiple of chunksize


    [Header("Noise Values")]
    public int seed;
    public float heightScale = 3f;
    public float frequency = 1f;
    public float lacunarity = 1f;
    private float persistance = 0.5f;
    public int octaves = 3;
    public bool useFalloff = false;
    public float fallOffValueA = 3;
    public float fallOffValueB = 2.2f;

    [Header("Plateau Values")]
    [SerializeField] private bool usePlateaus = true;
    [SerializeField] private float plateau1 = 0.2f;
    [SerializeField] private float plateau2 = 0.4f;
    [SerializeField] private float plateau3 = 0.6f;
    [SerializeField] private float plateau4 = 0.9f;
    [SerializeField] private float plateauSmoothing = 0.5f;


    // Mesh generation


    private PerlinNoise noise;
    private float[,] noiseValues;
    private float[,] falloffMap;
    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;
    private Vector2[] uv;


    private int rowSize;
    private int numberOfJunks;


    private List<GameObject> chunks = new List<GameObject>();
    GameObject tempObj;
    Mesh tempMesh;

    void Start()
    {
        InitializeMap();
        GenerateTerrain();   
    }


    private void OnValidate()
    {
        GenerateTerrain();
    }

    private void GenerateTerrain()
    {
        Random.InitState(seed);
        int totalVertexSideCount = chunkSize * rowSize + 1;


        noise = new PerlinNoise(seed.GetHashCode(), frequency, lacunarity, persistance, octaves);
        noiseValues = noise.GetNoiseValues(totalVertexSideCount, totalVertexSideCount);

        falloffMap = FalloffGenerator.GenerateFalloffMap(totalVertexSideCount, totalVertexSideCount, fallOffValueA, fallOffValueB, false, 5);


        if (useFalloff)
            noiseValues = TerrainTools.ApplyFalloffMap(noiseValues, falloffMap);

        if (usePlateaus)
            noiseValues = TerrainTools.SmoothToPlateaus(noiseValues, plateau1, plateau2, plateau3, plateau4, plateauSmoothing);

        for (int i = 0, z = 0; z < rowSize; z++)
        {
            for (int x = 0; x < rowSize; x++)
            {
                CreateMeshData(new Vector2(x, z));
                //SmoothRectangularEdges();
                UpdateMesh(chunks[i], new Vector2(x, z));
                i++;
            }
        }
    }

    private void InitializeMap()
    {
        // Don't change mapsize/chunksize while running
        numberOfJunks = (mapSize / chunkSize) * (mapSize / chunkSize);
        tempMesh = new Mesh();
        mesh = new Mesh();
        rowSize = mapSize / chunkSize;

        uv = new Vector2[(chunkSize + 1) * (chunkSize + 1)];

        GenerateChunkObjects();
    }

    void GenerateChunkObjects()
    {
        chunks.Clear();

        for (int x = 0; x < rowSize; x++)
        {
            for (int z = 0; z < rowSize; z++)
            {
                tempObj = new GameObject("TerrainChunk");
                tempObj.AddComponent(typeof(MeshRenderer));
                tempObj.AddComponent(typeof(MeshFilter));
                tempObj.AddComponent(typeof(MeshCollider));
                tempObj.transform.SetParent(terrainTransform);
                chunks.Add(tempObj);
            }
        }
    }

    private void CreateMeshData(Vector2 terrainPosition)
    {
        vertices = new Vector3[(chunkSize + 1) * (chunkSize + 1)];
        

        int currentWorldPosX = (int)(terrainPosition.x * chunkSize);
        int currentWorldPosZ = (int)(terrainPosition.y * chunkSize);


        for (int i = 0, z = 0; z <= chunkSize; z++)
        {
            for (int x = 0; x <= chunkSize; x++)
            {
                float y = noiseValues[x + currentWorldPosX, z + currentWorldPosZ] * heightScale;
        
        
                vertices[i] = new Vector3(x, y, z);
                uv[i] = new Vector2(x / (float)chunkSize, z / (float)chunkSize);
                i++;
            }
        }

        triangles = new int[chunkSize * chunkSize * 12];

        int vert = 0;
        int tris = 0;

        // Create triangle quads
        for (int z = 0; z < chunkSize; z += 2)
        {
            for (int x = 0; x < chunkSize; x += 2)
            {
                // Bottom triangle
                triangles[tris + 0] = vert + 0 + (z * chunkSize / 2);
                triangles[tris + 1] = vert + chunkSize + 2 + (z * chunkSize / 2);
                triangles[tris + 2] = vert + 2 + (z * chunkSize / 2);

                // Right triangle
                triangles[tris + 3] = vert + 2 + (z * chunkSize / 2);
                triangles[tris + 4] = vert + chunkSize + 2 + (z * chunkSize / 2);
                triangles[tris + 5] = vert + (chunkSize + 1) * 2 + 2 + (z * chunkSize / 2);

                // Top triangle
                triangles[tris + 6] = vert + (chunkSize + 1) * 2 + 2 + (z * chunkSize / 2);
                triangles[tris + 7] = vert + chunkSize + 2 + (z * chunkSize / 2);
                triangles[tris + 8] = vert + (chunkSize + 1) * 2 + 0 + (z * chunkSize / 2);

                // Left triangle
                triangles[tris + 9] = vert + (chunkSize + 1) * 2 + 0 + (z * chunkSize / 2);
                triangles[tris + 10] = vert + chunkSize + 2 + (z * chunkSize / 2);
                triangles[tris + 11] = vert + 0 + (z * chunkSize / 2);

                vert += 2;
                tris += 12;
            }
            vert += 2;
        }
    }

    void SmoothRectangularEdges()
    {
        Vector3[] rectBatch = new Vector3[12];

        for (int v = 1; v < vertices.Length; v += 12)
        {
            for (int i = 0; i < 12; i++)
            {
                rectBatch[i] = vertices[i + v];
            }

            //North/South edge
            if (rectBatch[0].y == rectBatch[2].y && rectBatch[0].y != rectBatch[8].y && rectBatch[8].y == rectBatch[5].y)
            {
                // Put middlepoints of quad to average height between the two heights

                float y = (vertices[v + 0].y + vertices[v + 8].y) / 2;
                vertices[v + 1].y = y;
                vertices[v + 4].y = y;
                vertices[v + 7].y = y;
                vertices[v + 10].y = y;
                Debug.Log("Smoothed out middle vertex of quad");
            }

            //East/West edge
            if (rectBatch[0].y == rectBatch[8].y && rectBatch[2].y == rectBatch[5].y && rectBatch[0].y != rectBatch[2].y)
            {
                // Put middlepoints of quad to average height between the two heights

                float y = (vertices[v + 0].y + vertices[v + 2].y) / 2;
                vertices[v + 1].y = y;
                vertices[v + 4].y = y;
                vertices[v + 7].y = y;
                vertices[v + 10].y = y;
                Debug.Log("Smoothed out middle vertex of quad");
            }

            // 3 corners (except top right) same height = align middle vertex
            if (rectBatch[0].y == rectBatch[8].y && rectBatch[8].y == rectBatch[2].y)
            {
                float y = vertices[v + 0].y;
                vertices[v + 1].y = y;
                vertices[v + 4].y = y;
                vertices[v + 7].y = y;
                vertices[v + 10].y = y;
                Debug.Log("Smoothed out middle vertex of quad");
            }

            // 3 corners (except top left) same height = align middle vertex
            if (rectBatch[0].y == rectBatch[2].y && rectBatch[2].y == rectBatch[5].y)
            {
                float y = vertices[v + 2].y;
                vertices[v + 1].y = y;
                vertices[v + 4].y = y;
                vertices[v + 7].y = y;
                vertices[v + 10].y = y;
                Debug.Log("Smoothed out middle vertex of quad");
            }

            // 3 corners (except bottom right) same height = align middle vertex
            if (rectBatch[0].y == rectBatch[8].y && rectBatch[8].y == rectBatch[5].y)
            {
                float y = vertices[v + 0].y;
                vertices[v + 1].y = y;
                vertices[v + 4].y = y;
                vertices[v + 7].y = y;
                vertices[v + 10].y = y;
                Debug.Log("Smoothed out middle vertex of quad");
            }

            // 3 corners (except bottom left) same height = align middle vertex
            if (rectBatch[2].y == rectBatch[8].y && rectBatch[8].y == rectBatch[5].y)
            {
                float y = vertices[v + 2].y;
                vertices[v + 1].y = y;
                vertices[v + 4].y = y;
                vertices[v + 7].y = y;
                vertices[v + 10].y = y;
                Debug.Log("Smoothed out middle vertex of quad");
            }
        }
    }

    private void UpdateMesh(GameObject terrainChunk, Vector2 terrainPos)
    {
        
        terrainChunk.GetComponent<MeshFilter>().mesh.Clear();
        terrainChunk.GetComponent<MeshFilter>().mesh.vertices = vertices;
        terrainChunk.GetComponent<MeshFilter>().mesh.triangles = triangles;
        terrainChunk.GetComponent<MeshFilter>().mesh.uv = uv;
        terrainChunk.GetComponent<MeshFilter>().sharedMesh.RecalculateNormals();
        terrainChunk.GetComponent<MeshCollider>().sharedMesh = terrainChunk.GetComponent<MeshFilter>().sharedMesh;
        terrainChunk.GetComponent<MeshCollider>().enabled = true;
        terrainChunk.GetComponent<MeshRenderer>().material = terrainMat;
        terrainChunk.transform.position = new Vector3(terrainPos.x * chunkSize, 0, terrainPos.y * chunkSize);
    }
}
