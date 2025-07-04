using UnityEngine;

public class ScoreKeeper : MonoBehaviour {
    
    public static int score {  get; private set; }
    float lastEnemyKilledTime;
    int streakCount;
    float streakExpiryTime = 1;

    void Start () {
        Enemy.OnDeathStatic += OnEnemyKilled;
        FindAnyObjectByType<Player>().OnDeath += OnPlayerDeath;
    }

    void OnEnemyKilled() {
        
        if( Time.time < lastEnemyKilledTime + streakExpiryTime ) {
            streakCount++;
        }
        else {
            streakCount = 0;
        }

        lastEnemyKilledTime = Time.time;

        score += 5 + streakCount * streakCount;
    }

    void OnPlayerDeath() {
        Enemy.OnDeathStatic -= OnEnemyKilled; // Avoid double score counting. Static event belongs to the class which lives in memory even when objects are destroyed and persists to next scene.
    }

}
