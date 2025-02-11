using UnityEngine;
using UnityEngine.InputSystem;

public class CameraZoomController : MonoBehaviour
{
    public float pitchDelta;
    //Script to handle camera zooming through unity input actions
    public float zoomSpeed = 1f;
    public float zoomMin = 1f;
    public float zoomMax = 10f;
    public float zoomCurrent = 5f;
    public float zoomSensitivity = 1f;
    public float pitchMin = -30f;
    public float pitchMax = 30f;

    public InputActionReference zoomActionRef;
    public InputAction zoomAction => zoomActionRef.action;

    public InputActionReference panActionRef;
    public InputAction panAction => panActionRef.action;

    private void OnEnable()
    {
        zoomAction.Enable();
        panAction.Enable();
    }

    private void OnDisable()
    {
        zoomAction.Disable();
        panAction.Disable();
    }

    void UpdateZoom()
    {
        float zoomDelta = zoomAction.ReadValue<Vector2>().y * zoomSensitivity;
        zoomCurrent = Mathf.Clamp(zoomCurrent - zoomDelta * zoomSpeed * Time.fixedDeltaTime, zoomMin, zoomMax);
        transform.position += transform.forward * zoomDelta * zoomSpeed * Time.fixedDeltaTime;
    }

    void UpdatePitch()
    {
        pitchDelta = panAction.ReadValue<Vector2>().y;
        float pitch = transform.parent.eulerAngles.x - pitchDelta;
        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);
        Debug.Log("delta and new pitch are " + pitchDelta + " " + pitch);
        transform.parent.eulerAngles = new Vector3(pitch, transform.parent.eulerAngles.y, transform.parent.eulerAngles.z);

    }

    private void FixedUpdate()
    {
        UpdateZoom();
        UpdatePitch();
    }

}
