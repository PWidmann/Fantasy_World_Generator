using Mono.Cecil.Cil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.PlayerSettings;

public struct ChunkData
{
    public int chunkID;
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uvs;
    public Vector2 terrainPosition;
}


public class MapGenerator : MonoBehaviour
{
    [Header("Map Settings")]
    public int mapSize = 100; // Must be a multiple of chunkSize
    public int chunkSize = 20;
    public bool useFalloff = true;
    [SerializeField] GameObject terrain;
    [SerializeField] Material terrainMaterial;
    [SerializeField] RawImage fallOffImage;
    [SerializeField] AnimationCurve animCurve;

    [Header("Perlin Values")]
    public float heightScale = 2.0f;
    public int seed = 1337;
    public float frequency = 6.4f;
    public float amplitude = 3.8f;
    public float lacunarity = 1.8f;
    private float persistance = 0.5f;
    public int octaves = 3;

    private int junksPerRow;

    [SerializeField] private float fallOffValueA = 3;
    //[SerializeField] private float fallOffValueB = 2.2f;

    // Complete map noise
    private PerlinNoise noise;
    private float[,] noiseValues;
    private float[,] falloffMap;
    
    // Terrain chunk temp values
    private Mesh tempMesh;
    private Vector3[] vertices;
    private int[] triangles;
    private Vector2[] uv;

    private List<GameObject> chunks = new List<GameObject>();
    GameObject tempObj;

    private List<ChunkData> chunkDataList = new List<ChunkData>();

    int lastChunksPerRow = 0;

    NativeArray<int> jobResult;


    void Start()
    {
        //GenerateNewMap();

        ExecuteJobs();
    }

    private void Update()
    {
        
    }

    public void ExecuteJobs()
    {
        float startTime = Time.realtimeSinceStartup;

        NativeArray<JobHandle> jobHandleArray = new NativeArray<JobHandle>(1000, Allocator.Temp);
        jobResult = new NativeArray<int>(1000, Allocator.Persistent);

        for (int i = 0; i < 1000; i++)
        {
            if (i == 0)
            {
                JobHandle jobHandle = CreateMeshDataTaskJob(i, jobResult);
                jobHandleArray[i] = jobHandle;

            }
            else
            {
                JobHandle jobHandle = CreateMeshDataTaskJob(i, jobResult, jobHandleArray[i - 1]);
                jobHandleArray[i] = jobHandle;
            }
            

        }

        JobHandle.CompleteAll(jobHandleArray);

        jobHandleArray.Dispose();



        foreach (int i in jobResult)
        {
            if (i != 0)
            {
                Debug.Log("Result: " + i);
            }
        }

        jobResult.Dispose();
        //foreach (int val in jobResults)
        //{
        //    Debug.Log("Result: " + val);
        //}

        Debug.Log("Executing all jobs took: " + ((Time.realtimeSinceStartup - startTime) * 1000f) + "ms");
    }

    private JobHandle CreateMeshDataTaskJob(int jobID, NativeArray<int> resultArray, JobHandle previousJob)
    {
        CreateMeshDataJob job = new CreateMeshDataJob();
        job.number = 1;
        job.jobID = jobID;
        job.result = resultArray;

        return job.Schedule(previousJob);
    }

    private JobHandle CreateMeshDataTaskJob(int jobID, NativeArray<int> resultArray)
    {
        CreateMeshDataJob job = new CreateMeshDataJob();
        job.number = 1;
        job.jobID = jobID;
        job.result = resultArray;

        return job.Schedule();
    }

    public void GenerateNewMap()
    {
        CreateChunkObjects();

        noise = new PerlinNoise(seed, frequency, amplitude, lacunarity, persistance, octaves);
        junksPerRow = mapSize / chunkSize;
        
        uv = new Vector2[(chunkSize + 1) * (chunkSize + 1)];
        noiseValues = noise.GetNoiseValues(mapSize + junksPerRow, mapSize + junksPerRow);
        //falloffMap = FalloffGenerator.GenerateFalloffMap(mapSize + numberOfJunks, mapSize + numberOfJunks, fallOffValueA, fallOffValueB);

        //falloffMap = CreateGradientArray(mapSize + junksPerRow, mapSize + junksPerRow, fallOffValueA);
        //
        //if (useFalloff)
        //{
        //    noiseValues = SubtractingFalloff(noiseValues);
        //
        //    
        //}
        //else
        //{
        //    fallOffImage.color = Color.black;
        //}



        chunkDataList.Clear();
        for (int i = 0, z = 0; z < junksPerRow; z++)
        {
            for (int x = 0; x < junksPerRow; x++)
            {
                CreateChunkMeshData(new Vector2(x, z), i);
                i++;
            }
        }

        ApplyMeshDataToTerrainObjects();
    }

