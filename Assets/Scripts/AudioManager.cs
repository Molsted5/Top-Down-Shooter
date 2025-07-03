using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour {

    public enum AudioChannel { Master, Sfx, Music };

    float masterVolumeFraction = 0.2f;
    float sfxVolumeFraction = 1;
    float musicVolumeFraction = 1;

    AudioSource sfx2DSource;
    AudioSource[] musicSources;
    int activeMusicSourceIndex;

    Transform audioListener;
    Transform playerT;

    SoundLibrary library;

    public static AudioManager Instance { get; private set; }

    private void Awake() {
   
        if( Instance != null && Instance != this ) { 
            Destroy( gameObject ); // Destroy AudioManager if duplicate (for example if the next scene has one)
            return; // Skip rest of Awake because it was started again by a duplicate
        }
        Instance = this;
        DontDestroyOnLoad( gameObject );

        library = GetComponent<SoundLibrary>();

        musicSources = new AudioSource[2];
        for ( int i = 0; i < 2; i++ ) {
            GameObject newMusicSource = new GameObject( "Music Source " + ( i + 1 ) );
            musicSources[i] = newMusicSource.AddComponent<AudioSource>();
            newMusicSource.transform.parent = transform;
        }
        GameObject newSfx2DSource = new GameObject( "2D Sfx Source " );
        sfx2DSource = newSfx2DSource.AddComponent<AudioSource>();
        newSfx2DSource.transform.parent = transform;

        audioListener = FindAnyObjectByType<AudioListener>().transform;
        playerT = FindAnyObjectByType<Player>().transform;

        masterVolumeFraction = PlayerPrefs.GetFloat( "master vol", masterVolumeFraction );
        sfxVolumeFraction = PlayerPrefs.GetFloat( "sfx vol", sfxVolumeFraction );
        musicVolumeFraction = PlayerPrefs.GetFloat( "music vol", musicVolumeFraction );
    }

    private void Update() {
        if( playerT != null ) {
            audioListener.position = playerT.position;
        }
    }

    public void SetVolume( float volumeFraction, AudioChannel channel ) {
        switch( channel ) {
            case AudioChannel.Master:
                masterVolumeFraction = volumeFraction;
                break;
            case AudioChannel.Sfx:
                sfxVolumeFraction = volumeFraction;
                break;
            case AudioChannel.Music:
                musicVolumeFraction = volumeFraction;
                break;
        }

        musicSources[0].volume = musicVolumeFraction * masterVolumeFraction;
        musicSources[1].volume = musicVolumeFraction * masterVolumeFraction;

        PlayerPrefs.SetFloat( "master vol", masterVolumeFraction );
        PlayerPrefs.SetFloat( "sfx vol", sfxVolumeFraction );
        PlayerPrefs.SetFloat( "music vol", musicVolumeFraction );
    }

    public void PlayMusic( AudioClip clip, float fadeDuration = 1 ) {
        activeMusicSourceIndex = 1 - activeMusicSourceIndex;
        musicSources[activeMusicSourceIndex].clip = clip;
        musicSources[activeMusicSourceIndex].Play();

        StartCoroutine( AnimateMusicCrossfade( fadeDuration ) );
    }

    public void PlaySound( AudioClip clip, Vector3 position ) {
        if( clip != null ) {
            AudioSource.PlayClipAtPoint( clip, position, sfxVolumeFraction * masterVolumeFraction );
        }
    }

    public void PlaySound( string soundName, Vector3 position ) {
        PlaySound( library.GetClipFromName( soundName ), position );
    }

    public void PlaySound2D( string soundName ) {
        AudioClip clip = library.GetClipFromName( soundName );
        if( clip != null ) {
            sfx2DSource.PlayOneShot( clip, sfxVolumeFraction * masterVolumeFraction );
        }
    }

    IEnumerator AnimateMusicCrossfade( float fadeDuration ) {
        float fraction = 0;

        while( fraction < 1 ) {
            fraction += Time.deltaTime / fadeDuration;
            musicSources[activeMusicSourceIndex].volume = Mathf.Lerp( 0, musicVolumeFraction * masterVolumeFraction, fraction );
            musicSources[1-activeMusicSourceIndex].volume = Mathf.Lerp( musicVolumeFraction * masterVolumeFraction, 0, fraction );
            yield return null;
        }
    }

}
