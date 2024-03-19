using System.Collections;
using UnityEngine;

public static class Utils
{
    public static IEnumerator MoveToTarget(Transform transform, Vector3 target, float animationDuration)
    {
        Vector3 startPosition = transform.position;
        float time = 0;

        while (time < 1)
        {
            if (transform == null)
                yield break;

            transform.position = Vector3.LerpUnclamped(startPosition, target, Mathf.Pow(time, 2)) + Vector3.up * (Mathf.Sin(time * Mathf.PI) * 2);
            time += Time.deltaTime / animationDuration;
            yield return null;
        }
    }

    public static IEnumerator MoveToTarget(Transform transform, Transform target, float animationDuration)
    {
        Vector3 startPosition = transform.position;
        float time = 0;

        while (time < 1)
        {
            transform.position = Vector3.LerpUnclamped(startPosition, target.position, Mathf.Pow(time, 2)) + Vector3.up * (Mathf.Sin(time * Mathf.PI) * 2);
            time += Time.deltaTime / animationDuration;
            yield return null;
        }

        transform.position = target.position;
    }

    public static IEnumerator ChangeIntensity(Light light, float target, float animationDuration)
    {
        float startPosition = light.intensity;
        float time = 0;

        while (time < 1)
        {
            light.intensity = Mathf.Lerp(startPosition, target, time * time);
            time += Time.deltaTime / animationDuration;
            yield return null;
        }
    }

    public static IEnumerator MoveToPosition(Transform objectTransform, Vector3 targetPosition, float duration, AnimationCurve moveCurve)
    {
        Vector3 startPosition = objectTransform.position;
        float time = 0f;

        while (time < duration)
        {
            float t = time / duration;
            objectTransform.position = Vector3.LerpUnclamped(startPosition, targetPosition, moveCurve.Evaluate(t));
            time += Time.deltaTime;
            yield return null;
        }

        objectTransform.position = targetPosition;
    }
}