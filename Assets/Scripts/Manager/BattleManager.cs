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

    [Header("References")]
    [SerializeField] private Player player;
    [SerializeField] private List<Enemy> activeEnemies = new List<Enemy>();

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
            if (player == null)
            {
                Debug.LogWarning("Không tìm thấy Player! Game tạm dừng.");
                return;
            }
        }
        StartCoroutine(ExploreRoutine());
    }

    private IEnumerator ExploreRoutine()
    {
        currentState = GameState.Running;
        player.SetRunningAnimation(true);

        float waitTime = Random.Range(3f, 5f);
        yield return new WaitForSeconds(waitTime);

        player.SetRunningAnimation(false);
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
        currentState = GameState.PlayerAttacking;
        currentActionPoints = maxActionPoints;
        ObserverManager<EventID>.PostEvent(EventID.OnPlayerTurnStart);
        Debug.Log($"LƯỢT PLAYER BẮT ĐẦU - Bạn có {currentActionPoints} lượt nối");
    }

    private void HandleGemsMatched(object param)
    {
        if (currentState != GameState.PlayerAttacking || player.IsDead()) return;

        if (param is MatchEventData data)
        {
            StartCoroutine(ProcessMatchRoutine(data));
        }
    }

    private IEnumerator ProcessMatchRoutine(MatchEventData data)
    {
        currentState = GameState.Matching;

        float multiplier = 1f + (data.MatchCount - 3) * 0.5f;

        switch (data.GemType)
        {
            case GemType.Damage:
                Enemy target = activeEnemies.Find(e => !e.IsDead());
                if (target != null)
                {
                    yield return StartCoroutine(player.PerformAttackSequence(target, multiplier));
                }
                break;
            case GemType.Health:
                player.Heal(20f * multiplier);
                break;
            case GemType.Shield:
                player.AddShield(15f * multiplier);
                break;
            case GemType.CritRate:
                player.AddCritRate(2f * multiplier);
                break;
            case GemType.CritDamage:
                player.AddCritDamage(10f * multiplier);
                break;
            case GemType.Dodge:
                player.AddDodge(2f * multiplier);
                break;
        }

        currentActionPoints--;
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

        currentState = GameState.PlayerAttacking;
        Debug.Log($"Còn {currentActionPoints} lượt nối");
    }

    private IEnumerator EnemyTurnRoutine()
    {
        currentState = GameState.EnemyAttacking;
        yield return new WaitForSeconds(1f);

        ObserverManager<EventID>.PostEvent(EventID.OnEnemyTurnStart);

        foreach (Enemy enemy in activeEnemies)
        {
            if (!enemy.IsDead())
            {
                yield return StartCoroutine(enemy.PerformAttackSequence(player, 1f));
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
            Debug.Log("GAME OVER");
            ObserverManager<EventID>.PostEvent(EventID.OnGameOver);
        }
    }
}