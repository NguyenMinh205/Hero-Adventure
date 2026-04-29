using UnityEngine;

public class Gem : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    private GemData _gemData;

    public Vector2Int gridPosition { get; private set; }

    public void Init(GemData data, Vector2Int gridPos)
    {
        _gemData = data;
        gridPosition = gridPos;

        if (_spriteRenderer != null && _gemData != null)
        {
            _spriteRenderer.sprite = _gemData.gemSprite;
        }
    }

    public GemData GetGemData() => _gemData;
}