using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameUI : MonoBehaviour {

    public Image fadePlane;
    public GameObject gameOverUI;

    void Start()
    {
        FindAnyObjectByType<Player>().OnDeath += OnGameOver;
    }

    void OnGameOver() {
        StartCoroutine( Fade( Color.clear, Color.black, 1 ) );
        gameOverUI.SetActive( true );
    }

    IEnumerator Fade( Color from, Color to, float time ) {
        float fraction = 0;

        while ( fraction < 1 ) {
            fraction += Time.deltaTime / time;
            fadePlane.color = Color.Lerp(from, to, fraction );
            yield return null;
        }
    }

    // UI Input
    public void StartNewGame() {
        SceneManager.LoadScene( "Game" );
    }

}
