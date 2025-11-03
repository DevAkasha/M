# 아카샤 프레임워크 API 문서

## 목차

1. [개요](#개요)
2. [반응형 프로퍼티 시스템](#반응형-프로퍼티-시스템)
   - [RxVar&lt;T&gt;](#rxvart)
   - [RxMod&lt;T&gt;](#rxmodt)
   - [RxComputed&lt;T&gt;](#rxcomputedt)
   - [RxFlag](#rxflag)
   - [RxFlagSet&lt;TEnum&gt;](#rxflagsettenu)
   - [RxFlagState&lt;TState&gt;](#rxflagstatetstate)
3. [애그리게이트 시스템](#애그리게이트-시스템)
   - [BaseModel](#basemodel)
   - [Controller](#controller)
   - [Entity](#entity)
   - [Part](#part)
4. [관리자 시스템](#관리자-시스템)
   - [DataManager](#datamanager)
   - [FactoryManager](#factorymanager)
   - [ControllerManager](#controllermanager)
5. [인터페이스 및 유틸리티](#인터페이스-및-유틸리티)

---

## 개요

아카샤 프레임워크는 Unity 기반의 게임 개발 프레임워크로, 반응형 프로퍼티 시스템과 애그리게이트 패턴을 중심으로 설계되었습니다.

### 주요 특징

- **반응형 프로퍼티 시스템**: 데이터 변경 감지 및 자동 업데이트
- **애그리게이트 패턴**: Controller, Entity, Model, Part로 구성된 모듈화된 구조
- **모디파이어 시스템**: 게임 스탯 계산에 최적화된 RxMod
- **객체 풀링**: 효율적인 메모리 관리
- **데이터 기반 개발**: Google Sheets 연동을 통한 데이터 주도 개발

---

## 반응형 프로퍼티 시스템

반응형 프로퍼티는 값의 변경을 자동으로 감지하고 구독자에게 알림을 전달하는 시스템입니다.

### RxVar&lt;T&gt;

일반적인 필드에 사용되는 기본 반응형 프로퍼티입니다.

#### 생성자

```csharp
public RxVar(T initialValue = default!, string? fieldName = null, IRxOwner? owner = null)
```

**매개변수:**
- `initialValue`: 초기값 (기본값: default)
- `fieldName`: 필드 이름 (선택 사항)
- `owner`: 소유자 객체 (선택 사항, IRxOwner 인터페이스를 구현한 객체)

**제약 사항:**
- owner는 `IsRxVarOwner`가 `true`여야 합니다.

#### 주요 프로퍼티

```csharp
public T Value { get; }
```

현재 값을 반환합니다.

#### 주요 메서드

```csharp
public void Set(T newValue)
```

값을 설정합니다. 값이 변경되면 모든 리스너에게 알림을 전송합니다.

```csharp
public void SetValue(T newValue, IRxCaller caller)
```

Caller 검증과 함께 값을 설정합니다.

**매개변수:**
- `newValue`: 새로운 값
- `caller`: 호출자 (IRxCaller, `IsMultiRolesCaller`가 `true`여야 함)

```csharp
public void AddListener(Action<T> listener)
```

값 변경 리스너를 추가합니다. 추가 즉시 현재 값으로 호출됩니다.

```csharp
public void RemoveListener(Action<T> listener)
```

값 변경 리스너를 제거합니다.

#### 사용 예시

```csharp
// BaseModel에서 사용
public class PlayerModel : BaseModel
{
    public RxVar<int> Level;
    public RxVar<string> Name;

    public PlayerModel()
    {
        Level = new RxVar<int>(1, "Level", this);
        Name = new RxVar<string>("Player", "Name", this);
    }
}

// 값 변경
playerModel.Level.Set(5);

// 리스너 등록
playerModel.Level.AddListener(newLevel => {
    Debug.Log($"Level changed to {newLevel}");
});
```

---

### RxMod&lt;T&gt;

Effect의 대상 필드에 사용되는 반응형 프로퍼티로, 주로 스탯 필드에 사용됩니다.

#### 제네릭 제약

```csharp
where T : struct, IConvertible
```

- T는 struct이어야 하며 IConvertible을 구현해야 합니다 (int, float 등).

#### 생성자

```csharp
public RxMod(T initialValue = default, string? fieldName = null, IRxOwner? owner = null)
```

**매개변수:**
- `initialValue`: 기본값 (baseValue)
- `fieldName`: 필드 이름
- `owner`: 소유자 객체 (IRxOwner, `IsRxAllOwner`가 `true`여야 함)

#### 주요 프로퍼티

```csharp
public T Value { get; }
```

모디파이어가 적용된 최종 계산 값을 반환합니다.

#### 주요 메서드

```csharp
public void Set(T value)
```

기본값(baseValue)을 설정합니다. 설정 후 자동으로 재계산됩니다.

```csharp
public void AddModifier(string key, ModifierType type, float value, float? duration = null, bool stackable = false)
```

모디파이어를 추가합니다.

**매개변수:**
- `key`: 모디파이어 고유 키
- `type`: 모디파이어 타입 (OriginAdd, AddMultiplier, Multiplier, FinalAdd)
- `value`: 모디파이어 값
- `duration`: 지속 시간 (초 단위, null이면 영구)
- `stackable`: 스택 가능 여부 (true면 누적, false면 교체)

```csharp
public void AddModifierWithPolicy(string key, ModifierType type, float value, float? duration = null, Func<float, float, float>? policy = null)
```

정책 함수를 사용하여 모디파이어를 추가합니다.

**매개변수:**
- `policy`: 기존 값과 새 값을 조합하는 함수 (예: ModifierPolicy.TakeMax)

```csharp
public void RemoveModifier(string key)
```

특정 모디파이어를 제거합니다.

```csharp
public void RemoveAllModifiers()
```

모든 모디파이어를 제거합니다.

```csharp
public bool HasModifier(string key)
```

특정 모디파이어가 존재하는지 확인합니다.

```csharp
public int GetStackCount(string key)
```

특정 모디파이어의 스택 개수를 반환합니다.

```csharp
public T GetBaseValue()
```

모디파이어가 적용되기 전의 기본값을 반환합니다.

#### ModifierType

```csharp
public enum ModifierType
{
    OriginAdd,      // 기본값에 더하기
    AddMultiplier,  // 더한 값에 곱하기
    Multiplier,     // 전체에 곱하기
    FinalAdd        // 마지막에 더하기
}
```

**계산 공식:**
```
최종값 = (baseValue + OriginAdd) × AddMultiplier × Multiplier + FinalAdd
```

#### ModifierPolicy

미리 정의된 정책 함수들:

```csharp
public static class ModifierPolicy
{
    public static readonly Func<float, float, float> TakeMax;     // 최댓값 선택
    public static readonly Func<float, float, float> TakeMin;     // 최솟값 선택
    public static readonly Func<float, float, float> Add;         // 더하기
    public static readonly Func<float, float, float> Replace;     // 교체
    public static readonly Func<float, float, float> KeepFirst;   // 첫 값 유지
    public static readonly Func<float, float, float> Average;     // 평균
}
```

#### 사용 예시

```csharp
public class CharacterModel : BaseModel
{
    public RxMod<float> Health;
    public RxMod<float> Damage;

    public CharacterModel()
    {
        Health = new RxMod<float>(100f, "Health", this);
        Damage = new RxMod<float>(10f, "Damage", this);
    }
}

// 모디파이어 추가
character.Health.AddModifier("buff_health", ModifierType.OriginAdd, 50f, 10f); // 10초 동안 +50
character.Damage.AddModifier("weapon_bonus", ModifierType.AddMultiplier, 0.5f); // +50% 데미지

// 정책을 사용한 모디파이어
character.Health.AddModifierWithPolicy("shield", ModifierType.FinalAdd, 100f, policy: ModifierPolicy.TakeMax);

// 모디파이어 제거
character.Health.RemoveModifier("buff_health");
```

---

### RxComputed&lt;T&gt;

RxVar, RxMod, 또는 다른 RxComputed를 이용한 파생형 필드에 사용되는 반응형 프로퍼티입니다.

#### 생성자

```csharp
public RxComputed(Func<T> compute, string? fieldName, IRxOwner owner)
```

**매개변수:**
- `compute`: 값을 계산하는 함수
- `fieldName`: 필드 이름
- `owner`: 소유자 객체 (IRxOwner, `IsRxAllOwner`가 `true`여야 함, 일반적으로 Model)

#### 주요 프로퍼티

```csharp
public T Value { get; }
```

계산된 값을 반환합니다. 처음 접근 시 lazy initialization됩니다.

#### 주요 메서드

##### 연쇄참조 메서드

```csharp
public RxComputed<T> DependsOn(IRxDependency? dependency)
```

IRxDependency 의존성을 추가합니다. **RxData뿐만 아니라 다른 RxComputed도 의존 가능**합니다.

**매개변수:**
- `dependency`: IRxDependency (RxData 또는 RxComputed)

**예외:**
- `InvalidOperationException`: 순환참조가 감지된 경우 (개발 환경에서만)

**반환값:**
- this (메서드 체이닝 지원)

```csharp
public RxComputed<T> DependsOn(params IRxDependency[] dependencies)
```

여러 IRxDependency 의존성을 추가합니다.

##### 리스너 관리

```csharp
public void AddListener(Action<T> listener)
```

값 변경 리스너를 추가합니다.

```csharp
public void RemoveListener(Action<T> listener)
```

값 변경 리스너를 제거합니다.

#### 주요 특징

- **Lazy Initialization**: 처음 Value에 접근할 때 계산됩니다.
- **자동 재계산**: 의존하는 필드가 변경되면 자동으로 재계산됩니다.
- **연쇄참조 지원**: RxComputed가 다른 RxComputed를 의존할 수 있습니다.
- **순환참조 검증**: DFS + Recursion Stack 알고리즘으로 순환참조를 자동 감지합니다.
- **고성능**: 방문 캐시, 리플렉션 캐싱, 증분 검증으로 최적화되었습니다.
- **조건부 검증**: 개발 환경에서만 순환참조를 검증하여 릴리즈 빌드의 성능 영향을 제거합니다.
- **동일 Model 내 의존성 권장**: 다른 Model의 필드에 의존하면 경고가 발생합니다.

#### 순환참조 검증

RxComputed는 의존성 추가 시 자동으로 순환참조를 검증합니다:

**검증 시점:**
- 개발 환경 (`UNITY_EDITOR` 또는 `DEVELOPMENT_BUILD`)에서만 활성화
- 릴리즈 빌드에서는 완전히 제거되어 오버헤드 없음

**검증 알고리즘:**
- DFS(Depth-First Search) + Recursion Stack 방식
- 시간복잡도: O(V + E) (V: 노드 수, E: 간선 수)
- 증분 검증: 새로운 의존성이 현재 노드로 돌아오는 경로만 체크

**성능 최적화:**
1. **방문 캐시**: 중복 탐색 방지 (HashSet 사용)
2. **리플렉션 캐싱**: 타입별 FieldInfo를 캐싱하여 리플렉션 비용 최소화
3. **증분 검증**: 전체 그래프 재구축 없이 새 의존성만 검증
4. **조건부 컴파일**: 릴리즈 빌드에서 완전 제거

**순환참조 예시:**
```csharp
// ❌ 직접 순환참조 (A → B → A)
computedA.DependsOn(computedB);
computedB.DependsOn((computedA); // 예외 발생!

// ❌ 간접 순환참조 (A → B → C → A)
computedA.DependsOn(computedB);
computedB.DependsOn(computedC);
computedC.DependsOn(computedA); // 예외 발생!

// ❌ 자기 참조 (A → A)
computedA.DependsOn(computedA); // 예외 발생!
```

#### 사용 예시

##### 연쇄참조 사용 (RxComputed 의존)

```csharp
public class CharacterModel : BaseModel
{
    public RxVar<int> Level;
    public RxMod<float> BaseHealth;
    public RxMod<float> BaseMana;

    // 1단계: 기본 스탯 계산
    public RxComputed<float> TotalHealth;
    public RxComputed<float> TotalMana;

    // 2단계: 파생 스탯 계산 (RxComputed 의존)
    public RxComputed<float> CombatPower;
    public RxComputed<bool> IsLowResource;

    public override void AtInit()
    {
        Level = new RxVar<int>(1, "Level", this);
        BaseHealth = new RxMod<float>(100f, "BaseHealth", this);
        BaseMana = new RxMod<float>(50f, "BaseMana", this);

        // 1단계 계산
        TotalHealth = new RxComputed<float>(
            () => BaseHealth.Value + Level.Value * 10,
            "TotalHealth",
            this
        ).DependsOn(BaseHealth, Level);

        TotalMana = new RxComputed<float>(
            () => BaseMana.Value + Level.Value * 5,
            "TotalMana",
            this
        ).DependsOn(BaseMana, Level);

        // 2단계 계산: RxComputed를 의존 (연쇄참조)
        CombatPower = new RxComputed<float>(
            () => TotalHealth.Value * 0.5f + TotalMana.Value * 0.3f,
            "CombatPower",
            this
        ).DependsOn((IRxDependency)TotalHealth, TotalMana);

        IsLowResource = new RxComputed<bool>(
            () => TotalHealth.Value < 30 || TotalMana.Value < 20,
            "IsLowResource",
            this
        ).DependsOn((IRxDependency)TotalHealth, TotalMana);
    }
}

// Level 변경 시 연쇄적으로 모두 재계산됨
character.Level.Set(10);
// → TotalHealth, TotalMana 재계산
// → CombatPower, IsLowResource 재계산
```

##### 복잡한 연쇄참조

```csharp
public class ComplexModel : BaseModel
{
    public RxVar<int> Strength;
    public RxVar<int> Intelligence;
    public RxMod<float> BaseAttack;
    public RxMod<float> BaseMagic;

    // 1단계
    public RxComputed<float> PhysicalPower;
    public RxComputed<float> MagicalPower;

    // 2단계
    public RxComputed<float> HybridPower;

    // 3단계
    public RxComputed<string> PowerRank;

    public override void AtInit()
    {
        Strength = new RxVar<int>(10, "Strength", this);
        Intelligence = new RxVar<int>(8, "Intelligence", this);
        BaseAttack = new RxMod<float>(20f, "BaseAttack", this);
        BaseMagic = new RxMod<float>(15f, "BaseMagic", this);

        // 1단계: 기본 계산
        PhysicalPower = new RxComputed<float>(
            () => BaseAttack.Value + Strength.Value * 2,
            "PhysicalPower",
            this
        ).DependsOn(BaseAttack, Strength);

        MagicalPower = new RxComputed<float>(
            () => BaseMagic.Value + Intelligence.Value * 2,
            "MagicalPower",
            this
        ).DependsOn(BaseMagic, Intelligence);

        // 2단계: 1단계 결과 활용
        HybridPower = new RxComputed<float>(
            () => PhysicalPower.Value * 0.6f + MagicalPower.Value * 0.4f,
            "HybridPower",
            this
        ).DependsOn((IRxDependency)PhysicalPower, MagicalPower);

        // 3단계: 2단계 결과 활용
        PowerRank = new RxComputed<string>(
            () => HybridPower.Value switch
            {
                >= 100 => "S",
                >= 80 => "A",
                >= 60 => "B",
                >= 40 => "C",
                _ => "D"
            },
            "PowerRank",
            this
        ).DependsOn(HybridPower);
    }
}

// Strength 변경 시 3단계 연쇄 재계산
model.Strength.Set(20);
// → PhysicalPower 재계산
// → HybridPower 재계산
// → PowerRank 재계산
```

##### 서로 다른 타입 간 연쇄참조

```csharp
public class UIModel : BaseModel
{
    public RxVar<int> CurrentHealth;
    public RxVar<int> MaxHealth;
    public RxComputed<float> HealthPercent;
    public RxComputed<string> HealthText;
    public RxComputed<Color> HealthColor;

    public override void AtInit()
    {
        CurrentHealth = new RxVar<int>(80, "CurrentHealth", this);
        MaxHealth = new RxVar<int>(100, "MaxHealth", this);

        // float 타입
        HealthPercent = new RxComputed<float>(
            () => (float)CurrentHealth.Value / MaxHealth.Value * 100f,
            "HealthPercent",
            this
        ).DependsOn(CurrentHealth, MaxHealth);

        // string 타입 (float RxComputed 의존)
        HealthText = new RxComputed<string>(
            () => $"{CurrentHealth.Value} / {MaxHealth.Value} ({HealthPercent.Value:F1}%)",
            "HealthText",
            this
        ).DependsOn(HealthPercent);

        // Color 타입 (float RxComputed 의존)
        HealthColor = new RxComputed<Color>(
            () => HealthPercent.Value switch
            {
                >= 70 => Color.green,
                >= 30 => Color.yellow,
                _ => Color.red
            },
            "HealthColor",
            this
        ).DependsOn(HealthPercent);
    }
}
```

#### 성능 참고

**개발 환경 (순환참조 검증 활성화):**
- 단순 체인 (5단계): ~0ms
- 복잡한 구조 (20개 노드): ~0-1ms
- 50개 체인: ~4ms
- 서로 다른 타입 (리플렉션): ~7ms

**릴리즈 빌드 (순환참조 검증 비활성화):**
- 오버헤드: **0ms** (완전히 제거됨)

따라서 연쇄참조를 자유롭게 사용해도 성능에 영향이 없습니다.

---

### RxFlag

bool 기반 플래그를 다루기 위한 반응형 프로퍼티입니다.

#### 생성자

```csharp
internal RxFlag(string name, IRxOwner owner)
```

**참고:**
- RxFlag는 일반적으로 직접 생성하지 않고 `RxFlagSet`을 통해 생성됩니다.

#### 주요 프로퍼티

```csharp
public string Name { get; }
```

플래그 이름을 반환합니다.

```csharp
public bool Value { get; }
```

현재 플래그 값을 반환합니다.

```csharp
public event Action<bool>? OnChanged;
```

값이 변경될 때 발생하는 이벤트입니다.

#### 주요 메서드

```csharp
public void Set(bool value)
```

플래그 값을 설정합니다.

**제약 사항:**
- condition이 설정된 플래그에는 사용할 수 없습니다.

```csharp
public void SetValue(bool value, IRxCaller caller)
```

Caller 검증과 함께 플래그 값을 설정합니다.

```csharp
public void AddListener(Action<bool> listener)
```

값 변경 리스너를 추가합니다. 추가 즉시 현재 값으로 호출됩니다.

```csharp
public void RemoveListener(Action<bool> listener)
```

값 변경 리스너를 제거합니다.

#### 내부 메서드

```csharp
internal void Evaluate()
```

condition 함수를 평가하여 플래그 값을 업데이트합니다.

```csharp
internal void SetCondition(Func<bool>? newCondition)
```

condition 함수를 설정합니다. condition이 설정되면 `Set()` 메서드를 사용할 수 없습니다.

---

### RxFlagSet&lt;TEnum&gt;

Enum을 사용한 플래그 묶음을 다루기 위한 반응형 프로퍼티입니다.

#### 제네릭 제약

```csharp
where TEnum : Enum
```

#### 생성자

```csharp
public RxFlagSet(IRxOwner owner)
```

**매개변수:**
- `owner`: 소유자 객체 (IRxOwner, `IsRxAllOwner`가 `true`여야 함)

#### 인덱서

```csharp
public RxFlag this[TEnum state] { get; }
```

특정 Enum 값에 해당하는 RxFlag를 반환합니다.

#### 주요 메서드

```csharp
public void SetValue(TEnum state, bool value)
```

특정 플래그의 값을 설정합니다.

```csharp
public void SetValue(TEnum state, bool value, IRxCaller caller)
```

Caller 검증과 함께 플래그 값을 설정합니다.

```csharp
public bool GetValue(TEnum state)
```

특정 플래그의 값을 반환합니다.

```csharp
public void Evaluate(TEnum state)
```

특정 플래그의 condition을 평가합니다.

```csharp
public void EvaluateAll()
```

모든 플래그의 condition을 평가합니다.

```csharp
public void SetCondition(TEnum state, Func<bool>? condition)
```

특정 플래그에 condition 함수를 설정합니다.

```csharp
public void AddListener(TEnum state, Action<bool> listener)
```

특정 플래그에 리스너를 추가합니다.

```csharp
public void RemoveListener(TEnum state, Action<bool> listener)
```

특정 플래그에서 리스너를 제거합니다.

#### 조건 검사 메서드

```csharp
public bool AnyActive()
```

하나 이상의 플래그가 활성화되어 있는지 확인합니다.

```csharp
public bool AnyActive(params TEnum[] subset)
```

지정된 플래그들 중 하나 이상이 활성화되어 있는지 확인합니다.

```csharp
public bool AllSatisfied()
```

모든 플래그가 활성화되어 있는지 확인합니다.

```csharp
public bool AllSatisfied(params TEnum[] subset)
```

지정된 플래그들이 모두 활성화되어 있는지 확인합니다.

```csharp
public bool NoneActive()
```

모든 플래그가 비활성화되어 있는지 확인합니다.

```csharp
public bool NoneActive(params TEnum[] subset)
```

지정된 플래그들이 모두 비활성화되어 있는지 확인합니다.

#### 유틸리티 메서드

```csharp
public IEnumerable<(TEnum, bool)> Snapshot()
```

모든 플래그의 현재 상태 스냅샷을 반환합니다.

```csharp
public IEnumerable<TEnum> ActiveFlags()
```

활성화된 플래그들의 Enum 값을 반환합니다.

#### 사용 예시

```csharp
public enum PlayerCondition
{
    CanMove,
    CanAttack,
    CanUseSkill,
    IsStunned
}

public class PlayerModel : BaseModel
{
    public RxFlagSet<PlayerCondition> Conditions;

    public PlayerModel()
    {
        Conditions = new RxFlagSet<PlayerCondition>(this);

        // 초기 설정
        Conditions.SetValue(PlayerCondition.CanMove, true);
        Conditions.SetValue(PlayerCondition.CanAttack, true);
    }
}

// 조건부 플래그 설정
player.Conditions.SetCondition(PlayerCondition.CanMove,
    () => !player.Conditions.GetValue(PlayerCondition.IsStunned));

// 플래그 검사
if (player.Conditions.AllSatisfied(PlayerCondition.CanMove, PlayerCondition.CanAttack))
{
    // 공격 수행
}

// 활성화된 플래그 출력
foreach (var condition in player.Conditions.ActiveFlags())
{
    Debug.Log($"Active: {condition}");
}
```

---

### RxFlagState&lt;TState&gt;

FSM(Finite State Machine)을 대체하는 반응형 플래그 기반 상태 프로퍼티입니다.

#### 제네릭 제약

```csharp
where TState : Enum
```

#### 생성자

```csharp
public RxFlagState(TState initial, IRxOwner owner)
```

**매개변수:**
- `initial`: 초기 상태
- `owner`: 소유자 객체 (IRxOwner, `IsRxAllOwner`가 `true`여야 함)

#### 주요 프로퍼티

```csharp
public RxVar<TState> State { get; }
```

내부 상태를 관리하는 RxVar를 반환합니다.

```csharp
public TState Value { get; }
```

현재 상태를 반환합니다.

#### 주요 메서드

```csharp
public RxFlagState<TState> Request(TState next)
```

상태 전환을 요청합니다.

**반환값:**
- this (메서드 체이닝 지원)

```csharp
public RxFlagState<TState> AddListener(Action<TState>? listener)
```

상태 변경 리스너를 추가합니다. 추가 즉시 현재 상태로 호출됩니다.

```csharp
public RxFlagState<TState> RemoveListener(Action<TState>? listener)
```

상태 변경 리스너를 제거합니다.

```csharp
public RxFlagState<TState> SetPriority(TState state, int priority)
```

특정 상태의 우선순위를 설정합니다. 우선순위가 높을수록 먼저 선택됩니다.

```csharp
public RxFlagState<TState> SetCandidates(params TState[] candidateStates)
```

자동 상태 전환에 사용할 후보 상태들을 설정합니다.

```csharp
public RxFlagState<TState> SetStateFlag(TState state, bool value)
```

상태 플래그를 설정합니다. 플래그가 false인 상태는 후보에서 제외됩니다.

**참고:**
- 값이 변경되면 자동으로 상태를 재계산합니다 (candidates가 설정된 경우).

```csharp
public bool GetStateFlag(TState state)
```

상태 플래그를 조회합니다. 설정되지 않은 경우 기본값은 true입니다.

```csharp
public void RequestByPriority(params TState[] candidates)
```

후보 상태들 중에서 우선순위가 가장 높고 활성화된 상태로 전환합니다.

```csharp
public void RequestByPriority(TState single)
```

단일 상태로 전환을 시도합니다 (플래그가 활성화된 경우만).

#### 사용 예시

```csharp
public enum CharacterState
{
    Idle,
    Walk,
    Run,
    Jump,
    Attack
}

public class CharacterModel : BaseModel
{
    public RxFlagState<CharacterState> State;

    public CharacterModel()
    {
        State = new RxFlagState<CharacterState>(CharacterState.Idle, this);

        // 우선순위 설정 (높을수록 우선)
        State.SetPriority(CharacterState.Attack, 100)
             .SetPriority(CharacterState.Jump, 80)
             .SetPriority(CharacterState.Run, 60)
             .SetPriority(CharacterState.Walk, 40)
             .SetPriority(CharacterState.Idle, 0);

        // 후보 상태 설정
        State.SetCandidates(
            CharacterState.Idle,
            CharacterState.Walk,
            CharacterState.Run,
            CharacterState.Jump,
            CharacterState.Attack
        );
    }
}

// 상태 전환
character.State.Request(CharacterState.Walk);

// 플래그 기반 자동 전환
character.State.SetStateFlag(CharacterState.Jump, true);  // Jump 활성화
character.State.SetStateFlag(CharacterState.Attack, false); // Attack 비활성화

// 우선순위에 따른 자동 전환
character.State.RequestByPriority(
    CharacterState.Idle,
    CharacterState.Walk,
    CharacterState.Jump
); // Jump 우선순위가 가장 높으므로 Jump로 전환
```

---

### RxFlagState&lt;TState, TCondition&gt;

2개의 플래그셋(상태 플래그, 활성조건 플래그)을 사용하는 고급 RxFlagState입니다.

#### 제네릭 제약

```csharp
where TState : Enum
where TCondition : Enum
```

#### 생성자

```csharp
public RxFlagState(TState initial, IRxOwner owner)
```

기본 생성자는 `RxFlagState<TState>`와 동일합니다.

#### 추가 메서드

```csharp
public RxFlagState<TState, TCondition> BindConditions(RxFlagSet<TCondition> conditions)
```

외부에서 관리되는 조건 플래그셋을 바인딩합니다. 조건 플래그가 변경되면 자동으로 상태를 재계산합니다.

**매개변수:**
- `conditions`: 외부 RxFlagSet<TCondition>

```csharp
public RxFlagState<TState, TCondition> AddActivationCondition(TState state, TCondition condition, bool requiredValue = true)
```

상태에 활성조건을 추가합니다. 상태가 선택되려면 모든 활성조건이 만족되어야 합니다.

**매개변수:**
- `state`: 대상 상태
- `condition`: 조건 플래그
- `requiredValue`: 필요한 조건 값 (기본값: true)

```csharp
public bool IsStateActive(TState state)
```

상태가 활성화되었는지 확인합니다. 상태 플래그와 모든 활성조건이 만족되어야 true를 반환합니다.

#### 사용 예시

```csharp
public enum CharacterState
{
    Idle,
    Walk,
    Sprint,
    Jump
}

public enum CharacterCondition
{
    HasStamina,
    IsGrounded,
    IsInputting
}

public class CharacterModel : BaseModel
{
    public RxFlagSet<CharacterCondition> Conditions;
    public RxFlagState<CharacterState, CharacterCondition> State;

    public CharacterModel()
    {
        Conditions = new RxFlagSet<CharacterCondition>(this);
        State = new RxFlagState<CharacterState, CharacterCondition>(CharacterState.Idle, this);

        // 조건 플래그셋 바인딩
        State.BindConditions(Conditions);

        // 상태별 활성조건 설정
        State.AddActivationCondition(CharacterState.Sprint, CharacterCondition.HasStamina)
             .AddActivationCondition(CharacterState.Sprint, CharacterCondition.IsGrounded)
             .AddActivationCondition(CharacterState.Jump, CharacterCondition.IsGrounded);

        // 우선순위 및 후보 설정
        State.SetPriority(CharacterState.Sprint, 100)
             .SetPriority(CharacterState.Walk, 50)
             .SetPriority(CharacterState.Idle, 0)
             .SetCandidates(CharacterState.Idle, CharacterState.Walk, CharacterState.Sprint);
    }
}

// 조건 변경 시 자동 상태 전환
character.Conditions.SetValue(CharacterCondition.HasStamina, false);
// Sprint 조건 불만족 -> 자동으로 Walk 또는 Idle로 전환

character.Conditions.SetValue(CharacterCondition.HasStamina, true);
// Sprint 조건 만족 -> 우선순위에 따라 Sprint로 자동 전환
```

---

## 애그리게이트 시스템

애그리게이트 시스템은 Controller, Entity, Model, Part로 구성된 모듈화된 객체 구조입니다.

### 애그리게이트 타입

```csharp
public enum AggregateType
{
    Pure,    // Model과 Entity 없이 Controller만 사용
    Model,   // Controller와 Model로 구성
    Entity   // Controller, Entity, Model, Part로 구성
}
```

---

### BaseModel

모든 Model의 기반 클래스로, 반응형 프로퍼티를 관리합니다.

#### 인터페이스 구현

```csharp
public abstract class BaseModel : IRxCaller, IRxOwner
```

- **IRxCaller**:
  - `IsLogicalCaller = true`
  - `IsMultiRolesCaller = true`
  - `IsFunctionalCaller = true`

- **IRxOwner**:
  - `IsRxVarOwner = true`
  - `IsRxAllOwner = true`

#### 주요 메서드

```csharp
public void RegisterRx(RxBase rx)
```

반응형 프로퍼티를 등록합니다. RxVar, RxMod, RxComputed 등이 생성자에서 자동으로 호출합니다.

```csharp
public IModifiable? GetModifiableField(string fieldName)
```

특정 이름의 모디파이어 가능 필드(RxMod)를 반환합니다.

```csharp
public IRxField<T>? GetRxField<T>(string fieldName)
```

특정 이름의 반응형 필드를 반환합니다.

```csharp
public void RemoveAllModifiers()
```

모든 RxMod 필드의 모디파이어를 제거합니다.

```csharp
public IEnumerable<RxData> GetAllRxDatas()
```

등록된 모든 RxData를 반환합니다.

```csharp
public RxData? GetRxDataByName(string fieldName)
```

특정 이름의 RxData를 반환합니다.

```csharp
public bool TrySetRxDataValue(string fieldName, object value)
```

특정 필드의 값을 설정합니다 (문자열 이름 기반).

```csharp
public object? GetRxDataValue(string fieldName)
```

특정 필드의 값을 반환합니다 (문자열 이름 기반).

```csharp
public void Unload()
```

모든 반응형 프로퍼티의 관계를 정리하고 리소스를 해제합니다.

#### 가상 메서드

```csharp
public virtual void AtInit()
```

Model 초기화 시 호출됩니다. 반응형 프로퍼티를 생성하는 시점입니다.

```csharp
public virtual void AtReadyModel()
```

Model이 완전히 준비된 후 호출됩니다. 데이터가 모두 로드된 후의 초기화 로직을 작성합니다.

#### 사용 예시

```csharp
public class PlayerModel : BaseModel
{
    public RxVar<string> Name;
    public RxVar<int> Level;
    public RxMod<float> Health;
    public RxMod<float> Mana;
    public RxComputed<bool> IsAlive;

    public override void AtInit()
    {
        // 반응형 프로퍼티 생성
        Name = new RxVar<string>("Player", "Name", this);
        Level = new RxVar<int>(1, "Level", this);
        Health = new RxMod<float>(100f, "Health", this);
        Mana = new RxMod<float>(50f, "Mana", this);

        IsAlive = new RxComputed<bool>(
            () => Health.Value > 0,
            "IsAlive",
            this
        ).DependsOn(Health);
    }

    public override void AtReadyModel()
    {
        // 데이터 로드 후 추가 초기화
        Debug.Log($"Player {Name.Value} ready! Level: {Level.Value}");
    }
}
```

---

### Controller

애그리게이트의 로직을 담당하는 객체입니다.

#### 클래스 계층

```csharp
public abstract class BaseController : AggregateRoot
public abstract class Controller<E, M> : BaseController, IRxOwner, IRxCaller
    where E : BaseEntity<M>
    where M : BaseModel
```

#### 편의 클래스

```csharp
// Pure 타입: Model과 Entity 없이 Controller만 사용
public abstract class PureController : Controller<NoneEntity<NoneModel>, NoneModel>

// Model 타입: Controller와 Model만 사용
public abstract class ModelController<M> : Controller<NoneEntity<M>, M>
    where M : BaseModel
```

#### 주요 프로퍼티

```csharp
public M Model { get; }
```

Model에 접근합니다 (Model, Entity 타입에서만 사용 가능).

```csharp
public E Entity { get; }
```

Entity에 접근합니다 (Entity 타입에서만 사용 가능).

#### 타입 확인 메서드

```csharp
public bool IsPureType()
public bool IsModelType()
public bool IsEntityType()
```

현재 Controller의 애그리게이트 타입을 확인합니다.

#### 생명주기 훅 메서드

```csharp
protected virtual void AtAwake()
protected virtual void AtInit()
protected virtual void AtDeinit()
protected virtual void AtStart()
protected virtual void AtLateStart()
protected virtual void AtEnable()
protected virtual void AtDisable()
protected virtual void AtDestroy()
protected virtual void AtPoolInit()
protected virtual void AtPoolDeinit()
protected virtual void AtModelReady()  // Model/Entity 타입
protected virtual void AtSave()        // Entity 타입
protected virtual void AtLoad()        // Entity 타입
```

#### Model 타입 전용 메서드

```csharp
protected virtual M? SetModel()
```

Model을 생성하고 반환합니다. Model 타입에서 반드시 구현해야 합니다.

#### 사용 예시

**Pure 타입:**

```csharp
public class SimpleController : PureController
{
    private RxVar<int> counter;

    protected override void AtInit()
    {
        counter = new RxVar<int>(0, "counter", this);
        Debug.Log("SimpleController initialized");
    }

    public void Increment()
    {
        counter.Set(counter.Value + 1);
    }
}
```

**Model 타입:**

```csharp
public class PlayerController : ModelController<PlayerModel>
{
    protected override PlayerModel? SetModel()
    {
        // DataManager를 통해 Model 생성
        return DataManager.Instance.CreateModel<PlayerModel>("default");
    }

    protected override void AtModelReady()
    {
        // Model이 준비된 후 실행
        Debug.Log($"Player {Model.Name.Value} ready!");

        Model.Health.AddListener(health => {
            if (health <= 0)
            {
                OnPlayerDeath();
            }
        });
    }

    private void OnPlayerDeath()
    {
        Debug.Log("Player died!");
    }
}
```

**Entity 타입:**

```csharp
public class CharacterController : Controller<CharacterEntity, CharacterModel>
{
    protected override void AtModelReady()
    {
        // Model과 Entity가 모두 준비된 후 실행
        Entity.PlayAnimation("Idle");

        Model.State.AddListener(state => {
            Entity.PlayAnimation(state.ToString());
        });
    }

    public void Move(Vector3 direction)
    {
        // Controller는 로직을 담당
        if (Model.CanMove.Value)
        {
            Entity.MoveCharacter(direction);
        }
    }
}
```

---

### Entity

파트를 관리하고 기능을 제공하는 객체입니다 (Entity 타입 애그리게이트에서만 사용).

#### 클래스 정의

```csharp
public abstract class BaseEntity<M> : BaseEntity, IModelOwner<M>
    where M : BaseModel
```

#### 주요 프로퍼티

```csharp
public M Model { get; set; }
```

Model에 접근합니다.

#### 주요 메서드

```csharp
public T? GetPart<T>() where T : BasePart
```

특정 타입의 Part를 반환합니다. 없으면 null을 반환합니다.

```csharp
public IEnumerable<T> GetParts<T>() where T : BasePart
```

특정 타입의 모든 Part를 반환합니다.

#### 추상 메서드

```csharp
protected abstract M SetupModel()
```

Model을 생성하고 반환합니다. Entity에서 반드시 구현해야 합니다.

#### 생명주기 훅 메서드

```csharp
protected virtual void AtAwake()
protected virtual void AtEnable()
protected virtual void AtInit()
protected virtual void AtStart()
protected virtual void AtLateStart()
protected virtual void AtPoolInit()
protected virtual void AtPoolDeinit()
protected virtual void AtModelReady()
protected virtual void AtLoad()
protected virtual void AtSave()
protected virtual void AtDeinit()
protected virtual void AtDisable()
protected virtual void AtDestroy()
```

#### 사용 예시

```csharp
public class CharacterEntity : BaseEntity<CharacterModel>
{
    private Animator animator;
    private MovementPart movementPart;

    protected override CharacterModel SetupModel()
    {
        // DataManager를 통해 Model 생성
        return DataManager.Instance.CreateModel<CharacterModel>("warrior");
    }

    protected override void AtAwake()
    {
        animator = GetComponent<Animator>();
    }

    protected override void AtInit()
    {
        movementPart = GetPart<MovementPart>();
    }

    protected override void AtModelReady()
    {
        // Model 준비 후 초기화
        Model.State.AddListener(OnStateChanged);
    }

    private void OnStateChanged(CharacterState state)
    {
        PlayAnimation(state.ToString());
    }

    public void PlayAnimation(string animationName)
    {
        animator.Play(animationName);
    }

    public void MoveCharacter(Vector3 direction)
    {
        movementPart?.Move(direction * Model.MoveSpeed.Value);
    }
}
```

---

### Part

모듈화된 기능을 담당하는 컴포넌트입니다 (Entity 타입 애그리게이트에서만 사용).

#### 클래스 정의

```csharp
public abstract class BasePart<E, M> : BasePart
    where E : BaseEntity<M>
    where M : BaseModel
```

#### 주요 프로퍼티

```csharp
protected E Entity { get; }
```

소속된 Entity에 접근합니다.

```csharp
public M Model { get; }
```

Model에 접근합니다.

#### 생명주기 훅 메서드

```csharp
protected virtual void AtEnable()
protected virtual void AtAwake()
protected virtual void AtStart()
protected virtual void AtInit()
protected virtual void AtLateStart()
protected virtual void AtModelReady()
protected virtual void AtSave()
protected virtual void AtLoad()
protected virtual void AtDeinit()
protected virtual void AtDisable()
protected virtual void AtDestroy()
protected virtual void AtPoolInit()
protected virtual void AtPoolDeinit()
```

#### 사용 예시

```csharp
// Movement 기능을 담당하는 Part
public class MovementPart : BasePart<CharacterEntity, CharacterModel>
{
    private Rigidbody rb;

    protected override void AtAwake()
    {
        rb = GetComponent<Rigidbody>();
    }

    protected override void AtModelReady()
    {
        // Model의 값을 사용한 초기화
        Model.MoveSpeed.AddListener(OnMoveSpeedChanged);
    }

    public void Move(Vector3 direction)
    {
        if (Model.CanMove.Value)
        {
            rb.velocity = direction * Model.MoveSpeed.Value;
        }
    }

    private void OnMoveSpeedChanged(float newSpeed)
    {
        Debug.Log($"Move speed changed to {newSpeed}");
    }
}

// Combat 기능을 담당하는 Part
public class CombatPart : BasePart<CharacterEntity, CharacterModel>
{
    protected override void AtModelReady()
    {
        Model.Health.AddListener(OnHealthChanged);
    }

    public void Attack(CharacterModel target)
    {
        if (Model.CanAttack.Value)
        {
            float damage = Model.AttackPower.Value;
            target.Health.AddModifier(
                "damage",
                ModifierType.FinalAdd,
                -damage,
                stackable: true
            );
        }
    }

    private void OnHealthChanged(float newHealth)
    {
        if (newHealth <= 0)
        {
            Entity.PlayAnimation("Death");
        }
    }
}
```

---

## 관리자 시스템

프레임워크의 핵심 기능을 제공하는 관리자 클래스들입니다.

### DataManager

Model의 생성 및 데이터 초기화를 담당합니다.

#### 초기화 우선순위

```csharp
public override int InitializationPriority => 0; // 첫 번째로 초기화
```

#### 주요 메서드

```csharp
public T CreateModel<T>(string? variationName = null) where T : BaseModel, new()
```

Model을 생성하고 variation 데이터로 초기화합니다.

**매개변수:**
- `variationName`: Variation 이름 (null이면 "default" 사용)

**반환값:**
- 초기화된 Model 인스턴스

**예외:**
- `ArgumentException`: Variation을 찾을 수 없는 경우

```csharp
public void InitializeModel<T>(T model, string variationName) where T : BaseModel
```

기존 Model을 variation 데이터로 초기화합니다.

```csharp
public void ApplyVariation<T>(T model, string variationName, bool reinitialize = false) where T : BaseModel
```

Model에 variation 데이터를 적용합니다.

**매개변수:**
- `model`: 대상 Model
- `variationName`: Variation 이름
- `reinitialize`: true면 Model을 완전히 재초기화 (Unload 후 Init 재호출)

```csharp
public bool IsValidVariation(string modelType, string variationName)
```

특정 Model 타입에 해당 variation이 존재하는지 확인합니다.

```csharp
public List<string>? GetVariations(string modelType)
```

특정 Model 타입의 모든 variation 이름 목록을 반환합니다.

```csharp
public List<string> GetAllModelTypes()
```

등록된 모든 Model 타입 이름 목록을 반환합니다.

```csharp
public void InvalidateCache()
```

내부 캐시를 무효화하고 재구축합니다.

#### 프로퍼티

```csharp
public int VariationCount { get; }
public int ModelTypeCount { get; }
```

#### 사용 예시

```csharp
// Model 생성 (기본 variation)
var player = DataManager.Instance.CreateModel<PlayerModel>();

// Model 생성 (특정 variation)
var warrior = DataManager.Instance.CreateModel<CharacterModel>("warrior");
var mage = DataManager.Instance.CreateModel<CharacterModel>("mage");

// Variation 적용
DataManager.Instance.ApplyVariation(warrior, "warrior_elite");

// Variation 확인
if (DataManager.Instance.IsValidVariation("CharacterModel", "assassin"))
{
    var assassin = DataManager.Instance.CreateModel<CharacterModel>("assassin");
}

// 사용 가능한 Variation 목록
var variations = DataManager.Instance.GetVariations("CharacterModel");
foreach (var variation in variations)
{
    Debug.Log($"Available variation: {variation}");
}
```

#### Variation 데이터 구조

Variation 데이터는 Google Sheets에서 관리되며, VariationRegistry ScriptableObject로 임포트됩니다.

**Google Sheets 형식:**

| VariationName | FieldName1 | FieldName2 | FieldName3 |
|--------------|------------|------------|------------|
| default      | 100        | 50         | 10         |
| warrior      | 150        | 30         | 20         |
| mage         | 80         | 100        | 15         |

---

### FactoryManager

애그리게이트의 생성을 담당합니다.

#### 초기화 우선순위

```csharp
public override int InitializationPriority => 1; // DataManager 다음
```

#### 주요 프로퍼티

```csharp
public ResourceSystem Resources { get; }
```

Addressable 리소스 시스템에 접근합니다.

#### Addressable 기반 생성 메서드

```csharp
public async Task<T> CreateFromAddressableAsync<T>(
    string addressableKey,
    CancellationToken cancellationToken = default)
    where T : AggregateRoot
```

Addressable Key로 Prefab을 로드하고 인스턴스를 생성합니다.

**매개변수:**
- `addressableKey`: Addressable 키
- `cancellationToken`: 취소 토큰

**반환값:**
- 생성된 AggregateRoot 인스턴스

**예외:**
- `ArgumentException`: addressableKey가 null 또는 빈 문자열
- `ResourceLoadException`: Prefab 로드 실패
- `AggregateCreationException`: 컴포넌트를 찾을 수 없음

```csharp
public async Task<T> CreateFromAddressableAsync<T>(
    string addressableKey,
    Vector3 position,
    Quaternion rotation,
    CancellationToken cancellationToken = default)
    where T : AggregateRoot
```

위치와 회전을 지정하여 생성합니다.

```csharp
public async Task<T> CreateFromAddressableAsync<T>(
    string addressableKey,
    Transform parent,
    CancellationToken cancellationToken = default)
    where T : AggregateRoot
```

부모 Transform을 지정하여 생성합니다.

#### Prefab 기반 생성 메서드

```csharp
public Task<T> CreateFromPrefabAsync<T>(
    GameObject prefab,
    CancellationToken cancellationToken = default)
    where T : AggregateRoot
```

Prefab으로부터 인스턴스를 생성합니다.

#### 클래스 기반 생성 메서드

```csharp
public T CreateFromClass<T>() where T : AggregateRoot
```

Pure 또는 Model 타입 Controller를 클래스로부터 직접 생성합니다.

**참고:**
- Entity 타입은 지원하지 않습니다 (Prefab 필요).

```csharp
public T CreateFromClass<T>(Vector3 position, Quaternion rotation) where T : AggregateRoot
public T CreateFromClass<T>(Transform parent) where T : AggregateRoot
```

#### 복구 메서드

```csharp
public async Task<T> RestoreAsync<T>(
    SaveData saveData,
    RestoreConfig config,
    CancellationToken cancellationToken = default)
    where T : AggregateRoot
```

저장 데이터로부터 객체를 복구합니다.

#### 대량 생성 메서드

```csharp
public async Task<List<T>> CreateMultipleAsync<T>(
    string addressableKey,
    int count,
    CancellationToken cancellationToken = default)
    where T : AggregateRoot
```

동일한 Prefab의 여러 인스턴스를 생성합니다.

```csharp
public async Task<List<T>> CreateMultipleBatchedAsync<T>(
    string addressableKey,
    int count,
    int batchSize = 10,
    CancellationToken cancellationToken = default)
    where T : AggregateRoot
```

배치 단위로 나누어 생성합니다 (프레임 분산).

#### 사용 예시

```csharp
// Addressable로 생성
var player = await FactoryManager.Instance.CreateFromAddressableAsync<PlayerController>(
    "Player",
    new Vector3(0, 0, 0),
    Quaternion.identity
);

// Prefab으로 생성
GameObject enemyPrefab = Resources.Load<GameObject>("Enemy");
var enemy = await FactoryManager.Instance.CreateFromPrefabAsync<EnemyController>(enemyPrefab);

// 클래스로 생성 (Pure/Model 타입만)
var manager = FactoryManager.Instance.CreateFromClass<GameManager>();

// 대량 생성
var enemies = await FactoryManager.Instance.CreateMultipleAsync<EnemyController>(
    "Enemy",
    10
);

// 배치 생성 (프레임 분산)
var bullets = await FactoryManager.Instance.CreateMultipleBatchedAsync<BulletController>(
    "Bullet",
    100,
    batchSize: 10
);
```

---

### ControllerManager

Controller의 생명주기와 객체 풀링을 관리합니다.

#### 초기화 우선순위

```csharp
public override int InitializationPriority => 2; // FactoryManager 다음
```

#### 주요 프로퍼티

```csharp
public int RegisteredCount { get; }  // 등록된 총 객체 수
public int ActiveCount { get; }      // 활성화된 객체 수
public int PooledCount { get; }      // 풀에 있는 객체 수
public int TotalPoolCount { get; }   // 총 풀 개수
```

#### 등록/해제 메서드

```csharp
public void RegisterAggregate(AggregateRoot aggregate)
```

Controller를 등록합니다. Controller의 생성자에서 자동으로 호출됩니다.

```csharp
public void UnregisterAggregate(AggregateRoot aggregate)
```

Controller를 등록 해제합니다.

#### 생성 메서드

```csharp
public async Task<TController> CreateAndRegisterAsync<TController>(
    string addressableKey,
    CancellationToken cancellationToken = default)
    where TController : BaseController
```

Addressable Key로 Controller를 생성하고 자동으로 등록합니다.

```csharp
public async Task<TController> CreateAndRegisterAsync<TController>(
    string addressableKey,
    Vector3 position,
    Quaternion rotation,
    CancellationToken cancellationToken = default)
    where TController : BaseController
```

위치와 회전을 지정하여 생성합니다.

#### 풀링 메서드

```csharp
public async Task<TController> SpawnAsync<TController>(
    TController prefab,
    CancellationToken cancellationToken = default)
    where TController : BaseController
```

풀에서 객체를 가져오거나 새로 생성합니다.

**매개변수:**
- `prefab`: Prefab 참조
- `cancellationToken`: 취소 토큰

**반환값:**
- Spawn된 Controller 인스턴스

```csharp
public async Task<TController> SpawnAsync<TController>(
    TController prefab,
    Vector3 position,
    Quaternion rotation,
    CancellationToken cancellationToken = default)
    where TController : BaseController
```

위치와 회전을 지정하여 Spawn합니다.

```csharp
public async Task<TController> SpawnAsync<TController>(
    TController prefab,
    Transform parent,
    CancellationToken cancellationToken = default)
    where TController : BaseController
```

부모 Transform을 지정하여 Spawn합니다.

```csharp
public bool Return(BaseController controller)
```

Controller를 풀로 반환합니다.

**반환값:**
- true: 성공, false: 실패 (풀 객체가 아님)

```csharp
public void ReturnAll()
```

모든 활성 Controller를 풀로 반환합니다.

```csharp
public void ReturnAllOfType(AggregateType aggregateType)
```

특정 타입의 모든 Controller를 풀로 반환합니다.

#### 풀 관리 메서드

```csharp
public async Task PrewarmPoolAsync<TController>(
    TController prefab,
    int count,
    CancellationToken cancellationToken = default)
    where TController : BaseController
```

풀을 미리 채웁니다 (Prewarm).

**매개변수:**
- `prefab`: Prefab 참조
- `count`: 미리 생성할 개수

```csharp
public void ClearPool<TController>(TController prefab)
    where TController : BaseController
```

특정 Prefab의 풀을 비웁니다.

```csharp
public void ClearAllPools()
```

모든 풀을 비웁니다.

```csharp
public int GetPoolCount<TController>(TController prefab)
    where TController : BaseController
```

특정 Prefab의 풀 개수를 반환합니다.

```csharp
public Dictionary<string, (int available, int spawned)> GetPoolStats(AggregateType aggregateType)
```

특정 타입의 풀 통계를 반환합니다.

```csharp
public Dictionary<string, (int available, int spawned)> GetPoolStats<T>() where T : BaseController
```

모든 풀 통계를 반환합니다.

#### 조회 메서드

```csharp
public IEnumerable<BaseController> GetActive(AggregateType aggregateType)
```

특정 타입의 활성 Controller들을 반환합니다.

```csharp
public IEnumerable<T> GetActive<T>() where T : BaseController
```

특정 클래스의 활성 Controller들을 반환합니다.

```csharp
public BaseController? GetById(int instanceId)
```

인스턴스 ID로 Controller를 찾습니다.

```csharp
public bool IsRegistered(int instanceId)
```

특정 인스턴스 ID가 등록되어 있는지 확인합니다.

```csharp
public IEnumerable<BaseController> GetAll()
```

등록된 모든 Controller를 반환합니다.

```csharp
public IEnumerable<BaseController> GetAllActive()
```

활성화된 모든 Controller를 반환합니다.

#### 사용 예시

```csharp
// Controller 생성 및 등록
var player = await ControllerManager.Instance.CreateAndRegisterAsync<PlayerController>(
    "Player",
    Vector3.zero,
    Quaternion.identity
);

// 풀링 사용
[SerializeField] private BulletController bulletPrefab;

// 풀 Prewarm
await ControllerManager.Instance.PrewarmPoolAsync(bulletPrefab, 50);

// Spawn
var bullet = await ControllerManager.Instance.SpawnAsync(
    bulletPrefab,
    firePoint.position,
    firePoint.rotation
);

// Return
ControllerManager.Instance.Return(bullet);

// 모든 총알 반환
ControllerManager.Instance.ReturnAll();

// 활성 Controller 조회
var enemies = ControllerManager.Instance.GetActive<EnemyController>();
foreach (var enemy in enemies)
{
    Debug.Log($"Enemy: {enemy.Model.Name.Value}");
}

// 풀 통계
var stats = ControllerManager.Instance.GetPoolStats<BulletController>();
foreach (var kvp in stats)
{
    Debug.Log($"{kvp.Key}: Available={kvp.Value.available}, Spawned={kvp.Value.spawned}");
}
```

---

## 인터페이스 및 유틸리티

### 주요 인터페이스

#### IRxOwner

반응형 프로퍼티의 소유자를 나타냅니다.

```csharp
public interface IRxOwner
{
    bool IsRxVarOwner { get; }   // RxVar 소유 가능 여부
    bool IsRxAllOwner { get; }   // 모든 Rx 타입 소유 가능 여부

    void RegisterRx(RxBase rx);  // Rx 등록
    void Unload();               // 모든 Rx 정리
}
```

**구현 클래스:**
- BaseModel: `IsRxVarOwner = true`, `IsRxAllOwner = true`
- Controller (Pure 타입): `IsRxVarOwner = true`, `IsRxAllOwner = false`

#### IRxCaller

반응형 프로퍼티의 호출자를 나타냅니다.

```csharp
public interface IRxCaller
{
    bool IsLogicalCaller { get; }     // 논리적 호출자 (Model, Controller)
    bool IsMultiRolesCaller { get; }  // 다중 역할 호출자 (Model, Pure Controller)
    bool IsFunctionalCaller { get; }  // 기능적 호출자 (Entity, Part, Model Controller)
}
```

**구현 클래스:**
- BaseModel: 모두 true
- Controller (Pure): IsLogicalCaller=true, IsMultiRolesCaller=true, IsFunctionalCaller=false
- Controller (Model): IsLogicalCaller=true, IsMultiRolesCaller=false, IsFunctionalCaller=true
- Controller (Entity): IsLogicalCaller=true, IsMultiRolesCaller=false, IsFunctionalCaller=false
- Entity, Part: IsLogicalCaller=false, IsMultiRolesCaller=false, IsFunctionalCaller=true

#### IRxField&lt;T&gt;

반응형 필드를 나타냅니다.

```csharp
public interface IRxField<T>
{
    string FieldName { get; }
    IRxOwner? Owner { get; }
    T Value { get; }
    void AddListener(Action<T> listener);
    void RemoveListener(Action<T> listener);
}
```

#### IModifiable

모디파이어를 적용할 수 있는 필드를 나타냅니다.

```csharp
public interface IModifiable
{
    void AddModifier(string key, ModifierType type, float value, float? duration = null, bool stackable = false);
    void AddModifierWithPolicy(string key, ModifierType type, float value, float? duration = null, Func<float, float, float>? policy = null);
    void RemoveModifier(string key);
    void RemoveAllModifiers();
    bool HasModifier(string key);
}
```

**구현 클래스:**
- RxMod&lt;T&gt;

#### IModelOwner&lt;M&gt;

Model을 소유하는 객체를 나타냅니다.

```csharp
public interface IModelOwner<M> : IModelOwner where M : BaseModel
{
    M Model { get; }
    M GetModel();
}
```

**구현 클래스:**
- Controller (Model, Entity 타입)
- Entity
- Part

---

## 부록

### 생명주기 순서

#### Pure 타입 Controller

1. Awake → `AtAwake()`
2. OnEnable
3. Start:
   - `OnInit()` (내부)
   - `AtInit()`
   - `AtStart()`
   - `AtLateStart()`
4. OnDisable → `AtDisable()`
5. OnDestroy:
   - `OnDeinit()` (내부)
   - `AtDeinit()`
   - `AtDestroy()`

#### Model 타입 Controller

1. Awake → `AtAwake()`
2. OnEnable
3. Start:
   - `OnInit()` (내부) → `SetModel()` 호출
   - `AtInit()`
   - `AtModelReady()`
   - `AtStart()`
   - `AtLateStart()`
4. OnDisable → `AtDisable()`
5. OnDestroy:
   - `OnDeinit()` (내부)
   - `AtDeinit()`
   - `AtDestroy()`

#### Entity 타입 (Controller + Entity + Part)

**Controller:**

1. Awake → `AtAwake()`
2. OnEnable
3. Start:
   - Entity.SetEnable()
   - Entity.CallInit() → Part.CallInit()
   - `OnInit()` (내부)
   - `AtInit()`
   - Entity.CallStart() → Part.CallStart()
   - `AtStart()`
   - Entity.CallLateStart() → Part.CallLateStart()
   - `AtLateStart()`
4. OnDisable:
   - Entity.CallDisable() → Part.CallDisable()
   - `AtDisable()`
5. OnDestroy:
   - Entity.CallDeinit() → Part.CallDeinit()
   - `OnDeinit()` (내부)
   - `AtDeinit()`
   - Entity.CallDestroy() → Part.CallDestroy()
   - `AtDestroy()`

**Entity:**

1. Awake:
   - Controller 찾기
   - Controller.RegistEntity() 호출
   - `AtAwake()`
2. SetEnable() (Controller.Start에서 호출):
   - `SetupModel()` → Model 생성
   - Model.AtInit()
   - Model.AtReadyModel()
   - Part 초기화
   - `AtModelReady()`
   - Controller.CallModelReady()

**Part:**

1. Awake:
   - enabled = false
   - `AtAwake()`
2. RegisterEntity() (Entity.SetEnable에서 호출):
   - Entity 및 Model 참조 설정
   - enabled = true
   - `AtModelReady()`

### 풀링 생명주기

**Spawn 시:**

1. OnEnable
2. OnSpawnFromPool() (AggregateRoot)
3. Controller: `OnPoolInit()` → `AtPoolInit()`
4. Entity: `CallPoolInit()` → Part: `CallPoolInit()` → `AtPoolInit()`

**Return 시:**

1. OnDisable
2. OnReturnToPool() (AggregateRoot)
3. Entity: `CallPoolDeinit()` → Part: `CallPoolDeinit()` → `AtPoolDeinit()`
4. Controller: `OnPoolDeinit()` → `AtPoolDeinit()`

### 권장 사용 패턴

#### 1. Model에 데이터, Controller에 로직

```csharp
// Model: 데이터만
public class PlayerModel : BaseModel
{
    public RxVar<int> Level;
    public RxMod<float> Health;
}

// Controller: 로직
public class PlayerController : ModelController<PlayerModel>
{
    protected override void AtModelReady()
    {
        Model.Health.AddListener(health => {
            if (health <= 0) Die();
        });
    }

    public void TakeDamage(float damage)
    {
        Model.Health.AddModifier("damage", ModifierType.FinalAdd, -damage);
    }

    private void Die()
    {
        // 사망 로직
    }
}
```

#### 2. Part를 통한 기능 모듈화

```csharp
// 이동 기능
public class MovementPart : BasePart<CharacterEntity, CharacterModel>
{
    public void Move(Vector3 direction)
    {
        if (Model.CanMove.Value)
        {
            transform.position += direction * Model.MoveSpeed.Value * Time.deltaTime;
        }
    }
}

// 전투 기능
public class CombatPart : BasePart<CharacterEntity, CharacterModel>
{
    public void Attack(CharacterModel target)
    {
        if (Model.CanAttack.Value)
        {
            target.Health.AddModifier("damage", ModifierType.FinalAdd, -Model.AttackPower.Value);
        }
    }
}
```

#### 3. 반응형 프로퍼티 활용

```csharp
public class UIHealthBar : MonoBehaviour
{
    private PlayerController player;

    private void Start()
    {
        player = ControllerManager.Instance.GetActive<PlayerController>().FirstOrDefault();

        // 체력 변경 시 UI 자동 업데이트
        player.Model.Health.AddListener(UpdateHealthBar);
    }

    private void UpdateHealthBar(float health)
    {
        healthBarImage.fillAmount = health / player.Model.MaxHealth.Value;
    }
}
```

---

## 개발 워크플로우

### 1. 새로운 캐릭터 추가하기

**1단계: Model 정의**

```csharp
public class CharacterModel : BaseModel
{
    public RxVar<string> Name;
    public RxVar<int> Level;
    public RxMod<float> Health;
    public RxMod<float> Mana;
    public RxMod<float> AttackPower;
    public RxFlagSet<CharacterCondition> Conditions;
    public RxFlagState<CharacterState> State;

    public override void AtInit()
    {
        Name = new RxVar<string>("Character", "Name", this);
        Level = new RxVar<int>(1, "Level", this);
        Health = new RxMod<float>(100f, "Health", this);
        Mana = new RxMod<float>(50f, "Mana", this);
        AttackPower = new RxMod<float>(10f, "AttackPower", this);
        Conditions = new RxFlagSet<CharacterCondition>(this);
        State = new RxFlagState<CharacterState>(CharacterState.Idle, this);
    }
}
```

**2단계: Google Sheets에 데이터 추가**

CharacterModel 시트:

| VariationName | Name    | Level | Health | Mana | AttackPower |
|--------------|---------|-------|--------|------|-------------|
| warrior      | Warrior | 1     | 150    | 30   | 20          |
| mage         | Mage    | 1     | 80     | 100  | 15          |

**3단계: Entity 및 Part 작성**

```csharp
public class CharacterEntity : BaseEntity<CharacterModel>
{
    protected override CharacterModel SetupModel()
    {
        return DataManager.Instance.CreateModel<CharacterModel>("warrior");
    }

    protected override void AtModelReady()
    {
        Model.State.AddListener(OnStateChanged);
    }

    private void OnStateChanged(CharacterState state)
    {
        Debug.Log($"State changed to {state}");
    }
}
```

**4단계: Controller 작성**

```csharp
public class CharacterController : Controller<CharacterEntity, CharacterModel>
{
    protected override void AtModelReady()
    {
        Debug.Log($"{Model.Name.Value} ready!");
    }

    public void Attack(CharacterModel target)
    {
        if (Model.Conditions.GetValue(CharacterCondition.CanAttack))
        {
            target.Health.AddModifier(
                "attack_damage",
                ModifierType.FinalAdd,
                -Model.AttackPower.Value
            );
        }
    }
}
```

**5단계: Prefab 설정 및 생성**

```csharp
var character = await FactoryManager.Instance.CreateFromAddressableAsync<CharacterController>(
    "Character_Warrior"
);
```

### 2. 데이터 주도 개발

**Google Sheets 업데이트 → Import → 자동 적용**

1. Google Sheets에서 데이터 수정
2. Unity 에디터: `Akasha/Data/Import from Google Sheets` 실행
3. VariationRegistry 자동 업데이트
4. 런타임에 자동 반영

```csharp
// 런타임에 variation 전환
DataManager.Instance.ApplyVariation(character.Model, "mage", reinitialize: true);
```

---

#아카샤 프레임워크 개발가이드

##모델 관련 개발가이드

1. 생성 및 초기화 : 모델은 데이터매니저를 통해 생성 및 초기화 해야 한다.

2. 필드 
2.1기본적인 반응형 프로퍼티는 다음과 같다.
 1) RxVar<T> : 일반적인 필드에 쓰이는 반응형 프로퍼티
 2) RxMod<T> : Effect의 대상 필드에 쓰이는 반응형 프로퍼티. 주로 스테이터스 필드에 쓰임.
 3) RxComputed<T> : RxVar나 RxMod를 이용한 파생형 필드에 쓰이는 반응형 프로퍼티.

2.2특수 프로퍼티는 다음과 같다.
 1) RxFlag : RxVar<bool> 형식의 확장형. 플래그를 다루기 위한 반응형 프로퍼티. condition을 주입받아 동작할 수 있다.
 2) RxFlagSet<Tenum> :  Enum을 사용한 플래그묶음을 다루기 위한 반응형 프로퍼티. RxFlagState의 조건에 쓰인다.
 3) RxFlagState<TState> 또는 RxFlagStatee<TState, TCondition> : FSM을 대체하는 반응형 플래그 기반 상태 프로퍼티. 

## 컨트롤러, 엔티티, 파트 관련 개발 가이드

1 애그리게이트 사용 기준 : 애그리게이트는 오브젝트 풀, 세이브로드, 반응형 프로퍼티, 모델을 통한 파트 확장 등 많은 기능을 이용할 수 있는 개념이다.
이러한 기능이 필요하지 않은 객체는 일반적인 모노비헤이비어를 사용하면 되지만 지원기능을 이용하려면 애그리게이트 형태를 구현해야 한다. 

2. 애그리게이트 타입
1) Pure : 가장 간단한 애그리게이트 타입으로 컨트롤러 단독으로 구성된 애그리게이트이다. 오브젝트 풀과 간단한 반응형 프로퍼티를 사용할 수 있다.
2) Model :  컨트롤러와 모델로 구성된 애그리게이트이다. 추가로 세이브로드, 모든 반응형 프로퍼티, 이펙트 기능을 사용할 수 있다.
3) Entity : 컨트롤러, 엔티티, 모델, 파트(선택적, 확장가능)러 구성된 애그리게이트이다. 추가로 안정된 라이프사이클 기능과 손쉬운 애그리게이트 확장이 가능하다.

3. 생성 : 애그리게이트의 생성은 팩토리매니저를 통해 생성한다. Entity타입의 경우 프리팹을 이용한 생성만 가능하다. 애그리게이트가 생성되면 자동으로 컨트롤러 매니저에 등록된다.

4. Entity타입에서 컨트롤러의 역할
컨트롤러는 기본적으로 로직을 담당하는 객체의 역할을 한다. Entity는 파트를 관리하는 기능을 담당하는 객체이다. 다른 애그리게이트 또는 매니저와의 인터렉션도 컨트롤러가 담당한다.  


5. 엔티티&파트의 자동화된 프로퍼티래퍼
엔티티와 파트는 프레임워크에 의해 에디트타임에 자동화된 프로퍼티래퍼를 지원받는다. 모델의 프로퍼티가 변경되면 자동으로 프로퍼티래퍼가 갱신되고 엔티티와 파트에선 마치 자체 프로퍼티를 사용하는 듯한 문법으로 기능을 작성할 수 있다.

6. Entity타입에서 엔티티&파트의 역할
엔티티와 파트는 기능을 담당한다. 엔티티와 파트의 차이는 엔티티는 애그리게이트에서 유일하게 1개만 존재할 수 있다. 따라서 모듈화가 쉽지 않은 기능은 엔티티에 작성하는 것이 좋다. 


## 매니저 관련 개발가이드

1. 모델의 생성은 데이터매니저를 사용해야 하고 애그리게이트의 생성은 팩토리매니저를 사용해야 한다.
2. 컨트롤러매니저를 통해 애그리게이트간 상호작용이 가능하다.

이것으로 아카샤 프레임워크의 API 문서를 마칩니다.