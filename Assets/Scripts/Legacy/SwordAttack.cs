using UnityEngine;

namespace Legacy
{
    public class SwordAttack : MonoBehaviour
    {
        public Vector3[] rots;
        public float time;
        public int id;

        private void Start()
        {
        }

        private void Update()
        {
            if (time <= 0)
            {
                transform.rotation = Quaternion.Euler(Vector3.zero);

                if (Input.GetKey(KeyCode.Mouse0))
                {
                    transform.rotation = Quaternion.Euler(rots[id]);
                    time = 0.75f;
                    id++;
                    id = id % rots.Length;
                }
            }
            else
            {
                time -= Time.deltaTime;
            }
        }
    }
}