using Akasha;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 전투 공격 타입
/// </summary>
public enum AttackType
{
    Melee,      // 근접
    Ranged,     // 원거리
    Magic,      // 마법
    Divine      // 신성
}

/// <summary>
/// 캐릭터 전투를 담당하는 Part
/// </summary>
public class CombatPart<TEntity, TModel> : CharacterPartBase<TEntity, TModel>
    where TEntity : BaseEntity<TModel>
    where TModel : CharModel
{
    [SerializeField] protected AttackType primaryAttackType = AttackType.Melee;
    [SerializeField] protected float attackRange = 2f;
    [SerializeField] protected float attackCooldown = 1f;

    protected float lastAttackTime;
    protected BaseEntity<CharModel> currentTarget;
    protected List<BaseEntity<CharModel>> targetsInRange = new List<BaseEntity<CharModel>>();

    protected override void AtModelReady()
    {
        // 사망 시 전투 중지
        Model.IsAlive.AddListener(alive =>
        {
            if (!alive)
            {
                StopCombat();
            }
        });
    }

    /// <summary>
    /// 공격 가능 여부 확인
    /// </summary>
    public virtual bool CanAttack()
    {
        return IsAlive && Time.time >= lastAttackTime + attackCooldown;
    }

    /// <summary>
    /// 기본 공격
    /// </summary>
    public virtual void Attack(BaseEntity<CharModel> targetEntity)
    {
        if (!CanAttack() || targetEntity == null || !targetEntity.Model.IsAlive.Value)
            return;

        currentTarget = targetEntity;

        // 거리 확인
        if (Vector3.Distance(transform.position, targetEntity.transform.position) > attackRange)
        {
            Debug.Log($"{name}: Target out of range");
            return;
        }

        // 공격 처리
        float damage = CalculateDamage(primaryAttackType);
        int finalDamage = Mathf.RoundToInt(damage);

        targetEntity.Model.TakeDamage(finalDamage);
        lastAttackTime = Time.time;

        OnAttackExecuted(targetEntity, finalDamage);
    }

    /// <summary>
    /// 데미지 계산
    /// </summary>
    protected virtual float CalculateDamage(AttackType attackType)
    {
        float baseDamage = 0f;

        switch (attackType)
        {
            case AttackType.Melee:
                baseDamage = MeleeAttack;
                break;
            case AttackType.Ranged:
                baseDamage = RangedAttack;
                break;
            case AttackType.Magic:
                baseDamage = MagicAttack;
                break;
            case AttackType.Divine:
                baseDamage = DivineAttack;
                break;
        }

        // 크리티컬 확률 계산
        float criticalChance = Model.CriticalChance.Value;
        if (Random.value * 100f < criticalChance)
        {
            baseDamage *= Model.CriticalDamage.Value / 100f;
            Debug.Log($"{name}: Critical Hit!");
        }

        return baseDamage;
    }

    /// <summary>
    /// 공격 실행 후 호출 (애니메이션, 이펙트 등)
    /// </summary>
    protected virtual void OnAttackExecuted(BaseEntity<CharModel> targetEntity, int damage)
    {
        Debug.Log($"{name} attacked {targetEntity.name} for {damage} damage");
    }

    /// <summary>
    /// 범위 내 적 탐색
    /// </summary>
    public virtual List<BaseEntity<CharModel>> FindTargetsInRange(float range)
    {
        targetsInRange.Clear();

        Collider[] colliders = Physics.OverlapSphere(transform.position, range);
        foreach (var col in colliders)
        {
            var targetEntity = col.GetComponentInParent<BaseEntity<CharModel>>();
            if (targetEntity != null && targetEntity != Entity && targetEntity.Model.IsAlive.Value)
            {
                targetsInRange.Add(targetEntity);
            }
        }

        return targetsInRange;
    }

    /// <summary>
    /// 가장 가까운 적 찾기
    /// </summary>
    public virtual BaseEntity<CharModel> FindNearestTarget(float maxRange)
    {
        var targets = FindTargetsInRange(maxRange);
        if (targets.Count == 0)
            return null;

        BaseEntity<CharModel> nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (var targetEntity in targets)
        {
            float distance = Vector3.Distance(transform.position, targetEntity.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = targetEntity;
            }
        }

        return nearest;
    }

    /// <summary>
    /// 전투 중지
    /// </summary>
    public virtual void StopCombat()
    {
        currentTarget = null;
    }

    /// <summary>
    /// 스킬 사용 (MP 소모)
    /// </summary>
    public virtual bool UseSkill(int mpCost, System.Action onSkillExecute)
    {
        if (!IsAlive)
            return false;

        if (!Model.ConsumeMP(mpCost))
        {
            Debug.Log($"{name}: Not enough MP");
            return false;
        }

        onSkillExecute?.Invoke();
        return true;
    }
}
