using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacterStat", menuName = "ScriptableObjects/CharacterStat")]
public class CharacterStatSO : ScriptableObject
{
    [Header("Health & Defense")]
    public float maxHealth = 100f;
    public float baseArmor = 10f;
    public float baseDodge = 5f;

    [Header("Offense")]
    public float baseDamage = 10f;
    public float baseCritRate = 5f;
    public float baseCritDamage = 50f;
}