using UnityEngine;

public class GameModeManager : Singleton<GameModeManager>
{
    [Header("Current Mode")]
    [SerializeField] private GameModeType currentMode = GameModeType.Level;
    public GameModeType CurrentMode => currentMode;

    [Header("Level Progress")]
    [SerializeField] private LevelConfig currentLevelConfig;
    public LevelConfig CurrentLevelConfig => currentLevelConfig;

    protected override void Awake()
    {
        base.Awake();
    }

    public void SetGameMode(GameModeType mode)
    {
        currentMode = mode;
    }

    public void SetLevelConfig(LevelConfig config)
    {
        currentLevelConfig = config;
    }
}
