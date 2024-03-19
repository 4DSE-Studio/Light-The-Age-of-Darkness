using UnityEngine;

namespace Legacy
{
    public class PlayerMovement : MonoBehaviour
    {
        public Rigidbody rb;

        public float speed;
        public float currentspeed;
        public Vector3 possibleVelocity;

        public int dir = 1;

        public Transform gfx;
        public Transform yAxisRotator;

        public ParticleSystem particle;
        public float rotation;
        private Quaternion baseRot;

        private void Start()
        {
            baseRot = gfx.rotation;
        }

        private void Update()
        {
            ParticleSystem.MainModule main = particle.main;
            main.emitterVelocity = possibleVelocity;

            if (Input.GetKey(KeyCode.A))
            {
                currentspeed -= speed * Time.fixedDeltaTime * 2;
                dir = 1;
            }
            else if (Input.GetKey(KeyCode.D))
            {
                currentspeed += speed * Time.fixedDeltaTime * 2;
                dir = -1;
            }
            else
            {
                rb.velocity = new Vector2(0, rb.velocity.y);
                currentspeed = 0f;
            }

            currentspeed = Mathf.Clamp(currentspeed, -speed, speed);

            if (Input.GetKeyDown(KeyCode.Space))
            {
                rb.velocity = new Vector2(rb.velocity.x, 0f);
                rb.AddForce(Vector3.up * 45f, ForceMode.Impulse);
            }
        }

        private void FixedUpdate()
        {
            rb.AddForce(Vector3.down * 50, ForceMode.Acceleration);

            if (rb.velocity.magnitude < 2)
                possibleVelocity = new Vector3();

            possibleVelocity = rb.velocity;
            rb.velocity = new Vector2(currentspeed, rb.velocity.y);

            yAxisRotator.rotation = Quaternion.RotateTowards(yAxisRotator.rotation, Quaternion.Euler(new Vector3(yAxisRotator.rotation.x, rotation * dir, yAxisRotator.rotation.z)), 12f);

            if (rb.velocity.magnitude > 2)
            {
                Quaternion newquat = Quaternion.Euler(new Vector3(gfx.rotation.x, gfx.rotation.y, Mathf.Lerp(35, -35, Mathf.InverseLerp(-speed, speed, rb.velocity.x))));
                gfx.transform.rotation = Quaternion.RotateTowards(gfx.transform.rotation, newquat, 12f);
            }
            else
            {
                gfx.transform.rotation = Quaternion.RotateTowards(gfx.transform.rotation, baseRot, 8f);
            }
        }
    }
}