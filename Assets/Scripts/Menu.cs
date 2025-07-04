using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour {

    public GameObject mainMenuHolder;
    public GameObject optonsMenuHolder;

    public Slider[] volumeSliders;
    public Toggle[] resolutionToggles;
    public Toggle fullscreenToggle;
    public int[] screenWidths;
    int activeScreenResIndex;

    private void Start() {
        activeScreenResIndex = PlayerPrefs.GetInt( "screen res index" );
        bool isFullscreen = PlayerPrefs.GetInt( "fullscreen" ) == 1 ? true : false;

        volumeSliders[0].value = AudioManager.Instance.masterVolumeFraction;
        volumeSliders[1].value = AudioManager.Instance.musicVolumeFraction;
        volumeSliders[2].value = AudioManager.Instance.sfxVolumeFraction;

        for( int i = 0; i < resolutionToggles.Length; i++ ) {
            resolutionToggles[i].isOn = 1 == activeScreenResIndex;
        }

        fullscreenToggle.isOn = isFullscreen;
    }

    public void Play() {
        SceneManager.LoadScene( "Game" );
    }

    public void Quit() {
        Application.Quit();
    }

    public void OptionsMenu() {
        mainMenuHolder.SetActive( false );
        optonsMenuHolder.SetActive( true );
    }

    public void MainMenu() {
        mainMenuHolder.SetActive( true );
        optonsMenuHolder.SetActive( false );
    }

    public void SetScreenResolution( int i ) {
        if( resolutionToggles[i].isOn ) {
            activeScreenResIndex = i;
            float aspectRatio = 16 / 9f;
            Screen.SetResolution( screenWidths[i], Mathf.RoundToInt( screenWidths[i] / aspectRatio ), false );
            PlayerPrefs.SetInt( "screen res index", activeScreenResIndex );
            PlayerPrefs.Save();
        }
    }

    public void SetFullscreen( bool isFullscreen ) {
        // Make resolution toggles noninteractable if in fullscreen, because unity can have trouble switching
        for( int i = 0; i < resolutionToggles.Length; i++ ) {
            resolutionToggles[i].interactable = !isFullscreen;
        }

        if( isFullscreen ) {
            Resolution[] allResolutions = Screen.resolutions;
            Resolution maxResolution = allResolutions[allResolutions.Length - 1];
            Screen.SetResolution( maxResolution.width, maxResolution.height, true );
        }
        else {
            SetScreenResolution( activeScreenResIndex );
        }

        PlayerPrefs.SetInt( "fullscreen", isFullscreen ? 1 : 0 );
        PlayerPrefs.Save();
    }

    public void SetMasterVolume( float value ) {
        AudioManager.Instance.SetVolume( value, AudioManager.AudioChannel.Master );
    }

    public void SetMusicVolume( float value ) {
        AudioManager.Instance.SetVolume( value, AudioManager.AudioChannel.Music );
    }

    public void SetSfxVolume( float value) {
        AudioManager.Instance.SetVolume( value, AudioManager.AudioChannel.Sfx );
    }

}
