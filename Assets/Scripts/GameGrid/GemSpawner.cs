using System.Collections.Generic;
using UnityEngine;

public class GemSpawner : MonoBehaviour
{
    [SerializeField] private Gem gemPrefab;
    [SerializeField] private List<GemData> gemDataList = new List<GemData>();

    public Gem SpawnGem(Vector2 position, Transform parent)
    {
        Gem newGem = PoolingManager.Spawn<Gem>(gemPrefab, position, Quaternion.identity, parent);
        return newGem;
    }

    public void DespawnGem(Gem gem)
    {
        PoolingManager.Despawn(gem.gameObject);
    }

    public GemData GetRandomGemData()
    {
        return gemDataList[Random.Range(0, gemDataList.Count)];
    }
}