    private void ApplyMeshDataToTerrainObjects()
    {
        for (int i = 0; i < chunkDataList.Count; i++)
        {
            chunks[i].GetComponent<MeshFilter>().mesh.Clear();
            chunks[i].GetComponent<MeshFilter>().mesh.vertices = chunkDataList[i].vertices;
            chunks[i].GetComponent<MeshFilter>().mesh.triangles = chunkDataList[i].triangles;
            chunks[i].GetComponent<MeshFilter>().mesh.uv = chunkDataList[i].uvs;
            chunks[i].GetComponent<MeshFilter>().mesh.RecalculateNormals();
            chunks[i].GetComponent<MeshCollider>().sharedMesh = chunks[i].GetComponent<MeshFilter>().sharedMesh;
            chunks[i].GetComponent<MeshCollider>().enabled = true;
            chunks[i].GetComponent<MeshRenderer>().material = terrainMaterial;
            chunks[i].transform.position = new Vector3(chunkDataList[i].terrainPosition.x * chunkSize, 0, chunkDataList[i].terrainPosition.y * chunkSize);
        }
    }

    void CreateChunkObjects()
    {
        int chunksPerRow = mapSize / chunkSize;

        if (chunksPerRow != lastChunksPerRow || chunks.Count == 0)
        {
            foreach (GameObject go in chunks)
            {
                Destroy(go);
            }

            chunks.Clear();

            // Create terrain chunks and add them to list
            for (int x = 0; x < chunksPerRow * chunksPerRow; x++)
            {
                tempObj = new GameObject("TerrainChunk");
                tempObj.AddComponent(typeof(MeshRenderer));
                tempObj.AddComponent(typeof(MeshFilter));
                tempObj.AddComponent(typeof(MeshCollider));
                tempObj.transform.SetParent(terrain.transform);
                chunks.Add(tempObj);
            }

            lastChunksPerRow = chunksPerRow;
        }
    }

    
    void CreateChunkMeshData(Vector2 terrainPosition, int chunkID)
    {
        vertices = new Vector3[(chunkSize + 1) * (chunkSize + 1)];

        int currentWorldPosX = (int)(terrainPosition.x * chunkSize);
        int currentWorldPosZ = (int)(terrainPosition.y * chunkSize);

        for (int i = 0, z = 0; z <= chunkSize; z++)
        {
            for (int x = 0; x <= chunkSize; x++)
            {
                float y = noiseValues[currentWorldPosZ + z, currentWorldPosX + x] * heightScale;
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

        ChunkData chunkData = new ChunkData();
        chunkData.chunkID = chunkID;
        chunkData.vertices = vertices;
        chunkData.triangles = triangles;
        chunkData.uvs = uv;
        chunkData.terrainPosition = terrainPosition;

        chunkDataList.Add(chunkData);
    }

    public float[,] CreateGradientArray(int width, int height, float maxDistance)
    {
        float[,] gradientArray = new float[width, height];

        float centerX = width / 2f;
        float centerY = height / 2f;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(centerX, centerY));

                float value = (distance / maxDistance); // Calculate the gradient value

                gradientArray[x, y] = Mathf.Clamp01(value); // Clamp the value between 0 and 1
            }
        }

        return gradientArray;
    }

    float[,] SubtractingFalloff(float[,] _noiseValues)
    {
        float[,] newNoiseValues = _noiseValues;
        Texture2D tex = new Texture2D(newNoiseValues.GetLength(0), newNoiseValues.GetLength(1));

        for (int x = 0; x < _noiseValues.GetLength(1); x++)
        {
            for (int y = 0; y < _noiseValues.GetLength(0); y++)
            {
                // SUBSTRACT FALLOFF MAP VALUES
                newNoiseValues[x, y] = Mathf.Clamp01(newNoiseValues[x, y] - falloffMap[x, y]);

                float c = Mathf.Clamp01(newNoiseValues[x, y]);
                tex.SetPixel(x, y, new Color(c, c, c));
            }
        }

        tex.Apply();

        fallOffImage.texture = tex;
        fallOffImage.texture.filterMode = FilterMode.Point;
        fallOffImage.color = Color.white;

        return newNoiseValues;
    }

    
}

public struct CreateMeshDataJob : IJob
{
    public int number;
    public int jobID;

    /// <summary>
    /// result is a reference to memory, not a copy of a value
    /// </summary>
    public NativeArray<int> result;

    public void Execute()
    {
        for (int i = 0; i < 50000; i++)
        {
            number += 1;
        }

        result[jobID] = number;
    }
}
