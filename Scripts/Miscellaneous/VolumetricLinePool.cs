using UnityEngine;
using UnityEngine.Pool;
using UnityUtils;
using VolumetricLines;

public class VolumetricLinePool 
{
    [SerializeField]
    VolumetricLineStripBehavior stripBehaviorPrefab;

    public static ObjectPool<VolumetricLineStripBehavior> lineStripPool;

    public static VolumetricLineStripBehavior DrawLine(Vector3 start, Vector3 end, Color color, VolumetricLineStripBehavior cachedLine = null, float duration = 0f)
    {
        var line = cachedLine == null ? VolumetricLinePool.lineStripPool.Get() : cachedLine;
        Vector3[] linePositions = { start, end.With(y : end.y + (start.y - end.y)/2f),  end };
        line.UpdateLineVertices(linePositions);
        line.LineColor = color;

        return line;
        //LineSegment seg = new LineSegment
        //{
        //    start = start,
        //    end = end,
        //    color = color,
        //    expireTime = (duration > 0f) ? Time.time + duration : 0f
        //};
        //_lines.Add(seg);
    }

    public static VolumetricLineStripBehavior DrawLine(Vector3[] linePositions, Color color, VolumetricLineStripBehavior cachedLine = null)
    {
        var line = cachedLine == null ? VolumetricLinePool.lineStripPool.Get() : cachedLine;
        line.UpdateLineVertices(linePositions);
        line.LineColor = color;
        return line;
    }

    public void Initialize()
    {
        stripBehaviorPrefab = Resources.Load<VolumetricLineStripBehavior>("LineStripLightSaber");
        InitPool();
    }

    void InitPool()
    {
        lineStripPool = new ObjectPool<VolumetricLineStripBehavior>(CreatePooledObject,
            OnGetFromPool,
            ReturnToPool,
            OnDestroyPooledCard,
            maxSize: 4);
    }

    public VolumetricLineStripBehavior CreatePooledObject()
    {
        return UnityEngine.Object.Instantiate(stripBehaviorPrefab);
    }

    public void OnGetFromPool(VolumetricLineStripBehavior card)
    {
        card.gameObject.SetActive(true);
    }
    public void ReturnToPool(VolumetricLineStripBehavior card)
    {
        card.gameObject.SetActive(false);
    }

    public void OnDestroyPooledCard(VolumetricLineStripBehavior card)
    {
        UnityEngine.Object.Destroy(card.gameObject);
    }

    public void ReleaseCardData(VolumetricLineStripBehavior cardData)
    {
        lineStripPool.Release(cardData);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
