using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GemType
{
    Damage,
    Health,
    Shield,
    CritDamage,
    CritRate,
    Dodge
}

[CreateAssetMenu(fileName = "GemData", menuName = "ScriptableObjects/GemData")]
public class GemData : ScriptableObject
{
    public GemType gemType;
    public Sprite gemSprite;
    public Color lineColor;
    public float powerValue;
}
