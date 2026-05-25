using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] private Slider _musicSlider;
    [SerializeField] private Slider _sfxSlider;

    [Header("Buttons")]
    [SerializeField] private Button _vibrationButton;
    [SerializeField] private Button _backButton;
    [SerializeField] private Image _vibrationBtnImage;
    [SerializeField] private Sprite _vibrationOnSprite;
    [SerializeField] private Sprite _vibrationOffSprite;

    public void Init()
    {
        _musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
        _sfxSlider.onValueChanged.AddListener(OnSFXSliderChanged);
        _vibrationButton.onClick.AddListener(OnVibrationButtonClicked);
        
        if (_backButton != null)
        {
            _backButton.onClick.AddListener(OnBackButtonClicked);
        }
    }

    public void DoOpenSettings()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayPopupOpen();
        InitializeUI();
        this.gameObject.SetActive(true);
    }

    private void InitializeUI()
    {
        if (DataManager.Instance != null && DataManager.Instance.GameData != null)
        {
            var data = DataManager.Instance.GameData;
            
            _musicSlider.value = data.MusicVolume;
            _sfxSlider.value = data.SoundVolume;
            
            UpdateVibrationUI(data.Vibration);
        }
    }

    private void OnMusicSliderChanged(float value)
    {
        SettingsManager.Instance.SetMusicVolume(value);
    }

    private void OnSFXSliderChanged(float value)
    {
        SettingsManager.Instance.SetSoundVolume(value);
    }

    private void OnVibrationButtonClicked()
    {
        SettingsManager.Instance.ToggleVibration();
        UpdateVibrationUI(DataManager.Instance.GameData.Vibration);
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySoundButtonClick();
        }
    }

    private void UpdateVibrationUI(bool isOn)
    {
        if (_vibrationBtnImage != null)
        {
            _vibrationBtnImage.sprite = isOn ? _vibrationOnSprite : _vibrationOffSprite;
        }
    }

    private void OnBackButtonClicked()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPopupClose();
        }
        
        this.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        _musicSlider.onValueChanged.RemoveListener(OnMusicSliderChanged);
        _sfxSlider.onValueChanged.RemoveListener(OnSFXSliderChanged);
        _vibrationButton.onClick.RemoveListener(OnVibrationButtonClicked);
        if (_backButton != null)
        {
            _backButton.onClick.RemoveListener(OnBackButtonClicked);
        }
    }
}
