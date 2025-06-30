using System.Collections;
using UnityEngine;

public class Spawner : MonoBehaviour {

    public bool devMode;

    public Wave[] waves;
    public Enemy enemy;

    LivingEntity playerEntity;
    Transform playerTransform;

    Wave currentWave;
    int currentWaveNumber;

    int enemiesRemainingToSpawn;
    int enemiesRemainingAlive;
    float nextSpawnTime;

    MapGenerator map;

    float timeBetweenCampingChecks = 2;
    float campTresholdDistance = 1.5f;
    float nextCampCheckTime;
    Vector3 campPositionOld;
    bool isCamping;
    bool isDisabled;

    public event System.Action<int> OnNewWave;

    Coroutine spawnEnemyCoroutine;

    void Start() {
        playerEntity = FindAnyObjectByType<Player>();
        playerTransform = playerEntity.transform;

        nextCampCheckTime = timeBetweenCampingChecks + Time.time;
        campPositionOld = playerTransform.position;
        playerEntity.OnDeath += OnPlayerDeath;

        map = FindAnyObjectByType<MapGenerator>();
        NextWave();
    }

    void Update() {
        if( !isDisabled ) {
            if( Time.time > nextCampCheckTime ) {
                nextCampCheckTime = Time.time + timeBetweenCampingChecks;

                isCamping = Vector3.Distance(playerTransform.position, campPositionOld) < campTresholdDistance;
                campPositionOld = playerTransform.position;
            }

            if( ( enemiesRemainingToSpawn > 0 || currentWave.infinite ) && Time.time > nextSpawnTime ) {
                enemiesRemainingToSpawn--;
                nextSpawnTime = Time.time + currentWave.timeBetweenSpawns;
                
                spawnEnemyCoroutine = StartCoroutine(SpawnEnemy());
            }
        }

        if( devMode ) {
            if( Input.GetKeyDown( KeyCode.Return ) ) {
                if( spawnEnemyCoroutine != null ) {
                    StopCoroutine( spawnEnemyCoroutine );
                    spawnEnemyCoroutine = null;
                }

                foreach( Enemy enemy in FindObjectsByType<Enemy>( FindObjectsSortMode.None ) ) {
                    GameObject.Destroy( enemy.gameObject );
                }
                NextWave();
            }

        }
    }

    IEnumerator SpawnEnemy() {
        float spawnDelay = 1;
        float tileFlashSpeed = 4;

        Transform spawnTile = map.GetRandomOpenTile();
        if( isCamping ) {
            spawnTile = map.GetTileFromPosition(playerTransform.position);
        }
        Material tileMat = spawnTile.GetComponent<Renderer>().material;
        Color initialColor = tileMat.color;
        Color flashColor = Color.red;
        float spawnTimer = 0;

        while( spawnTimer < spawnDelay ) {

            float t = Mathf.Sin(2 * Mathf.PI * tileFlashSpeed * spawnTimer - 0.5f * Mathf.PI) * 0.5f + 0.5f;
            tileMat.color = Color.Lerp(initialColor, flashColor, t);

            spawnTimer += Time.deltaTime;
            yield return null;
        }

        tileMat.color = initialColor;

        Enemy spawnedEnemy = Instantiate(enemy, spawnTile.position + Vector3.up, Quaternion.identity) as Enemy;
        spawnedEnemy.OnDeath += OnEnemyDeath;
        spawnedEnemy.SetCharacteristics( currentWave.moveSpeed, currentWave.hitsToKillPlayer, currentWave.enemyHealth, currentWave.skinColor );
        
        spawnEnemyCoroutine = null;
    }
    
    void OnPlayerDeath() {
        isDisabled = true;
    }

    void OnEnemyDeath() {
        enemiesRemainingAlive--;

        if( enemiesRemainingAlive == 0 ) {
            NextWave();
        }
    }

    void ResetPlayerPosition() {
        playerTransform.position = map.GetTileFromPosition( Vector3.zero ).position + Vector3.up * 3;
    }

    void NextWave() {
        currentWaveNumber++;
        
        if(currentWaveNumber - 1 < waves.Length ) {
            currentWave = waves[currentWaveNumber - 1];

            enemiesRemainingToSpawn = currentWave.enemyCount;
            enemiesRemainingAlive = enemiesRemainingToSpawn;

            if(OnNewWave != null ) {
                OnNewWave(currentWaveNumber);
            }

            ResetPlayerPosition();
        }
    }

    [System.Serializable]
    public class Wave {
        public bool infinite;
        public int enemyCount;
        public float timeBetweenSpawns;

        public float moveSpeed;
        public int hitsToKillPlayer;
        public float enemyHealth;
        public Color skinColor;
    }
}
