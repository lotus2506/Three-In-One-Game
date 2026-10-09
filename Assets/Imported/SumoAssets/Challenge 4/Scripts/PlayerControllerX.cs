using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FinalGame.Imported.SumoAssets
{
public class PlayerControllerX : MonoBehaviour
{
    private Rigidbody playerRb;
    public float speed = 500.0f;
    private GameObject focalPoint;

    public bool hasPowerup;
    public GameObject powerupIndicator;
    public float powerUpDuration = 7;

    private float normalStrength = 10;
    private float powerupStrength = 25;

    public float turboSpeed = 10.0f;
    public ParticleSystem turboParticle;

    void Start()
    {
        playerRb = GetComponent<Rigidbody>();
        focalPoint = GameObject.Find("Focal Point");
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;
        float verticalInput = FinalGame.GameInput.GetAxis("Vertical");
        playerRb.AddForce(focalPoint.transform.forward * verticalInput * speed * Time.deltaTime);

        if (powerupIndicator != null)
        {
            powerupIndicator.transform.position = transform.position + new Vector3(0, -0.6f, 0);
        }

        if (FinalGame.GameInput.SpacePressed)
        {
            playerRb.AddForce(focalPoint.transform.forward * turboSpeed, ForceMode.Impulse);
            if (turboParticle != null)
            {
                turboParticle.Play();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Powerup"))
        {
            Destroy(other.gameObject);
            hasPowerup = true;
            if (powerupIndicator != null)
            {
                powerupIndicator.gameObject.SetActive(true);
            }
            
            // FIXED FOR CHALLENGE 3: Start the coroutine so the powerup expires after time
            StartCoroutine(PowerupCooldownRoutine());
        }
    }

    IEnumerator PowerupCooldownRoutine()
    {
        yield return new WaitForSeconds(powerUpDuration);
        hasPowerup = false;
        if (powerupIndicator != null)
        {
            powerupIndicator.gameObject.SetActive(false);
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            Rigidbody enemyRigidbody = other.gameObject.GetComponent<Rigidbody>();
            Vector3 awayFromPlayer = other.gameObject.transform.position - transform.position;

            if (hasPowerup)
            {
                enemyRigidbody.AddForce(awayFromPlayer * powerupStrength, ForceMode.Impulse);
            }
            else
            {
                enemyRigidbody.AddForce(awayFromPlayer * normalStrength, ForceMode.Impulse);
            }
        }
    }
}
}
