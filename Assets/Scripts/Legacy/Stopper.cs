using System.Collections.Generic;
using UnityEngine;

namespace Legacy
{
    public class Stopper : MonoBehaviour
    {
        public Rigidbody rb;
        public Collider cldr;

        public Vector2 drag;
        public List<ContactPoint> contacts;

        private void OnTriggerExit(Collider other)
        {
            rb.angularDrag = drag.x;
        }

        private void OnTriggerStay(Collider other)
        {
            rb.angularDrag = drag.y;
        }
    }
}