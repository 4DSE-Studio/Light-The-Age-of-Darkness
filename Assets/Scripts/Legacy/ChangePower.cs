using System.Collections;
using UnityEngine;

namespace Legacy
{
    public class ChangePower : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _particleSystem;
        [SerializeField] private GameObject _body;
        [SerializeField] private SpriteRenderer _disabled;
        [SerializeField] private ParticleSystem.MinMaxGradient _endColor = Color.white;
        [SerializeField] private Power _currentPower = Power.None;

        [SerializeField] private GameObject _darkRayEffect;
        [SerializeField] private GameObject _lightRayEffect;

        [SerializeField] private float _highlightAnimationSpeed = 0.25f;
        [SerializeField] private float _changeAnimationSpeed = 2f;

        private Coroutine _highlightCoroutine;
        private ParticleSystem.MinMaxGradient _startColor;
        private Power _requiredPower = Power.None;
        private Renderer _bodyRenderer;
        private Transform _changerPosition;

        public bool IsChanging { get; private set; }

        public Power CurrentPower => _currentPower;

        private void Start()
        {
            _startColor = _particleSystem.trails.colorOverLifetime;
            _bodyRenderer = _body.GetComponent<Renderer>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F) == false)
                return;

            if (_requiredPower == Power.None)
                return;

            if (_requiredPower == _currentPower)
                return;

            Change();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (IsChanging)
                return;

            if (other.TryGetComponent(out Player player))
            {
                _requiredPower = Power.Light;
                _changerPosition = player.transform;

                if (_currentPower != Power.Light)
                    _highlightCoroutine = StartCoroutine(Highlight());
            }
            else if (other.TryGetComponent(out EnemyAI enemy))
            {
                _requiredPower = Power.Dark;
                _changerPosition = enemy.transform;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (IsChanging)
                return;

            _requiredPower = _currentPower;
            _changerPosition = null;
        }

        private void Change()
        {
            StartCoroutine(ChangeColor());
        }

        private IEnumerator ChangeColor()
        {
            IsChanging = true;

            yield return _highlightCoroutine;

            GameObject rayEffect = CurrentPower switch
            {
                Power.Light => _lightRayEffect,
                Power.Dark => _darkRayEffect,
                var _ => _lightRayEffect
            };

            StartCoroutine(FollowTarget(rayEffect, _changerPosition));

            yield return StartCoroutine(SwapColor(_startColor.color, _endColor.color, _changeAnimationSpeed));

            IsChanging = false;

            (_startColor, _endColor) = (_endColor, _startColor);
            _currentPower = _requiredPower;

            StartCoroutine(ChangeSprite(_disabled.color.a, Mathf.Approximately(_disabled.color.a, Color.clear.a) ? 255 : 0, 0.5f));
        }

        private IEnumerator ChangeSprite(float start, float end, float animationDuration)
        {
            float time = 0;

            while (time < 1)
            {
                time += Time.deltaTime / animationDuration;
                _disabled.color = new Color(_disabled.color.r, _disabled.color.g, _disabled.color.b, Mathf.Lerp(start, end, time));

                yield return null;
            }
        }

        private IEnumerator FollowTarget(GameObject rayEffect, Transform followingTransform)
        {
            GameObject ray = Instantiate(rayEffect, transform.position, Quaternion.identity);
            Vector3 originalScale = ray.transform.localScale;
            float originalDistance = Vector3.Distance(ray.transform.position, followingTransform.position);

            while (IsChanging)
            {
                ray.transform.LookAt(followingTransform);

                float distance = Vector3.Distance(ray.transform.position, followingTransform.position);
                float coefficient = distance / originalDistance;

                ray.transform.localScale = new Vector3(originalScale.x, originalScale.y, originalScale.z * coefficient);

                yield return null;
            }

            float delay = 0.05f;

            if (ray.TryGetComponent(out ParticleSystem particles))
            {
                particles.Stop();
                delay = particles.main.duration;
            }

            Destroy(ray.gameObject, delay);
        }

        private IEnumerator Highlight()
        {
            yield return StartCoroutine(SwapColor(_startColor.color, _endColor.color / 2, _highlightAnimationSpeed));
            yield return StartCoroutine(SwapColor(_endColor.color / 2, _startColor.color, _highlightAnimationSpeed));
        }

        private IEnumerator SwapColor(Color startColor, Color endColor, float animationDuration)
        {
            float time = 0;
            ParticleSystem.TrailModule colorModule = _particleSystem.trails;

            while (time < 1)
            {
                time += Time.deltaTime / animationDuration;
                Color color = Color.Lerp(startColor, endColor, time);
                colorModule.colorOverLifetime = new ParticleSystem.MinMaxGradient(color);
                _bodyRenderer.material.color = color;

                yield return null;
            }
        }
    }

    public enum Power : ushort
    {
        None,
        Light,
        Dark
    }
}