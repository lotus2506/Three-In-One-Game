using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FinalGame.Imported.SumoAssets
{
public class EnemyX : MonoBehaviour
{
    public float speed = 500.0f;
    private Rigidbody enemyRb;
    private GameObject playerGoal;

    void Start()
    {
        enemyRb = GetComponent<Rigidbody>();
        playerGoal = GameObject.Find("Player Goal");

        GameObject spawnManager = GameObject.Find("Spawn Manager");
        if (spawnManager != null)
        {
            SpawnManagerX spawnManagerX = spawnManager.GetComponent<SpawnManagerX>();
            if (spawnManagerX != null)
            {
                speed = spawnManagerX.enemySpeed;
            }
        }
    }

    void FixedUpdate()
    {
        if (Time.timeScale == 0f) return;
        if (playerGoal != null && enemyRb != null)
        {
            Vector3 lookDirection = (playerGoal.transform.position - transform.position).normalized;
            enemyRb.AddForce(lookDirection * speed * Time.deltaTime);
        }

        // Destroy enemy if it falls off the platform
        if (transform.position.y < -10)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Player Goal" || other.gameObject.name == "Goals" || other.CompareTag("Goal"))
        {
            Destroy(gameObject);
        }
    }
}
}
