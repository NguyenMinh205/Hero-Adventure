using UnityEngine;

public enum EventID
{
    OnGemsMatched,
    OnPlayerTurnStart,
    OnEnemyTurnStart,
    OnGameOver,
    OnShowDamagePopup,
    OnUpdatePlayerStats,
    OnUpdateTurnCount,
    OnUpdateRoundCount,
    OnShowEnemyInfo,
    OnHideEnemyInfo,
    OnUpdateEnemyHP
}

public class MatchEventData
{
    public GemType GemType;
    public int MatchCount;
    public float PowerValue;
}