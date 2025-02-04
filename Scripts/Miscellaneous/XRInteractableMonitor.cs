using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// XRInteractableMonitor tracks interactables currently held by interactors (hands and controllers)
/// and exposes this information for easy access by other scripts.
/// </summary>
public class XRInteractableMonitor : MonoBehaviour
{
    public Shader shader;
    public GameObject diffusionGeneratorPrefab;
    public StableDiffusionGenerator currentActiveGenerator;

    // Singleton instance for global access
    public static XRInteractableMonitor Instance { get; private set; }

    /// <summary>
    /// Dictionary mapping each interactor to its currently held interactable.
    /// </summary>
    private Dictionary<UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor, UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable> interactorHeldInteractables = new Dictionary<UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor, UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();

    /// <summary>
    /// Event triggered when an interactor starts holding an interactable.
    /// </summary>
    public event System.Action<UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor, UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable> OnInteractorGrab;

    /// <summary>
    /// Event triggered when an interactor releases an interactable.
    /// </summary>
    public event System.Action<UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor, UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable> OnInteractorRelease;

    /// <summary>
    /// List of all interactors to monitor. Assign via the Inspector or dynamically.
    /// </summary>
    [Tooltip("Assign all XR Interactors (hands/controllers) here.")]
    public List<UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor> interactors = new List<UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor>();

    private void Awake()
    {
        // Implement singleton pattern
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple instances of XRInteractableMonitor detected. Destroying duplicate.");
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        // If interactors list is empty, attempt to find all XRBaseInteractor in the scene
        if (interactors == null || interactors.Count == 0)
        {
            UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor[] foundInteractors = FindObjectsByType<UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor>(FindObjectsSortMode.None);
            interactors.AddRange(foundInteractors);
            Debug.Log($"XRInteractableMonitor: Found and added {foundInteractors.Length} interactors.");
        }

        // Subscribe to events for each interactor
        foreach (var interactor in interactors)
        {
            if (interactor != null)
            {
                interactor.selectEntered.AddListener(OnSelectEntered);
                interactor.selectExited.AddListener(OnSelectExited);
            }
            else
            {
                Debug.LogWarning("XRInteractableMonitor: An interactor in the list is null.");
            }
        }
    }

    private void OnDisable()
    {
        // Unsubscribe from events to prevent memory leaks
        foreach (var interactor in interactors)
        {
            if (interactor != null)
            {
                interactor.selectEntered.RemoveListener(OnSelectEntered);
                interactor.selectExited.RemoveListener(OnSelectExited);
            }
        }
    }

    /// <summary>
    /// Event handler for when an interactor selects (grabs) an interactable.
    /// </summary>
    /// <param name="args">SelectEnterEventArgs containing interaction details.</param>
    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor interactor = args.interactorObject as UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor;
        UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable = args.interactableObject as UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable;

