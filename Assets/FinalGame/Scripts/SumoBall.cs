using UnityEngine;

namespace FinalGame
{
    public class SumoBall : MonoBehaviour
    {
        public bool isPlayer;
        public SumoArena arena;
        Rigidbody body;
        Transform visual;
        Vector3 facing = Vector3.back;
        public Vector3 Facing => facing;
        void Awake() { body = GetComponent<Rigidbody>(); visual = transform.Find("Wrestler"); }
        void FixedUpdate()
        {
            if (arena == null || arena.Player == null || arena.GameOver) return;
            // Adapted from the soccer controller: force movement and collision knockback.
            Vector3 direction = isPlayer ? new Vector3(GameInput.GetAxis("Horizontal"), 0, GameInput.GetAxis("Vertical")) : arena.Player.position - transform.position;
            direction.y = 0;
            direction = Vector3.ClampMagnitude(direction, 1);
            if (direction.sqrMagnitude > .01f) facing = direction.normalized;
            body.AddForce(direction * (isPlayer ? 22f : 5f + Mathf.Min(arena.Wave, 6) * .6f), ForceMode.Acceleration);
            Vector3 velocity = body.linearVelocity;
            Vector3 flat = Vector3.ClampMagnitude(new Vector3(velocity.x, 0, velocity.z), isPlayer ? 10f : 12f);
            body.linearVelocity = new Vector3(flat.x, velocity.y, flat.z);
        }
        void LateUpdate()
        {
            if (Time.timeScale == 0 || visual == null) return;
            visual.rotation = Quaternion.Slerp(visual.rotation, Quaternion.LookRotation(facing), Time.deltaTime * 12);
        }
        void OnCollisionEnter(Collision collision)
        {
            if (!isPlayer || arena == null) return;
            var other = collision.gameObject.GetComponent<SumoBall>();
            if (other == null || other.isPlayer) return;
            Vector3 direction = other.transform.position - transform.position;
            direction.y = 0;
            other.body.AddForce(direction.normalized * (arena.Powered ? 20 : 7), ForceMode.Impulse);
        }
    }
}
