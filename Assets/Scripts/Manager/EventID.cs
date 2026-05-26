using UnityEngine;

public enum EventID
{
    OnGemsMatched,
    OnPlayerTurnStart,
    OnEnemyTurnStart,

    OnShowDamagePopup,
    OnUpdatePlayerStats,
    OnUpdateTurnCount,
    OnUpdateRoundCount,
    OnShowEnemyInfo,
    OnHideEnemyInfo,
    OnUpdateEnemyHP,
    OnPause,
    OnResume,
    OnEnemyTargetSelectionRequired,
    OnEnemyTargetSelected
}

public class MatchEventData
{
    public GemType GemType;
    public int MatchCount;
    public float PowerValue;
}