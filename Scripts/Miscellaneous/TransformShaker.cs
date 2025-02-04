using UnityEngine;
using System.Collections;
using Cysharp.Threading.Tasks;
using UnityUtils;

public class TransformShaker : MonoBehaviour, IShakeable
{
    public float shakeDuration = 0.3f;
    public float shakeMagnitude = 0.7f;
    public bool isShaking = false;

    public void Shake()
    {
        //Vector3 originalPos = transform.localPosition;
        //transform.DOShakePosition(shakeDuration, shakeMagnitude)
        //    .OnComplete(() =>
        //    { 
        //        isShaking = false; 
        //        transform.localPosition = originalPos;
        //    });
        Shake(shakeDuration, shakeMagnitude);
    }

    public async void Shake(float duration, float magnitude)
    {
        if(!isShaking)
            await ShakeCoroutine(duration, magnitude).ToUniTask();
    }

    IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        isShaking = true;
        Vector3 originalPos = transform.localPosition;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);

            elapsed += Time.deltaTime;

            yield return null;
        }

        transform.localPosition = originalPos;
        isShaking = false;
    }
}

public interface IShakeable
{
    void Shake();
    void Shake(float duration, float magnitude);
}
