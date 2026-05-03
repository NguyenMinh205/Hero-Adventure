using UnityEditor.Animations;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacterInfo", menuName = "ScriptableObjects/CharacterInfo")]
public class CharacterInfoSO : ScriptableObject
{
    [Header("Health & Defense")]
    public float maxHealth = 100f;
    public float baseShield = 10f;
    public float baseBlockRate = 5f;

    [Header("Offense")]
    public float baseDamage = 10f;
    public float baseCritRate = 5f;
    public float baseCritDamage = 50f;

    [Header("Other Attribute")]
    public AnimatorController characterAnim;
    public Sprite defaultCharacterSprite;
}