        if (interactor != null && interactable != null)
        {
            // Add or update the dictionary
            interactorHeldInteractables[interactor] = interactable;
            currentActiveGenerator = interactable.GetComponentInChildren<StableDiffusionGenerator>();

            // Trigger the OnInteractorGrab event
            OnInteractorGrab?.Invoke(interactor, interactable);

            Debug.Log($"{GetInteractorName(interactor)} grabbed {interactable.name}");
        }
    }

    /// <summary>
    /// Event handler for when an interactor deselects (releases) an interactable.
    /// </summary>
    /// <param name="args">SelectExitEventArgs containing interaction details.</param>
    private void OnSelectExited(SelectExitEventArgs args)
    {
        UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor interactor = args.interactorObject as UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor;
        UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable = args.interactableObject as UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable;

        if (interactor != null && interactable != null)
        {
            // Remove from the dictionary if it exists
            if (interactorHeldInteractables.ContainsKey(interactor))
            {
                interactorHeldInteractables.Remove(interactor);

                // Trigger the OnInteractorRelease event
                OnInteractorRelease?.Invoke(interactor, interactable);

                Debug.Log($"{GetInteractorName(interactor)} released {interactable.name}");
            }
        }
    }

    /// <summary>
    /// Retrieves the name/type of the interactor for logging purposes.
    /// </summary>
    /// <param name="interactor">The XRBaseInteractor instance.</param>
    /// <returns>A string representing the interactor.</returns>
    private string GetInteractorName(UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor interactor)
    {
        // Customize this method based on how you identify your interactors (tags, names, etc.)
        //if (interactor.gameObject.CompareTag("LeftHand"))
        //    return "Left Hand";
        //if (interactor.gameObject.CompareTag("RightHand"))
        //    return "Right Hand";
        //if (interactor.gameObject.CompareTag("LeftController"))
        //    return "Left Controller";
        //if (interactor.gameObject.CompareTag("RightController"))
        //    return "Right Controller";

        // Fallback to GameObject name
        return interactor.gameObject.name;
    }

    /// <summary>
    /// Checks if any interactor is currently holding an interactable.
    /// </summary>
    /// <returns>True if at least one interactor is holding an interactable, otherwise false.</returns>
    public bool IsAnyInteractorHolding()
    {
        return interactorHeldInteractables.Count > 0;
    }

    /// <summary>
    /// Retrieves the interactable currently held by a specific interactor.
    /// </summary>
    /// <param name="interactor">The XRBaseInteractor to query.</param>
    /// <returns>The held XRBaseInteractable, or null if none.</returns>
    public UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable GetHeldInteractable(UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor interactor)
    {
        if (interactorHeldInteractables.TryGetValue(interactor, out UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable))
        {
            return interactable;
        }
        return null;
    }

    /// <summary>
    /// Retrieves all interactables currently being held.
    /// </summary>
    /// <returns>A list of XRBaseInteractable currently held.</returns>
    public List<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable> GetAllHeldInteractables()
    {
        return new List<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>(interactorHeldInteractables.Values);
    }

    /// <summary>
    /// Retrieves a dictionary mapping interactors to their held interactables.
    /// </summary>
    /// <returns>A dictionary of XRBaseInteractor to XRBaseInteractable.</returns>
    public Dictionary<UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor, UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable> GetAllInteractorHeldInteractables()
    {
        return new Dictionary<UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor, UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>(interactorHeldInteractables);
    }

    /// <summary>
    /// Retrieves all interactors that are currently holding an interactable.
    /// </summary>
    /// <returns>A list of XRBaseInteractors holding interactables.</returns>
    public List<UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor> GetAllActiveInteractors()
    {
        return new List<UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor>(interactorHeldInteractables.Keys);
    }

    /// <summary>
    /// Retrieves the interactable currently held by the left hand, if any.
    /// </summary>
    public UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable LeftHandHeldInteractable
    {
        get
        {
            foreach (var kvp in interactorHeldInteractables)
            {
                if (kvp.Key.gameObject.CompareTag("LeftHand") || kvp.Key.gameObject.CompareTag("LeftController"))
                    return kvp.Value;
            }
            return null;
        }
    }

    /// <summary>
    /// Retrieves the interactable currently held by the right hand, if any.
    /// </summary>
    public UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable RightHandHeldInteractable
    {
        get
        {
            foreach (var kvp in interactorHeldInteractables)
            {
                if (kvp.Key.gameObject.CompareTag("RightHand") || kvp.Key.gameObject.CompareTag("RightController"))
                    return kvp.Value;
            }
            return null;
        }
    }

    //public void SetCurrentGenerator()
    //{
    //    //check if the current interactable has a generator child object
    //    if (LeftHandHeldInteractable != null)
    //    {
    //        currentActiveGenerator = LeftHandHeldInteractable.GetComponentInChildren<StableDiffusionGenerator>();
    //    }
    //    else if (RightHandHeldInteractable != null)
    //    {
    //        currentActiveGenerator = RightHandHeldInteractable.GetComponentInChildren<StableDiffusionGenerator>();
    //    }
    //}

    public void GenerateCurrentImage()
    {
        if (currentActiveGenerator != null)
        {
            currentActiveGenerator.StartImageGeneration();
        }
    }

    public void GenerateCurrentImageToModel()
    {
        if (currentActiveGenerator != null)
        {
            currentActiveGenerator.CreateModelFromCurrentTexture();
        }
    }

    public void InstantiateNewGenerator()
    {
        if (diffusionGeneratorPrefab != null)
        {
            Vector3 spawnPos = transform.position + (transform.forward) + transform.up;
            Vector3 camToSpawnDir = spawnPos - Camera.main.transform.position;
            Quaternion rotation = Quaternion.LookRotation(camToSpawnDir, Vector3.up);
            Instantiate(diffusionGeneratorPrefab, spawnPos, Quaternion.identity);
        }
    }
}
