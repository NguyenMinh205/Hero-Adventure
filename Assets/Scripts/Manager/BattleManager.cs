using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleManager : Singleton<BattleManager>
{
    [Header("State")]
    [SerializeField] private GameState currentState;
    public GameState CurrentState => currentState;
    private GameState previousState;

    [SerializeField] private int maxActionPoints = 5;
    [SerializeField] private int currentActionPoints;

    [Header("Level Progress")]
    [SerializeField] private BackgroundScroller bgScroller;
    private IGameModeStrategy currentStrategy;
    public IGameModeStrategy CurrentStrategy => currentStrategy;

    [Header("References")]
    [SerializeField] private GameplayUIManager gameplayUIManager;
    public GameplayUIManager GameplayUIManager => gameplayUIManager;
    [SerializeField] private GameGrid gameGrid;
    [SerializeField] private Player player;
    public Player Player => player;
    [SerializeField] private List<Enemy> activeEnemies = new List<Enemy>();
    [SerializeField] private float multiplierPerGem = 0.25f;
    [SerializeField] private EnemySelectionManager enemySelectionManager;

    [Header("Enemy Spawner Settings")]
    [SerializeField] private Enemy enemyPrefab;
    [SerializeField] private List<CharacterInfoSO> listEnemySO;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private Transform characterParent;

    protected override void Awake()
    {
        base.Awake();
    }

    private void OnEnable()
    {
        ObserverManager<EventID>.AddRegisterEvent(EventID.OnGemsMatched, HandleGemsMatched);
        ObserverManager<EventID>.AddRegisterEvent(EventID.OnPause, HandlePause);
        ObserverManager<EventID>.AddRegisterEvent(EventID.OnResume, HandleResume);
    }

    private void OnDisable()
    {
        ObserverManager<EventID>.RemoveAddListener(EventID.OnGemsMatched, HandleGemsMatched);
        ObserverManager<EventID>.RemoveAddListener(EventID.OnPause, HandlePause);
        ObserverManager<EventID>.RemoveAddListener(EventID.OnResume, HandleResume);
    }

    public void InitBattle()
    {
        CleanupBattle();
        if (gameplayUIManager != null)
        {
            gameplayUIManager.Init();
        }
        else
        {
            gameplayUIManager = FindObjectOfType<GameplayUIManager>();
            if (gameplayUIManager != null) gameplayUIManager.Init();
        }

        if (gameGrid != null)
        {
            gameGrid.Init();
        }
        else
        {
            gameGrid = FindObjectOfType<GameGrid>();
            if (gameGrid != null) gameGrid.Init();
        }

        if (player == null)
        {
            player = FindObjectOfType<Player>();
            if (player == null) return;
        }

        player.InitStat();

        Debug.Log("Initializing BattleManager with GameMode: " + (GameModeManager.Instance != null ? GameModeManager.Instance.CurrentMode.ToString() : "None"));

        if (GameModeManager.Instance != null)
        {
            if (GameModeManager.Instance.CurrentMode == GameModeType.Level)
            {
                Debug.Log("Selected Game Mode: Level");
                currentStrategy = new LevelModeStrategy();
            }
            else
            {
                Debug.Log("Selected Game Mode: Endless");
                currentStrategy = new EndlessModeStrategy();
            }
        }
        else
        {
            currentStrategy = new LevelModeStrategy();
        }

        Debug.Log("Initialized Strategy: " + currentStrategy.GetType().Name);
        currentStrategy.Initialize(this);
        StartCoroutine(currentStrategy.OnWaveCleared(this));
    }

    public IEnumerator ExploreRoutine()
    {
        currentState = GameState.Running;

        if (AudioManager.Instance != null) AudioManager.Instance.PlayMusicExplore();

        player.SetRunningAnimation(true);
        if (AudioManager.Instance != null) AudioManager.Instance.PlayPlayerFootstep(true);

        if (bgScroller != null) bgScroller.StartScrolling();

        float waitTime = Random.Range(3f, 5f);
        yield return new WaitForSeconds(waitTime);

        player.SetRunningAnimation(false);
        if (AudioManager.Instance != null) AudioManager.Instance.PlayPlayerFootstep(false);

        if (bgScroller != null) bgScroller.StopScrolling();

        SpawnEnemies();
        StartPlayerTurn();
    }

    private void SpawnEnemies()
    {
        bool isBoss = GameModeManager.Instance != null &&
                      GameModeManager.Instance.CurrentMode == GameModeType.Level &&
                      GameModeManager.Instance.CurrentLevelConfig != null &&
                      GameModeManager.Instance.CurrentLevelConfig.IsBossLevel;

        if (isBoss)
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayMusicBattleBoss();
        }
        else
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayMusicBattleNormal();
        }

        List<CharacterInfoSO> enemiesToSpawn = currentStrategy.GetEnemiesToSpawn(listEnemySO);

        if (enemiesToSpawn == null || enemiesToSpawn.Count == 0)
        {
            Debug.LogWarning("No enemies to spawn from strategy.");
            return;
        }

        float difficultyMultiplier = currentStrategy.GetDifficultyMultiplier();
        Debug.Log($"[BattleManager] Spawning {enemiesToSpawn.Count} enemies with difficulty multiplier: {difficultyMultiplier:F2}");

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("[BattleManager] spawnPoints chưa được gán!");
            return;
        }

        if (enemiesToSpawn.Count == 1)
        {
            Enemy newEnemy = PoolingManager.Spawn(enemyPrefab, spawnPoints[spawnPoints.Length - 1].position, Quaternion.identity, characterParent);
            newEnemy.InitStat(enemiesToSpawn[0]);
            newEnemy.ApplyDifficultyMultiplier(difficultyMultiplier);
            activeEnemies.Add(newEnemy);
        }
        else
        {
            for (int i = 0; i < enemiesToSpawn.Count; i++)
            {
                if (i >= spawnPoints.Length) break;

                Enemy newEnemy = PoolingManager.Spawn(enemyPrefab, spawnPoints[i].position, Quaternion.identity);
                newEnemy.InitStat(enemiesToSpawn[i]);
                newEnemy.ApplyDifficultyMultiplier(difficultyMultiplier);
                activeEnemies.Add(newEnemy);
            }
        }

        if (AudioManager.Instance != null) AudioManager.Instance.PlayRoundStart();
    }

    private void StartPlayerTurn()
    {
        currentState = GameState.PlayerTurn;
        currentActionPoints = maxActionPoints;
        ObserverManager<EventID>.PostEvent(EventID.OnPlayerTurnStart);
        gameplayUIManager?.UpdateTurnCount(currentActionPoints);
    }

    private void HandleGemsMatched(object param)
    {
        if (currentState != GameState.PlayerTurn || player.IsDead()) return;

        if (param is MatchEventData data)
        {
            StartCoroutine(ProcessMatchRoutine(data));
        }
    }

    private IEnumerator ProcessMatchRoutine(MatchEventData data)
    {
        currentState = GameState.Matching;

        float multiplier = 1f + (data.MatchCount - 3) * multiplierPerGem;
        float totalPower = data.PowerValue * multiplier;

        switch (data.GemType)
        {
            case GemType.Damage:
                currentState = GameState.SelectingTarget;

                Enemy selectedTarget = null;

                if (enemySelectionManager != null)
                {
                    yield return StartCoroutine(
                        enemySelectionManager.SelectTarget(activeEnemies, (t) => selectedTarget = t)
                    );
                }
                else
                {
                    selectedTarget = activeEnemies.Find(e => !e.IsDead());
                }

                currentState = GameState.Matching;

                if (selectedTarget != null)
                {
                    gameplayUIManager?.ShowEnemyInfo(selectedTarget);
                    yield return StartCoroutine(player.PerformAttackSequence(selectedTarget, totalPower));
                    yield return new WaitForSeconds(0.5f);
                    selectedTarget.HideTargetIndicator();
                    gameplayUIManager?.HideEnemyInfo();
                }
                break;

            case GemType.Health:
                player.Heal(totalPower);
                break;
            case GemType.Shield:
                player.AddShield(totalPower);
                break;
            case GemType.CritRate:
                player.AddCritRate(totalPower);
                break;
            case GemType.CritDamage:
                player.AddCritDamage(totalPower);
                break;
            case GemType.Dodge:
                player.AddDodge(totalPower);
                break;
        }

        currentActionPoints--;
        gameplayUIManager?.UpdateTurnCount(currentActionPoints);
        activeEnemies.RemoveAll(e => e.IsDead());

        if (activeEnemies.Count == 0)
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayWaveClear();
            StartCoroutine(currentStrategy.OnWaveCleared(this));
            yield break;
        }

        if (currentActionPoints <= 0 && !player.IsDead())
        {
            StartCoroutine(EnemyTurnRoutine());
            yield break;
        }

        currentState = GameState.PlayerTurn;
    }

    private IEnumerator EnemyTurnRoutine()
    {
        currentState = GameState.EnemyTurn;
        yield return new WaitForSeconds(1f);

        ObserverManager<EventID>.PostEvent(EventID.OnEnemyTurnStart);

        foreach (Enemy enemy in activeEnemies)
        {
            
            if (player == null || player.IsDead())
            {
                break;
            }

            if (!enemy.IsDead())
            {
                yield return StartCoroutine(enemy.PerformAttackSequence(player, 1f));
                yield return new WaitForSeconds(0.5f); 
            }
        }

        if (!currentStrategy.IsGameOver(player))
        {
            StartPlayerTurn();
        }
        else
        {
            currentState = GameState.Finished;
            gameplayUIManager?.ShowGameOver(player);
        }
    }

    public void SetGameState(GameState state)
    {
        currentState = state;
    }

    private void HandlePause(object param)
    {
        if (currentState != GameState.Paused)
        {
            previousState = currentState;
            currentState = GameState.Paused;
        }
    }

    private void HandleResume(object param)
    {
        if (currentState == GameState.Paused)
        {
            currentState = previousState;
        }
    }

    public void CleanupBattle()
    {
        StopAllCoroutines();

        foreach (var enemy in activeEnemies)
        {
            if (enemy != null && enemy.gameObject.activeInHierarchy)
            {
                PoolingManager.Despawn(enemy.gameObject);
            }
        }
        activeEnemies.Clear();

        if (gameGrid != null)
        {
            gameGrid.ClearGrid();
        }

        if (player != null)
        {
            player.StopAllCoroutines();
        }
    }
}