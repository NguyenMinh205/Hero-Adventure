using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelModeStrategy : IGameModeStrategy
{
    private int currentWave;
    private int maxWaves;
    private LevelConfig config;

    private float levelDifficultyMultiplier;
    private float waveScalingIncrement;

    public void Initialize(BattleManager battleManager)
    {
        currentWave = 0;
        config = GameModeManager.Instance.CurrentLevelConfig;

        if (config != null)
        {
            maxWaves = config.MaxWaves;
            levelDifficultyMultiplier = config.levelDifficultyMultiplier;
            waveScalingIncrement = config.waveScalingIncrement;
        }
        else
        {
            maxWaves = 3;
            levelDifficultyMultiplier = 1f;
            waveScalingIncrement = 0.05f;
        }
    }

    public List<CharacterInfoSO> GetEnemiesToSpawn(List<CharacterInfoSO> availableEnemies)
    {
        List<CharacterInfoSO> pool = (config != null && config.PossibleEnemies.Count > 0) ? config.PossibleEnemies : availableEnemies;

        List<CharacterInfoSO> selectedEnemies = new List<CharacterInfoSO>();
        
        int safeMaxWaves = Mathf.Max(1, maxWaves);
        float progress = Mathf.Clamp01((float)currentWave / safeMaxWaves);
        int targetEnemyCount = Mathf.Clamp(Mathf.CeilToInt(progress * 3f), 1, 3);
        
        bool isBossWave = (config != null && config.IsBossLevel && currentWave == safeMaxWaves);

        if (isBossWave)
        {
            int minionCount = Mathf.Max(0, targetEnemyCount - 1);
            for (int i = 0; i < minionCount; i++)
            {
                selectedEnemies.Add(pool[Random.Range(0, pool.Count)]);
            }
            
            if (config.BossEnemy != null)
            {
                selectedEnemies.Add(config.BossEnemy);
            }
            else
            {
                selectedEnemies.Add(pool[pool.Count - 1]); // fallback
            }
        }
        else
        {
            for (int i = 0; i < targetEnemyCount; i++)
            {
                selectedEnemies.Add(pool[Random.Range(0, pool.Count)]);
            }
        }

        return selectedEnemies;
    }

    public IEnumerator OnWaveCleared(BattleManager battleManager)
    {
        if (currentWave >= maxWaves)
        {
            battleManager.SetGameState(GameState.Finished);
            battleManager.GameplayUIManager?.ShowVictory(battleManager.Player);
            yield break;
        }

        currentWave++;
        ObserverManager<EventID>.PostEvent(EventID.OnUpdateRoundCount, currentWave);

        yield return battleManager.StartCoroutine(battleManager.ExploreRoutine());
    }

    public bool IsGameOver(Player player)
    {
        return player.IsDead();
    }

    public float GetDifficultyMultiplier()
    {
        int completedWaves = Mathf.Max(0, currentWave - 1);
        return levelDifficultyMultiplier + completedWaves * waveScalingIncrement;
    }
}
