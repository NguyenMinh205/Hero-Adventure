using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsManager : Singleton<SettingsManager>
{
    [SerializeField] private SettingsUI _settingsUI;

    protected override void Awake()
    {
        base.KeepActive(true);
        base.Awake();
    }

    private void Start()
    {
        _settingsUI.Init();
    }

    public void OpenSettings()
    {
        _settingsUI.DoOpenSettings();
    }

    public void SetMusicVolume(float volume)
    {
        AudioManager.Instance.SetMusicVolume(volume);
    }

    public void SetSoundVolume(float volume)
    {
        AudioManager.Instance.SetSoundVolume(volume);
    }

    public void ToggleVibration()
    {
        bool currentStatus = DataManager.Instance.GameData.Vibration;
        SetVibration(!currentStatus);
    }

    public void SetVibration(bool isOn)
    {
        DataManager.Instance.GameData.Vibration = isOn;
        
        if (isOn)
        {
            #if !UNITY_EDITOR
            Vibration.VibratePop();
            #endif
            Debug.Log("Vibration ON");
        }
        else
        {
            Debug.Log("Vibration OFF");
        }
        
        DataManager.Instance.GameData.Save();
    }

    public void Vibrate(long milliseconds = 50)
    {
        if (DataManager.Instance.GameData.Vibration)
        {
            #if UNITY_ANDROID && !UNITY_EDITOR
            Vibration.VibrateAndroid(milliseconds);
            #elif !UNITY_EDITOR
            Vibration.VibratePop();
            #endif
        }
    }
}
