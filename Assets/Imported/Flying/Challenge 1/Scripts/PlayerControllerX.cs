using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FinalGame.Imported.Flying
{
public class PlayerControllerX : MonoBehaviour
{
    public float speed;
    public float rotationSpeed;
    public float verticalInput;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Time.timeScale == 0f) return;
        // get the user's vertical input
        verticalInput = FinalGame.GameInput.GetAxis("Vertical");

        // move the plane forward at a constant rate
        transform.Translate(Vector3.forward * speed * Time.deltaTime);

        // tilt the plane up/down based on up/down arrow keys
        transform.Rotate(Vector3.right * rotationSpeed * Time.deltaTime *verticalInput);
    }
}

}
