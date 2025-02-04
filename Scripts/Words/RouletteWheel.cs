using GLTFast;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Pool;
using UnityEngine.UIElements;

[System.Serializable]
public class RouletteLabel
{
    public RectTransform root;
    public TMPro.TextMeshProUGUI label;
    public float angle
    {
        set
        {
            root.localRotation = Quaternion.Euler(0, 0, value);

        }
        get
        {
            return root.localRotation.eulerAngles.z;
        }
    }
}

public class RouletteWheel : MonoBehaviour, IPointerClickHandler
{
    public Rigidbody rb;
    public float force = 100f;

    [Header("Roulette Options")]
    public int numberOfOptions = 8;
    public float spacing
    {
        get { return 360f / numberOfOptions; }
    }

    public int selectedIndex = -1;
    public float spinVelocityThreshold = 0.05f;
    public Transform SpinSelector;
    public RectTransform labelPrefab;
    public List<RouletteLabel> labelList = new();
    public ObjectPool<RouletteLabel> labelPool;
    public bool generateTestLabels = false;

    public Action<int> OnRouletteWheelStopped;

    private void OnValidate()
    {
        //check if engine is playing
        if (generateTestLabels && Application.isPlaying)
            GenerateTestLabels();
    }
    public RouletteLabel CreateRouletteLabel()
    {
        RectTransform root = Instantiate(labelPrefab, labelPrefab.transform.parent);
        root.localRotation = Quaternion.Euler(0, 0, 0);
        return new RouletteLabel
        {
            root = root,
            label = root.GetComponentInChildren<TMPro.TextMeshProUGUI>(),
            angle = 0
        };
    }

    public void OnGetLabel(RouletteLabel label)
    {
        label.root.gameObject.SetActive(true);
    }

    public void OnReturnLabel(RouletteLabel label)
    {
        label.root.gameObject.SetActive(false);
    }

    public void OnDestroyLabel(RouletteLabel label)
    {
        Destroy(label.root.gameObject);
    }

    public RouletteLabel GetRouletteLabel(string text, float angle)
    {
        var rouletteLabel = labelPool.Get();
        rouletteLabel.angle = angle;
        rouletteLabel.label.text = text;

        return rouletteLabel;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        labelPool = new ObjectPool<RouletteLabel>(CreateRouletteLabel, OnGetLabel, OnReturnLabel, OnDestroyLabel, maxSize: 20);
        GenerateTestLabels();
    }

    void CheckSelected()
    {
        Vector3 centerToSelector = (SpinSelector.position - transform.position).normalized;
        float minAngle = 360;
        for (int i = 0; i < labelList.Count; i++)
        {
            var label = labelList[i];
            if (Vector3.Dot(labelList[i].root.up, centerToSelector) > 0)
            {
                float angleToCenter = Vector3.Angle(labelList[i].root.up, centerToSelector);
                if (selectedIndex != i && angleToCenter < minAngle)
                {
                    minAngle = angleToCenter;
                    if(selectedIndex >= 0)
                    {
                        labelList[selectedIndex].label.color = Color.white;                        
                    }
                    selectedIndex = i;
                    labelList[selectedIndex].label.color = Color.red;
                }
            }
            //else
            //{
            //    label.label.color = Color.white;
            //}
        }

        //if (selectedIndex >= 0)
        //    labelList[selectedIndex].label.color = Color.red;
    }

    void GenerateTestLabels()
    {
        labelList.ForEach(x => labelPool.Release(x));
        labelList.Clear();

        var localSpacing = spacing;
        for (int i = 0; i < numberOfOptions; i++)
        {
            var label = GetRouletteLabel("Option " + i, i * localSpacing);
            labelList.Add(label);
        }
    }

    bool isSpinning = false;
    bool previouslySpinning = false;
    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Debug.Log("Space key was pressed");
            SpinWheel();
        }
    //}

    //private void FixedUpdate()
    //{
        if (rb.angularVelocity.magnitude > spinVelocityThreshold)
        {
            isSpinning = true;
            CheckSelected();
        }
        else
        {
            rb.angularVelocity = Vector3.zero;
            isSpinning = false;
        }

        if (previouslySpinning && !isSpinning)
        {
            OnRouletteWheelStopped?.Invoke(selectedIndex);
            Debug.Log("Selected Index: " + selectedIndex);
        }

        previouslySpinning = isSpinning;
    }

    public void SpinWheel()
    {
        if(isSpinning)
        {
            return;
        }

        rb.AddRelativeTorque(Vector3.forward * UnityEngine.Random.Range(force * 0.5f, force * 1.5f));
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        SpinWheel();
    }
}
