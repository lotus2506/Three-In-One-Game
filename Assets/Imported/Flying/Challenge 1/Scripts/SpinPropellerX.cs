using UnityEngine;

namespace FinalGame.Imported.Flying
{
public class SpinPropellerX : MonoBehaviour

{

    public float rotationSpeed = 1000f;

    void Update()

    {
        if (Time.timeScale == 0f) return;

        transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);

    }

}
 
}
