using UnityEngine;

public interface ICardSpawner
{
    IPlayerCard CurrentSpawnedCard { get; }
    GameObject SpawnInteractablePrefab(Transform spawnerTransform);
    //bool CanSpawn(float deltaTime);
    //bool CheckInteractablePosition();
}
