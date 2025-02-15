using UnityEngine;

public interface ICardSpawner
{
    GameObject SpawnInteractablePrefab(Transform spawnerTransform);
    //bool CanSpawn(float deltaTime);
    //bool CheckInteractablePosition();
}
