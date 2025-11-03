using Akasha;
using UnityEngine;

/// <summary>
/// 병종 타입
/// </summary>
public enum UnitType
{
    Warrior,        // 전사(밀집): 근거리 공격, 보통 기동력, 보통 DPS 지속
    Cavalry,        // 기병: 근거리 공격, 높은 기동력, 보통 DPS 폭딜
    Ranger,         // 레인저: 원거리 공격, 보통 기동력, 낮은 DPS 지속
    Crossbowman,    // 석궁수(밀집): 원거리 공격, 낮은 기동력, 낮은 DPS 관통
    TacticalMage,   // 전술마법사(밀집): 마법 공격, 낮은 기동력, 높은 DPS 폭딜, 광역
    BattleMage,     // 전투마법사: 마법 공격, 낮은 기동력, 높은 DPS 폭딜, 단일
    Priest          // 성직자: 치료
}

/// <summary>
/// 병사 등급
/// </summary>
public enum UnitGrade
{
    Recruit,    // 신병: 기본 능력치
    Elite       // 정예병: 능력치 +100%
}

/// <summary>
/// 병사 Model
/// CharModel + 병종별 특성 + 정예화 시스템
/// </summary>
public class UnitModel : CharModel
{
    // 병종 타입
    public RxVar<UnitType> Type { get; private set; }

    // 병사 등급
    public RxVar<UnitGrade> Grade { get; private set; }
    public RxComputed<float> GradeMultiplier { get; private set; }  // 정예병: 2.0, 신병: 1.0

    // 사기 시스템
    public RxMod<float> Morale { get; private set; }  // 범위: 0 ~ 200, 기본: 100
    public RxComputed<float> MoraleEffect { get; private set; }  // 사기 효과 배수: 0.7 ~ 1.2

    // 사기 효과가 적용된 실제 전투 스탯 (연쇄참조)
    public RxComputed<float> EffectiveMeleeAttack { get; private set; }
    public RxComputed<float> EffectiveRangedAttack { get; private set; }
    public RxComputed<float> EffectiveMagicAttack { get; private set; }
    public RxComputed<float> EffectiveDivineAttack { get; private set; }
    public RxComputed<float> EffectiveMeleeDefense { get; private set; }
    public RxComputed<float> EffectiveRangedDefense { get; private set; }
    public RxComputed<float> EffectiveMagicDefense { get; private set; }
    public RxComputed<float> EffectiveDivineDefense { get; private set; }

