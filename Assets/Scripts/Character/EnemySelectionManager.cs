using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySelectionManager : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Layer mask của Enemy để raycast click detection")]
    [SerializeField] private LayerMask enemyLayer;

    private bool _isSelecting = false;

    public IEnumerator SelectTarget(List<Enemy> activeEnemies, Action<Enemy> onSelected)
    {
        _isSelecting = true;

        List<Enemy> aliveEnemies = activeEnemies.FindAll(e => e != null && !e.IsDead());

        if (aliveEnemies.Count == 0)
        {
            _isSelecting = false;
            onSelected?.Invoke(null);
            yield break;
        }

        if (aliveEnemies.Count == 1)
        {
            aliveEnemies[0].ShowTargetIndicator();
            ObserverManager<EventID>.PostEvent(EventID.OnEnemyTargetSelectionRequired);
            yield return new WaitForSeconds(0.3f);
            ObserverManager<EventID>.PostEvent(EventID.OnEnemyTargetSelected, aliveEnemies[0]);
            _isSelecting = false;
            onSelected?.Invoke(aliveEnemies[0]);
            yield break;
        }

        foreach (Enemy enemy in aliveEnemies)
        {
            enemy.ShowTargetIndicator();
        }

        ObserverManager<EventID>.PostEvent(EventID.OnEnemyTargetSelectionRequired);

        Enemy selectedEnemy = null;
        while (selectedEnemy == null)
        {
            if (InputHelper.GetPointerDown())
            {
                Vector3 pointerWorldPos = InputHelper.GetPointerWorldPosition();

                Collider2D hit = Physics2D.OverlapPoint(pointerWorldPos, enemyLayer);
                if (hit != null)
                {
                    Enemy clickedEnemy = hit.GetComponent<Enemy>();
                    if (clickedEnemy != null && !clickedEnemy.IsDead() && aliveEnemies.Contains(clickedEnemy))
                    {
                        if (AudioManager.Instance != null) AudioManager.Instance.PlayTargetSelected();
                        selectedEnemy = clickedEnemy;
                    }
                }
            }
            yield return null;
        }

        foreach (Enemy enemy in aliveEnemies)
        {
            if (enemy != selectedEnemy)
            {
                enemy.HideTargetIndicator();
            }
        }

        ObserverManager<EventID>.PostEvent(EventID.OnEnemyTargetSelected, selectedEnemy);

        _isSelecting = false;
        onSelected?.Invoke(selectedEnemy);
    }

    public bool IsSelecting => _isSelecting;
}
