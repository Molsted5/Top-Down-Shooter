using UnityEngine;

public class MusicManager : MonoBehaviour {

    public AudioClip mainTheme;
    public AudioClip menuTheme;

    private void Start() {
        AudioManager.Instance.PlayMusic( menuTheme, 2 );
    }

    private void Update() {
        if( Input.GetKeyDown( KeyCode.Space ) ) {
            AudioManager.Instance.PlayMusic( mainTheme, 3 );
        }
    }
}
