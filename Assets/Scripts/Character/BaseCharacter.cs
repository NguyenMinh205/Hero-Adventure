using UnityEngine;
using System;
using System.Collections;
using DG.Tweening;

public class BaseCharacter : MonoBehaviour
{
    [Header("Base Data")]
    [SerializeField] protected CharacterInfoSO baseStatData;
    [SerializeField] protected Animator animator;
    [SerializeField] protected SpriteRenderer spriteRenderer;

    [Header("Current Runtime Stats")]
    protected float currentMaxHealth;
    protected float currentHealth;
    protected float currentShield;
    protected float currentDamage;
    protected float currentCritRate;
    protected float currentCritDamage;
    protected float currentDodge;
    private const float MAX_DODGE = 80f;

    private Vector3 originalPosition;

    public event Action<float, float> OnHealthChanged;
    public event Action<float> OnShieldChanged;
    public event Action OnDeath;

    public virtual void InitStat(CharacterInfoSO statData)
    {
        baseStatData = statData;
        currentMaxHealth = statData.maxHealth;
        currentHealth = currentMaxHealth;
        currentShield = 0;
        currentDamage = statData.baseDamage;
        currentCritRate = statData.baseCritRate;
        currentCritDamage = statData.baseCritDamage;
        currentDodge = statData.baseDodge;
        animator.runtimeAnimatorController = statData.characterAnim;
        spriteRenderer.sprite = statData.defaultCharacterSprite;

        originalPosition = transform.position;
        OnHealthChanged?.Invoke(currentHealth, currentMaxHealth);
        OnShieldChanged?.Invoke(currentShield);
    }

    public IEnumerator PerformAttackSequence(BaseCharacter target, float damageMultiplier)
    {
        Vector3 direction = (transform.position - target.transform.position).normalized;
        Vector3 attackPosition = target.transform.position + direction * 1.5f;

        yield return StartCoroutine(MoveToPosition(attackPosition, 10f));

        bool isCrit;
        float rawDamage = CalculateDamage(out isCrit) * damageMultiplier;

        if (isCrit) yield return StartCoroutine(PlayAnimationBool("IsCritAttacking"));
        else yield return StartCoroutine(PlayAnimationBool("IsBaseAttacking"));

        yield return new WaitForSeconds(0.5f);

        target.TakeDamage(rawDamage);

        yield return new WaitForSeconds(0.5f);

        yield return StartCoroutine(MoveToPosition(originalPosition, 10f));
    }

    private IEnumerator MoveToPosition(Vector3 targetPosition, float speed)
    {
        animator.SetBool("IsRunning", true);

        float distance = Vector3.Distance(transform.position, targetPosition);
        float duration = distance / speed;

        yield return transform.DOMove(targetPosition, duration).SetEase(Ease.Linear).WaitForCompletion();

        transform.position = targetPosition;
        animator.SetBool("IsRunning", false);

        yield return new WaitForSeconds(0.1f);
    }

    public void SetRunningAnimation(bool isRunning)
    {
        if (animator != null) animator.SetBool("IsRunning", isRunning);
    }

    public virtual void TakeDamage(float rawDamage)
    {
        if (UnityEngine.Random.Range(0f, 100f) <= currentDodge)
        {
            Debug.Log($"{gameObject.name} né được đòn!");
            return;
        }

        Debug.Log($"{gameObject.name} nhận {rawDamage} sát thương thô!");

        float damageMultiplier = 100f / (100f + currentShield);
        float damageToHealth = rawDamage * damageMultiplier;
        float damagePrevented = rawDamage - damageToHealth;

        if (currentShield > 0 && damagePrevented > 0)
        {
            currentShield -= damagePrevented;
            currentShield = Mathf.Max(0, currentShield);
            OnShieldChanged?.Invoke(currentShield);

            StartCoroutine(PlayAnimationBool("IsBlocking"));
            transform.DOShakePosition(0.2f, 0.1f, 15);
        }

        if (damageToHealth > 0)
        {
            currentHealth -= damageToHealth;
            currentHealth = Mathf.Clamp(currentHealth, 0, currentMaxHealth);
            OnHealthChanged?.Invoke(currentHealth, currentMaxHealth);

            if (currentHealth <= 0)
            {
                animator.SetBool("IsDie", true);
                Die();
            }
            else
            {
                StartCoroutine(PlayAnimationBool("IsHurting"));
                transform.DOShakePosition(0.3f, 0.3f, 20);
            }
        }
    }

    public virtual void Heal(float amount)
    {
        if (currentHealth >= currentMaxHealth) currentMaxHealth += amount/2;
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, currentMaxHealth);
        OnHealthChanged?.Invoke(currentHealth, currentMaxHealth);
        Debug.Log($"{gameObject.name} hồi phục {amount} HP!");
    }

    public virtual void AddShield(float amount)
    {
        currentShield += amount;
        OnShieldChanged?.Invoke(currentShield);
        Debug.Log($"{gameObject.name} nhận được {amount} lá chắn!");
    }

    public virtual void AddCritRate(float amount)
    {
        currentCritRate = Mathf.Clamp(currentCritRate + amount, 0, 100f);
        Debug.Log($"{gameObject.name} tăng {amount}% tỉ lệ chí mạng!");
    }

    public virtual void AddCritDamage(float amount)
    {
        currentCritDamage += amount;
        Debug.Log($"{gameObject.name} tăng {amount}% sát thương chí mạng!");
    }

    public virtual void AddDodge(float amount)
    {
        currentDodge = Mathf.Clamp(currentDodge + amount, 0, MAX_DODGE);
        Debug.Log($"{gameObject.name} tăng {amount}% tỉ lệ né đòn!");
    }

    public virtual float CalculateDamage(out bool isCrit)
    {
        float damageOut = currentDamage;
        isCrit = false;

        if (UnityEngine.Random.Range(0f, 100f) <= currentCritRate)
        {
            damageOut += (damageOut * (currentCritDamage / 100f));
            isCrit = true;
        }
        return damageOut;
    }

    public bool IsDead() => currentHealth <= 0;

    protected virtual void Die()
    {
        OnDeath?.Invoke();
        transform.DOKill();
        DestroyOrDespawn();
    }

    protected virtual void DestroyOrDespawn()
    {
        Destroy(gameObject, 2f);
    }

    public IEnumerator PlayAnimationBool(string paramName)
    {
        if (animator != null)
        {
            animator.SetBool(paramName, true);
            yield return new WaitForSeconds(0.1f);
            animator.SetBool(paramName, false);
        }
    }
}