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

    public float displacement = 0.01f;
    Vector3 defaultScale;
    float sphereCastSize => gameBoard.tileGameData.sphereCastSize;
    public ISelectableTarget placedTarget;
    private ISelectableTarget highlightedTarget;
    int layerMask;// = LayerMask.GetMask("HexTile");
    public VolumetricLineStripBehavior currentRaycastLine = null;
    public ISelectableTarget PlacedTarget
    {
        get { return placedTarget; }
        set
        {
            if (placedTarget != null && placedTarget != value)
                placedTarget.OnSelect -= OnTargetPlace;

            placedTarget = value;

            if (placedTarget != null)
                placedTarget.OnSelect += OnTargetPlace;
        }
    }
    public ISelectableTarget HighlightedTarget
    {
        get { return highlightedTarget; }
        set
        {
            if (highlightedTarget != null && highlightedTarget != value)
            {
                highlightedTarget.OnHoverExit();
            }

            highlightedTarget = value;

            if (highlightedTarget != null)
            {
                highlightedTarget.OnHoverEnter();
            }

            if (highlightedTarget == null)
            {
                ReleaseCurrentLine();
            }
            else
                currentRaycastLine = VolumetricLinePool.DrawLine(transform.position, highlightedTarget.GetTransform().position.With(y: 0f), Color.cyan, currentRaycastLine);
        }
    }
    public bool isPlaced = false;
    public float jumpPower = 0.025f;
    public float placementDuration = 0.5f;
    private Rigidbody rb;
    Vector3 placedLocalPosition = Vector3.zero;
    Quaternion placedLocalRotation = Quaternion.identity;
    public Transform localPlayerTransform;
    public UnityEvent OnTilePlaced = new();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        defaultScale = transform.lossyScale;
        rb = GetComponent<Rigidbody>();
        layerMask = LayerMask.GetMask("HexTile");
    }

    // Update is called once per frame
    public virtual void Update()
    {
        if(!isPlaced)
        {
            RayCastTile();
        }
    }

    void ReleaseCurrentLine()
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
                rb.MovePosition(transform.parent.position + placedLocalPosition);
                rb.MoveRotation(transform.parent.rotation * placedLocalRotation);
            }
        }
    }

    public void OnSelected()
    {
        transform.parent = null;
        //transform.parent = null;
        Sequence sequence = DOTween.Sequence();
        sequence.SetDelay(0.01f);
        sequence.Append(transform.DOScale(defaultScale, gameBoard.tileGameData.selectAnimationDuration).OnComplete(() =>
        {
            Debug.Log("SHOULD HAVE RESET GRABBABLE SCALE");
        }));
        
        sequence.Play();       
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
            PlacedTarget = HighlightedTarget;
        }

        if (PlacedTarget != null)
        {
            PlaceOnTile();
        }
        Debug.Log("UNSELECTED" + gameObject.name);
    }

    public virtual void PlaceOnTile()
    {
        ReleaseCurrentLine();
        //transform.parent = null;
        //var localPlayer = XRINetworkGameManager.Instance.GetLocalPlayer();
        localPlayerTransform = Camera.main.transform;
        placedLocalRotation = Quaternion.LookRotation((localPlayerTransform.position - PlacedTarget.GetTransform().position).With(y:0f), Vector3.up);
        placedLocalPosition = Vector3.up * displacement;
        Sequence sequence = DOTween.Sequence();
        sequence.Append(transform.DOMove(PlacedTarget.GetTransform().position.With(y: 0) + placedLocalPosition, placementDuration))//DOJump(placedTile.transform.position + placedLocalPosition, jumpPower, 1, placementDuration))
            .Join(transform.DORotateQuaternion(placedLocalRotation, placementDuration))
            .Join(transform.DOScale(gameBoard.tileGameData.PlacedCardScale * Vector3.one, placementDuration))
            .OnComplete(() =>
        {
            isPlaced = true;
            //transform.parent = placedTile.transform;
            PlacedTarget.OnHoverExit();
            PlacedTarget.OnSelectionClick();
            //placedTile.transform.DOShakePosition(0.25f, 0.08f);
        });
        
        sequence.Play();
        OnTilePlaced.Invoke();
    }

    public virtual void OnTargetPlace()
    {
        if (PlacedTarget != null && isPlaced)
        {
            PrimeTweenExtensions.PulseY(transform, PlacedTarget.GetTransform().position.With(y: 0f) + placedLocalPosition, jumpPower, 1, gameBoard.tileGameData.pulseData.duration);
        }
    }

    public virtual void RayCastTile()
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        RaycastHit hit;

        if (Physics.SphereCast(ray, sphereCastSize, out hit, 100f, layerMask))
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
                    //Debug.Log("Hovered Tile is not among selected " + hit.collider.name + " dict count is " + HexGridManager.tileColliderDict.Count);
                    return;
                }

                if (HighlightedTarget != hexTile)
                    HighlightedTarget = hexTile;

                //Debug.Log("Tile hit" + hexTile.GridPosition);
            }
            else
            {
                HighlightedTarget = null;
                //Debug.Log("No tile hit but found collider " + hit.collider.name + " dict count is " + HexGridManager.tileColliderDict.Count);
            }
        }
        else
        {
            HighlightedTarget = null;
            //Debug.Log("No tile hit" + HexGridManager.tileColliderDict.Count);
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
}
