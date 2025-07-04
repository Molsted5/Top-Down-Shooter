using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameUI : MonoBehaviour {

    public Image fadePlane;
    public GameObject gameOverUI;

    public GameObject newWaveUI;
    public RectTransform newWaveBanner;
    public TextMeshProUGUI newWaveTitle;
    public TextMeshProUGUI newWaveEnemyCount;
    public TextMeshProUGUI scoreUI;
    public TextMeshProUGUI gameOverScoreUI;
    public RectTransform healthBar;

    Spawner spawner;
    Player player;
    
    Coroutine animateCoroutine;

    void Awake() {
        spawner = FindAnyObjectByType<Spawner>();
        spawner.OnNewWave += OnNewWave;
    }

    void Start()
    {
        player = FindAnyObjectByType<Player>();
        player.OnDeath += OnGameOver;
    }

    void Update() {
        scoreUI.text = ScoreKeeper.score.ToString( "D6" );
        float healthFraction = 0;
        if( player != null ) {
            healthFraction = player.health / player.startingHealth;
        }
        healthBar.localScale = new Vector3( healthFraction, 1, 1 );
    }

    void OnNewWave( int waveNumber ) {
        newWaveUI.SetActive( true );
        string[] numbers = { "One", "Two", "Three", "Four", "Five" };
        newWaveTitle.text = "- Wave " + numbers[waveNumber - 1] + " -";
        string enemyCountString = spawner.waves[waveNumber - 1].infinite ? "Infinite" : spawner.waves[waveNumber - 1].enemyCount + "";
        newWaveEnemyCount.text = "Enemies: " + enemyCountString;

        if( animateCoroutine != null ) {
            StopCoroutine( animateCoroutine );
        }
        animateCoroutine = StartCoroutine( AnimateNewWaveBanner() );
    }

    void OnGameOver() {
        Cursor.visible = true;
        StartCoroutine( Fade( Color.clear, new Color( 0, 0, 0, 0.99f ), 1 ) );
        gameOverScoreUI.text = scoreUI.text;
        scoreUI.gameObject.SetActive( false );
        healthBar.transform.parent.gameObject.SetActive( false );
        gameOverUI.SetActive( true );
    }

    IEnumerator AnimateNewWaveBanner() {
        float delayTime = 1.5f;
        float speed = 3f;
        float animateFraction = 0;
        int dir = 1;

        float endDelayTime = Time.time + 1 / speed + delayTime;

        while( animateFraction >= 0 ) {
            animateFraction += Time.deltaTime * speed * dir;

            if( animateFraction >= 1 ) {
                animateFraction = 1;
                if( Time.time > endDelayTime ) {
                    dir = -1;
                }
            }

            newWaveBanner.anchoredPosition = Vector2.up * Mathf.Lerp( -206, 45, animateFraction );
            yield return null;
        }
    }

    IEnumerator Fade( Color from, Color to, float time ) {
        float fraction = 0;

        while ( fraction < 1 ) {
            fraction += Time.deltaTime / time;
            fadePlane.color = Color.Lerp(from, to, fraction );
            yield return null;
        }

        Cursor.visible = true;
    }

    // UI Input
    public void StartNewGame() {
        SceneManager.LoadScene( "Game" );
    }

    public void ReturnToMainMenu() {
        SceneManager.LoadScene( "Menu" );
    }
}
