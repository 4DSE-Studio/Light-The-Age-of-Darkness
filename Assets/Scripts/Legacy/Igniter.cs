using System.Collections;
using UnityEngine;

namespace Legacy
{
    public class Igniter : MonoBehaviour
    {
        [SerializeField] private GameObject _spherePrefab;
        [SerializeField] private float _flyingSpeed = 1f;

        public void Ignite(Torch torch)
        {
            GameObject sphere = Instantiate(_spherePrefab, transform.position, Quaternion.identity);
            StartCoroutine(Ignite(sphere, torch));
        }

        private IEnumerator Ignite(GameObject sphere, Torch torch)
        {
            Vector3 startPosition = sphere.transform.position;
            Vector3 target = torch.transform.position;

            float distance = Vector3.Distance(startPosition, target);
            yield return StartCoroutine(Utils.MoveToTarget(sphere.transform, target, distance / _flyingSpeed));

            torch.Fire();
            Destroy(sphere);
        }
    }
}