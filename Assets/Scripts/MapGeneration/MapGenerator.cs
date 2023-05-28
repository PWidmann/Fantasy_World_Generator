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
    public float[,] noiseValues;
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
    public bool createColliders = false;
    public bool showMeshes = true;
    [SerializeField] GameObject terrain;
    [SerializeField] Material terrainMaterial;
    [SerializeField] RawImage fallOffImage;
    [SerializeField] GUI_DebugPanel guiDebugPanel;
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
    [SerializeField] private float fallOffValueB = 2.2f;

    // Complete map noise
    private PerlinNoise noise;
    
    private float[,] falloffMap;

    // Terrain chunk temp values
    private float[,] noiseValues;
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


    [Header("Plateau Values")]
    [SerializeField] private bool usePlateaus = true;
    [SerializeField] private float plateau1 = 0.2f;
    [SerializeField] private float plateau2 = 0.4f;
    [SerializeField] private float plateau3 = 0.6f;
    [SerializeField] private float plateau4 = 0.9f;
    [SerializeField] private float plateauSmoothing = 0.5f;

    public void GenerateNewMap()
    {
        CreateChunkObjects();

        junksPerRow = mapSize / chunkSize;
        uv = new Vector2[(chunkSize + 1) * (chunkSize + 1)];

        float startTime = Time.realtimeSinceStartup;
        noise = new PerlinNoise(seed, frequency, amplitude, lacunarity, persistance, octaves);
        Debug.Log("instantiating 'noise' took: " + ((Time.realtimeSinceStartup - startTime) * 1000f) + "ms");

        float startTime3 = Time.realtimeSinceStartup;
        falloffMap = FalloffGenerator.GenerateFalloffMap(mapSize + junksPerRow, mapSize + junksPerRow, fallOffValueA, fallOffValueB);
        Debug.Log("Creating falloffarray and fill with values: " + ((Time.realtimeSinceStartup - startTime3) * 1000f) + "ms");

        //falloffMap = CreateGradientArray(mapSize + junksPerRow, mapSize + junksPerRow, fallOffValueA);

        



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
            mesh.RecalculateNormals();

            meshFilters[i].mesh = mesh;
            
            if (createColliders)
            {
                meshColliders[i].sharedMesh = mesh;
                meshColliders[i].enabled = true;
            }
            
            meshRenderers[i].material = terrainMaterial;
            chunks[i].transform.position = new Vector3(chunkDataList[i].terrainPosition.x * chunkSize, 0, chunkDataList[i].terrainPosition.y * chunkSize);
            chunks[i].SetActive(showMeshes);

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
            meshRenderers.Clear();
            meshFilters.Clear();
            meshColliders.Clear();

            // Create terrain chunks and add them to list
            for (int x = 0; x < chunksPerRow * chunksPerRow; x++)
            {
                
                tempObj = new GameObject("TerrainChunk");
                tempObj.SetActive(showMeshes);
                
                MeshRenderer meshRenderer = (MeshRenderer)tempObj.AddComponent(typeof(MeshRenderer));
                meshRenderers.Add(meshRenderer);

                MeshFilter meshFilter = (MeshFilter)tempObj.AddComponent(typeof(MeshFilter));
                meshFilter.mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                meshFilters.Add(meshFilter);

                if (createColliders)
                {
                    MeshCollider meshCollider = (MeshCollider)tempObj.AddComponent(typeof(MeshCollider));
                    meshColliders.Add(meshCollider);
                }

                tempObj.transform.SetParent(terrain.transform);
                tempObj.isStatic = true;
                chunks.Add(tempObj);
            }

            lastChunksPerRow = chunksPerRow;
        }

        //Debug.Log("Creating chunk Game Objects took: " + ((Time.realtimeSinceStartup - startTime) * 1000f) + "ms");
    }

    

    void CreateChunkVerticesData(Vector2 terrainPosition, int chunkID)
    {
        vertices = new Vector3[(chunkSize + 1) * (chunkSize + 1)];

        //float startTime2 = Time.realtimeSinceStartup;
        noiseValues = noise.GetNoiseValues((int)terrainPosition.x, (int)terrainPosition.y, mapSize, chunkSize);
        //Debug.Log("Creating noise array and fill with values: " + ((Time.realtimeSinceStartup - startTime2) * 1000f) + "ms");

        

        if (useFalloff)
        {
            noiseValues = SubtractingFalloff(noiseValues, (int)terrainPosition.x, (int)terrainPosition.y);
        }

        if (usePlateaus)
            noiseValues = TerrainTools.SmoothToPlateaus(noiseValues, plateau1, plateau2, plateau3, plateau4, plateauSmoothing);



        for (int i = 0, z = 0; z <= chunkSize; z++)
        {
            for (int x = 0; x <= chunkSize; x++)
            {
                float y = animCurve.Evaluate(noiseValues[x, z]) * heightScale;
                vertices[i] = new Vector3(x, y, z);
                i++;
            }
        }

        ChunkData chunkData = new ChunkData();
        chunkData.chunkID = chunkID;
        chunkData.noiseValues = noiseValues;
        chunkData.vertices = vertices;
        chunkData.triangles = triangles;
        chunkData.uvs = uv;
        chunkData.terrainPosition = terrainPosition;
        chunkDataList.Add(chunkData);
        //Debug.Log("Vertices data generation took: " + ((Time.realtimeSinceStartup - startTime) * 1000f) + "ms");
    }

    float[,] SubtractingFalloff(float[,] noiseValues,int terrainXpos, int terrainYpos)
    {
        float[,] newNoiseValues = noiseValues;
        //Texture2D tex = new Texture2D(newNoiseValues.GetLength(0), newNoiseValues.GetLength(1));

        //blah to do wo auf der falloffmap???

        int xMinPos = terrainXpos * chunkSize;
        int yMinPos = terrainYpos * chunkSize;


        for (int x = 0; x < newNoiseValues.GetLength(0); x++)
        {
            for (int y = 0; y < newNoiseValues.GetLength(1); y++)
            {
                // SUBSTRACT FALLOFF MAP VALUES || test: string [] c = a.Except(b).ToArray();
                newNoiseValues[x, y] = Mathf.Clamp01(newNoiseValues[x, y] - falloffMap[x + xMinPos, y + yMinPos]);

                //float c = Mathf.Clamp01(newNoiseValues[x, y]);
                //tex.SetPixel(x, y, new Color(c, c, c));
            }
        }

        

        //tex.Apply();
        //
        //fallOffImage.texture = tex;
        //fallOffImage.texture.filterMode = FilterMode.Point;
        //fallOffImage.color = Color.white;

        return newNoiseValues;
    }


}
