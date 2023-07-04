using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] Button[] menuButtons;
    [SerializeField] Button[] settingsButtons;
    [Header("Panels")]
    [SerializeField] GameObject newGamePanel;
    [SerializeField] GameObject loadGamePanel;
    [SerializeField] GameObject settingsPanel;
    [SerializeField] GameObject generalSettingsPanel;
    [SerializeField] GameObject graphicsSettingsPanel;
    [SerializeField] GameObject audioSettingsPanel;
    [SerializeField] GameObject controlSettingsPanel;

    private int currentSettingsNavSelected;

    void Start()
    {
        BackButton();
        currentSettingsNavSelected = 0;
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
