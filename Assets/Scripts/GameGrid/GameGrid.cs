using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class GameGrid : MonoBehaviour
{
    [SerializeField] private int gridSize = 7;
    [SerializeField] private int matchThreshold = 3;
    public int MatchThreshold => matchThreshold;
    [SerializeField] private float spacing = 0.75f;
    [SerializeField] private float startY = 0f;

    [SerializeField] private GemSpawner gemSpawner;

    public Gem[,] gridGems { get; private set; }
    private float startX;

    public void Init()
    {
        ClearGrid();
        startX = -(gridSize - 1) * spacing / 2f;
        CreateGrid();
    }

    public void ClearGrid()
    {
        if (gridGems != null)
        {
            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    if (gridGems[x, y] != null)
                    {
                        gridGems[x, y].transform.DOKill();
                        gemSpawner.DespawnGem(gridGems[x, y]);
                        gridGems[x, y] = null;
                    }
                }
            }
        }
    }

    private void CreateGrid()
    {
        gridGems = new Gem[gridSize, gridSize];
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                SpawnGemAtPosition(x, y, true);
            }
        }
    }

    private void SpawnGemAtPosition(int x, int y, bool animate = true)
    {
        Vector2 finalPosition = GetWorldPosition(x, y);
        Vector2 spawnPosition = animate ? GetWorldPosition(x, -4) : finalPosition;

        Gem newGem = gemSpawner.SpawnGem(spawnPosition, this.transform);

        if (newGem != null)
        {
            newGem.Init(gemSpawner.GetRandomGemData(), new Vector2Int(x, y));
            gridGems[x, y] = newGem;

            if (animate)
            {
                newGem.transform.DOMove(finalPosition, 0.4f).SetEase(Ease.OutBounce);
            }
        }
    }

    public Vector2 GetWorldPosition(int x, int y)
    {
        float posX = startX + (x * spacing);
        float posY = startY - (y * spacing);
        return new Vector2(posX, posY);
    }

    public void ProcessMatch(List<Gem> matchedGems)
    {
        HashSet<int> affectedColumns = new HashSet<int>();

        GemData gemData = matchedGems[0].GetGemData();
        GemType type = gemData.gemType;
        float power = gemData.powerValue;
        int matchCount = matchedGems.Count;

        ObserverManager<EventID>.PostEvent(EventID.OnGemsMatched, new MatchEventData
        {
            GemType = type,
            MatchCount = matchCount,
            PowerValue = power
        });

        Debug.Log($"Matched {matchCount} gems of type {type} with power {power}");
        if (AudioManager.Instance != null) AudioManager.Instance.PlayGemMatch(matchCount);

        foreach (Gem gem in matchedGems)
        {
            affectedColumns.Add(gem.gridPosition.x);
            gridGems[gem.gridPosition.x, gem.gridPosition.y] = null;

            gem.transform.DOKill();
            Gem capturedGem = gem;
            capturedGem.transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack).OnComplete(() =>
            {
                gemSpawner.DespawnGem(capturedGem);
            });
        }

        foreach (int x in affectedColumns)
        {
            int emptySpots = 0;

            for (int y = gridSize - 1; y >= 0; y--)
            {
                if (gridGems[x, y] == null)
                {
                    emptySpots++;
                }
                else if (emptySpots > 0)
                {
                    Gem gem = gridGems[x, y];
                    int targetY = y + emptySpots;

                    gridGems[x, targetY] = gem;
                    gridGems[x, y] = null;

                    gem.Init(gem.GetGemData(), new Vector2Int(x, targetY));
                    gem.transform.DOMove(GetWorldPosition(x, targetY), 0.3f).SetEase(Ease.OutBounce);
                }
            }

            for (int y = 0; y < emptySpots; y++)
            {
                SpawnGemAtPosition(x, y, true);
            }
        }
    }
}