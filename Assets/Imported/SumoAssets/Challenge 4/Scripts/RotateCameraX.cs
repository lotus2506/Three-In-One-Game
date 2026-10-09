using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FinalGame.Imported.SumoAssets
{
public class RotateCameraX : MonoBehaviour
{
    private float speed = 200;
    public GameObject player;

    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale == 0f) return;
        float horizontalInput = FinalGame.GameInput.GetAxis("Horizontal");
        transform.Rotate(Vector3.up, horizontalInput * speed * Time.deltaTime);

        transform.position = player.transform.position; // Move focal point with player

    }
}

}
