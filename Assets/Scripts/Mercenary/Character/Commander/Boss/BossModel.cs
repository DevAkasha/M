using Akasha;
using UnityEngine;

/// <summary>
/// 보스 페이즈
/// </summary>
public enum BossPhase
{
    Phase1,     // 100% ~ 66% HP
    Phase2,     // 66% ~ 33% HP
    Phase3      // 33% ~ 0% HP
}

/// <summary>
/// 보스 난이도
/// </summary>
public enum BossDifficulty
{
    Beginner,   // 초급: HP 1000%, 공격력 200%
    Intermediate, // 중급: HP 1500%, 공격력 250%
    Advanced,   // 고급: HP 2500%, 공격력 300%
    Nightmare   // 악몽: HP 5000%, 공격력 400%
}

/// <summary>
/// 보스 Model
/// CharModel + 페이즈 시스템 + 패턴 시스템
/// </summary>
public class BossModel : CharModel
{
    // 난이도
    public RxVar<BossDifficulty> Difficulty { get; private set; }

    // 페이즈 시스템
    public RxComputed<BossPhase> CurrentPhase { get; private set; }
    public RxComputed<float> HPPercentage { get; private set; }

    // 광폭화 (페이즈 3 전용)
    public RxComputed<bool> IsEnraged { get; private set; }
    public RxComputed<float> EnragedDamageMultiplier { get; private set; }

    // 광폭화 효과가 적용된 실제 공격력 (연쇄참조)
    public RxComputed<float> EffectiveMeleeAttack { get; private set; }

    // 패턴 쿨타임 관리
    public RxVar<float> PatternCooldown { get; private set; }
    public RxVar<int> PatternCount { get; private set; }  // 사용 가능한 패턴 수

    // 부활 (일부 보스 전용)
    public RxVar<bool> CanRevive { get; private set; }
    public RxVar<bool> HasRevived { get; private set; }

    public BossModel()
    {
        Difficulty = new RxVar<BossDifficulty>(BossDifficulty.Beginner, nameof(Difficulty), this);
        PatternCooldown = new RxVar<float>(0f, nameof(PatternCooldown), this);
        PatternCount = new RxVar<int>(3, nameof(PatternCount), this);
        CanRevive = new RxVar<bool>(false, nameof(CanRevive), this);
        HasRevived = new RxVar<bool>(false, nameof(HasRevived), this);

        // HP 비율 계산 (디스플레이용)
        HPPercentage = new RxComputed<float>(() =>
        {
            int maxHp = GetBaseMaxHP();
            return maxHp > 0 ? (float)CurrentHP.Value / maxHp * 100f : 0f;
        }, nameof(HPPercentage), this).DependsOn(CurrentHP, Difficulty);

        // 현재 페이즈 계산 (HPPercentage 연쇄참조)
        CurrentPhase = new RxComputed<BossPhase>(() =>
        {
            float hpPercent = HPPercentage.Value;

            if (hpPercent > 66f)
                return BossPhase.Phase1;
            else if (hpPercent > 33f)
                return BossPhase.Phase2;
            else
                return BossPhase.Phase3;
        }, nameof(CurrentPhase), this).DependsOn(HPPercentage);

        // 광폭화 (페이즈 3) - HPPercentage 연쇄참조
        IsEnraged = new RxComputed<bool>(() =>
        {
            return HPPercentage.Value <= 33f;
        }, nameof(IsEnraged), this).DependsOn(HPPercentage);

        // 광폭화 데미지 배수 (IsEnraged 연쇄참조)
        EnragedDamageMultiplier = new RxComputed<float>(() =>
        {
            return IsEnraged.Value ? 1.5f : 1.0f;
        }, nameof(EnragedDamageMultiplier), this).DependsOn(IsEnraged);

        // 광폭화 효과가 적용된 실제 공격력 (MeleeAttack, EnragedDamageMultiplier 연쇄참조)
        EffectiveMeleeAttack = new RxComputed<float>(() =>
        {
            return MeleeAttack.Value * EnragedDamageMultiplier.Value;
        }, nameof(EffectiveMeleeAttack), this).DependsOn(MeleeAttack, EnragedDamageMultiplier);

        // 페이즈 변경 시 패턴 수 증가
        CurrentPhase.AddListener(OnPhaseChanged);

        // HP가 0이 되었을 때 부활 체크
        CurrentHP.AddListener(OnHPChanged);
    }

    private void OnPhaseChanged(BossPhase newPhase)
    {
        switch (newPhase)
        {
            case BossPhase.Phase1:
                PatternCount.Set(3);
                break;
            case BossPhase.Phase2:
                PatternCount.Set(5);
                // 졸개 소환 (이벤트 발생 필요)
                break;
            case BossPhase.Phase3:
                PatternCount.Set(10);
                // 광폭화: 공격력 +50%
                // Effect 시스템으로 처리 가능
                break;
        }
    }

    private void OnHPChanged(int newHP)
    {
        if (newHP <= 0 && CanRevive.Value && !HasRevived.Value)
        {
            // 부활 처리
            Revive();
        }
    }

    /// <summary>
    /// 부활 (HP 30% 회복)
    /// </summary>
    private void Revive()
    {
        HasRevived.Set(true);
        int reviveHP = Mathf.RoundToInt(MaxHP.Value * 0.3f);
        CurrentHP.Set(reviveHP);
        IsAlive.Set(true);
    }

    protected override int GetBaseMaxHP()
    {
        int baseHP = 100;
        switch (Difficulty.Value)
        {
            case BossDifficulty.Beginner:
                return baseHP * 10;  // 1000%
            case BossDifficulty.Intermediate:
                return baseHP * 15;  // 1500%
            case BossDifficulty.Advanced:
                return baseHP * 25;  // 2500%
            case BossDifficulty.Nightmare:
                return baseHP * 50;  // 5000%
            default:
                return baseHP * 10;
        }
    }

    protected override float GetBaseMeleeAttack()
    {
        switch (Difficulty.Value)
        {
            case BossDifficulty.Beginner:
                return 20f;  // 200%
            case BossDifficulty.Intermediate:
                return 25f;  // 250%
            case BossDifficulty.Advanced:
                return 30f;  // 300%
            case BossDifficulty.Nightmare:
                return 40f;  // 400%
            default:
                return 20f;
        }
    }

    /// <summary>
    /// 광폭화 공격력 배수
    /// </summary>
    [System.Obsolete("Use EnragedDamageMultiplier.Value instead")]
    public float GetEnragedDamageMultiplier()
    {
        return EnragedDamageMultiplier.Value;
    }
}
