using UnityEngine;

public enum EventID
{
    OnGemsMatched,
    OnPlayerTurnStart,
    OnEnemyTurnStart,
    OnGameOver,
    OnVictory,
    OnShowDamagePopup,
    OnUpdatePlayerStats,
    OnUpdateTurnCount,
    OnUpdateRoundCount,
    OnShowEnemyInfo,
    OnHideEnemyInfo,
    OnUpdateEnemyHP,
    OnPause,
    OnResume
}

public class MatchEventData
{
    public GemType GemType;
    public int MatchCount;
    public float PowerValue;
}