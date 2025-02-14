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

    //public InputActionReference zoomActionRef;
    //public InputAction zoomAction => screenSpaceInputs.screenSpacePinchScaleInput.pinchGapDeltaInput.inputAction;//zoomActionRef.action;

    //public InputActionReference panActionRef;
    //public InputAction panAction => screenSpaceInputs.screenSpaceRotateInput.dragDeltaInput.inputAction;
    Transform parent;

    private void OnEnable()
    {
        //zoomAction.Enable();
        //panAction.Enable();
        parent = transform.parent;
    }

    //private void OnDisable()
    //{
    //    zoomAction.Disable();
    //    panAction.Disable();
    //}

    void UpdateZoom()
    {
        float zoomDelta = screenSpaceInputs.screenSpacePinchScaleInput.pinchGapDeltaInput.ReadValue() * zoomSensitivity;//.deltaPosition.y
        //zoomCurrent = Vector3.Distance(Vector3.zero, transform.position);//Mathf.Clamp(zoomCurrent - zoomDelta * zoomSpeed * Time.fixedDeltaTime, zoomMin, zoomMax);
        Vector3 newPos = transform.position + transform.forward * zoomDelta * zoomSpeed * Time.fixedDeltaTime;
        zoomCurrent = Vector3.Distance(Vector3.zero, newPos);
        if (zoomCurrent < zoomMin || zoomCurrent > zoomMax)
            return;

        transform.position = newPos;
    }

    public float previousPitch = 0;
    void UpdatePitch()
    {
        Vector2 pitchDelta = screenSpaceInputs.screenSpaceRotateInput.dragDeltaInput.ReadValue() * panFactor;
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
        }

       

        //if (pitchDelta.y > pitchDelta.x)
        //    parent.rotation = Quaternion.Euler(parent.eulerAngles.With(x: transform.parent.eulerAngles.x + pitchDelta.y));
        //else
        //    parent.rotation = Quaternion.Euler(parent.eulerAngles.With(y: transform.parent.eulerAngles.y + pitchDelta.x));

        //float pitch = transform.parent.eulerAngles.x - pitchDelta;
        //pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);
        //Debug.Log("delta and new pitch are " + pitchDelta + " " + pitch);
        //transform.parent.eulerAngles = new Vector3(pitch, transform.parent.eulerAngles.y, transform.parent.eulerAngles.z);

    }

    private void FixedUpdate()
    {
        UpdateZoom();
        UpdatePitch();
    }

}
