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
        // Giữ Manager không bị destroy khi chuyển scene nếu cần
        // DontDestroyOnLoad(gameObject);
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
