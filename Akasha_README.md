# Akasha Framework
### Unity를 위한 엔터프라이즈급 게임 개발 프레임워크

[![Unity Version](https://img.shields.io/badge/Unity-6000.0%2B-blue.svg)](https://unity.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Tests](https://img.shields.io/badge/Tests-309%20Passed-brightgreen.svg)](/)
[![Coverage](https://img.shields.io/badge/Coverage-48.7%25-yellow.svg)](/)

---

## 📖 목차

- [개요](#개요)
- [폴더 구조](#폴더-구조)
- [핵심 철학](#핵심-철학)
- [Aggregate 시스템](#aggregate-시스템)
- [반응형 프로퍼티 시스템](#반응형-프로퍼티-시스템)
- [Effect 시스템](#effect-시스템)
- [상태 관리 시스템](#상태-관리-시스템)
- [Manager 시스템](#manager-시스템)
- [Entity-Part 시스템](#entity-part-시스템)
- [Save/Load 시스템](#saveload-시스템)
- [테스트 및 품질](#테스트-및-품질)

---

## 개요

**Akasha**는 **Domain-Driven Design(DDD)** 원칙과 **반응형 프로그래밍**을 기반으로 설계된 Unity 게임 개발 프레임워크입니다.

복잡한 게임 로직과 대규모 프로젝트를 체계적으로 관리할 수 있도록 설계되었으며, **유지보수성**, **확장성**, **테스트 가능성**을 최우선으로 고려합니다.

### 주요 특징

- 🏗️ **Aggregate 중심 아키텍처** - DDD 패턴 적용
- ⚡ **반응형 상태 관리** - 자동 UI 업데이트
- 🎮 **완전한 Effect 시스템** - 버프/디버프/DOT 관리
- 🧩 **Entity-Part 조합 패턴** - 재사용 가능한 컴포넌트
- 🔄 **우선순위 기반 FSM** - 지능형 상태 전환
- 💾 **자동 Save/Load** - Reflection 기반 직렬화
- 🎯 **타입 안전성** - Null 안전 코드베이스
- 📊 **데이터 기반 설계** - Google Sheets 통합

---

## 폴더 구조

프레임워크는 명확한 책임 분리를 위해 체계적으로 구조화되어 있습니다.

```
Akasha/
   ├── Aggregate/
   │   ├── AggregateObject/
   │   └── RxProperty/
   │  
   ├── Infrastructure/
   │  
   ├── Management/
   │   ├── Data/
   │   │   └── Editor/
   │   │
   │   ├── Effects/
   │   │   ├── Core/
   │   │   └── Items/
   │   │
   │   ├── Factory/
   │   ├── ObjectManage/
   │   └── Restore/
   │
   └── Test/

```

### 핵심 디렉토리 설명

#### 📂 Aggregate/ - 프레임워크 코어
- **AggregateObject/**: DDD의 Aggregate 패턴 구현체
- **RxProperty/**: 반응형 프로그래밍 시스템 전체

#### 📂 Infrastructure/ - 싱글톤, Manager, System 기반 클래스

#### 📂 Management/ - 게임 시스템
- **Data/**: 데이터 중앙 관리 및 Google Sheets 통합
- **Effects/**: 버프/디버프/DOT 완전한 Effect 시스템, 추상클래스 ItemBase
- **Factory/**: Aggregate 생성 및 등록 관리
- **ObjectManage/**: 오브젝트 풀링 및 생명주기 관리
- **Restore/**: Save/Load 자동 직렬화 시스템

#### 📂 Tests/ - 테스트용 클래스

---

## 핵심 철학

### 1. Aggregate 중심 아키텍처
모든 게임 오브젝트는 **Aggregate**라는 명확한 경계를 가진 단위로 관리됩니다.
이는 DDD의 핵심 개념을 게임 개발에 적용한 것으로, 각 Aggregate는 **독립적인 생명주기와 책임**을 가지며 일관된 규칙에 따라 생성, 관리, 소멸됩니다.

### 2. 반응형 상태 관리
모든 상태 변화는 **반응형 프로퍼티(Reactive Properties)** 시스템을 통해 흐릅니다.
상태가 변경되면 의존하는 모든 컴포넌트가 자동으로 업데이트되어, 수동 업데이트 호출이나 상태 불일치 문제를 근본적으로 제거합니다.

### 3. 엄격한 소유권 계층
명확한 **소유권과 호출자 검증 시스템**을 통해 아키텍처 경계를 컴파일 타임과 런타임에 모두 강제합니다.
이를 통해 잘못된 접근이나 수정을 사전에 방지합니다.

### 4. 데이터 기반 설계
게임 로직과 데이터를 완전히 분리하여, **기획자나 디자이너도 게임 밸런스와 설정을 수정**할 수 있습니다.
**Google Sheets 통합**을 통해 외부 데이터 기반 초기화를 지원합니다.

### 5. Composition over Inheritance
복잡한 상속 구조 대신 **Entity-Part 패턴**을 사용하여 작고 재사용 가능한 컴포넌트들을 조합해 복잡한 게임 오브젝트를 구성합니다.

---

## Aggregate 시스템

**Akasha**의 핵심은 세 가지 타입의 Controller로 구성된 **Aggregate 시스템**입니다.

### 🎮 PureController (순수 Controller)
가장 단순한 형태의 Aggregate로, **데이터 모델이 필요 없는 게임 시스템이나 UI 컨트롤러**에 사용합니다.

```csharp
public class GameUIController : PureController
{
    public RxVar<int> Score { get; private set; }

    protected override void AtAwake()
    {
        Score = new RxVar<int>(0, nameof(Score), this);
    }
}
```

**주요 특징**
- 8단계 생명주기: `Awake → Init → Start → LateStart → Enable → 활성 → Disable → Destroy`
- 오브젝트 풀 지원 (`AtPoolInit`, `AtPoolDeinit` 훅 제공)
- 자동 `ControllerManager` 등록
- Transform 캐싱으로 성능 최적화
- 반응형 변수 소유 가능

---

### 📦 ModelController (Model 포함 Controller)
데이터 모델을 포함하는 Aggregate입니다.
단일하고 명확한 데이터 구조를 가진 게임 오브젝트에 적합합니다.

```csharp
public class PlayerController : ModelController<PlayerModel>
{
    protected override PlayerModel SetModel()
    {
        return DataManager.Instance.CreateModel<PlayerModel>("Default");
    }

    protected override void AtModelReady()
    {
        // 모델 초기화 완료 후 로직
        Model.Health.AddListener(OnHealthChanged);
    }
}
```

**주요 특징**
- Model 자동 생성 및 생명주기 관리
- `AtModelReady` 단계에서 모델 기반 초기화 수행
- Save/Load 훅 제공
- Model의 Variation 기반 초기화 지원

---

### 🧠 Controller&lt;E, M&gt; (Entity-Model Controller)
가장 정교한 형태의 Aggregate로, **Entity와 Model을 모두 관리**합니다.
복잡한 행동 패턴과 여러 컴포넌트가 필요한 게임 오브젝트에 사용됩니다.

```csharp
public class EnemyController : Controller<EnemyEntity, EnemyModel>
{
    protected override EnemyModel SetModel()
    {
        return DataManager.Instance.CreateModel<EnemyModel>("Goblin");
    }

    protected override void AtModelReady()
    {
        Entity.Initialize(Model);
        // Entity의 모든 Part가 Model에 접근 가능
    }
}
```

**주요 특징**
- Entity를 통한 Part 시스템 관리
- 생명주기가 Entity와 Part로 전파
- 여러 Part의 조합으로 복잡한 동작 구현
- 최대한의 재사용성과 조립성

---

## Aggregate 사용 규칙

프레임워크의 일관성과 안정성을 보장하기 위한 필수 규칙입니다.

1. **FactoryManager를 통한 생성**
   모든 Aggregate는 반드시 FactoryManager를 통해 생성되어야 합니다.
   ```csharp
   var player = await FactoryManager.Instance.CreateAsync<PlayerController>(
       playerPrefab, cancellationToken);
   ```

2. **Entity Controller는 Prefab 필수**
   Entity와 Part 기반 구조를 보장하기 위해 Prefab으로 생성되어야 합니다.

3. **DataManager를 통한 Model 초기화**
   Variation 데이터 기반 초기화 및 기획 데이터 통합을 보장합니다.
   ```csharp
   var model = DataManager.Instance.CreateModel<PlayerModel>("Default");
   ```

4. **ControllerManager 자동 등록**
   모든 Aggregate는 생성 시 ControllerManager에 자동 등록됩니다.

5. **래핑 프로퍼티 권장**
   Entity와 Part에서 Model 필드 접근 시 래핑 프로퍼티를 사용하여 재사용성과 안전성을 높입니다.

---

## 반응형 프로퍼티 시스템

Akasha의 **"신경계"**에 해당하는 핵심 시스템으로, 모든 상태 변화를 자동으로 전파합니다.

### 🔢 RxVar — 기본 반응형 변수

값이 변경될 때마다 리스너에게 자동 알림을 제공하는 기본 반응형 타입입니다.

```csharp
public class PlayerModel : BaseModel
{
    public RxVar<int> Health { get; private set; }

    public override void AtInit()
    {
        Health = new RxVar<int>(100, nameof(Health), this);
        Health.AddListener(value => Debug.Log($"Health: {value}"));
    }
}
```

**주요 특징**
- 값 변경 자동 알림
- 타입 안전성
- 소유자 검증
- 필드 이름 기반 직렬화

**사용 사례:** 체력, 이름, 카운터, 플래그 등

---

### ⚙️ RxMod — 수정 가능한 스탯 시스템

게임 스탯을 관리하는 강력한 Modifier 시스템입니다.

**계산식**
```
최종값 = (기본값 + OriginAdd) × (1 + AddMultiplier) × Multiplier + FinalAdd
```

```csharp
public RxMod<float> AttackPower { get; private set; }

// Modifier 추가
AttackPower.AddModifier("sword_bonus", 10f, ModifierType.OriginAdd);
AttackPower.AddModifier("buff", 0.5f, ModifierType.AddMultiplier); // +50%
```

**Modifier 타입**
1. **OriginAdd**: 평면 보너스 (기본값에 더함)
2. **AddMultiplier**: 백분율 보너스 (1 + 값으로 곱함)
3. **Multiplier**: 곱셈 스케일링 (값을 직접 곱함)
4. **FinalAdd**: 최종 보너스 (계산 후 더함)

**고급 기능**
- Stack 정책 (Stack, Replace, Custom)
- 지속 시간 관리 (UnityTimer 통합)
- Modifier 그룹 관리

**사용 사례:** 공격력, 이동 속도, 방어력 등

---

### 🔗 RxComputed — 파생 값

다른 Reactive 값들로부터 자동 계산되는 읽기 전용 값입니다.

```csharp
public RxComputed<bool> IsDead { get; private set; }
public RxComputed<float> HealthPercent { get; private set; }

public override void AtReadyModel()
{
    IsDead = new RxComputed<bool>(
        () => Health.Value <= 0,
        nameof(IsDead), this)
        .DependsOn(Health);

    HealthPercent = new RxComputed<float>(
        () => (float)Health.Value / MaxHealth.Value * 100f,
        nameof(HealthPercent), this)
        .DependsOn(Health, MaxHealth);
}
```

**주요 특징**
- 지연 평가 (lazy evaluation)
- 자동 의존성 추적
- 순환 의존성 감지
- 크로스 필드 반응성

**사용 사례:** 체력 퍼센트, IsDead, 총 방어력, UI 바인딩 등

---

### 🧱 RxData — 통합 기반 클래스

RxVar와 RxMod의 공통 부모 클래스로, 초기화가 필요한 필드의 공통 인터페이스를 제공합니다.

---

## Effect 시스템

데이터 기반의 **완전한 버프/디버프 프레임워크**를 제공합니다.

### 아키텍처

```
EffectSystem (Registry)
    ↓
EffectDefinition (Template) → Builder Pattern
    ↓
EffectInstance (Runtime)
    ↓
EffectManager (Lifecycle)
    → Modifiers (Stats)
    → DirectActions (Immediate)
    → PeriodicActions (Ticks)
    → Interpolated (Curves)
```

### Builder 패턴 사용 예시

```csharp
// 독 DoT 효과
var poisonEffect = new EffectDefinition.Builder("poison")
    .WithDisplayName("맹독")
    .WithCategory(EffectCategory.Debuff)
    .WithDuration(10f)
    .AddModifier("MoveSpeed", ModifierType.AddMultiplier, -0.3f) // -30% 속도
    .AddPeriodicAction(1f, instance => {
        var model = instance.Target;
        var damage = -5;
        model.GetRxField<RxVar<int>>("Health")?.Set(
            model.GetRxField<RxVar<int>>("Health").Value + damage
        );
    })
    .WithStackable(3)
    .Build();

// Effect 등록 및 적용
EffectManager.Instance.Effects.RegisterDefinition(poisonEffect);
EffectManager.Instance.ApplyEffect("poison", targetModel);
```

### 효과 적용 모드

1. **Immediate**: 즉시 적용 (버프/디버프)
   ```csharp
   .WithApplicationMode(EffectApplicationMode.Immediate)
   ```

2. **Delayed**: 지연 적용 (시한 폭탄, 저주)
   ```csharp
   .WithDelay(2f) // 2초 후 적용
   ```

3. **Periodic**: 주기적 틱 (독, 재생)
   ```csharp
   .AddPeriodicAction(1f, callback) // 1초마다 실행
   ```

4. **Interpolated**: 곡선 보간 (페이드 효과)
   ```csharp
   .WithInterpolation("Alpha", ModifierType.Multiplier, curve)
   ```

### Modifier vs DirectAction

- **Modifier**: 임시 수정 (제거 시 원래대로)
  ```csharp
  .AddModifier("AttackPower", ModifierType.Additive, 20f)
  ```

- **DirectAction**: 영구 수정 (HP 회복 등)
  ```csharp
  .DirectAdd("Health", 50f)
  .Percentage("Health", 0.5f) // 50% 회복
  ```

### 고급 기능

**Stack 시스템**
```csharp
.WithStackable(5)  // 최대 5중첩
.AddModifier(..., ModifierStackPolicy.Stack)     // 누적
.AddModifier(..., ModifierStackPolicy.Replace)   // 갱신
.AddModifier(..., ModifierStackPolicy.Custom)    // 커스텀
```

**면역 시스템**
```csharp
.WithImmunityTag("Poison")
EffectManager.Instance.AddImmunity(model, "Poison");
```

**조건 시스템**
```csharp
.WithCondition(model => model.Health.Value > 0)
```

**Dispel 시스템**
```csharp
EffectManager.Instance.DispelEffects(
    target,
    EffectCategory.Debuff,
    count: 2
);
```

---

## 상태 관리 시스템

### RxFlagState - 우선순위 기반 상태 머신

Enum 기반의 지능형 상태 관리 시스템으로, **우선순위와 조건에 따라 자동으로 상태를 전환**합니다.

```csharp
public enum CharacterState { Idle, Move, Attack, Stunned, Dead }

public class CharacterModel : BaseModel
{
    public RxFlagState<CharacterState> State { get; private set; }

    public override void AtInit()
    {
        State = new RxFlagState<CharacterState>(CharacterState.Idle, this);

        // 우선순위 설정 (높을수록 우선)
        State.SetPriority(CharacterState.Dead, 100)
             .SetPriority(CharacterState.Stunned, 50)
             .SetPriority(CharacterState.Attack, 20)
             .SetPriority(CharacterState.Move, 10)
             .SetPriority(CharacterState.Idle, 0);

        // 후보 상태 설정 (자동 전환 대상)
        State.SetCandidates(
            CharacterState.Idle,
            CharacterState.Move,
            CharacterState.Attack,
            CharacterState.Stunned,
            CharacterState.Dead
        );

        // 리스너 등록
        State.AddListener(newState => {
            Debug.Log($"State changed to: {newState}");
        });
    }
}

// 사용 예시
character.State.Request(CharacterState.Attack);

// 상태 비활성화
character.State.SetStateFlag(CharacterState.Attack, false);
character.State.RequestByPriority(); // Attack 스킵하고 다음 우선순위 선택
```

**주요 특징**
- 우선순위 기반 자동 전환
- 상태 플래그를 통한 조건 제어
- Fluent API (메서드 체이닝)
- 반응형 리스너 지원
- Unity Editor 디버그 뷰 지원

### RxFlagState&lt;TState, TCondition&gt; - 2단계 조건 시스템

외부 플래그와 연동하여 더욱 복잡한 상태 조건을 관리합니다.

```csharp
public enum CharacterFlag { CanMove, CanAttack, IsStunned, IsInvincible }

public class AdvancedModel : BaseModel
{
    public RxFlagState<CharacterState, CharacterFlag> State { get; private set; }
    public RxFlagSet<CharacterFlag> Flags { get; private set; }

    public override void AtInit()
    {
        Flags = new RxFlagSet<CharacterFlag>(this);
        State = new RxFlagState<CharacterState, CharacterFlag>(
            CharacterState.Idle, this);

        // 플래그와 바인딩 (플래그 변경 시 자동 상태 재계산)
        State.BindConditions(Flags);

        // 활성 조건 설정
        State.AddActivationCondition(CharacterState.Move, CharacterFlag.CanMove, true)
             .AddActivationCondition(CharacterState.Attack, CharacterFlag.CanAttack, true)
             .AddActivationCondition(CharacterState.Attack, CharacterFlag.IsStunned, false);
    }
}

// 사용 예시
model.Flags.SetValue(CharacterFlag.CanAttack, false); // 공격 불가능
// → State가 자동으로 Attack에서 다른 상태로 전환
```

**고급 기능**
- 복수 조건 AND 연산 (모든 조건 만족 필요)
- 자동 상태 재계산
- 플래그 변경 시 즉시 반응

---

### RxFlagSet - Enum 기반 플래그 관리

```csharp
public RxFlagSet<CharacterFlag> Flags { get; private set; }

Flags.SetValue(CharacterFlag.CanMove, true);
Flags.SetValue(CharacterFlag.IsStunned, false);

// 개별 플래그에 리스너 등록
Flags[CharacterFlag.CanMove].AddListener(value => {
    Debug.Log($"CanMove changed: {value}");
});
```

**주요 특징**
- Enum 기반 타입 안전성
- 개별 플래그 리스너 지원
- 조건 함수 바인딩
- 자동 평가 시스템

---

## Manager 시스템

모든 Manager는 `ManagerBase`를 상속하며 싱글톤, 생명주기, 씬 이벤트 구독을 자동 지원합니다.

### ControllerManager
- AggregateType별 컨테이너
- 비동기 Spawn
- 풀링 관리 및 정리
- 활성/비활성 쿼리

### FactoryManager
- Addressable 로딩
- Prefab 인스턴스화
- SaveData 복원
- 비동기 및 취소 토큰 지원

### EffectManager
- Effect 적용, 지속, Dispel, 면역 관리
- Update 루프 최적화
- Stack, 면역, 조건 처리

### DataManager
- Google Sheets 통합
- Variation 초기화
- 중앙 데이터 레지스트리

### RestoreManager
- Save/Load 관리
- Reflection 기반 자동 직렬화
- 복원 설정 관리

---

## Entity-Part 시스템

복잡한 오브젝트를 **작고 재사용 가능한 Part**로 조합합니다.

### BaseEntity

```csharp
public class PlayerEntity : BaseEntity<PlayerModel>
{
    private MovementPart movementPart;
    private CombatPart combatPart;

    protected override void AtInit()
    {
        movementPart = GetPart<MovementPart>();
        combatPart = GetPart<CombatPart>();
    }
}
```

**주요 기능**
- Model 소유
- Part 자동 발견
- 생명주기 전파
- 타입 기반 Part 조회

### BasePart

```csharp
public class MovementPart : BasePart<PlayerEntity, PlayerModel>
{
    protected override void AtInit()
    {
        // Model의 MoveSpeed에 접근
        Model.MoveSpeed.AddListener(OnSpeedChanged);
    }

    private void OnSpeedChanged(float newSpeed)
    {
        // 이동 속도 업데이트
    }
}
```

**주요 기능**
- Entity/Model 접근
- 생명주기 동기화
- Save/Load 훅 지원

**예시 Part 구조**
```
PlayerController (Controller<PlayerEntity, PlayerModel>)
    └── PlayerEntity
        ├── MovementPart
        ├── CombatPart
        ├── AnimationPart
        └── AudioPart
```

---

## Save/Load 시스템

**Reflection 기반 자동 직렬화 시스템**

```csharp
// 저장
await SaveLoadSystem.Instance.SaveGameAsync("slot1");

// 로드
await SaveLoadSystem.Instance.LoadGameAsync("slot1");

// 커스텀 Save 로직
protected override void AtSave(RestoreConfig config)
{
    config.Set("customData", myData);
}

// 커스텀 Load 로직
protected override void AtLoad(RestoreConfig config)
{
    myData = config.Get<MyDataType>("customData");
}
```

**특징**
- Rx 필드 자동 직렬화
- Transform 데이터 보존
- 타입 안전성
- JSON 직렬화
- 암호화 옵션
- 전체 스냅샷 지원

---

## 테스트 및 품질

### 테스트 커버리지

- **전체 테스트**: 309개
- **라인 커버리지**: 48.7% (2,947 / 6,049 라인)
- **메소드 커버리지**: 52.5% (534 / 1,017 메소드)

### 높은 커버리지 클래스

| 클래스 | 라인 커버리지 | 메소드 커버리지 |
|--------|---------------|-----------------|
| Singleton | 100% | 100% |
| ObjectContainer | 95.7% | 100% |
| EffectDefinition | 92.8% | 93.4% |
| EffectInstance | 92.8% | 88.5% |
| AggregateRoot | 87.3% | 100% |
| RxFlagState | 70.9% | 87.5% |

### 테스트 구조

```
Assets/Tests/
├── Runtime/
│   ├── AggregateRootTests.cs
│   ├── EffectManagerTests.cs
│   ├── RxFlagStateTests.cs
│   ├── BaseItemTests.cs
│   └── ... (총 30개 테스트 파일)
└── TestAsset/
    ├── TestModels.cs
    └── TestControllers.cs
```

---

## 핵심 원칙

1. **소유권과 호출자 검증**
   - `IRxOwner`, `IRxCaller` 인터페이스를 통한 접근 제어

2. **예측 가능한 생명주기**
   - 8단계 명확한 생명주기 흐름

3. **반응형 데이터 플로우**
   - 모든 상태 변화는 Reactive Properties를 통해 전파

4. **Composition Over Inheritance**
   - Entity-Part 패턴으로 재사용성 극대화

5. **데이터 기반 설계**
   - Google Sheets 통합, Variation 시스템

6. **Effect 기반 수정 시스템**
   - 모든 스탯 변경은 Effect를 통해 추적 가능

7. **Async 우선 아키텍처**
   - 비동기 생성, 로딩으로 프레임 히칭 방지

8. **Null 안전성**
   - `#nullable enable`로 전체 코드베이스 안전성 보장

---

## 프레임워크 강점

### 아키텍처
1. ✅ 클린 아키텍처 (DDD 기반)
2. ✅ 명확한 책임 분리
3. ✅ 높은 테스트 가능성

### 개발 생산성
4. ✅ Builder 패턴으로 직관적인 API
5. ✅ 자동 직렬화로 Save/Load 간소화
6. ✅ Reactive 시스템으로 UI 자동 업데이트

### 성능 최적화
7. ✅ 내장 오브젝트 풀링
8. ✅ 비동기 로딩으로 프레임 드롭 방지
9. ✅ Update 루프 최적화

### 확장성
10. ✅ 데이터 중심 설계로 밸런스 조정 용이
11. ✅ Part 시스템으로 기능 조합 자유
12. ✅ Effect 시스템으로 새 효과 추가 5분 이내

---

## 적합한 프로젝트

### 강력 추천 ⭐⭐⭐⭐⭐
- **RPG/ARPG** - 복잡한 스탯, 버프/디버프 시스템
- **로그라이크** - 아이템 시너지, 무작위 효과
- **대규모 프로젝트** - 장기 유지보수 중요

### 추천 ⭐⭐⭐⭐
- **PVP 액션** - CC(Crowd Control) 관리
- **전략 게임** - 복잡한 상태 관리
- **라이브 서비스** - 데이터 기반 업데이트

### 보통 ⭐⭐⭐
- **플랫포머** - 파워업 관리 유용
- **퍼즐 게임** - 상태 관리 필요 시

### 비추천 ⭐⭐
- **초단기 프로토타입** - 학습 시간 필요
- **매우 단순한 게임** - 오버엔지니어링
- **저사양 모바일** - 메모리 사용량 고려 필요

---

## 성능 지표

### 메모리 사용량
- Aggregate당 평균: ~2KB
- Effect 시스템: ~50KB (100개 효과 기준)
- Reactive 시스템: ~1KB per 10 properties

### 처리 속도
- Effect 업데이트: 1000개 인스턴스 < 1ms
- 상태 전환: < 0.1ms
- Reactive 전파: < 0.05ms per listener

---

## 라이센스

MIT License

---

## 결론

**Akasha**는 Unity 게임 개발을 위한 **프로덕션 레디 프레임워크**입니다.

유지보수성, 테스트 가능성, 확장성을 최우선으로 고려하며, 복잡한 게임 로직을 체계적으로 관리할 수 있는 강력한 도구를 제공합니다.

**Domain-Driven Design**과 **Reactive Programming**의 장점을 결합한
**엔터프라이즈급 게임 개발 솔루션**입니다.

---

**Made with ❤️ for Unity Developers**
