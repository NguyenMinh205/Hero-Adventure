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
    [SerializeField] private bool _vibration = true;

    [Header("Player Progression")]
    [SerializeField] private int _gold = 0;
    [SerializeField] private int _diamond = 0;
    [SerializeField] private int _playerLevel = 1;
    [SerializeField] private int _currentExp = 0;
    [SerializeField] private int _maxUnlockedLevel = 0;
    [SerializeField] private int[] _levelStars = new int[100];

    #region CONST
    private const float DEFAULT_MUSIC_VOLUME = 0.5f;
    private const float DEFAULT_SOUND_VOLUME = 0.5f;
    private const bool DEFAULT_VIBRATION = true;
    
    private const int DEFAULT_GOLD = 0;
    private const int DEFAULT_DIAMOND = 0;
    private const int DEFAULT_PLAYER_LEVEL = 1;
    private const int DEFAULT_EXP = 0;
    private const int DEFAULT_UNLOCKED_LEVEL = 0;
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

    public bool Vibration
    {
        get { return _vibration; }
        set { _vibration = value; }
    }

    public int Gold { get => _gold; set => _gold = value; }
    public int Diamond { get => _diamond; set => _diamond = value; }
    public int PlayerLevel { get => _playerLevel; set => _playerLevel = value; }
    public int CurrentExp { get => _currentExp; set => _currentExp = value; }
    public int MaxUnlockedLevel { get => _maxUnlockedLevel; set => _maxUnlockedLevel = value; }

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
        this._vibration = other._vibration;

        this._gold = other._gold;
        this._diamond = other._diamond;
        this._playerLevel = other._playerLevel;
        this._currentExp = other._currentExp;
        this._maxUnlockedLevel = other._maxUnlockedLevel;

        if (other._levelStars != null)
        {
            this._levelStars = other._levelStars;
        }
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
        _vibration = DEFAULT_VIBRATION;

        _gold = DEFAULT_GOLD;
        _diamond = DEFAULT_DIAMOND;
        _playerLevel = DEFAULT_PLAYER_LEVEL;
        _currentExp = DEFAULT_EXP;
        _maxUnlockedLevel = DEFAULT_UNLOCKED_LEVEL;
        _levelStars = new int[100];
    }

    #endregion

    #region LOGIC PROGRESSION
    
    public void AddResources(int addGold, int addDiamond, int addExp)
    {
        _gold += addGold;
        _diamond += addDiamond;
        _currentExp += addExp;

        int maxExpForCurrentLevel = _playerLevel * 100;
        while (_currentExp >= maxExpForCurrentLevel)
        {
            _currentExp -= maxExpForCurrentLevel;
            _playerLevel++;
            maxExpForCurrentLevel = _playerLevel * 100;
            Debug.Log($"Level Up! Current Level: {_playerLevel}");
        }
    }

    public void UnlockNextLevel(int currentLevelId)
    {
        if (currentLevelId >= _maxUnlockedLevel)
        {
            _maxUnlockedLevel = currentLevelId + 1;
        }
    }

    public void SaveLevelStar(int levelId, int starCount)
    {
        if (_levelStars == null)
            _levelStars = new int[100];

        if (levelId > 0 && levelId <= _levelStars.Length)
        {
            if (starCount > _levelStars[levelId])
            {
                _levelStars[levelId - 1] = starCount;
            }
        }
    }

    public int GetLevelStar(int levelId)
    {
        if (_levelStars != null && levelId >= 0 && levelId < _levelStars.Length)
        {
            return _levelStars[levelId];
        }
        return 0;
    }

    #endregion
}
