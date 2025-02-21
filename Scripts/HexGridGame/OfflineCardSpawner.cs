using Cysharp.Threading.Tasks;
using Reflex.Attributes;
using System;
using UnityEngine;

public class OfflineCardSpawner : MonoBehaviour, ICardSpawner
{
    [Inject]
    IStaticEvents staticEvents;

    [Inject]
    IGameManager gameManager;

    [Inject]
    IGameBoard gameBoard;

    public int playerIndex => gameManager.CurrentPlayerTurn;

    //offline version of NetworkObjectDispenser
    public GameObject cardPrefab;
    public GameObject currentInteractable;
    public float distanceToSpawnNew = .5f;
    public Transform[] spawnTransforms;
    public Transform spawnTransform => spawnTransforms[playerIndex];

    IPlayerCard m_card;
    public IPlayerCard CurrentSpawnedCard => currentInteractable != null? m_card : null;

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
        staticEvents.OnTurnStart += OnTurnStart;
    }

    public void OnDisable()
    {
        staticEvents.OnTurnStart -= OnTurnStart;
    }

    private async void OnTurnStart()
    {
        if (CheckInteractablePosition())
        {
            currentInteractable = null;
        }

        while(!gameBoard.boardPositions.isBoardCreated)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
        }

        //check if player turn is current player == playerId
        if (currentInteractable == null)//gameManager.CurrentPlayerTurn == playerIndex && 
        { 
            PlayableCard card = SpawnInteractablePrefab(spawnTransform).GetComponent<PlayableCard>();
            gameManager.Players[playerIndex].AddCard(card);
            m_card = card;
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
