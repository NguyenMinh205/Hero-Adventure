using System.Collections.Generic;
using UnityEngine;

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

    private void Start()
    {
        startX = -(gridSize - 1) * spacing / 2f;
        CreateGrid();
    }

    private void CreateGrid()
    {
        gridGems = new Gem[gridSize, gridSize];
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                SpawnGemAtPosition(x, y);
            }
        }
    }

    private void SpawnGemAtPosition(int x, int y)
    {
        Vector2 spawnPosition = GetWorldPosition(x, y);

        Gem newGem = gemSpawner.SpawnGem(spawnPosition, this.transform);

        if (newGem != null)
        {
            newGem.Init(gemSpawner.GetRandomGemData(), new Vector2Int(x, y));
            gridGems[x, y] = newGem;
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

        foreach (Gem gem in matchedGems)
        {
            affectedColumns.Add(gem.gridPosition.x);
            gridGems[gem.gridPosition.x, gem.gridPosition.y] = null;

            gemSpawner.DespawnGem(gem);
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
                    gem.transform.position = GetWorldPosition(x, targetY);
                }
            }

            for (int y = 0; y < emptySpots; y++)
            {
                SpawnGemAtPosition(x, y);
            }
        }
    }
}