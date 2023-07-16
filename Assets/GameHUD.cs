using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameHUD : MonoBehaviour
{
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private Text loadingText;


    public void SetLoadingPanel(string text, bool active)
    {
        loadingText.text = text;
        loadingPanel.SetActive(active);
    }
}
