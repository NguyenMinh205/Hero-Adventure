using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DataManager : Singleton<DataManager>
{
    public SecureGameData GameData { get; private set; }

    protected override void Awake()
    {
        base.KeepActive(true);
        base.Awake();
        GameData = new SecureGameData();
        GameData.Load();
    }

    private void OnApplicationQuit()
    {
        GameData.Save();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && GameData != null)
        {
            GameData.Save();
        }
    }
}

[System.Serializable]
public class SecureGameData
{
    [SerializeField] private float _musicVolume = 0.5f;
    [SerializeField] private float _soundVolume = 0.5f;

    #region CONST
    private const float DEFAULT_MUSIC_VOLUME = 0.5f;
    private const float DEFAULT_SOUND_VOLUME = 0.5f;
    #endregion

    #region PROPERTIES
    public float MusicVolume
    {
        get { return _musicVolume; } 
        set { _musicVolume = value; }
    }

    public float SoundVolume
    {
        get { return _soundVolume; }
        set { _soundVolume = value; }
    }

    #endregion

    #region SAVE/LOAD

    private const eData SaveFileKey = eData.SecureGameData;

    public void Save()
    {
        var saveUtil = new SaveUtility<SecureGameData>();
        saveUtil.SaveData(SaveFileKey, this);
    }

    public void Load()
    {
        var saveUtil = new SaveUtility<SecureGameData>();
        SecureGameData loadedData = new SecureGameData();
        saveUtil.LoadData(SaveFileKey, ref loadedData);
        CopyFrom(loadedData);
    }

    private void CopyFrom(SecureGameData other)
    {
        if (other == null) return;

        this._musicVolume = other._musicVolume;
        this._soundVolume = other._soundVolume;
    }

    public void ClearAllData()
      {
          SaveGameManager.DeleteSave(SaveFileKey);
          Reset();
      }

    private void Reset()
    {
        _musicVolume = DEFAULT_MUSIC_VOLUME;
        _soundVolume = DEFAULT_SOUND_VOLUME;
    }

    #endregion
}
