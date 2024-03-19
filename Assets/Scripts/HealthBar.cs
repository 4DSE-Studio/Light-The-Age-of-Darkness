using System.Collections;
using UnityEngine;
using UnityEngine.Animations;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private float _textureScrollSpeed = 8f;
    [SerializeField] private float _textureLengthScale = 3;
    [SerializeField, Range(0.1f, 2f)] private float _lineWidth = 0.5f;

    [SerializeField] private BasicHealth _health;

    [SerializeField] private GameObject _bar;

    [SerializeField] private GameObject _beamStart;
    [SerializeField] private GameObject _beamEnd;
    [SerializeField] private GameObject _beam;

    [SerializeField, Range(0.5f, 6f)] private float _maxBeamLength;
    [SerializeField] private float _currentHealthRatio;

    private LineRenderer _line;
    private bool _isActive = true;
    private float _targetHealthRatio;
    private Coroutine _healthUpdateCoroutine;
    private Coroutine _showBeamCoroutine;
    private Coroutine _hideBeamCoroutine;

    private ParticleSystem _particleSystemStart;
    private ParticleSystem _particleSystemEnd;

    private float _startEmissionRate;
    private float _endEmissionRate;

    private void Start()
    {
        _line = _beam.GetComponent<LineRenderer>();
        _line.useWorldSpace = false;

        _beamStart.transform.LookAt(_beamEnd.transform.position);
        _beamEnd.transform.LookAt(_beamStart.transform.position);

        _particleSystemStart = _beamStart.GetComponent<ParticleSystem>();
        _particleSystemEnd = _beamEnd.GetComponent<ParticleSystem>();

        _startEmissionRate = _particleSystemStart.emission.rateOverTime.constant;
        _endEmissionRate = _particleSystemEnd.emission.rateOverTime.constant;

        if (_bar.TryGetComponent(out RotationConstraint constraint) == false)
            return;

        ConstraintSource source = new()
        {
            sourceTransform = Camera.main.transform,
            weight = 1
        };

        constraint.SetSource(0, source);
        OnHealthChanged(_health.Health, _health.MaxHealth);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (_isActive)
                Hide();
            else
                Show();
        }

        if (_isActive)
            UpdateBeam();
    }

    private void OnEnable()
    {
        _health.HealthChanged += OnHealthChanged;
    }

    private void OnDisable()
    {
        _health.HealthChanged -= OnHealthChanged;
    }

    private void OnHealthChanged(float health, float maxHealth)
    {
        _targetHealthRatio = health / maxHealth;

        if (_healthUpdateCoroutine != null)
            StopCoroutine(_healthUpdateCoroutine);

        _healthUpdateCoroutine = StartCoroutine(UpdateHealthCoroutine());
    }

    private IEnumerator UpdateHealthCoroutine()
    {
        float elapsedTime = 0f;
        float duration = 0.5f;

        float startHealthRatio = _currentHealthRatio;

        while (elapsedTime < duration)
        {
            _currentHealthRatio = Mathf.Lerp(startHealthRatio, _targetHealthRatio, elapsedTime / duration);

            UpdateBeam();

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        _currentHealthRatio = _targetHealthRatio;
        UpdateBeam();

        if (_currentHealthRatio <= 0.05f)
            Hide();
        else
            Show();

        _healthUpdateCoroutine = null;
    }

    private void UpdateBeam()
    {
        Vector3 start = _beamStart.transform.position;

        Vector3 end = CalculateEndPosition(start, _currentHealthRatio);

        float distance = Vector3.Distance(start, end);
        UpdateBeamPositions(start, end, distance);

        Vector3[] pathPoints = { _beamStart.transform.localPosition, _beamEnd.transform.localPosition };
        _line.positionCount = 2;
        _line.SetPositions(pathPoints);

        _line.sharedMaterial.mainTextureScale = new Vector2(distance / _textureLengthScale, 1);
        _line.sharedMaterial.mainTextureOffset -= new Vector2(Time.deltaTime * _textureScrollSpeed, 0);
    }

    private void UpdateBeamPositions(Vector3 start, Vector3 end, float distance)
    {
        Vector3 direction = (end - start).normalized;

        Vector3 midpoint = transform.position;

        Vector3 newStart = midpoint - direction * (distance / 2);
        Vector3 newEnd = midpoint + direction * (distance / 2);

        _beamStart.transform.position = newStart;
        _beamEnd.transform.position = newEnd;
    }

    private Vector3 CalculateEndPosition(Vector3 start, float healthRatio)
    {
        if (healthRatio <= 0)
            healthRatio = 0.05f;

        float maxDistance = _maxBeamLength;
        float distance = Mathf.Lerp(0f, maxDistance, healthRatio);

        Vector3 direction = (_beamEnd.transform.position - start).normalized;
        return start + direction * distance;
    }

    [ContextMenu("Hide")]
    public void Hide()
    {
        if (_isActive == false)
            return;

        if (_hideBeamCoroutine != null)
            StopCoroutine(_hideBeamCoroutine);

        _hideBeamCoroutine = StartCoroutine(HideBeamCoroutine());
    }

    [ContextMenu("Show")]
    public void Show()
    {
        if (_isActive)
            return;

        if (_showBeamCoroutine != null)
            StopCoroutine(_showBeamCoroutine);

        _showBeamCoroutine = StartCoroutine(ShowBeamCoroutine());
    }

    private IEnumerator HideBeamCoroutine()
    {
        ParticleSystem.EmissionModule startEmissionModule = _particleSystemStart.emission;
        ParticleSystem.EmissionModule endEmissionModule = _particleSystemEnd.emission;

        float elapsedTime = 0f;
        float duration = 0.5f;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            startEmissionModule.rateOverTime = Mathf.Lerp(_startEmissionRate, 0f, t);
            endEmissionModule.rateOverTime = Mathf.Lerp(_endEmissionRate, 0f, t);

            _line.startWidth = Mathf.Lerp(_lineWidth, 0f, t);
            _line.endWidth = Mathf.Lerp(_lineWidth, 0f, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        _particleSystemStart.Stop();
        _particleSystemEnd.Stop();

        _bar.SetActive(false);
        _isActive = false;

        _hideBeamCoroutine = null;
    }

    private IEnumerator ShowBeamCoroutine()
    {
        _bar.SetActive(true);
        UpdateBeam();

        _particleSystemStart.Play();
        _particleSystemEnd.Play();

        ParticleSystem.EmissionModule startEmissionModule = _particleSystemStart.emission;
        ParticleSystem.EmissionModule endEmissionModule = _particleSystemEnd.emission;

        float elapsedTime = 0f;
        float duration = 0.5f;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            startEmissionModule.rateOverTime = Mathf.Lerp(0f, _startEmissionRate, t);
            endEmissionModule.rateOverTime = Mathf.Lerp(0f, _endEmissionRate, t);

            _line.startWidth = Mathf.Lerp(0f, _lineWidth, t);
            _line.endWidth = Mathf.Lerp(0f, _lineWidth, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        _isActive = true;
        _showBeamCoroutine = null;
    }
}