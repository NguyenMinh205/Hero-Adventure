using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGameModeStrategy
{
    void Initialize(BattleManager battleManager);
    List<CharacterInfoSO> GetEnemiesToSpawn(List<CharacterInfoSO> availableEnemies);
    IEnumerator OnWaveCleared(BattleManager battleManager);
    bool IsGameOver(Player player);
    float GetDifficultyMultiplier();
}
