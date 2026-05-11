using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Level Config", menuName = "Game Data/Level Config")]
public class LevelConfig : ScriptableObject
{
    [Header("Level Settings")]
    public int LevelID;
    public int MaxWaves = 3;

    [Header("Enemy Encounters")]
    public List<CharacterInfoSO> PossibleEnemies;
    
    [Header("Rewards")]
    public int GoldReward;
    public int DiamondReward;
    public int ExpReward;
}