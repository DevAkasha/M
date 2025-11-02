using Akasha;
using UnityEngine;

/// <summary>
/// 부대장 타입
/// </summary>
public enum CommanderType
{
    Warrior,    // 전사계열: 근접 공격
    Archer,     // 궁수계열: 원거리 공격
    Mage,       // 마법계열: 마법 공격
    Priest      // 성직자계열: 힐 & 신성 공격
}

/// <summary>
/// 부대장 Model
/// CharModel + 타입별 성장률 + 환생 시스템
/// </summary>
public class CommanderModel : CharModel
{
    // 부대장 타입
    public RxVar<CommanderType> Type { get; private set; }

    // 환생 시스템
    public RxVar<int> ReincarnationCount { get; private set; }
    public RxComputed<float> GrowthRateBonus { get; private set; }  // 환생 횟수 × 10%

    // 스킬 슬롯
    public RxVar<int> UnlockedSkillSlots { get; private set; }

    public CommanderModel()
    {
        Type = new RxVar<CommanderType>(CommanderType.Warrior, nameof(Type), this);
        ReincarnationCount = new RxVar<int>(0, nameof(ReincarnationCount), this);
        UnlockedSkillSlots = new RxVar<int>(1, nameof(UnlockedSkillSlots), this);

        // 성장률 보너스: 환생 횟수 × 10%
        GrowthRateBonus = new RxComputed<float>(() =>
            ReincarnationCount.Value * 10f,
            nameof(GrowthRateBonus), this).DependsOn(ReincarnationCount);

        // 레벨에 따른 스킬 슬롯 해금
        Level.AddListener(level =>
        {
            if (level >= 10 && UnlockedSkillSlots.Value < 2)
                UnlockedSkillSlots.Set(2);
            else if (level >= 20 && UnlockedSkillSlots.Value < 3)
                UnlockedSkillSlots.Set(3);
            else if (level >= 30 && UnlockedSkillSlots.Value < 4)
                UnlockedSkillSlots.Set(4);
        });
    }

    protected override void OnLevelUp()
    {
        base.OnLevelUp();

        // 타입별 성장률 적용
        ApplyGrowthRate();
    }

    /// <summary>
    /// 타입별 성장률 적용
    /// </summary>
    private void ApplyGrowthRate()
    {
        float growthMultiplier = 1f + (GrowthRateBonus.Value / 100f);

        switch (Type.Value)
        {
            case CommanderType.Warrior:
                // 힘: 2.5/레벨, 민첩: 1.5/레벨, 기타: 0.5/레벨
                Strength.Set(Strength.Value + Mathf.RoundToInt(2.5f * growthMultiplier));
                Agility.Set(Agility.Value + Mathf.RoundToInt(1.5f * growthMultiplier));
                Control.Set(Control.Value + Mathf.RoundToInt(0.5f * growthMultiplier));
                Intelligence.Set(Intelligence.Value + Mathf.RoundToInt(0.5f * growthMultiplier));
                Divine.Set(Divine.Value + Mathf.RoundToInt(0.5f * growthMultiplier));
                Spirit.Set(Spirit.Value + Mathf.RoundToInt(0.5f * growthMultiplier));
                Luck.Set(Luck.Value + Mathf.RoundToInt(0.5f * growthMultiplier));
                break;

            case CommanderType.Archer:
                // 제어: 2.5/레벨, 정신: 1.5/레벨, 기타: 0.5/레벨
                Control.Set(Control.Value + Mathf.RoundToInt(2.5f * growthMultiplier));
                Spirit.Set(Spirit.Value + Mathf.RoundToInt(1.5f * growthMultiplier));
                Strength.Set(Strength.Value + Mathf.RoundToInt(0.5f * growthMultiplier));
                Intelligence.Set(Intelligence.Value + Mathf.RoundToInt(0.5f * growthMultiplier));
                Divine.Set(Divine.Value + Mathf.RoundToInt(0.5f * growthMultiplier));
                Agility.Set(Agility.Value + Mathf.RoundToInt(0.5f * growthMultiplier));
                Luck.Set(Luck.Value + Mathf.RoundToInt(0.5f * growthMultiplier));
                break;

            case CommanderType.Mage:
                // 지능: 2.5/레벨, 민첩: 1.0/레벨, 기타: 0.5/레벨
                Intelligence.Set(Intelligence.Value + Mathf.RoundToInt(2.5f * growthMultiplier));
                Agility.Set(Agility.Value + Mathf.RoundToInt(1.0f * growthMultiplier));
                Strength.Set(Strength.Value + Mathf.RoundToInt(0.5f * growthMultiplier));
                Control.Set(Control.Value + Mathf.RoundToInt(0.5f * growthMultiplier));
                Divine.Set(Divine.Value + Mathf.RoundToInt(0.5f * growthMultiplier));
                Spirit.Set(Spirit.Value + Mathf.RoundToInt(0.5f * growthMultiplier));
                Luck.Set(Luck.Value + Mathf.RoundToInt(0.5f * growthMultiplier));
                break;

            case CommanderType.Priest:
                // 신성: 2.5/레벨, 정신: 1.5/레벨, 기타: 0.5/레벨
                Divine.Set(Divine.Value + Mathf.RoundToInt(2.5f * growthMultiplier));
                Spirit.Set(Spirit.Value + Mathf.RoundToInt(1.5f * growthMultiplier));
                Strength.Set(Strength.Value + Mathf.RoundToInt(0.5f * growthMultiplier));
                Control.Set(Control.Value + Mathf.RoundToInt(0.5f * growthMultiplier));
                Intelligence.Set(Intelligence.Value + Mathf.RoundToInt(0.5f * growthMultiplier));
                Agility.Set(Agility.Value + Mathf.RoundToInt(0.5f * growthMultiplier));
                Luck.Set(Luck.Value + Mathf.RoundToInt(0.5f * growthMultiplier));
                break;
        }
    }

    /// <summary>
    /// 환생 (레벨 1로 초기화, 성장률 증가)
    /// </summary>
    public void Reincarnate()
    {
        // 레벨 1로 초기화
        Level.Set(1);
        Experience.Set(0);

        // 스탯 초기화
        Strength.Set(10);
        Control.Set(10);
        Intelligence.Set(10);
        Divine.Set(10);
        Agility.Set(10);
        Spirit.Set(10);
        Luck.Set(10);

        // 환생 횟수 증가
        ReincarnationCount.Set(ReincarnationCount.Value + 1);

        // HP/MP 회복
        CurrentHP.Set(MaxHP.Value);
        CurrentMP.Set(MaxMP.Value);

        // 스킬 슬롯 초기화
        UnlockedSkillSlots.Set(1);
    }

    /// <summary>
    /// 환생 비용 계산 (영혼석)
    /// </summary>
    public int GetReincarnationCost()
    {
        return 100 * (int)Mathf.Pow(2, ReincarnationCount.Value);
    }
}
