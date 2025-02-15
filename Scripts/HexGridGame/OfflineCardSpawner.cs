using Reflex.Attributes;
using System;
using UnityEngine;

public class OfflineCardSpawner : MonoBehaviour, ICardSpawner
{
    [Inject]
    IStaticEvents staticEvents;

    public int playerId = 0;

    //offline version of NetworkObjectDispenser
    public GameObject cardPrefab;
    public GameObject currentInteractable;
    public float distanceToSpawnNew = .5f;
    public Transform spawnTransform;
    public float spawnCooldown = .5f;
    internal float m_SpawnCooldownTimer = 0f;
    public float currentDistance = 0f;
    public bool CheckInteractablePosition()
    {
        if (currentInteractable == null)
            return false;

        currentDistance = Vector3.Distance(currentInteractable.transform.position, spawnTransform.position);
        return currentDistance > distanceToSpawnNew;
    }

    public bool CanSpawn(float deltaTime)
    {
        if (currentInteractable != null) return false;
        if (m_SpawnCooldownTimer > 0)
        {
            UpdateCooldown(m_SpawnCooldownTimer - deltaTime);
            return false;
        }

        UpdateCooldown(spawnCooldown);
        return true;
    }

    void UpdateCooldown(float newTime)
    {
        m_SpawnCooldownTimer = newTime;
    }

    public GameObject SpawnInteractablePrefab(Transform spawnerTransform)
    {
        UpdateCooldown(spawnCooldown);
        currentInteractable = UnityEngine.Object.Instantiate
        (
            cardPrefab,
            spawnerTransform.position,
            spawnerTransform.rotation,
            spawnerTransform
        );
        currentInteractable.transform.localScale = Vector3.one;//spawnerTransform.localScale;

        return currentInteractable;
    }

    public void OnEnable()
    {
        staticEvents.OnTurnEnd += OnTurnEnd;
        SpawnInteractablePrefab(spawnTransform);
    }

    public void OnDisable()
    {
        staticEvents.OnTurnEnd -= OnTurnEnd;
    }

    private void OnTurnEnd(int player)
    {
        if (CheckInteractablePosition())
        {
            currentInteractable = null;
        }

        if (player == playerId && currentInteractable == null)
        {
            SpawnInteractablePrefab(spawnTransform);
        }
    }

    //private void LateUpdate()
    //{
    //    float deltaTime;
    //    deltaTime = Time.deltaTime;

    //    if(CheckInteractablePosition())
    //    {
    //        currentInteractable = null;
    //    }

    //    if (CanSpawn(deltaTime))
    //    {
    //        SpawnInteractablePrefab(spawnTransform);
    //    }
    //}
}
