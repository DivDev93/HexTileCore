using UnityEngine;

public class Transform3DLayoutGroup : MonoBehaviour
{
    public enum LayoutAxis
    {
        Horizontal,
        Vertical,
        Grid
    }

    [Header("Layout Settings")]
    public bool applyLayoutOnValidate = true;
    public LayoutAxis layoutAxis = LayoutAxis.Horizontal;
    public float spacing = 10f;
    public Vector3 padding = Vector3.zero;
    public Vector2 gridCellSize = new Vector2(5f, 5f); // Grid dimensions if using grid layout
    public Vector3 alignment = new Vector3(0.5f, 0.5f, 0.5f); // (0,0,0) is bottom-left, (1,1,1) is top-right
    public bool maintainAspectRatio = true; // Adjust layout if the object's aspect ratio is important

    private void OnValidate()
    {
        if (applyLayoutOnValidate)
        {
            ApplyLayout();
        }
    }

    private void ApplyLayout()
    {
        // Get all child transforms
        Transform[] children = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            children[i] = transform.GetChild(i);
        }

        // Calculate total size and positions
        Vector3 currentPos = new Vector3(padding.x, padding.y, padding.z);
        float totalWidth = 0;
        float totalHeight = 0;

        if (layoutAxis == LayoutAxis.Horizontal)
        {
            // Calculate the total width and arrange objects horizontally
            foreach (var child in children)
            {
                Renderer renderer = child.GetComponent<Renderer>();
                float width = renderer != null ? renderer.bounds.size.x : 0;
                totalWidth += width + spacing;
            }

            currentPos.x -= totalWidth / 2 - spacing / 2; // Centering the layout

            // Apply positions to children
            foreach (var child in children)
            {
                Renderer renderer = child.GetComponent<Renderer>();
                float width = renderer != null ? renderer.bounds.size.x : 0;
                child.localPosition = currentPos;
                currentPos.x += width + spacing;
            }
        }
        else if (layoutAxis == LayoutAxis.Vertical)
        {
            // Calculate the total height and arrange objects vertically
            foreach (var child in children)
            {
                Renderer renderer = child.GetComponent<Renderer>();
                float height = renderer != null ? renderer.bounds.size.y : 0;
                totalHeight += height + spacing;
            }

            currentPos.y -= totalHeight / 2 - spacing / 2; // Centering the layout

            // Apply positions to children
            foreach (var child in children)
            {
                Renderer renderer = child.GetComponent<Renderer>();
                float height = renderer != null ? renderer.bounds.size.y : 0;
                child.localPosition = currentPos;
                currentPos.y += height + spacing;
            }
        }
        else if (layoutAxis == LayoutAxis.Grid)
        {
            // Calculate positions based on grid system
            int columns = Mathf.CeilToInt(Mathf.Sqrt(children.Length)); // Trying to balance columns and rows
            int rows = Mathf.CeilToInt((float)children.Length / columns);

            currentPos.x -= (columns * gridCellSize.x) / 2 - gridCellSize.x / 2;
            currentPos.y -= (rows * gridCellSize.y) / 2 - gridCellSize.y / 2;

            // Apply positions to children
            for (int i = 0; i < children.Length; i++)
            {
                int row = i / columns;
                int column = i % columns;

                Vector3 targetPos = currentPos + new Vector3(column * gridCellSize.x, -row * gridCellSize.y, 0);
                children[i].localPosition = targetPos;
            }
        }
    }
}
