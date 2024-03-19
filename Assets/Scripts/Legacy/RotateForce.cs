using UnityEngine;

namespace Legacy
{
    public class RotateForce : MonoBehaviour
    {
        public Rigidbody rb;
        public Vector3 input;
        public float speed;

        private void Start()
        {
        }

        private void Update()
        {
            if (Input.GetKey(KeyCode.Mouse0))
                input = Vector3.one;
            else if (Input.GetKey(KeyCode.Mouse1))
                input = Vector3.one * -1;
            else
                input = new Vector3();
        }

        private void FixedUpdate()
        {
            rb.AddTorque(rb.transform.right * (speed * input.z), ForceMode.Force);
        }
    }
}