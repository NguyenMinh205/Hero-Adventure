using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Level Config", menuName = "Game Data/Level Config")]
public class LevelConfig : ScriptableObject
{
    [Header("Level Settings")]
    public int LevelID;
    public bool IsBossLevel;
    public int MaxWaves = 3;

    [Header("Difficulty Scaling")]
    public float levelDifficultyMultiplier = 1f;
    public float waveScalingIncrement = 0.05f;

    [Header("Enemy Encounters")]
    public List<CharacterInfoSO> PossibleEnemies;
    
    [Header("Rewards")]
    public int GoldReward;
    public int DiamondReward;
    public int ExpReward;
}