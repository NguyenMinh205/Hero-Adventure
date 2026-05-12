using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessModeStrategy : IGameModeStrategy
{
    private int currentRound;
    private float difficultyMultiplier;

    public void Initialize(BattleManager battleManager)
    {
        currentRound = 0;
        difficultyMultiplier = 0.9f;
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
        difficultyMultiplier += 0.1f;
        
        ObserverManager<EventID>.PostEvent(EventID.OnUpdateRoundCount, currentRound);
        
        yield return battleManager.StartCoroutine(battleManager.ExploreRoutine());
    }

    public bool IsGameOver(Player player)
    {
        return player.IsDead();
    }

    public string GetProgressText()
    {
        return $"Round {currentRound}";
    }

    public float GetDifficultyMultiplier()
    {
        return difficultyMultiplier;
    }
}
