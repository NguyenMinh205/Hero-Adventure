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

    [Header("References")]
    [SerializeField] private GameplayUIManager gameplayUIManager;
    [SerializeField] private GameGrid gameGrid;
    [SerializeField] private Player player;
    public Player Player => player;
    [SerializeField] private List<Enemy> activeEnemies = new List<Enemy>();
    [SerializeField] private float multiplierPerGem = 0.25f;

    [Header("Enemy Spawner Settings")]
    [SerializeField] private Enemy enemyPrefab;
    [SerializeField] private List<CharacterInfoSO> listEnemySO;
    [SerializeField] private Transform[] spawnPoints;

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

        player.SetRunningAnimation(true);
        if (bgScroller != null) bgScroller.StartScrolling();

        float waitTime = Random.Range(3f, 5f);
        yield return new WaitForSeconds(waitTime);

        player.SetRunningAnimation(false);
        if (bgScroller != null) bgScroller.StopScrolling();

        SpawnEnemies();
        StartPlayerTurn();
    }

    private void SpawnEnemies()
    {
        activeEnemies.Clear();
        List<CharacterInfoSO> enemiesToSpawn = currentStrategy.GetEnemiesToSpawn(listEnemySO);

        if (enemiesToSpawn == null || enemiesToSpawn.Count == 0)
        {
            Debug.LogWarning("No enemies to spawn from strategy.");
            return;
        }

        if (enemiesToSpawn.Count == 1)
        {
            Enemy newEnemy = PoolingManager.Spawn(enemyPrefab, spawnPoints[spawnPoints.Length - 1].position, Quaternion.identity);
            newEnemy.InitStat(enemiesToSpawn[0]);

            if (currentStrategy is EndlessModeStrategy endlessStrategy)
            {
                float multiplier = endlessStrategy.GetDifficultyMultiplier();
                newEnemy.ApplyDifficultyMultiplier(multiplier);
            }

            activeEnemies.Add(newEnemy);
            return;
        }    

        for (int i = 0; i < enemiesToSpawn.Count; i++)
        {
            if (i >= spawnPoints.Length) break;
            
            Enemy newEnemy = PoolingManager.Spawn(enemyPrefab, spawnPoints[i].position, Quaternion.identity);
            newEnemy.InitStat(enemiesToSpawn[i]);
            
            if (currentStrategy is EndlessModeStrategy endlessStrategy)
            {
                float multiplier = endlessStrategy.GetDifficultyMultiplier();
                newEnemy.ApplyDifficultyMultiplier(multiplier);
            }

            activeEnemies.Add(newEnemy);
        }
    }

    private void StartPlayerTurn()
    {
        currentState = GameState.PlayerTurn;
        currentActionPoints = maxActionPoints;
        ObserverManager<EventID>.PostEvent(EventID.OnPlayerTurnStart);
        ObserverManager<EventID>.PostEvent(EventID.OnUpdateTurnCount, currentActionPoints);
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
                Enemy target = activeEnemies.Find(e => !e.IsDead());
                if (target != null)
                {
                    ObserverManager<EventID>.PostEvent(EventID.OnShowEnemyInfo, target);
                    yield return StartCoroutine(player.PerformAttackSequence(target, totalPower, data.MatchCount - 3));
                    yield return new WaitForSeconds(1f);
                    ObserverManager<EventID>.PostEvent(EventID.OnHideEnemyInfo);
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
        ObserverManager<EventID>.PostEvent(EventID.OnUpdateTurnCount, currentActionPoints);
        activeEnemies.RemoveAll(e => e.IsDead());

        if (activeEnemies.Count == 0)
        {
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
            if (!enemy.IsDead())
            {
                yield return StartCoroutine(enemy.PerformAttackSequence(player, 1f, 0));
                yield return new WaitForSeconds(0.2f);
            }
        }

        if (!currentStrategy.IsGameOver(player))
        {
            StartPlayerTurn();
        }
        else
        {
            currentState = GameState.Finished;
            ObserverManager<EventID>.PostEvent(EventID.OnGameOver, player);
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
}