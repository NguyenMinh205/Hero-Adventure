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
    [SerializeField] private AudioClip _musicBattleNormal;
    [SerializeField] private AudioClip _musicBattleBoss;
    [SerializeField] private AudioClip _musicExplore;

    [Header("General UI")]
    [SerializeField] private AudioClip _buttonClick;
    [SerializeField] private AudioClip _winSound;
    [SerializeField] private AudioClip _loseSound;
    [SerializeField] private AudioClip _popupOpen;
    [SerializeField] private AudioClip _popupClose;

    [Header("Grid & Puzzle SFX")]
    [SerializeField] private AudioClip _gemSelect;
    [SerializeField] private AudioClip _gemDrag;
    [SerializeField] private AudioClip _gemMatchBase;

    [Header("Combat SFX")]
    [SerializeField] private AudioClip _playerFootstep;
    [SerializeField] private AudioClip _attackSwing;
    [SerializeField] private AudioClip _attackCritSwing;
    [SerializeField] private AudioClip _attackHit;
    [SerializeField] private AudioClip _playerHurt;
    [SerializeField] private AudioClip _enemyHurt;
    [SerializeField] private AudioClip _playerBlock;
    [SerializeField] private AudioClip _enemyBlock;
    [SerializeField] private AudioClip _playerDie;
    [SerializeField] private AudioClip _enemyDie;

    [Header("System & Targets")]
    [SerializeField] private AudioClip _roundStart;
    [SerializeField] private AudioClip _targetSelected;
    [SerializeField] private AudioClip _waveClear;

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
        PlayMusicIntro();
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

    public void PlayMusicIntro()
    {
        if (_musicIntro != null)
        {
            PlayMusicGame(_musicIntro);
        }
    }

    public void PlayMusicInMenu()
    {
        if (_musicInMenu != null)
        {
            PlayMusicGame(_musicInMenu);
        }
    }

    public void PlayMusicBattleNormal()
    {
        if (_musicBattleNormal != null) PlayMusicGame(_musicBattleNormal);
    }

    public void PlayMusicBattleBoss()
    {
        if (_musicBattleBoss != null) PlayMusicGame(_musicBattleBoss);
    }

    public void PlayMusicExplore()
    {
        if (_musicExplore != null)
        {
            PlayMusicGame(_musicExplore);
        }
        else if (_musicBattleNormal != null)
        {
            if (_musicSource != null && _musicSource.clip == _musicBattleNormal)
            {
                _musicSource.DOFade(DataManager.Instance.GameData.MusicVolume * 0.5f, 0.5f).SetUpdate(true);
            }
            else
            {
                PlayMusicGame(_musicBattleNormal);
                if (_musicSource != null) _musicSource.volume = DataManager.Instance.GameData.MusicVolume * 0.5f;
            }
        }
    }

    public void PlayMusicGame(AudioClip clip, bool fullVolume = true)
    {
        if (_musicSource == null || clip == null) return;

        if (_musicSource.isPlaying && _musicSource.clip == clip)
        {
            if (fullVolume) _musicSource.DOFade(DataManager.Instance.GameData.MusicVolume, 0.5f).SetUpdate(true);
            return;
        }

        _musicSource.DOKill(); 

        _musicSource.loop = true;
        _musicSource.clip = clip;
        _musicSource.volume = 0f;
        _musicSource.Play();
        float targetVolume = fullVolume ? DataManager.Instance.GameData.MusicVolume : DataManager.Instance.GameData.MusicVolume * 0.5f;
        _musicSource.DOFade(targetVolume, 0.5f).SetUpdate(true);
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
            _soundSource.pitch = 1f;
            _soundSource.Play();
        }
        else
        {
            _soundSource.loop = false;
            _soundSource.pitch = 1f;
            _soundSource.PlayOneShot(sound, _soundSource.volume);
        }
    }

    public void PlaySFXWithPitch(AudioClip sound, float minPitch = 0.9f, float maxPitch = 1.1f)
    {
        if (sound == null || _soundSource == null) return;

        _soundSource.loop = false;
        _soundSource.pitch = Random.Range(minPitch, maxPitch);
        _soundSource.PlayOneShot(sound, _soundSource.volume);
    }
    
    private void ResetPitch()
    {
        if (_soundSource != null) _soundSource.pitch = 1f;
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
    public void PlaySoundClick() => PlaySFX(_buttonClick);
    public void PlaySoundWin() => PlaySFX(_winSound);
    public void PlaySoundLose() => PlaySFX(_loseSound);
    public void PlayPopupOpen() => PlaySFX(_popupOpen);
    public void PlayPopupClose() => PlaySFX(_popupClose);

    public void PlayGemSelect() => PlaySFX(_gemSelect);
    public void PlayGemDrag() => PlaySFXWithPitch(_gemDrag, 0.9f, 1.1f);
    
    public void PlayGemMatch(int matchCount) 
    {
        float pitch = Mathf.Clamp(1.0f + (matchCount - 3) * 0.1f, 1.0f, 1.5f);
        if (_soundSource != null)
        {
            _soundSource.pitch = pitch;
            _soundSource.PlayOneShot(_gemMatchBase, _soundSource.volume);
            ResetPitch();
        }
    }

    public void PlayPlayerFootstep(bool play)
    {
        if (play) PlaySFX(_playerFootstep, true);
        else StopSFX();
    }
    
    public void PlayAttackSwing() => PlaySFXWithPitch(_attackSwing);
    public void PlayAttackCritSwing() => PlaySFXWithPitch(_attackCritSwing);
    public void PlayAttackHit() => PlaySFXWithPitch(_attackHit);
    public void PlayPlayerHurt() => PlaySFXWithPitch(_playerHurt);
    public void PlayEnemyHurt() => PlaySFXWithPitch(_enemyHurt);
    public void PlayPlayerBlock() => PlaySFXWithPitch(_playerBlock);
    public void PlayEnemyBlock() => PlaySFXWithPitch(_enemyBlock);
    public void PlayPlayerDie() => PlaySFX(_playerDie);
    public void PlayEnemyDie() => PlaySFX(_enemyDie);
    public void PlayRoundStart() => PlaySFX(_roundStart);

    public void PlayTargetSelected() => PlaySFX(_targetSelected);
    public void PlayWaveClear() => PlaySFX(_waveClear);

    #endregion
}