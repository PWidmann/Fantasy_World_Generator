using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GUI_DebugPanel : MonoBehaviour
{
    [SerializeField] private Text seedText;
    [SerializeField] private Slider seedSlider;
    [SerializeField] private MapGenerator mapGenerator;


    public void SetSeed()
    {
        mapGenerator.seed = (int)seedSlider.value;
        seedText.text = "Seed: " + (int)seedSlider.value;
    }

    public void GenerateNewMap()
    {
        mapGenerator.GenerateNewMap();
    }
}
