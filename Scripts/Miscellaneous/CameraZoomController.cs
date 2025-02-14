using Reflex.Attributes;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.AR.Inputs;
using UnityUtils;

public class CameraZoomController : MonoBehaviour
{
    [Inject]
    ScreenSpaceInputs screenSpaceInputs;

    public float pitchDelta;
    //Script to handle camera zooming through unity input actions
    public float zoomSpeed = 1f;
    public float zoomMin = 1f;
    public float zoomMax = 10f;
    public float zoomCurrent = 5f;
    public float zoomSensitivity = 1f;
    public float pitchMin = -30f;
    public float pitchMax = 30f;
    public float panFactor = 20f;

    Transform parent;

    private void OnEnable()
    {
        parent = transform.parent;
    }

    void UpdateZoom()
    {
        float zoomDelta = screenSpaceInputs.screenSpacePinchScaleInput.pinchGapDeltaInput.ReadValue() * zoomSensitivity;//.deltaPosition.y
        Vector3 newPos = transform.position + transform.forward * zoomDelta * zoomSpeed * Time.fixedDeltaTime;
        zoomCurrent = Vector3.Distance(Vector3.zero, newPos);
        if (newPos.y < 0 || zoomCurrent < zoomMin || zoomCurrent > zoomMax)
            return;

        //Debug.Log("zoom delta is " + zoomDelta + " zoom current is " + zoomCurrent);
        transform.position = newPos;
    }

    public float previousPitch = 0;
    bool UpdatePitch()
    {
#if UNITY_ANDROID
        if (!Application.isEditor && screenSpaceInputs.screenSpaceRotateInput.screenTouchCountInput.ReadValue() < 2)
        {
            //Debug.Log("touch count is " + screenSpaceInputs.screenSpaceRotateInput.screenTouchCountInput.ReadValue());
            return false;
        }
#endif

        Vector2 pitchDelta = screenSpaceInputs.screenSpaceRotateInput.dragDeltaInput.ReadValue() * panFactor;
        
        if(pitchDelta == Vector2.zero)
        {
            return false;
        }

        //Debug.Log("parent x rot is " + parent.eulerAngles.x + " pitch delta is " + pitchDelta);
        float adjustedAngle = parent.eulerAngles.x > 180 ? parent.eulerAngles.x - 360 : parent.eulerAngles.x;
        float newPitch = adjustedAngle + pitchDelta.y * Time.fixedDeltaTime;
        if (newPitch > pitchMax)
        {
            //Debug.Log("pitch is greater than max value is " + parent.eulerAngles.x);
            newPitch = previousPitch;
            //pitchDelta.y = 0;
            //parent.eulerAngles = parent.eulerAngles.With(x: previousPitch);

        }
        else if (newPitch < pitchMin)
        {
            //Debug.Log("pitch is less than max value is " + parent.eulerAngles.x);
            newPitch = previousPitch;
            //pitchDelta.y = 0;
            //parent.eulerAngles = parent.eulerAngles.With(previousPitch);
            //Debug.Log("new pitch is " + parent.eulerAngles.x);
        }
        else
        {
            previousPitch = parent.eulerAngles.x;
            parent.rotation = Quaternion.Euler(parent.eulerAngles.With(x: newPitch,
          y: parent.eulerAngles.y + pitchDelta.x * Time.fixedDeltaTime));
            return true;
        }

        return false;
    }

    private void FixedUpdate()
    {
        if(!UpdatePitch())
            UpdateZoom();
    }
}
