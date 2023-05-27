using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GUI_DebugPanel : MonoBehaviour
{
    [SerializeField] private Text seedText;
    [SerializeField] private Slider seedSlider;
    [SerializeField] private MapGenerator mapGenerator;
    [SerializeField] public GameObject loadingScreenPanel;
    [SerializeField] public Text loadingScreenText;

    private void Start()
    {
        loadingScreenPanel.SetActive(false);
    }

    public void SetSeed()
    {
        mapGenerator.seed = (int)seedSlider.value;
        seedText.text = "Seed: " + (int)seedSlider.value;
    }

    public void ActivateLoadingScreen()
    {
        StartCoroutine(SetLoadingScreen());
    }

    public IEnumerator SetLoadingScreen()
    {
        loadingScreenPanel.SetActive(true);
        loadingScreenText.text = "Generating Terrain Data...";
        yield return null;
        GenerateNewMap();
    }

    public void GenerateNewMap()
    {
        mapGenerator.GenerateNewMap();
    }

    public void SetLoading(bool active, string text)
    {
        loadingScreenPanel.SetActive(active);
        loadingScreenText.text = text;
    }
}
