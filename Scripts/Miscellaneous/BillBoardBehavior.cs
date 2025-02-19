using UnityEngine;

public class BillBoardBehavior : MonoBehaviour
{
    Camera m_Camera;

    private void Start()
    {
        m_Camera = Camera.main;
    }

    public void Update()
    {
        transform.LookAt(m_Camera.transform.position,
            m_Camera.transform.up);

    }
}
