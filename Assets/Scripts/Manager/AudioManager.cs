using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource _musicSource;
    public AudioSource MusicSource => _musicSource;

    [SerializeField] private AudioSource _soundSource;
    public AudioSource SoundSource => _soundSource;

    [Header("Music")]
    [SerializeField] private AudioClip _musicIntro;
    [SerializeField] private AudioClip _musicInMenu;
    [SerializeField] private AudioClip _musicInGame;

    [Header("General UI")]
    [SerializeField] private AudioClip _buttonClick;
    [SerializeField] private AudioClip _click;
    [SerializeField] private AudioClip _winSound;
    [SerializeField] private AudioClip _loseSound;

    protected override void Awake()
    {
        base.KeepActive(true);
        base.Awake();

        if (_musicSource == null || _soundSource == null)
        {
            Debug.LogError("AudioManager: AudioSource chưa được gán trong Inspector!");
        }
    }

    private void Start()
    {
        SetMusicVolume(DataManager.Instance.GameData.MusicVolume);
        SetSoundVolume(DataManager.Instance.GameData.SoundVolume);
        PlayMusicInMenu();
    }

    public void SetMusicVolume(float volume)
    {
        if (_musicSource != null)
        {
            _musicSource.volume = volume;
            if (DataManager.Instance != null && DataManager.Instance.GameData != null)
            {
                DataManager.Instance.GameData.MusicVolume = volume;
            }
        }
    }

    public void SetSoundVolume(float volume)
    {
        if (_soundSource != null)
        {
            _soundSource.volume = volume;
            if (DataManager.Instance != null && DataManager.Instance.GameData != null)
            {
                DataManager.Instance.GameData.SoundVolume = volume;
            }
        }
    }

    public void PlayMusicInMenu()
    {
        if (_musicInMenu != null)
        {
            PlayMusicGame(_musicInMenu);
        }
    }

    public void PlayMusicInGame()
    {
        if (_musicInGame != null)
        {
            PlayMusicGame(_musicInGame);
        }
    }

    public void PlayMusicGame(AudioClip clip)
    {
        if (_musicSource == null || clip == null) return;

        if (_musicSource.isPlaying && _musicSource.clip == clip) return;

        _musicSource.DOKill(); 

        _musicSource.loop = true;
        _musicSource.clip = clip;
        _musicSource.volume = 0f;
        _musicSource.Play();
        _musicSource.DOFade(DataManager.Instance.GameData.MusicVolume, 0.5f).SetUpdate(true);
    }

    public void StopMusic()
    {
        if (_musicSource != null && _musicSource.isPlaying)
        {
            _musicSource.DOKill();
            _musicSource.DOFade(0f, 0.5f).OnComplete(() =>
            {
                _musicSource.Stop();
            }).SetUpdate(true);
        }
    }

    public void PlaySFX(AudioClip sound, bool repeat = false)
    {
        if (sound == null || _soundSource == null) return;

        if (repeat)
        {
            _soundSource.loop = true;
            _soundSource.clip = sound;
            _soundSource.Play();
        }
        else
        {
            _soundSource.loop = false;
            _soundSource.PlayOneShot(sound, _soundSource.volume);
        }
    }

    public void StopSFX()
    {
        if (_soundSource != null && _soundSource.isPlaying)
        {
            _soundSource.Stop();
            _soundSource.loop = false;
            _soundSource.clip = null;
        }
    }

    #region Specific Game Play Methods

    public void PlaySoundButtonClick() => PlaySFX(_buttonClick);
    public void PlaySoundClick() => PlaySFX(_click);
    public void PlaySoundWin() => PlaySFX(_winSound);
    public void PlaySoundLose() => PlaySFX(_loseSound);
    #endregion
}