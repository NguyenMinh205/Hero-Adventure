using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleManager : Singleton<BattleManager>
{
    [Header("State")]
    [SerializeField] private GameState currentState;
    public GameState CurrentState => currentState;
    [SerializeField] private int maxActionPoints = 5;
    [SerializeField] private int currentActionPoints;

    [Header("Level Progress")]
    [SerializeField] private int maxRounds = 3;
    private int currentRound = 0;
    [SerializeField] private BackgroundScroller bgScroller;

    [Header("References")]
    [SerializeField] private Player player;
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
    }

    private void OnDisable()
    {
        ObserverManager<EventID>.RemoveAddListener(EventID.OnGemsMatched, HandleGemsMatched);
    }

    private void Start()
    {
        if (player == null)
        {
            player = FindObjectOfType<Player>();
            if (player == null) return;
        }

        player.InitStat();
        currentRound = 0;
        StartCoroutine(ExploreRoutine());
    }

    private IEnumerator ExploreRoutine()
    {
        if (currentRound >= maxRounds)
        {
            currentState = GameState.Finished;
            ObserverManager<EventID>.PostEvent(EventID.OnGameOver);
            yield break;
        }

        currentRound++;
        ObserverManager<EventID>.PostEvent(EventID.OnUpdateRoundCount, currentRound);
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
        int enemyCount = Random.Range(1, spawnPoints.Length + 1);

        if (enemyCount == 1)
        {
            Enemy newEnemy = PoolingManager.Spawn(enemyPrefab, spawnPoints[spawnPoints.Length - 1].position, Quaternion.identity);
            newEnemy.InitStat(listEnemySO[Random.Range(0, listEnemySO.Count)]);
            activeEnemies.Add(newEnemy);
            return;
        }

        for (int i = 0; i < enemyCount; i++)
        {
            Enemy newEnemy = PoolingManager.Spawn(enemyPrefab, spawnPoints[i].position, Quaternion.identity);
            newEnemy.InitStat(listEnemySO[Random.Range(0, listEnemySO.Count)]);
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
            StartCoroutine(ExploreRoutine());
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

        if (!player.IsDead())
        {
            StartPlayerTurn();
        }
        else
        {
            currentState = GameState.Finished;
            ObserverManager<EventID>.PostEvent(EventID.OnGameOver);
        }
    }
}