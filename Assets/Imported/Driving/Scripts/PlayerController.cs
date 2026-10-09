using UnityEngine;

namespace FinalGame.Imported.Driving
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour
    {
        public float speed = 18f;
        public float turnSpeed = 65f;
        Rigidbody body;
        Vector3 spawnPosition;
        Quaternion spawnRotation;
        public float DistanceDriven { get; private set; }
        public float CurrentSpeed => Vector3.ProjectOnPlane(body.linearVelocity, Vector3.up).magnitude;

        void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            spawnPosition = transform.position;
            spawnRotation = transform.rotation;
        }
        void FixedUpdate()
        {
            float throttle = FinalGame.GameInput.GetAxis("Vertical");
            float steering = FinalGame.GameInput.GetAxis("Horizontal");
            // Drive the physics body, so collision resolution does not undo Transform movement.
            Quaternion heading = body.rotation * Quaternion.Euler(0, steering * turnSpeed * Time.fixedDeltaTime, 0);
            body.MoveRotation(heading);
            Vector3 forward = heading * Vector3.forward;
            Vector3 velocity = forward * (speed * throttle);
            velocity.y = body.linearVelocity.y;
            body.linearVelocity = velocity;
            DistanceDriven += Mathf.Abs(throttle) * speed * Time.fixedDeltaTime;
            if (body.position.y < -12)
            {
                body.position = spawnPosition + Vector3.up;
                body.rotation = spawnRotation;
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
            }
        }
    }
}
