using DG.Tweening;
using PrimeTween;
using UnityEngine;
using UnityEngine.Events;

public class CameraRotationBehavior : MonoBehaviour
{
    public Camera cam;
    public float rotationSpeed = 12.5f;
    public float snapDuration = 0.25f;
    bool gameStarted = false;
    public UnityEvent OnCameraRotationStop;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cam = Camera.main;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (gameStarted)
            return;

        transform.Rotate(Vector3.up, rotationSpeed * Time.fixedDeltaTime);
    }

    public void OnGameStart(float targetYRot = 0f)
    {
        gameStarted = true;
        transform.DORotate(Vector3.up * targetYRot, snapDuration).OnComplete(
            () => {
                OnCameraRotationStop.Invoke();
                enabled = false;
            } );
    }
}
