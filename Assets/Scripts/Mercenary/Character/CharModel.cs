using Akasha;
using UnityEngine;

/// <summary>
/// 모든 캐릭터의 기본 Model
/// 7개 기본 스탯 (힘, 제어, 지능, 신성, 민첩, 정신, 행운)을 포함
/// </summary>
public abstract class CharModel : BaseModel
{
    // 기본 스탯 (7개)
    public RxMod<int> Strength { get; private set; }      // 힘: 근접 공격력(대량), HP(대량), 적중력
    public RxMod<int> Control { get; private set; }       // 제어: 원거리 공격력(대량), 적중력(대량), HP
    public RxMod<int> Intelligence { get; private set; }  // 지능: 마법 공격력(대량), 마법 방어력, MP(대량)
    public RxMod<int> Divine { get; private set; }        // 신성: 신성 공격력(대량), 신성 방어력, MP(대량)
    public RxMod<int> Agility { get; private set; }       // 민첩: 근접 방어력, HP 회복력, 마법 방어력(소량)
    public RxMod<int> Spirit { get; private set; }        // 정신: 원거리 방어력, MP 회복력, 신성 방어력(소량)
    public RxMod<int> Luck { get; private set; }          // 행운: 크리티컬, 회피력, 적중력(소량)

    // 전투 스탯 (파생)
    public RxComputed<float> MeleeAttack { get; private set; }      // 근접 공격력
    public RxComputed<float> RangedAttack { get; private set; }     // 원거리 공격력
    public RxComputed<float> MagicAttack { get; private set; }      // 마법 공격력
    public RxComputed<float> DivineAttack { get; private set; }     // 신성 공격력

    public RxComputed<float> MeleeDefense { get; private set; }     // 근접 방어력
    public RxComputed<float> RangedDefense { get; private set; }    // 원거리 방어력
    public RxComputed<float> MagicDefense { get; private set; }     // 마법 방어력
    public RxComputed<float> DivineDefense { get; private set; }    // 신성 방어력

    public RxComputed<int> MaxHP { get; private set; }              // 최대 HP
    public RxComputed<int> MaxMP { get; private set; }              // 최대 MP
    public RxVar<int> CurrentHP { get; private set; }               // 현재 HP
    public RxVar<int> CurrentMP { get; private set; }               // 현재 MP

    public RxComputed<float> HPRecovery { get; private set; }       // HP 회복력
    public RxComputed<float> MPRecovery { get; private set; }       // MP 회복력

    public RxComputed<float> Accuracy { get; private set; }         // 적중력
    public RxComputed<float> Evasion { get; private set; }          // 회피력
    public RxComputed<float> CriticalChance { get; private set; }   // 크리티컬 확률
    public RxComputed<float> CriticalDamage { get; private set; }   // 크리티컬 데미지

    // 레벨 시스템
    public RxVar<int> Level { get; private set; }
    public RxVar<int> Experience { get; private set; }
    public RxComputed<int> RequiredExperience { get; private set; }

    // 상태
    public RxVar<bool> IsAlive { get; private set; }

