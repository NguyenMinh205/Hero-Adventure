using System.Collections.Generic;
using UnityEngine;

public class GridInput : MonoBehaviour
{
    [SerializeField] private LineRendererManager lineManager;
    [SerializeField] private GameGrid gameGrid;
    [SerializeField] private LayerMask gemLayer;

    private bool isDrawing = false;
    private List<Gem> selectedGems = new List<Gem>();
    private GemType currentGemType;

    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        if (BattleManager.Instance != null && BattleManager.Instance.CurrentState != GameState.PlayerTurn) return;

        Vector3 pointerWorldPos = InputHelper.GetPointerWorldPosition();

        if (InputHelper.GetPointerDown())
        {
            Gem hitGem = GetGemAtPosition(pointerWorldPos);
            if (hitGem != null)
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayGemSelect();
                isDrawing = true;
                selectedGems.Clear();
                lineManager.ClearLine();
                lineManager.LineColor = hitGem.GetGemData().lineColor;
                AddGemToSelection(hitGem);
                currentGemType = hitGem.GetGemData().gemType;
            }
        }

        if (InputHelper.GetPointerHeld() && isDrawing)
        {
            Gem hitGem = GetGemAtPosition(pointerWorldPos);

            if (hitGem != null)
            {
                if (!selectedGems.Contains(hitGem))
                {
                    if (hitGem.GetGemData().gemType == currentGemType && IsAdjacent(selectedGems[selectedGems.Count - 1], hitGem))
                    {
                        if (AudioManager.Instance != null) AudioManager.Instance.PlayGemDrag();
                        AddGemToSelection(hitGem);
                    }
                }
                else if (selectedGems.Count > 1 && hitGem == selectedGems[selectedGems.Count - 2])
                {
                    RemoveLastGemFromSelection();
                }
            }
        }

        if (InputHelper.GetPointerUp())
        {
            isDrawing = false;

            if (selectedGems.Count >= gameGrid.MatchThreshold)
            {
                gameGrid.ProcessMatch(selectedGems);
                Debug.Log($"Matched {selectedGems.Count} gems of type {currentGemType}");
            }

            lineManager.ClearLine();
            selectedGems.Clear();
        }
    }

    private Gem GetGemAtPosition(Vector2 pos)
    {
        Collider2D hit = Physics2D.OverlapPoint(pos, gemLayer);
        if (hit != null)
        {
            return hit.GetComponent<Gem>();
        }
        return null;
    }

    private void AddGemToSelection(Gem gem)
    {
        selectedGems.Add(gem);
        lineManager.AddPoint(gem.transform.position);
    }

    private void RemoveLastGemFromSelection()
    {
        selectedGems.RemoveAt(selectedGems.Count - 1);
        lineManager.RemoveLastPoint();
    }

    private bool IsAdjacent(Gem g1, Gem g2)
    {
        int dx = Mathf.Abs(g1.gridPosition.x - g2.gridPosition.x);
        int dy = Mathf.Abs(g1.gridPosition.y - g2.gridPosition.y);
        return (dx <= 1 && dy <= 1) && (dx + dy > 0);
    }
}