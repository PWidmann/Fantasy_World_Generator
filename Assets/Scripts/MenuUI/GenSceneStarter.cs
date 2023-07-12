using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenSceneStarter : MonoBehaviour
{
    public string worldName;
    public string seed;
    public int worldSize;

    private void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
        worldName = "TestWorld";
        seed = "1337";
        worldSize = 8000;
    }
}
