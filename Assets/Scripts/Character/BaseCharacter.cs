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
    protected float currentBlockRate;
    private const float MAX_DODGE = 80f;

    private Vector3 originalPosition;

    public float CurrentHealth => currentHealth;
    public float CurrentMaxHealth => currentMaxHealth;
    public float CurrentShield => currentShield;
    public float CurrentDamage => currentDamage;
    public float CurrentCritRate => currentCritRate;
    public float CurrentCritDamage => currentCritDamage;
    public float CurrentBlockRate => currentBlockRate;
    public Sprite CharacterSprite => spriteRenderer.sprite;

    public virtual void BroadcastUIUpdate() { }

    public virtual void InitStat(CharacterInfoSO statData = null)
    {
        if (statData != null) baseStatData = statData;
        currentMaxHealth = baseStatData.maxHealth;
        currentHealth = currentMaxHealth;
        currentShield = baseStatData.baseShield;
        currentDamage = baseStatData.baseDamage;
        currentCritRate = baseStatData.baseCritRate;
        currentCritDamage = baseStatData.baseCritDamage;
        currentBlockRate = baseStatData.baseBlockRate;
        animator.runtimeAnimatorController = baseStatData.characterAnim;
        spriteRenderer.sprite = baseStatData.defaultCharacterSprite;

        originalPosition = transform.position;
        BroadcastUIUpdate();
    }

    public IEnumerator PerformAttackSequence(BaseCharacter target, float damageMultiplier)
    {
        Vector3 direction = (transform.position - target.transform.position).normalized;
        Vector3 attackPosition = target.transform.position + direction * 1.25f;
        yield return StartCoroutine(MoveToPosition(attackPosition, 10f));

        bool isCrit;
        float rawDamage = CalculateDamage(out isCrit) * damageMultiplier;
        string animParam = isCrit ? "IsCritAttacking" : "IsBaseAttacking";

        animator.SetBool(animParam, true);
        yield return null;

        AnimatorStateInfo stateInfo = animator.IsInTransition(0) ? animator.GetNextAnimatorStateInfo(0) : animator.GetCurrentAnimatorStateInfo(0);
        float animLength = stateInfo.length;

        float impactDelay = animLength * 0.75f;
        yield return new WaitForSeconds(impactDelay);

        target.TakeDamage(rawDamage);

        yield return new WaitForSeconds(animLength - impactDelay);
        animator.SetBool(animParam, false);

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
        if (UnityEngine.Random.Range(0f, 100f) <= currentBlockRate)
        {
            Debug.Log($"{gameObject.name} né được đòn!");
            return;
        }

        float damageMultiplier = 100f / (100f + currentShield);
        float damageToHealth = rawDamage * damageMultiplier;
        float damagePrevented = rawDamage - damageToHealth;

        if (currentShield > 0 && damagePrevented > 0)
        {
            currentShield -= damagePrevented;
            currentShield = Mathf.Max(0, currentShield);
            StartCoroutine(PlayAnimationBool("IsBlocking"));
            transform.DOShakePosition(0.2f, 0.1f, 15);
        }

        if (damageToHealth > 0)
        {
            currentHealth -= damageToHealth;
            currentHealth = Mathf.Clamp(currentHealth, 0, currentMaxHealth);

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

        BroadcastUIUpdate();
    }

    public virtual void Heal(float amount)
    {
        if (currentHealth >= currentMaxHealth) currentMaxHealth += amount / 2;
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, currentMaxHealth);
        BroadcastUIUpdate();
    }

    public virtual void AddShield(float amount)
    {
        currentShield += amount;
        BroadcastUIUpdate();
    }

    public virtual void AddCritRate(float amount)
    {
        currentCritRate = Mathf.Clamp(currentCritRate + amount, 0, 100f);
        BroadcastUIUpdate();
    }

    public virtual void AddCritDamage(float amount)
    {
        currentCritDamage += amount;
        BroadcastUIUpdate();
    }

    public virtual void AddDodge(float amount)
    {
        currentBlockRate = Mathf.Clamp(currentBlockRate + amount, 0, MAX_DODGE);
        BroadcastUIUpdate();
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
            yield return null;
            AnimatorStateInfo stateInfo = animator.IsInTransition(0) ? animator.GetNextAnimatorStateInfo(0) : animator.GetCurrentAnimatorStateInfo(0);
            yield return new WaitForSeconds(stateInfo.length);
            animator.SetBool(paramName, false);
        }
    }
}