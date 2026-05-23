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
        int totalWeight = 0;
        foreach (GemData data in gemDataList)
        {
            totalWeight += data.spawnWeight;
        }

        int roll = Random.Range(0, totalWeight);
        int cumulative = 0;

        foreach (GemData data in gemDataList)
        {
            cumulative += data.spawnWeight;
            if (roll < cumulative)
            {
                return data;
            }
        }

        return gemDataList[gemDataList.Count - 1];
    }
}