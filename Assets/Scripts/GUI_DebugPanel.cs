using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GUI_DebugPanel : MonoBehaviour
{
    [SerializeField] private Text seedText;
    [SerializeField] private Slider seedSlider;
    [SerializeField] private MapGenerator mapGenerator;
    [SerializeField] public GameObject loadingScreenPanel;
    [SerializeField] public Text loadingScreenText;
    [SerializeField] private Dropdown dropDownWorldSize;
    [SerializeField] private Dropdown dropDownWindow;
    [SerializeField] private Toggle toggleColliders;
    [SerializeField] private Toggle toggleMeshes;

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

    public void SetWorldSize()
    {
        mapGenerator.mapSize = Convert.ToInt16(dropDownWorldSize.options[dropDownWorldSize.value].text);
    }

    public void ResetScene()
    {
        SceneManager.LoadScene("MainScene");
    }

    public void SetWindow()
    {
        if (dropDownWindow.value == 0)
        {
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
        }
        else
        {
            Screen.fullScreenMode = FullScreenMode.Windowed;
        }
    }

    public void SetLoading(bool active, string text)
    {
        loadingScreenPanel.SetActive(active);
        loadingScreenText.text = text;
    }

    public void ToggleColliders()
    {
        mapGenerator.createColliders = toggleColliders.isOn;
    }

    public void ToggleShowMeshes()
    {
        mapGenerator.showMeshes = toggleMeshes.isOn;
    }
}
