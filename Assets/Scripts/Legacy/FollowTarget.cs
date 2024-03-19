using UnityEngine;

namespace Legacy
{
    public class FollowTarget : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private float _smoothing = 0.2f;

        private void Update()
        {
            Move();
        }

        private void Move()
        {
            Vector3 position = transform.position;
            position = new Vector3(_target.position.x, position.y, position.z);

            if (position.sqrMagnitude < 0.1f)
                return;

            transform.position = Vector3.Lerp(transform.position, position, Time.deltaTime / _smoothing);
        }
    }
}