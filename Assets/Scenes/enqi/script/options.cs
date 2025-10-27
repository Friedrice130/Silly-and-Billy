using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class options : MonoBehaviour
{
    public Dropdown graphicsDrop, resoDrop;
    public Slider volumeSlider;
    public bool inGame;

    void Start()
    {
        if(PlayerPrefs.GetInt("settingsSaved", 0) == 0)
        {
            PlayerPrefs.SetInt("graphics", 0);
            PlayerPrefs.SetInt("resolution", 0);
            PlayerPrefs.SetFloat("mastervolume", 1.0f);
        }
        //Graphics
        if(PlayerPrefs.GetInt("graphics", 2) == 2)
        {
            graphicsDrop.value = 0;
            QualitySettings.SetQualityLevel(0);
        }
        if (PlayerPrefs.GetInt("graphics", 1) == 1)
        {
            graphicsDrop.value = 1;
            QualitySettings.SetQualityLevel(1);
        }
        if (PlayerPrefs.GetInt("graphics", 0) == 0)
        {
            graphicsDrop.value = 2;
            QualitySettings.SetQualityLevel(2);
        }
        //Resolution
        if (PlayerPrefs.GetInt("resolution", 2) == 2)
        {
            resoDrop.value = 0;
            Screen.SetResolution(854, 480, true);
        }
        if (PlayerPrefs.GetInt("resolution", 1) == 1)
        {
            resoDrop.value = 1;
            Screen.SetResolution(1280, 720, true);
        }
        if (PlayerPrefs.GetInt("resolution", 0) == 0)
        {
            resoDrop.value = 2;
            Screen.SetResolution(1920, 1080, true);
        }
        //Volume
        volumeSlider.value = PlayerPrefs.GetFloat("mastervolume");
        AudioListener.volume = PlayerPrefs.GetFloat("mastervolume");
    }
    public void setGraphics()
    {
        if (graphicsDrop.value == 0)
        {
            PlayerPrefs.SetInt("graphics", 2);
            PlayerPrefs.Save();
            QualitySettings.SetQualityLevel(0);
        }
        if (graphicsDrop.value == 1)
        {
            PlayerPrefs.SetInt("graphics", 1);
            PlayerPrefs.Save();
            QualitySettings.SetQualityLevel(1);
        }
        if (graphicsDrop.value == 2)
        {
            PlayerPrefs.SetInt("graphics", 0);
            PlayerPrefs.Save();
            QualitySettings.SetQualityLevel(2);
        }
    }
    public void setResolution()
    {
        if (resoDrop.value == 0)
        {
            PlayerPrefs.SetInt("resolution", 2);
            PlayerPrefs.Save();
            Screen.SetResolution(854, 480, true);
            Debug.Log("480p set");
        }
        if (resoDrop.value == 1)
        {
            PlayerPrefs.SetInt("resolution", 1);
            PlayerPrefs.Save();
            Screen.SetResolution(1280, 720, true);
            Debug.Log("720p set");
        }
        if (resoDrop.value == 2)
        {
            PlayerPrefs.SetInt("resolution", 0);
            PlayerPrefs.Save();
            Screen.SetResolution(1920, 1080, true);
            Debug.Log("1080p set");
        }
    }
    public void setVolume()
    {
        PlayerPrefs.SetFloat("mastervolume", volumeSlider.value);
        PlayerPrefs.Save();
        AudioListener.volume = volumeSlider.value;
    }
    public void saveSettings()
    {
        PlayerPrefs.SetInt("settingsSaved", 1);
        PlayerPrefs.Save();
    }
}