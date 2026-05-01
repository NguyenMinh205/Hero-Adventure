using UnityEngine;

public enum EventID
{
    OnGemsMatched,
    OnPlayerTurnStart,
    OnEnemyTurnStart,
    OnGameOver
}

public class MatchEventData
{
    public GemType GemType;
    public int MatchCount;
}