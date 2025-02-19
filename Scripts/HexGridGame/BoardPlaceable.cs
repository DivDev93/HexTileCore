using UnityEngine;
//using DG.Tweening;
using PrimeTween;
using UnityUtils;
using Reflex.Attributes;
using VolumetricLines;
using UnityEngine.Events;

public class BoardPlaceable : MonoBehaviour
{
    [Inject]
    IGameBoard gameBoard;

    Vector3 defaultScale;
    float displacement => gameBoard.tileGameData.cardPlacedDisplacement;
    float sphereCastSize => gameBoard.tileGameData.sphereCastSize;
    public ISelectableTarget placedTarget;
    private ISelectableTarget highlightedTarget;
    public LayerMask layerMask;// = LayerMask.GetMask("HexTile");
    int m_layerMask => layerMask.value;// = LayerMask.GetMask("HexTile");
    public VolumetricLineStripBehavior currentRaycastLine = null;
    public ISelectableTarget PlacedTarget
    {
        get { return placedTarget; }
        set
        {
            if (placedTarget != value)
            {
                if (placedTarget != null)
                    placedTarget.OnSelect -= OnTargetPlace;

                placedTarget = value;

                if (placedTarget != null)
                    placedTarget.OnSelect += OnTargetPlace;
            }
        }
    }
    public virtual ISelectableTarget HighlightedTarget
    {
        get { return highlightedTarget; }
        set
        {
            if (highlightedTarget != value)
            {
                if (highlightedTarget != null)
                {
                    highlightedTarget.OnHoverExit();
                }

                highlightedTarget = value;

                if (highlightedTarget != null)
                {
                    highlightedTarget.OnHoverEnter();
                    //Debug.Log("HIGHLIGHTED TARGET IS " + highlightedTarget.GetTransform().name);
                }

                HandleHighlightLine();
            }
        }
    }
    public bool isPlaced = false;
    public bool isPlacing = false;
    public float jumpPower => gameBoard.tileGameData.pulseData.height * gameBoard.tileGameData.parentScale; //0.025f;
    public float placementDuration = 0.5f;
    private Rigidbody rb;
    Vector3 placedPosition = Vector3.zero;
    Quaternion placedLocalRotation = Quaternion.identity;
    public Transform localPlayerTransform;
    public UnityEvent OnTilePlaced = new();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public virtual void Start()
    {
        defaultScale = transform.lossyScale;
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    public virtual void Update()
    {
        if(!isPlaced && !isPlacing)
        {
            RayCastTile();
        }

        UpdateLine();
    }

    public virtual void UpdateLine()
    {
        if (currentRaycastLine != null)
        {
            HandleHighlightLine();
        }
    }

    protected void ReleaseCurrentLine()
    {
        if (currentRaycastLine != null)
        {
            VolumetricLinePool.lineStripPool.Release(currentRaycastLine);
            currentRaycastLine = null;
        }
    }

    public virtual void FixedUpdate()
    {
        if (isPlaced)
        {
            if (rb != null && rb.isKinematic && transform.parent != null)
            {
                // Synchronize the Rigidbody's position and rotation with the parent
                rb.MovePosition(transform.parent.position + placedPosition);
                rb.MoveRotation(transform.parent.rotation * placedLocalRotation);
            }
        }
    }

    public void OnSelected()
    {        
        transform.parent = null;
        Tween scaleTween = Tween.Delay(0.01f);
        scaleTween.Chain(transform.DOScale(defaultScale, gameBoard.tileGameData.selectAnimationDuration));        
    }

    public void ClickPlacedTile()
    {
        isPlaced = false;
        if (PlacedTarget != null)
        {
            PlacedTarget.OnSelectionClick();
        }
    }

    public void OnSelectExit()
    {
        if (HighlightedTarget != null)
        {
            PlaceOnTile();
        }
        else if (PlacedTarget != null)
        {
            HighlightedTarget = PlacedTarget;
            PlaceOnTile();
        }
        //Debug.Log("UNSELECTED" + gameObject.name);
    }

    public virtual void PlaceOnTile()
    {
        isPlacing = true;
        ReleaseCurrentLine();
        //transform.parent = null;
        //var localPlayer = XRINetworkGameManager.Instance.GetLocalPlayer();
        localPlayerTransform = Camera.main.transform;
        //placedLocalRotation = Quaternion.LookRotation((localPlayerTransform.position - HighlightedTarget.GetTransform().position), Vector3.up);
        placedPosition = HighlightedTarget.GetTransform().position + Vector3.up * displacement;    
       
        Sequence sequence = Sequence.Create();
        sequence.Group(transform.DOMove(placedPosition, placementDuration));
        sequence.Group(transform.DORotateQuaternion(placedLocalRotation, placementDuration));
        sequence.Group(transform.DOScale(gameBoard.tileGameData.PlacedCardScale * Vector3.one, placementDuration));
        sequence.OnComplete(() =>
        {
            HighlightedTarget.OnHoverExit();
            HighlightedTarget.OnSelectionClick();
            PlacedTarget = HighlightedTarget; //neccessary to prevent unneccessary listener invocation making object pulse
            isPlaced = true;
        });
        sequence.Play();
        OnTilePlaced.Invoke();
    }

    //comes roundabout through events from the OnSelect event invocation in the SelectedTarget
    public virtual void OnTargetPlace()
    {
        if (!isPlaced)
            return;        

        if (PlacedTarget != null)
        {
            if (isPlacing)
                isPlacing = false;
            else
            {
                Debug.Log("Placed target is jumppower is" + jumpPower);
                float tempPlacedPos = placedPosition.y;
                Tween.Custom(tempPlacedPos, placedPosition.y + gameBoard.tileGameData.cardPlacedDisplacement, gameBoard.tileGameData.pulseData.duration, (val) => transform.position = placedPosition.With(y: val), Ease.InOutCirc, 2, CycleMode.Rewind);
                //PrimeTweenExtensions.PulseY(transform, placedPosition, jumpPower, 1, gameBoard.tileGameData.pulseData.duration, true).SetEase(Ease.Linear);
            }
        }
    }

    public virtual void RayCastTile()
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        RaycastHit hit;

        if (Physics.SphereCast(ray, sphereCastSize, out hit, 100f, m_layerMask))
        {
            IBoardSelectablePosition hexTile = null;
            if (gameBoard.tileColliderDict.TryGetValue(hit.collider, out hexTile))
            {                
                //if (highlightedTile != null && hexTile != highlightedTile)
                //{
                //    Debug.Log("Should clear highlight");
                //    highlightedTile.OnHoverExit();
                //}

                if (!gameBoard.selectedTiles.Contains(hexTile))
                {
                    HighlightedTarget = null;
                    //Debug.Log("Hovered Tile is not among selected " + hit.collider.name + " dict count is " + gameBoard.tileColliderDict.Count);
                    return;
                }

                if (HighlightedTarget != hexTile)
                {
                    HighlightedTarget = hexTile;
                    //Debug.Log("Tile hit" + hexTile.GridPosition);
                }

            }
            else if (HighlightedTarget != null)
            {
                HighlightedTarget = null;
                //Debug.Log("No tile hit but found collider " + hit.collider.name + " dict count is " + gameBoard.tileColliderDict.Count);
            }
        }
        else if (HighlightedTarget != null)
        {
            HighlightedTarget = null;
            //Debug.Log("No tile hit" + gameBoard.tileColliderDict.Count);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, Vector3.down);
        if(HighlightedTarget != null)
        {
            Gizmos.color = Color.red;
            Vector3 pos = transform.position;
            Gizmos.DrawSphere(pos.With(y: HighlightedTarget.GetTransform().position.y), sphereCastSize);
        }
    }

    public virtual void HandleHighlightLine()
    {
        if (highlightedTarget == null)
        {
            ReleaseCurrentLine();
        }
        else
        {
            //Debug.Log("DRAWING LINE");
            currentRaycastLine = VolumetricLinePool.DrawLine(transform.position.With(y : transform.position.y - gameBoard.tileGameData.VolumetricStartOffset), highlightedTarget.GetTransform().position, Color.cyan, currentRaycastLine);
        }
    }
}