    public UnitModel()
    {
        Type = new RxVar<UnitType>(UnitType.Warrior, nameof(Type), this);
        Grade = new RxVar<UnitGrade>(UnitGrade.Recruit, nameof(Grade), this);
        Morale = new RxMod<float>(100f, nameof(Morale), this);

        // 등급 배수: 정예병은 2.0, 신병은 1.0
        GradeMultiplier = new RxComputed<float>(() =>
            Grade.Value == UnitGrade.Elite ? 2.0f : 1.0f,
            nameof(GradeMultiplier), this).DependsOn(Grade);

        // 사기 효과 배수 계산
        MoraleEffect = new RxComputed<float>(() =>
        {
            if (Morale.Value >= 150f)
                return 1.2f;  // 높은 사기: 공격력/방어력 +20%
            else if (Morale.Value <= 50f)
                return 0.7f;  // 낮은 사기: 공격력/방어력 -30%
            else
                return 1.0f;  // 보통 사기
        }, nameof(MoraleEffect), this).DependsOn(Morale);

        // 사기 효과가 적용된 실제 전투 스탯 (MeleeAttack 등 연쇄참조)
        EffectiveMeleeAttack = new RxComputed<float>(() =>
            MeleeAttack.Value * MoraleEffect.Value,
            nameof(EffectiveMeleeAttack), this).DependsOn(MeleeAttack, MoraleEffect);

        EffectiveRangedAttack = new RxComputed<float>(() =>
            RangedAttack.Value * MoraleEffect.Value,
            nameof(EffectiveRangedAttack), this).DependsOn(RangedAttack, MoraleEffect);

        EffectiveMagicAttack = new RxComputed<float>(() =>
            MagicAttack.Value * MoraleEffect.Value,
            nameof(EffectiveMagicAttack), this).DependsOn(MagicAttack, MoraleEffect);

        EffectiveDivineAttack = new RxComputed<float>(() =>
            DivineAttack.Value * MoraleEffect.Value,
            nameof(EffectiveDivineAttack), this).DependsOn(DivineAttack, MoraleEffect);

        EffectiveMeleeDefense = new RxComputed<float>(() =>
            MeleeDefense.Value * MoraleEffect.Value,
            nameof(EffectiveMeleeDefense), this).DependsOn(MeleeDefense, MoraleEffect);

        EffectiveRangedDefense = new RxComputed<float>(() =>
            RangedDefense.Value * MoraleEffect.Value,
            nameof(EffectiveRangedDefense), this).DependsOn(RangedDefense, MoraleEffect);

        EffectiveMagicDefense = new RxComputed<float>(() =>
            MagicDefense.Value * MoraleEffect.Value,
            nameof(EffectiveMagicDefense), this).DependsOn(MagicDefense, MoraleEffect);

        EffectiveDivineDefense = new RxComputed<float>(() =>
            DivineDefense.Value * MoraleEffect.Value,
            nameof(EffectiveDivineDefense), this).DependsOn(DivineDefense, MoraleEffect);

        // 등급 변경 시 스탯 재적용
        Grade.AddListener(OnGradeChanged);
    }

    /// <summary>
    /// 등급 변경 시 스탯 재적용
    /// </summary>
    private void OnGradeChanged(UnitGrade newGrade)
    {
        // 정예병으로 승급 시 모든 스탯 2배
        // 신병으로 강등 시 원래대로
        // (실제로는 Modifier로 처리하는 것이 더 깔끔함)
    }

    /// <summary>
    /// 정예화 (신병 → 정예병)
    /// </summary>
    public void Promote()
    {
        if (Grade.Value == UnitGrade.Recruit)
        {
            Grade.Set(UnitGrade.Elite);
            CurrentHP.Set(MaxHP.Value);
            CurrentMP.Set(MaxMP.Value);
        }
    }

    /// <summary>
    /// 역정예화 (정예병 → 신병)
    /// </summary>
    public void Demote()
    {
        if (Grade.Value == UnitGrade.Elite)
        {
            Grade.Set(UnitGrade.Recruit);
        }
    }

    /// <summary>
    /// 사기 효과 계산
    /// </summary>
    [System.Obsolete("Use MoraleEffect.Value instead")]
    public float GetMoraleEffect()
    {
        return MoraleEffect.Value;
    }

    /// <summary>
    /// 사기 붕괴 확인
    /// </summary>
    public bool IsMoraleCollapsed()
    {
        return Morale.Value <= 0f;
    }

    protected override int GetBaseMaxHP()
    {
        // 병종별 기본 HP
        switch (Type.Value)
        {
            case UnitType.Warrior:
                return 150;
            case UnitType.Cavalry:
                return 120;
            case UnitType.Ranger:
                return 80;
            case UnitType.Crossbowman:
                return 100;
            case UnitType.TacticalMage:
                return 70;
            case UnitType.BattleMage:
                return 70;
            case UnitType.Priest:
                return 90;
            default:
                return 100;
        }
    }

    protected override int GetBaseMaxMP()
    {
        // 병종별 기본 MP
        switch (Type.Value)
        {
            case UnitType.Warrior:
                return 30;
            case UnitType.Cavalry:
                return 30;
            case UnitType.Ranger:
                return 50;
            case UnitType.Crossbowman:
                return 40;
            case UnitType.TacticalMage:
                return 150;
            case UnitType.BattleMage:
                return 120;
            case UnitType.Priest:
                return 100;
            default:
                return 50;
        }
    }
}
