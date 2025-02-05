using UnityEngine;
//using DG.Tweening;
using PrimeTween;
using UnityUtils;
using Reflex.Attributes;

public class TilePlaceable : MonoBehaviour
{
    [Inject]
    IGameBoard gameBoard;

    public float displacement = 0.1f;
    Vector3 defaultScale;
    float sphereCastSize => gameBoard.tileGameData.sphereCastSize;
    public ISelectableTarget placedTile;
    private ISelectableTarget highlightedTile;
    int layerMask;// = LayerMask.GetMask("HexTile");

    public ISelectableTarget PlacedTile
    {
        get { return placedTile; }
        set
        {
            if (placedTile != null && placedTile != value)
                placedTile.OnTilePulse -= OnTilePulse;

            placedTile = value;

            if (placedTile != null)
                placedTile.OnTilePulse += OnTilePulse;
        }
    }
    public ISelectableTarget HighlightedTile
    {
        get { return highlightedTile; }
        set
        {
            if (highlightedTile != null && highlightedTile != value)            
                highlightedTile.OnHoverExit();
            
            highlightedTile = value;

            if(highlightedTile != null)
                highlightedTile.OnHoverEnter();
        }
    }
    public bool isPlaced = false;
    public float jumpPower = 0.5f;
    public float placementDuration = 0.5f;
    private Rigidbody rb;
    Vector3 placedLocalPosition = Vector3.zero;
    Quaternion placedLocalRotation = Quaternion.identity;
    public Transform localPlayerTransform;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        defaultScale = transform.localScale;
        rb = GetComponent<Rigidbody>();
        layerMask = LayerMask.GetMask("HexTile");
    }

    // Update is called once per frame
    void Update()
    {
        if(!isPlaced)
        {
            RayCastTile();
        }
    }

    private void FixedUpdate()
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
        //transform.parent = null;
        Sequence sequence = DOTween.Sequence();
        sequence.SetDelay(0.01f);
        sequence.Append(transform.DOScale(defaultScale, 0.25f).OnComplete(() =>
        {
            Debug.Log("SHOULD HAVE RESET GRABBABLE SCALE");
        }));
        
        sequence.Play();

        isPlaced = false;
        if (PlacedTile != null)
        {
            PlacedTile.OnTileClick();
        }
    }

    public void OnSelectExit()
    {
        if (HighlightedTile != null)
        {
            PlacedTile = HighlightedTile;
        }

        if (PlacedTile != null)
        {
            PlaceOnTile();
        }
        Debug.Log("UNSELECTED" + gameObject.name);
    }

    void PlaceOnTile()
    {
        //transform.parent = null;
        //var localPlayer = XRINetworkGameManager.Instance.GetLocalPlayer();
        localPlayerTransform = Camera.main.transform;
        placedLocalRotation = Quaternion.LookRotation((localPlayerTransform.position - PlacedTile.GetTransform().position).With(y:0f), Vector3.up);
        placedLocalPosition = Vector3.up * displacement;
        Sequence sequence = DOTween.Sequence();
        sequence.Append(transform.DOMove(PlacedTile.GetTransform().position.With(y: 0) + placedLocalPosition, placementDuration))//DOJump(placedTile.transform.position + placedLocalPosition, jumpPower, 1, placementDuration))
            .Join(transform.DORotateQuaternion(placedLocalRotation, placementDuration))
            .Join(transform.DOScale(gameBoard.tileGameData.PlacedCardScale * Vector3.one, placementDuration))
            .OnComplete(() =>
        {
            isPlaced = true;
            //transform.parent = placedTile.transform;
            PlacedTile.OnHoverExit();
            PlacedTile.OnTileClick();
            //placedTile.transform.DOShakePosition(0.25f, 0.08f);
        });

        sequence.Play();
    }

    public void OnTilePulse()
    {
        if (PlacedTile != null && isPlaced)
        {
            PrimeTweenExtensions.PulseY(transform, PlacedTile.GetTransform().position.With(y: 0f) + placedLocalPosition, jumpPower, 1, gameBoard.tileGameData.pulseData.duration);
        }
    }

    void RayCastTile()
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        RaycastHit hit;
        if (Physics.SphereCast(ray, sphereCastSize, out hit, 100f, layerMask))
        {
            HexTile hexTile = null;
            if (HexGridManager.tileColliderDict.TryGetValue(hit.collider, out hexTile))
            {                
                //if (highlightedTile != null && hexTile != highlightedTile)
                //{
                //    Debug.Log("Should clear highlight");
                //    highlightedTile.OnHoverExit();
                //}

                if (!gameBoard.selectedTiles.Contains(hexTile as IBoardPosition))
                {
                    HighlightedTile = null;
                    //Debug.Log("Hovered Tile is not among selected " + hit.collider.name + " dict count is " + HexGridManager.tileColliderDict.Count);
                    return;
                }

                HighlightedTile = hexTile;
                //Debug.Log("Tile hit" + hexTile.GridPosition);
            }
            else
            {
                HighlightedTile = null;
                //Debug.Log("No tile hit but found collider " + hit.collider.name + " dict count is " + HexGridManager.tileColliderDict.Count);
            }
        }
        else
        {
            HighlightedTile = null;
            //Debug.Log("No tile hit" + HexGridManager.tileColliderDict.Count);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, Vector3.down);
        if(HighlightedTile != null)
        {
            Gizmos.color = Color.red;
            Vector3 pos = transform.position;
            Gizmos.DrawSphere(pos.With(y: HighlightedTile.GetTransform().position.y), sphereCastSize);
        }
    }
}
