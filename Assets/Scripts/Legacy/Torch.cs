using UnityEditor;
using UnityEngine;

namespace Legacy
{
    public class Torch : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _fireParticles;

        private void Start()
        {
            Extinguish();
        }

        public void Fire()
        {
            _fireParticles.Play(true);
        }

        public void Extinguish()
        {
            _fireParticles.Stop(true);
        }

#if UNITY_EDITOR
        [ContextMenu("Position torch")]
        public void Positioning(bool isSave = true)
        {
            if (!Physics.Raycast(transform.position + Vector3.up * 100, Vector3.down, out RaycastHit hit, 200
                    //, LayerMask.GetMask("Ground")
                ))
                return;

            if (isSave)
                Undo.RecordObject(transform, "position tree");

            transform.position = hit.point - hit.normal * .1f;

            Vector3 normal = hit.normal;

            Vector3 fwd = Vector3.Cross(normal, Vector3.right).normalized;
            fwd = Quaternion.AngleAxis(Random.Range(0, 360), normal) * fwd;
            transform.rotation = Quaternion.LookRotation(fwd, normal);
        }
#endif
    }
}