using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FinalGame.Imported.SumoAssets
{
public class SpawnManagerX : MonoBehaviour
{
    public GameObject enemyPrefab;
    public GameObject powerupPrefab;
    private float spawnRangeX = 18;
    private float spawnZMin = 15; 
    private float spawnZMax = 25; 

    public int enemyCount;
    public int waveNumber = 1;
    public GameObject player;

    // TRACKING ENEMY SPEED PER WAVE
    public float enemySpeed = 200.0f;

    void Start()
    {
        SpawnEnemyWave(waveNumber);
        Instantiate(powerupPrefab, GenerateSpawnPosition(), powerupPrefab.transform.rotation);
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;
        enemyCount = FindObjectsByType<EnemyX>(FindObjectsSortMode.None).Length;

        if (enemyCount == 0)
        {
            waveNumber++;
            // Increase enemy speed slightly with each new wave
            enemySpeed += 50.0f; 
            
            SpawnEnemyWave(waveNumber);
            Instantiate(powerupPrefab, GenerateSpawnPosition(), powerupPrefab.transform.rotation);
        }
    }

    Vector3 GenerateSpawnPosition()
    {
        float xPos = Random.Range(-spawnRangeX, spawnRangeX);
        float zPos = Random.Range(spawnZMin, spawnZMax);
        return new Vector3(xPos, 0, zPos);
    }

    void SpawnEnemyWave(int enemiesToSpawn)
    {
        for (int i = 0; i < enemiesToSpawn; i++)
        {
            Instantiate(enemyPrefab, GenerateSpawnPosition(), enemyPrefab.transform.rotation);
        }
    }
}
}
