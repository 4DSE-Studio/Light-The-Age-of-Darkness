using System.Collections;
using UnityEngine;

namespace Legacy
{
    public class IgniteTrigger : MonoBehaviour
    {
        [SerializeField] private Torch _torch;
        [SerializeField] private Light _highlightLight;
        [SerializeField] private float _highlightAnimationDuration = 0.5f;

        private bool _isFired;
        private bool _hasIgniter;
        private Igniter _igniter;
        private float _maxLightIntensity;

        private void Start()
        {
            _maxLightIntensity = _highlightLight.intensity;
            _highlightLight.intensity = 0;
            _highlightLight.enabled = false;
        }

        private void Update()
        {
            if (_isFired || !_hasIgniter || !Input.GetKeyDown(KeyCode.E))
                return;

            _isFired = true;
            _igniter.Ignite(_torch);
            StartCoroutine(Unhighlight());
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent(out Igniter igniter))
                return;

            StartCoroutine(Highlight());
            _hasIgniter = true;
            _igniter = igniter;
        }

        private void OnTriggerExit(Collider other)
        {
            if (_isFired)
                return;

            if (!_hasIgniter)
                return;

            if (!_igniter.gameObject.Equals(other.gameObject))
                return;

            StartCoroutine(Unhighlight());
            _hasIgniter = false;
            _igniter = null;
        }

        private IEnumerator Highlight()
        {
            _highlightLight.enabled = true;
            yield return StartCoroutine(Utils.ChangeIntensity(_highlightLight, _maxLightIntensity, _highlightAnimationDuration));
        }

        private IEnumerator Unhighlight()
        {
            yield return StartCoroutine(Utils.ChangeIntensity(_highlightLight, 0, _highlightAnimationDuration));
            _highlightLight.enabled = false;
        }
    }
}