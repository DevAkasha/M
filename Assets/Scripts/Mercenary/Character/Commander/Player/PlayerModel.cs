using Akasha;
using UnityEngine;

/// <summary>
/// 플레이어 (용병단장) Model
/// CharModel + 정밀/통찰 버프 스탯
/// </summary>
public sealed class PlayerModel : CharModel
{
    // 버프 스탯 (플레이어 전용)
    public RxVar<int> Precision { get; private set; }    // 정밀: 제어 + 지능 + 민첩 버프
    public RxVar<int> Insight { get; private set; }      // 통찰: 힘 + 정신 + 신성 버프

    // 계수
    public RxComputed<float> PrecisionCoefficient { get; private set; }  // 정밀 계수 (던전용)
    public RxComputed<float> InsightCoefficient { get; private set; }    // 통찰 계수 (전장용)

    // 전술 영역 (전장)
    public RxComputed<float> TacticalRange { get; private set; }         // 통찰 × 10m

    // 스탯 포인트 (레벨업 시 획득)
    public RxVar<int> AvailableStatPoints { get; private set; }
    public RxVar<int> AvailableBuffStatPoints { get; private set; }

    // 이동 속도
    public RxMod<float> MoveSpeed { get; private set; }

    public PlayerModel()
    {
        // 버프 스탯
        Precision = new RxVar<int>(0, nameof(Precision), this);
        Insight = new RxVar<int>(0, nameof(Insight), this);

        // 스탯 포인트
        AvailableStatPoints = new RxVar<int>(0, nameof(AvailableStatPoints), this);
        AvailableBuffStatPoints = new RxVar<int>(0, nameof(AvailableBuffStatPoints), this);

        // 이동 속도
        MoveSpeed = new RxMod<float>(5f, nameof(MoveSpeed), this);

        // 정밀 계수 계산: (정밀 × 2) + (제어 + 지능 + 민첩) × 0.5
        PrecisionCoefficient = new RxComputed<float>(() =>
            (Precision.Value * 2f) + (Control.Value + Intelligence.Value + Agility.Value) * 0.5f,
            nameof(PrecisionCoefficient), this).DependsOn(Precision, Control, Intelligence, Agility);

        // 통찰 계수 계산: (통찰 × 2) + (힘 + 정신 + 신성) × 0.3
        InsightCoefficient = new RxComputed<float>(() =>
            (Insight.Value * 2f) + (Strength.Value + Spirit.Value + Divine.Value) * 0.3f,
            nameof(InsightCoefficient), this).DependsOn(Insight, Strength, Spirit, Divine);

        // 전술 영역 반경: 통찰 × 10m
        TacticalRange = new RxComputed<float>(() =>
            Insight.Value * 10f,
            nameof(TacticalRange), this).DependsOn(Insight);
    }

    protected override void OnLevelUp()
    {
        base.OnLevelUp();

        // 레벨업 시 스탯 포인트 부여
        AvailableStatPoints.Set(AvailableStatPoints.Value + 5);      // 기본 스탯 5포인트
        AvailableBuffStatPoints.Set(AvailableBuffStatPoints.Value + 1); // 버프 스탯 1포인트
    }

    /// <summary>
    /// 기본 스탯에 포인트 할당
    /// </summary>
    public bool AllocateStatPoint(StatType statType, int points = 1)
    {
        if (AvailableStatPoints.Value < points)
            return false;

        switch (statType)
        {
            case StatType.Strength:
                Strength.Set(Strength.Value + points);
                break;
            case StatType.Control:
                Control.Set(Control.Value + points);
                break;
            case StatType.Intelligence:
                Intelligence.Set(Intelligence.Value + points);
                break;
            case StatType.Divine:
                Divine.Set(Divine.Value + points);
                break;
            case StatType.Agility:
                Agility.Set(Agility.Value + points);
                break;
            case StatType.Spirit:
                Spirit.Set(Spirit.Value + points);
                break;
            case StatType.Luck:
                Luck.Set(Luck.Value + points);
                break;
            default:
                return false;
        }

        AvailableStatPoints.Set(AvailableStatPoints.Value - points);
        return true;
    }

    /// <summary>
    /// 버프 스탯에 포인트 할당
    /// </summary>
    public bool AllocateBuffStatPoint(BuffStatType statType, int points = 1)
    {
        if (AvailableBuffStatPoints.Value < points)
            return false;

        switch (statType)
        {
            case BuffStatType.Precision:
                Precision.Set(Precision.Value + points);
                break;
            case BuffStatType.Insight:
                Insight.Set(Insight.Value + points);
                break;
            default:
                return false;
        }

        AvailableBuffStatPoints.Set(AvailableBuffStatPoints.Value - points);
        return true;
    }
}

/// <summary>
/// 기본 스탯 타입
/// </summary>
public enum StatType
{
    Strength,
    Control,
    Intelligence,
    Divine,
    Agility,
    Spirit,
    Luck
}

/// <summary>
/// 버프 스탯 타입
/// </summary>
public enum BuffStatType
{
    Precision,
    Insight
}
