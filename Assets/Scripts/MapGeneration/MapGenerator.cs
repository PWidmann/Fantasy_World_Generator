using Mono.Cecil.Cil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Collections;
using Unity.Jobs;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


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
    [SerializeField] GUI_DebugPanel guiDebugPanel;

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
    [SerializeField] private float fallOffValueB = 2.2f;

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


    List<MeshFilter> meshFilters = new List<MeshFilter>();
    List<MeshCollider> meshColliders = new List<MeshCollider>();
    List<MeshRenderer> meshRenderers = new List<MeshRenderer>();


    public void GenerateNewMap()
    {
        CreateChunkObjects();

        noise = new PerlinNoise(seed, frequency, amplitude, lacunarity, persistance, octaves);
        junksPerRow = mapSize / chunkSize;
        uv = new Vector2[(chunkSize + 1) * (chunkSize + 1)];

        noiseValues = noise.GetNoiseValues(mapSize + junksPerRow, mapSize + junksPerRow);
        falloffMap = FalloffGenerator.GenerateFalloffMap(mapSize + junksPerRow, mapSize + junksPerRow, fallOffValueA, fallOffValueB);

        //falloffMap = CreateGradientArray(mapSize + junksPerRow, mapSize + junksPerRow, fallOffValueA);
        
        if (useFalloff)
        {
            noiseValues = SubtractingFalloff(noiseValues);
        
            
        }
        else
        {
            fallOffImage.color = Color.black;
        }



        chunkDataList.Clear();
        CreateUVandTriangleData();
        StartCoroutine(CreateChunkVertices());

        guiDebugPanel.SetLoading(true, "Creating Terrain Objects...");

        StartCoroutine(ApplyMeshDataToTerrainObjects());

        
    }

    private IEnumerator CreateChunkVertices()
    {
        for (int i = 0, z = 0; z < junksPerRow; z++)
        {
            for (int x = 0; x < junksPerRow; x++)
            {
                CreateChunkVerticesData(new Vector2(x, z), i);
                i++;
                yield return null;
            }
            
        }
    }

    private void CreateUVandTriangleData()
    {
        //float startTime = Time.realtimeSinceStartup;

        triangles = new int[chunkSize * chunkSize * 6];
        int vert = 0;
        int tris = 0;

        // UVs
        for (int i = 0, z = 0; z <= chunkSize; z++)
        {
            for (int x = 0; x <= chunkSize; x++)
            {
                uv[i] = new Vector2(x / (float)chunkSize, z / (float)chunkSize);
                i++;
            }
        }
        
        // Triangles
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
        //Debug.Log("UV and triangle data generation took: " + ((Time.realtimeSinceStartup - startTime) * 1000f) + "ms");
    }

    private IEnumerator ApplyMeshDataToTerrainObjects()
    {
        float startTime = Time.realtimeSinceStartup;

        for (int i = 0; i < chunkDataList.Count; i++)
        {
            var mesh = new Mesh();
            mesh.SetVertices(chunkDataList[i].vertices);
            mesh.SetTriangles(chunkDataList[i].triangles, 0);
            mesh.SetUVs(0, chunkDataList[i].uvs);

            if (mesh.normals.Length == 0)
            {
                mesh.RecalculateNormals();
            }

            meshFilters[i].mesh = mesh;
            meshColliders[i].sharedMesh = mesh;
            meshColliders[i].enabled = true;
            meshRenderers[i].material = terrainMaterial;
            chunks[i].transform.position = new Vector3(chunkDataList[i].terrainPosition.x * chunkSize, 0, chunkDataList[i].terrainPosition.y * chunkSize);

            yield return null;
        }

        guiDebugPanel.SetLoading(false, "");
    }

    void CreateChunkObjects()
    {
        //float startTime = Time.realtimeSinceStartup;

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
                
                MeshRenderer meshRenderer = (MeshRenderer)tempObj.AddComponent(typeof(MeshRenderer));
                meshRenderers.Add(meshRenderer);

                MeshFilter meshFilter = (MeshFilter)tempObj.AddComponent(typeof(MeshFilter));
                meshFilter.mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                meshFilters.Add(meshFilter);

                MeshCollider meshCollider = (MeshCollider)tempObj.AddComponent(typeof(MeshCollider));
                meshColliders.Add(meshCollider);

                tempObj.transform.SetParent(terrain.transform);
                chunks.Add(tempObj);
            }

            lastChunksPerRow = chunksPerRow;
        }

        //Debug.Log("Creating chunk Game Objects took: " + ((Time.realtimeSinceStartup - startTime) * 1000f) + "ms");
    }

    

    void CreateChunkVerticesData(Vector2 terrainPosition, int chunkID)
    {
        //float startTime = Time.realtimeSinceStartup;

        vertices = new Vector3[(chunkSize + 1) * (chunkSize + 1)];

        int currentWorldPosX = (int)(terrainPosition.x * chunkSize);
        int currentWorldPosZ = (int)(terrainPosition.y * chunkSize);

        for (int i = 0, z = 0; z <= chunkSize; z++)
        {
            for (int x = 0; x <= chunkSize; x++)
            {
                float y = noiseValues[currentWorldPosZ + z, currentWorldPosX + x] * heightScale;
                vertices[i] = new Vector3(x, y, z);
                i++;
            }
        }

        ChunkData chunkData = new ChunkData();
        chunkData.chunkID = chunkID;
        chunkData.vertices = vertices;
        chunkData.triangles = triangles;
        chunkData.uvs = uv;
        chunkData.terrainPosition = terrainPosition;
        chunkDataList.Add(chunkData);
        //Debug.Log("Vertices data generation took: " + ((Time.realtimeSinceStartup - startTime) * 1000f) + "ms");
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
