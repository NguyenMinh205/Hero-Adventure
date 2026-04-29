using UnityEngine;
using System;

public class CharacterStat : MonoBehaviour
{
    [Header("Base Data")]
    [SerializeField] protected CharacterStatSO baseStatData;

    [Header("Current Runtime Stats")]
    protected float currentMaxHealth;
    protected float currentHealth;
    protected float currentShield;

    protected float currentDamage;
    protected float currentCritRate;
    protected float currentCritDamage;
    protected float currentDodge;

    public event Action<float, float> OnHealthChanged;
    public event Action<float> OnShieldChanged;
    public event Action OnDeath;

    protected virtual void Start()
    {
        if (baseStatData != null)
        {
            InitStat(baseStatData);
        }
    }

    public virtual void InitStat(CharacterStatSO statData)
    {
        baseStatData = statData;

        currentMaxHealth = statData.maxHealth;
        currentHealth = currentMaxHealth;
        currentShield = 0;

        currentDamage = statData.baseDamage;
        currentCritRate = statData.baseCritRate;
        currentCritDamage = statData.baseCritDamage;
        currentDodge = statData.baseDodge;

        OnHealthChanged?.Invoke(currentHealth, currentMaxHealth);
        OnShieldChanged?.Invoke(currentShield);

        Debug.Log($"[CharacterStat] Initialized stats for {gameObject.name}");
    }


    public virtual void TakeDamage(float rawDamage)
    {
        if (UnityEngine.Random.Range(0f, 100f) <= currentDodge)
        {
            Debug.Log($"{gameObject.name} Dodged the attack!");
            return;
        }

        float damageMultiplier = 100f / (100f + currentShield);
        float finalDamage = rawDamage * damageMultiplier;

        if (finalDamage > 0)
        {
            currentHealth -= finalDamage;
            currentHealth = Mathf.Clamp(currentHealth, 0, currentMaxHealth);
            OnHealthChanged?.Invoke(currentHealth, currentMaxHealth);

            if (currentHealth <= 0)
            {
                Die();
            }
        }
    }

    public virtual void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, currentMaxHealth);
        OnHealthChanged?.Invoke(currentHealth, currentMaxHealth);
    }

    public virtual void AddShield(float amount)
    {
        currentShield += amount;
        OnShieldChanged?.Invoke(currentShield);
    }

    public virtual float CalculateDamage()
    {
        float damageOut = currentDamage;

        if (UnityEngine.Random.Range(0f, 100f) <= currentCritRate)
        {
            damageOut += (damageOut * (currentCritDamage / 100f));
        }

        return damageOut;
    }

    protected virtual void Die()
    {
        Debug.Log($"{gameObject.name} has died.");
        OnDeath?.Invoke();
    }
}