using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button[] menuButtons;
    [SerializeField] private Button[] settingsButtons;
    [Header("Loading")]
    [SerializeField] private GameObject loadingPanel;
    [Header("TopPanels")]
    [SerializeField] private GameObject menuTitle;
    [SerializeField] private GameObject menuNav;
    [SerializeField] private GameObject menuContent;
    [Header("Panels")]
    [SerializeField] private GameObject newGamePanel;
    [SerializeField] private GameObject loadGamePanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject generalSettingsPanel;
    [SerializeField] private GameObject graphicsSettingsPanel;
    [SerializeField] private GameObject audioSettingsPanel;
    [SerializeField] private GameObject controlSettingsPanel;
    [Header("World Settings")]
    [SerializeField] private InputField inputWorldName;
    [SerializeField] private InputField inputSeed;
    [SerializeField] private TMP_Dropdown dropdownWorldSize;

    private int currentSettingsNavSelected;

    void Start()
    {
        BackButton();
        currentSettingsNavSelected = 0;
    }

    public void StartWorld()
    {
        if (inputWorldName.text != "" && inputSeed.text != "")
        {
            menuTitle.SetActive(false);
            menuNav.SetActive(false);
            menuContent.SetActive(false);
            loadingPanel.SetActive(true);

            GameObject starterObject = new GameObject("GenSceneStarter");
            starterObject.AddComponent<GenSceneStarter>();
            starterObject.GetComponent<GenSceneStarter>().worldName = inputWorldName.text;
            starterObject.GetComponent<GenSceneStarter>().seed = inputSeed.text;
            starterObject.GetComponent<GenSceneStarter>().worldSize = Convert.ToInt16(dropdownWorldSize.options[dropdownWorldSize.value].text);

            SceneManager.LoadScene("MainScene");
        }
    }

    private void HideContentPanels()
    {
        newGamePanel.SetActive(false);
        loadGamePanel.SetActive(false);
        HideAllSettingsPanels();
    }

    #region MainMenuButtons

    public void NewGameButton()
    {
        HideContentPanels();
        newGamePanel.SetActive(true);
    }

    public void LoadGameButton()
    {
        HideContentPanels();
        loadGamePanel.SetActive(true);
    }

    public void SettingsButton()
    {
        foreach(Button but in menuButtons)
        {
            but.gameObject.SetActive(false);
        }
        menuButtons[4].gameObject.SetActive(true);
        settingsPanel.SetActive(true);
        SetSettingsButtonColors();
        HideAllSettingsPanels();
        HideContentPanels();
        generalSettingsPanel.SetActive(true);
    }

    public void BackButton()
    {
        foreach(Button but in menuButtons)
        {
            but.gameObject.SetActive(true);
        }
        menuButtons[4].gameObject.SetActive(false);
        settingsPanel.SetActive(false);
        menuTitle.SetActive(true);
        menuNav.SetActive(true);
        menuContent.SetActive(true);
        loadingPanel.SetActive(false);
        currentSettingsNavSelected = 0;
        HideContentPanels();
    }

    public void QuitButton()
    {
        Application.Quit();
    }

    #endregion

    #region SettingsButtons

    private void SetSettingsButtonColors()
    {
        Color highlightColor = new Color(1f, 1f, 1f, 1f);
        Color normalColor = new Color(0.69f, 0.69f, 0.69f, 1f);
        for (int i = 0; i < settingsButtons.Length; i++)
        {
            if (i == currentSettingsNavSelected)
            {
                ColorBlock colors = settingsButtons[i].colors;
                colors.normalColor = highlightColor;
                settingsButtons[i].colors = colors;
            }
            else
            {
                ColorBlock colors = settingsButtons[i].colors;
                colors.normalColor = normalColor;
                settingsButtons[i].colors = colors;
            }
        }
    }

    private void HideAllSettingsPanels()
    {
        generalSettingsPanel.SetActive(false);
        graphicsSettingsPanel.SetActive(false);
        audioSettingsPanel.SetActive(false);
        controlSettingsPanel.SetActive(false);
    }

    public void GeneralSettingsButton()
    {
        currentSettingsNavSelected = 0;
        SetSettingsButtonColors();
        HideAllSettingsPanels();
        generalSettingsPanel.SetActive(true);
    }
    public void GraphicsSettingsButton()
    {
        currentSettingsNavSelected = 1;
        SetSettingsButtonColors();
        HideAllSettingsPanels();
        graphicsSettingsPanel.SetActive(true);
    }
    public void AudioSettingsButton()
    {
        currentSettingsNavSelected = 2;
        SetSettingsButtonColors();
        HideAllSettingsPanels();
        audioSettingsPanel.SetActive(true);
    }
    public void ControlsSettingsButton()
    {
        currentSettingsNavSelected = 3;
        SetSettingsButtonColors();
        HideAllSettingsPanels();
        controlSettingsPanel.SetActive(true);
    }

    #endregion
}
