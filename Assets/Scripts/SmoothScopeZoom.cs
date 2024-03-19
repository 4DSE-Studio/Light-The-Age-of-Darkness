using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SmoothScopeZoom : MonoBehaviour
{
    [SerializeField] private float _zoomMultiplier = 2;
    [SerializeField] private float _zoomDuration = 2;
    [SerializeField] private Transform _player;

    private float _defaultFov;
    private Camera _camera;
    private Quaternion _defaultRotation;

    private void Start()
    {
        _camera = GetComponent<Camera>();
        _defaultFov = _camera.fieldOfView;
        _defaultRotation = _camera.transform.rotation;
    }

    [ContextMenu("ZoomIn")]
    public void ZoomIn()
    {
        StartCoroutine(Zooming(_defaultFov / _zoomMultiplier, _zoomDuration));
    }

    [ContextMenu("ZoomOut")]
    public void ZoomOut()
    {
        StartCoroutine(Zooming(_defaultFov, _zoomDuration, true));
    }

    private IEnumerator Zooming(float target, float animationDuration, bool isReset = false)
    {
        float startPosition = _camera.fieldOfView;
        float time = 0;

        while (time < 1)
        {
            _camera.fieldOfView = Mathf.Lerp(startPosition, target, time * time);
            time += Time.deltaTime / animationDuration;
            yield return null;
        }

        yield return StartCoroutine(SmoothTranslation(_camera.transform, _camera.transform.position, _player, 0.5f));

        _camera.transform.LookAt(_player);

        if (isReset)
            _camera.transform.rotation = _defaultRotation;
    }

    private IEnumerator SmoothTranslation(Transform startTransform, Vector3 finalPosition, Transform lookAtTransform, float animationDuration)
    {
        float currentDelta = 0;
        Vector3 startPosition = startTransform.position;
        Quaternion startRotation = startTransform.rotation;

        while (currentDelta <= 1f)
        {
            currentDelta += Time.deltaTime / animationDuration;

            transform.position = Vector3.Lerp(startPosition, finalPosition, currentDelta);

            transform.LookAt(lookAtTransform);
            transform.rotation = Quaternion.Lerp(startRotation, transform.rotation, currentDelta);

            yield return null;
        }
    }
}