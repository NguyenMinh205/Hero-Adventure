using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessModeStrategy : IGameModeStrategy
{
    private int currentRound;
    private float difficultyMultiplier;
    public int CurrentRound => currentRound;

    private const float IncrementPerRound = 0.1f;

    public void Initialize(BattleManager battleManager)
    {
        currentRound = 0;
        difficultyMultiplier = 1.0f;
    }

    public List<CharacterInfoSO> GetEnemiesToSpawn(List<CharacterInfoSO> availableEnemies)
    {
        List<CharacterInfoSO> selectedEnemies = new List<CharacterInfoSO>();
        int enemyCount = Mathf.Min(3, 1 + (currentRound / 3));

        for (int i = 0; i < enemyCount; i++)
        {
            selectedEnemies.Add(availableEnemies[Random.Range(0, availableEnemies.Count)]);
        }

        return selectedEnemies;
    }

    public IEnumerator OnWaveCleared(BattleManager battleManager)
    {
        currentRound++;
        difficultyMultiplier = 1.0f + (currentRound - 1) * IncrementPerRound;

        ObserverManager<EventID>.PostEvent(EventID.OnUpdateRoundCount, currentRound);

        yield return battleManager.StartCoroutine(battleManager.ExploreRoutine());
    }

    public bool IsGameOver(Player player)
    {
        return player.IsDead();
    }

    public float GetDifficultyMultiplier()
    {
        return difficultyMultiplier;
    }
}
