using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour {

    public static SettingsManager Instance { get; private set; }

    [Header("UI References")]
    public Slider[] volumeSliders;
    public Toggle[] resolutionToggles;
    public Toggle fullscreenToggle;
    public int[] screenWidths;

    private int activeScreenResIndex;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        if (volumeSliders.Length > 0) {
            LoadSettings();
        }
    }

    public void LoadSettings() {
        activeScreenResIndex = PlayerPrefs.GetInt("screen res index", 0);
        bool isFullscreen = PlayerPrefs.GetInt("fullscreen", 0) == 1;

        volumeSliders[0].value = AudioManager.Instance.masterVolumeFraction;
        volumeSliders[1].value = AudioManager.Instance.musicVolumeFraction;
        volumeSliders[2].value = AudioManager.Instance.sfxVolumeFraction;

        for (int i = 0; i < resolutionToggles.Length; i++) {
            resolutionToggles[i].isOn = (i == activeScreenResIndex);
        }

        fullscreenToggle.isOn = isFullscreen;
    }

    public void SetScreenResolution(int i) {
        if (resolutionToggles[i].isOn) {
            activeScreenResIndex = i;
            float aspectRatio = 16 / 9f;
            Screen.SetResolution(screenWidths[i], Mathf.RoundToInt(screenWidths[i] / aspectRatio), false);
            PlayerPrefs.SetInt("screen res index", activeScreenResIndex);
            PlayerPrefs.Save();
        }
    }

    public void SetFullscreen(bool isFullscreen) {
        for (int i = 0; i < resolutionToggles.Length; i++) {
            resolutionToggles[i].interactable = !isFullscreen;
        }

        if (isFullscreen) {
            Resolution maxRes = Screen.resolutions[Screen.resolutions.Length - 1];
            Screen.SetResolution(maxRes.width, maxRes.height, true);
        } else {
            SetScreenResolution(activeScreenResIndex);
        }

        PlayerPrefs.SetInt("fullscreen", isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetMasterVolume(float value) {
        AudioManager.Instance.SetVolume(value, AudioManager.AudioChannel.Master);
    }

    public void SetMusicVolume(float value) {
        AudioManager.Instance.SetVolume(value, AudioManager.AudioChannel.Music);
    }

    public void SetSfxVolume(float value) {
        AudioManager.Instance.SetVolume(value, AudioManager.AudioChannel.Sfx);
    }
}

