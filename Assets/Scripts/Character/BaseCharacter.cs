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
        
        if (baseStatData == null)
        {
            Debug.LogError($"[BaseCharacter] Lỗi: baseStatData (CharacterInfoSO) chưa được gán cho {gameObject.name}!");
            return;
        }

        currentMaxHealth = baseStatData.maxHealth;
        currentHealth = currentMaxHealth;
        currentShield = baseStatData.baseShield;
        currentDamage = baseStatData.baseDamage;
        currentCritRate = baseStatData.baseCritRate;
        currentCritDamage = baseStatData.baseCritDamage;
        currentBlockRate = baseStatData.baseBlockRate;
        
        if (animator != null) animator.runtimeAnimatorController = baseStatData.characterAnim;
        if (spriteRenderer != null) spriteRenderer.sprite = baseStatData.defaultCharacterSprite;

        originalPosition = transform.position;

        if (animator != null)
        {
            animator.SetBool(GameConstants.AnimParams.IsDie, false);
            animator.SetBool(GameConstants.AnimParams.IsRunning, false);
            animator.SetBool(GameConstants.AnimParams.IsHurting, false);
            animator.SetBool(GameConstants.AnimParams.IsBlocking, false);
            animator.SetBool(GameConstants.AnimParams.IsBaseAttacking, false);
            animator.SetBool(GameConstants.AnimParams.IsCritAttacking, false);
        }

        BroadcastUIUpdate();
    }

    protected float RoundStat(float value)
    {

        return Mathf.Round(value * 10f) / 10f;

    }

    public IEnumerator PerformAttackSequence(BaseCharacter target, float damageMultiplier)
    {
        Vector3 direction = (transform.position - target.transform.position).normalized;
        Vector3 attackPosition = target.transform.position + direction * 1.25f;
        yield return StartCoroutine(MoveToPosition(attackPosition, 10f));

        bool isCrit;
        float rawDamage = CalculateDamage(out isCrit) * damageMultiplier;
        AddDamage((damageMultiplier - 1f));
        string animParam = isCrit ? GameConstants.AnimParams.IsCritAttacking : GameConstants.AnimParams.IsBaseAttacking;

        if (AudioManager.Instance != null)
        {
            if (isCrit) AudioManager.Instance.PlayAttackCritSwing();
            else AudioManager.Instance.PlayAttackSwing();
        }

        animator.SetBool(animParam, true);
        yield return null;

        AnimatorStateInfo stateInfo = animator.IsInTransition(0) ? animator.GetNextAnimatorStateInfo(0) : animator.GetCurrentAnimatorStateInfo(0);
        float animLength = stateInfo.length;

        float impactDelay = animLength * 0.6f;
        yield return new WaitForSeconds(impactDelay);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayAttackHit();
        }

        target.TakeDamage(rawDamage, isCrit);

        yield return new WaitForSeconds(animLength - impactDelay);
        animator.SetBool(animParam, false);

        yield return StartCoroutine(MoveToPosition(originalPosition, 10f));
    }

    private IEnumerator MoveToPosition(Vector3 targetPosition, float speed)
    {
        animator.SetBool(GameConstants.AnimParams.IsRunning, true);
        float distance = Vector3.Distance(transform.position, targetPosition);
        float duration = distance / speed;

        yield return transform.DOMove(targetPosition, duration).SetEase(Ease.Linear).WaitForCompletion();

        transform.position = targetPosition;
        animator.SetBool(GameConstants.AnimParams.IsRunning, false);
        yield return new WaitForSeconds(0.1f);
    }

    public virtual void TakeDamage(float rawDamage, bool isCrit)
    {
        if (UnityEngine.Random.Range(0f, 100f) <= currentBlockRate)
        {
            if (AudioManager.Instance != null)
            {
                if (this is Player) AudioManager.Instance.PlayPlayerBlock();
                else AudioManager.Instance.PlayEnemyBlock();
            }
            StartCoroutine(PlayAnimationBool(GameConstants.AnimParams.IsBlocking));
            transform.DOShakePosition(0.2f, 0.1f, 15);
            Debug.Log($"{gameObject.name} né được đòn!");
            return;
        }

        float damageMultiplier = 100f / (100f + currentShield);
        float damageToHealth = rawDamage * damageMultiplier;
        float damagePrevented = rawDamage - damageToHealth;

        if (currentShield > 0 && damagePrevented > 0)
        {
            currentShield -= damagePrevented;
            currentShield = RoundStat(Mathf.Max(0, currentShield));
        }

        if (damageToHealth > 0)
        {
            currentHealth -= damageToHealth;
            currentHealth = RoundStat(Mathf.Clamp(currentHealth, 0, currentMaxHealth));

            ObserverManager<EventID>.PostEvent(EventID.OnShowDamagePopup, new DamagePopupData
            {
                Position = transform.position + Vector3.up * 1f,
                Damage = Mathf.RoundToInt(damageToHealth),
                IsCritical = isCrit
            });

            if (currentHealth <= 0)
            {
                if (AudioManager.Instance != null)
                {
                    if (this is Player) AudioManager.Instance.PlayPlayerDie();
                    else AudioManager.Instance.PlayEnemyDie();
                }
                animator.SetBool(GameConstants.AnimParams.IsDie, true);
                Die();
            }
            else
            {
                if (AudioManager.Instance != null)
                {
                    if (this is Player) AudioManager.Instance.PlayPlayerHurt();
                    else AudioManager.Instance.PlayEnemyHurt();
                }
                StartCoroutine(PlayAnimationBool(GameConstants.AnimParams.IsHurting));
                transform.DOShakePosition(0.3f, 0.3f, 20);
            }
        }

        BroadcastUIUpdate();
    }

    public virtual void Heal(float amount)
    {
        if (currentHealth >= currentMaxHealth)
        {
            amount /= 2f;
            currentMaxHealth = RoundStat(currentMaxHealth + amount);
        }
        currentHealth = RoundStat(currentHealth + amount);
        currentHealth = Mathf.Clamp(currentHealth, 0, currentMaxHealth);
        BroadcastUIUpdate();
    }

    public virtual void AddDamage(float amount)
    {
        currentDamage = RoundStat(currentDamage + amount);
        BroadcastUIUpdate();
    }

    public virtual void AddShield(float amount)
    {
        currentShield = RoundStat(currentShield + amount);
        BroadcastUIUpdate();
    }

    public virtual void AddCritRate(float amount)
    {
        currentCritRate = RoundStat(Mathf.Clamp(currentCritRate + amount, 0, 100f));
        BroadcastUIUpdate();
    }

    public virtual void AddCritDamage(float amount)
    {
        currentCritDamage = RoundStat(currentCritDamage + amount);
        BroadcastUIUpdate();
    }

    public virtual void AddDodge(float amount)
    {
        currentBlockRate = RoundStat(Mathf.Clamp(currentBlockRate + amount, 0, MAX_DODGE));
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
        damageOut = Mathf.Round(damageOut * 10f) / 10f;
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
        Destroy(gameObject, 1f);
    }

    public void SetRunningAnimation(bool isRunning)
    {
        if (animator != null) animator.SetBool(GameConstants.AnimParams.IsRunning, isRunning);
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