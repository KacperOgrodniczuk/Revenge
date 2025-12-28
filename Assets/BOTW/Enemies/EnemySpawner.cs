using UnityEngine;

public class EnemySpawner : MonoBehaviour
{

        [Header("Spawn Settings")]
        public GameObject enemyPrefab;
        public float spawnInterval = 5f;
        public int maxEnemies = 10;

        [Header("Spawn Points")]
        public Transform[] spawnPoints;

        private float timer = 0f;
        private int currentEnemyCount = 0;

        void Start()
        {
            timer = spawnInterval;
        }

        void Update()
        {
        
            timer -= Time.deltaTime;

            if (timer <= 0f)
            {
                TrySpawnEnemy();
                timer = spawnInterval;
            }
        }

        void TrySpawnEnemy()
        {
            if (currentEnemyCount >= maxEnemies)
                return;

            if (spawnPoints.Length == 0)
            {
                Debug.LogWarning("EnemySpawner has no spawn points assigned!");
                return;
            }

           
            Transform chosenPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

  
            GameObject newEnemy = Instantiate(enemyPrefab, chosenPoint.position, chosenPoint.rotation);

            
            currentEnemyCount++;


        }

 

    
}
