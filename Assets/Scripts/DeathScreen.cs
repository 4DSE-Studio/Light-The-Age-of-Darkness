using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeathScreen : MonoBehaviour
{
    [SerializeField] private SmoothScopeZoom _smoothScopeZoom;
    [SerializeField] private GameObject _ui;

    [SerializeField] private TextMeshProUGUI _textMeshPro;
    [SerializeField] private Image _background;
    [SerializeField] private AnimationCurve _outlineCurve;

    private float _minOutline;

    private void Start()
    {
        _minOutline = _textMeshPro.outlineWidth;
        _background.color = new Color(_background.color.r, _background.color.g, _background.color.b, 0);
        _textMeshPro.faceColor = new Color32(_textMeshPro.faceColor.r, _textMeshPro.faceColor.g, _textMeshPro.faceColor.b, 0);
        _ui.SetActive(false);
    }

    [ContextMenu("Show")]
    public void Show()
    {
        _ui.SetActive(true);
        StartCoroutine(StartSequence());
    }

    [ContextMenu("Hide")]
    public void Hide()
    {
        _textMeshPro.outlineWidth = _minOutline;
        _background.color = new Color(_background.color.r, _background.color.g, _background.color.b, 0);
        _textMeshPro.faceColor = new Color32(_textMeshPro.faceColor.r, _textMeshPro.faceColor.g, _textMeshPro.faceColor.b, 0);
        _ui.SetActive(false);
        _smoothScopeZoom.ZoomOut();
    }

    private IEnumerator StartSequence()
    {
        _smoothScopeZoom.ZoomIn();

        yield return new WaitForSeconds(2f);

        StartCoroutine(ChangeAlphaBackground(2f, 0.8f));

        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(ChangeAlpha(1f));

        StartCoroutine(ChangeOutline(2f));

        yield return null;
    }

    private IEnumerator ChangeOutline(float duration)
    {
        float time = 0f;

        while (time < duration)
        {
            float t = time / duration;
            _textMeshPro.outlineWidth = Mathf.Lerp(_minOutline, 1, _outlineCurve.Evaluate(t));
            time += Time.deltaTime;
            yield return null;
        }

        _textMeshPro.outlineWidth = 1;
    }

    private IEnumerator ChangeAlphaBackground(float duration, float endAlpha)
    {
        float time = 0;

        while (time < 1)
        {
            _background.color = new Color(_background.color.r, _background.color.g, _background.color.b, Mathf.Lerp(0, endAlpha, time * time * time));
            time += Time.deltaTime / duration;
            yield return null;
        }

        _background.color = new Color(_background.color.r, _background.color.g, _background.color.b, endAlpha);
    }

    private IEnumerator ChangeAlpha(float duration)
    {
        float time = 0;

        while (time < 1)
        {
            _textMeshPro.faceColor = new Color32(_textMeshPro.faceColor.r, _textMeshPro.faceColor.g, _textMeshPro.faceColor.b, (byte)Mathf.Lerp(0, 255, time * time * time));
            time += Time.deltaTime / duration;
            yield return null;
        }

        _textMeshPro.faceColor = new Color32(_textMeshPro.faceColor.r, _textMeshPro.faceColor.g, _textMeshPro.faceColor.b, 255);
    }
}