    public CharModel()
    {
        // 기본 스탯 초기화
        Strength = new RxMod<int>(10, nameof(Strength), this);
        Control = new RxMod<int>(10, nameof(Control), this);
        Intelligence = new RxMod<int>(10, nameof(Intelligence), this);
        Divine = new RxMod<int>(10, nameof(Divine), this);
        Agility = new RxMod<int>(10, nameof(Agility), this);
        Spirit = new RxMod<int>(10, nameof(Spirit), this);
        Luck = new RxMod<int>(10, nameof(Luck), this);

        // 레벨 시스템
        Level = new RxVar<int>(1, nameof(Level), this);
        Experience = new RxVar<int>(0, nameof(Experience), this);

        // 파생 스탯 - 전투 스탯
        MeleeAttack = new RxComputed<float>(() =>
            Strength.Value * 2.5f + GetBaseMeleeAttack(),
            nameof(MeleeAttack), this).DependsOn(Strength);

        RangedAttack = new RxComputed<float>(() =>
            Control.Value * 2.5f + Control.Value * 0.5f + GetBaseRangedAttack(),
            nameof(RangedAttack), this).DependsOn(Control);

        MagicAttack = new RxComputed<float>(() =>
            Intelligence.Value * 2.5f + GetBaseMagicAttack(),
            nameof(MagicAttack), this).DependsOn(Intelligence);

        DivineAttack = new RxComputed<float>(() =>
            Divine.Value * 2.5f + GetBaseDivineAttack(),
            nameof(DivineAttack), this).DependsOn(Divine);

        // 방어력
        MeleeDefense = new RxComputed<float>(() =>
            Agility.Value * 1.5f + GetBaseMeleeDefense(),
            nameof(MeleeDefense), this).DependsOn(Agility);

        RangedDefense = new RxComputed<float>(() =>
            Spirit.Value * 1.5f + GetBaseRangedDefense(),
            nameof(RangedDefense), this).DependsOn(Spirit);

        MagicDefense = new RxComputed<float>(() =>
            Intelligence.Value * 0.5f + Agility.Value * 0.3f + GetBaseMagicDefense(),
            nameof(MagicDefense), this).DependsOn(Intelligence, Agility);

        DivineDefense = new RxComputed<float>(() =>
            Divine.Value * 0.5f + Spirit.Value * 0.3f + GetBaseDivineDefense(),
            nameof(DivineDefense), this).DependsOn(Divine, Spirit);

        // HP/MP
        MaxHP = new RxComputed<int>(() =>
            Strength.Value * 10 + Control.Value * 5 + GetBaseMaxHP(),
            nameof(MaxHP), this).DependsOn(Strength, Control);

        MaxMP = new RxComputed<int>(() =>
            Intelligence.Value * 10 + Divine.Value * 10 + GetBaseMaxMP(),
            nameof(MaxMP), this).DependsOn(Intelligence, Divine);

        CurrentHP = new RxVar<int>(MaxHP.Value, nameof(CurrentHP), this);
        CurrentMP = new RxVar<int>(MaxMP.Value, nameof(CurrentMP), this);

        // 회복력
        HPRecovery = new RxComputed<float>(() =>
            Agility.Value * 0.5f + GetBaseHPRecovery(),
            nameof(HPRecovery), this).DependsOn(Agility);

        MPRecovery = new RxComputed<float>(() =>
            Spirit.Value * 0.5f + GetBaseMPRecovery(),
            nameof(MPRecovery), this).DependsOn(Spirit);

        // 명중/회피/크리티컬
        Accuracy = new RxComputed<float>(() =>
            Strength.Value * 0.5f + Control.Value * 1.0f + Luck.Value * 0.3f + GetBaseAccuracy(),
            nameof(Accuracy), this).DependsOn(Strength, Control, Luck);

        Evasion = new RxComputed<float>(() =>
            Luck.Value * 1.0f + GetBaseEvasion(),
            nameof(Evasion), this).DependsOn(Luck);

        CriticalChance = new RxComputed<float>(() =>
            5f + Luck.Value * 0.2f + GetBaseCriticalChance(),
            nameof(CriticalChance), this).DependsOn(Luck);

        CriticalDamage = new RxComputed<float>(() =>
            150f + GetBaseCriticalDamage(),
            nameof(CriticalDamage), this);

        // 경험치 요구량
        RequiredExperience = new RxComputed<int>(() =>
            Level.Value * 100,
            nameof(RequiredExperience), this).DependsOn(Level);

        // 상태
        IsAlive = new RxVar<bool>(true, nameof(IsAlive), this);

        // HP 변경 시 생존 여부 업데이트
        CurrentHP.AddListener(hp =>
        {
            IsAlive.Set(hp > 0);
        });
    }

    // 서브클래스에서 오버라이드 가능한 기본값 메서드들
    protected virtual float GetBaseMeleeAttack() => 0f;
    protected virtual float GetBaseRangedAttack() => 0f;
    protected virtual float GetBaseMagicAttack() => 0f;
    protected virtual float GetBaseDivineAttack() => 0f;

    protected virtual float GetBaseMeleeDefense() => 0f;
    protected virtual float GetBaseRangedDefense() => 0f;
    protected virtual float GetBaseMagicDefense() => 0f;
    protected virtual float GetBaseDivineDefense() => 0f;

    protected virtual int GetBaseMaxHP() => 100;
    protected virtual int GetBaseMaxMP() => 50;

    protected virtual float GetBaseHPRecovery() => 1f;
    protected virtual float GetBaseMPRecovery() => 1f;

    protected virtual float GetBaseAccuracy() => 85f;
    protected virtual float GetBaseEvasion() => 5f;
    protected virtual float GetBaseCriticalChance() => 0f;
    protected virtual float GetBaseCriticalDamage() => 0f;

    /// <summary>
    /// 경험치 획득
    /// </summary>
    public void GainExperience(int amount)
    {
        Experience.Set(Experience.Value + amount);

        while (Experience.Value >= RequiredExperience.Value)
        {
            LevelUp();
        }
    }

    /// <summary>
    /// 레벨업 처리
    /// </summary>
    protected virtual void LevelUp()
    {
        Experience.Set(Experience.Value - RequiredExperience.Value);
        Level.Set(Level.Value + 1);

        // HP/MP 회복
        CurrentHP.Set(MaxHP.Value);
        CurrentMP.Set(MaxMP.Value);

        OnLevelUp();
    }

    /// <summary>
    /// 레벨업 시 호출되는 가상 메서드 (서브클래스에서 오버라이드)
    /// </summary>
    protected virtual void OnLevelUp()
    {
    }

    /// <summary>
    /// 데미지 받기
    /// </summary>
    public void TakeDamage(int damage)
    {
        int newHP = Mathf.Max(0, CurrentHP.Value - damage);
        CurrentHP.Set(newHP);
    }

    /// <summary>
    /// 치유
    /// </summary>
    public void Heal(int amount)
    {
        int newHP = Mathf.Min(MaxHP.Value, CurrentHP.Value + amount);
        CurrentHP.Set(newHP);
    }

    /// <summary>
    /// MP 소모
    /// </summary>
    public bool ConsumeMP(int amount)
    {
        if (CurrentMP.Value < amount)
            return false;

        CurrentMP.Set(CurrentMP.Value - amount);
        return true;
    }

    /// <summary>
    /// MP 회복
    /// </summary>
    public void RestoreMP(int amount)
    {
        int newMP = Mathf.Min(MaxMP.Value, CurrentMP.Value + amount);
        CurrentMP.Set(newMP);
    }
}
