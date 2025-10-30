# M (Mercenary) - Akasha Framework 프로젝트

## 프로젝트 개요

**M**은 **Akasha Framework** 기반 Unity 게임 프로젝트입니다.
Akasha는 Domain-Driven Design(DDD) 원칙과 반응형 프로그래밍을 적용한 엔터프라이즈급 게임 개발 프레임워크로, 복잡한 게임 로직을 체계적으로 관리합니다.

## 환경 정보

- **Unity 버전**: 2021.3 이상 권장
- **.NET 버전**: .NET Standard 2.1
- **언어**: C#
- **Git 저장소**: https://github.com/DevAkasha/M.git

## 자주 사용하는 명령어

### Unity 빌드
```bash
# 프로젝트 빌드 (Unity CLI 사용 시)
Unity.exe -quit -batchmode -projectPath "C:\Users\minyoung\M" -buildWindows64Player "Build/M.exe"
```

### .NET 빌드
```bash
dotnet build M.sln
```

### Git 작업
```bash
git status
git add .
git commit -m "메시지"
git push origin main
```

## 폴더 구조

```
M/
├── Assets/
│   ├── Scripts/
│   │   ├── Akasha/              # 프레임워크 코어 (gitignore에 포함)
│   │   │   ├── Core/            # 핵심 시스템
│   │   │   │   ├── Aggregate/   # Controller, Model, Entity, Part
│   │   │   │   ├── Infrastructure/  # 기반 시스템
│   │   │   │   └── Reactive/    # RxVar, RxMod, RxComputed
│   │   │   ├── Editor/          # 에디터 도구
│   │   │   ├── Gameplay/        # 게임플레이 시스템
│   │   │   │   ├── Effects/     # Effect 시스템
│   │   │   │   └── States/      # FSM, RxStateFlagSet
│   │   │   └── Management/      # Manager 시스템
│   │   │       ├── Data/        # DataManager, Variation
│   │   │       ├── Factory/     # FactoryManager
│   │   │       ├── ObjectManage/  # ControllerManager
│   │   │       └── Restore/     # Save/Load
│   │   └── Mercenary/           # 게임 로직
│   │       ├── Character/       # 캐릭터 관련
│   │       │   ├── Commander/   # 지휘관 (Player, Hero, Boss)
│   │       │   └── Unit/        # 유닛
│   │       └── GameManager/     # 게임 매니저
│   └── OutSource/               # 외부 라이브러리 (gitignore)
├── Akasha_Framework_Documentation.md  # 프레임워크 상세 문서
└── ProjectSettings/
```

## 핵심 아키텍처 패턴

### Aggregate 시스템

모든 게임 오브젝트는 **Aggregate** 단위로 관리됩니다:

#### 1. Controller (로직 & 생명주기)

세 가지 타입:
- **PureController**: Model 없는 순수 로직 (시스템, 매니저)
- **ModelController<M>**: Model만 가짐 (UI, 데이터 컨트롤러)
- **Controller<E, M>** (EMController): Entity + Model 가짐 (캐릭터, 복잡한 오브젝트)

```csharp
// EMController 예시
public sealed class PlayerController : CommanderController<PlayerEntity, PlayerModel>
{
    // 생명주기 후크만 사용
    protected override void AtStart() { }
    protected override void AtModelReady() { }
}
```

#### 2. Model (데이터 & 상태)

반응형 프로퍼티를 포함하는 데이터 컨테이너:

```csharp
public sealed class PlayerModel : BaseModel
{
    public RxVar<float> MoveSpeed;        // 기본 반응형 변수
    public RxMod<float> AttackPower;      // Modifier 지원 (버프/디버프)
    public RxMod<int> Health;
    public RxComputed<float> TotalDamage; // 계산된 값

    public PlayerModel()
    {
        MoveSpeed = new RxVar<float>(5f, nameof(MoveSpeed), this);
        AttackPower = new RxMod<float>(10f, nameof(AttackPower), this);
        Health = new RxMod<int>(100, nameof(Health), this);
        TotalDamage = CreateComputed(() => AttackPower.Value * 1.5f, nameof(TotalDamage));
    }
}
```

#### 3. Entity (Model 생성 & Part 조립)

```csharp
[GenerateWrappers]  // Source Generator가 Model 프로퍼티 래퍼 자동 생성
public sealed partial class PlayerEntity : BaseEntity<PlayerModel>
{
    protected override PlayerModel SetupModel()
    {
        return DataManager.Instance.CreateModel<PlayerModel>();
    }
}
```

#### 4. Part (조립식 기능 컴포넌트)

특정 기능을 담당하는 독립적인 컴포넌트:

```csharp
[GenerateWrappers]
public sealed partial class PlayerMovementPart : BasePart<PlayerEntity, PlayerModel>
{
    protected override void AtModelReady()
    {
        // Entity.MoveSpeed로 직접 접근 가능 (래퍼 자동 생성됨)
        Debug.Log($"Move Speed: {Entity.MoveSpeed.Value}");
    }
}
```

## 코딩 컨벤션

### 네이밍 규칙

**파일명**
- Controller: `{Name}Controller.cs`
- Model: `{Name}Model.cs`
- Entity: `{Name}Entity.cs`
- Part: `{Name}{Feature}Part.cs` (예: `PlayerMovementPart.cs`)

**클래스**
- `sealed` 사용하여 상속 방지 (필요시)
- `partial` 키워드 사용 (Source Generator 사용 시 필수)

**필드**
- RxData는 **PascalCase** 사용
- private 필드는 **camelCase** 사용

### 코드 스타일

- **들여쓰기**: 4칸 (C# 표준)
- **중괄호**: K&R 스타일 (같은 줄에 여는 중괄호)
- **네임스페이스**: 파일당 하나, top-level 권장

## 필수 규칙 및 베스트 프랙티스

### ⚠️ 반드시 지켜야 할 규칙

#### 1. FactoryManager를 통한 생성

**절대** Unity의 `Instantiate`를 직접 사용하지 마세요. 항상 FactoryManager 사용:

```csharp
// ✅ 올바른 방법
var player = await FactoryManager.Instance.CreateFromAddressableAsync<PlayerController>("Player");

// ❌ 잘못된 방법
Instantiate(playerPrefab);  // 생명주기 보장 안됨, 풀링 불가
```

#### 2. EMController는 Prefab 필수

Controller<E, M>는 반드시 Prefab 또는 Addressable로 생성:

```csharp
// ✅ 올바른 방법
CreateFromPrefabAsync<PlayerController>(prefab);

// ❌ 잘못된 방법 (Entity/Part 없음)
CreateFromClass<PlayerController>();  // EMController는 사용 불가
```

#### 3. Unity 생명주기 메서드 오버라이드 금지

Unity 기본 메서드 대신 **후크 메서드** 사용:

```csharp
// ❌ 금지
void Start() { }
void OnEnable() { }
void Update() { }

// ✅ 사용
protected override void AtStart() { }
protected override void AtEnable() { }
// Update는 Unity Update에서 직접 호출 가능
```

**사용 가능한 후크 메서드**:
- `AtAwake()`, `AtInit()`, `AtStart()`, `AtLateStart()`
- `AtModelReady()` (Model/Entity 모드)
- `AtEnable()`, `AtDisable()`
- `AtPoolInit()`, `AtPoolDeinit()` (풀링 사용 시)
- `AtSave()`, `AtLoad()` (저장/로드 시)
- `AtDeinit()`, `AtDestroy()`

#### 4. DataManager로 Model 초기화

Variation 데이터 활용:

```csharp
// ✅ 올바른 방법
return DataManager.Instance.CreateModel<PlayerModel>("Warrior");

// ⚠️ 가능하지만 권장하지 않음 (기본값만 사용)
return new PlayerModel();
```

#### 5. Model의 RxData는 public으로 유지

```csharp
// ✅ 올바른 방법
public RxVar<float> MoveSpeed;

// ❌ 잘못된 방법 (DataManager Variation 시스템 동작 안 함)
private RxVar<float> moveSpeed;
```

### 권장 사항

#### Part 사용 시점

다음 경우 Part로 분리:
- 기능을 모듈화하고 싶을 때
- 여러 Entity에서 재사용하고 싶을 때
- 특정 기능을 선택적으로 추가/제거하고 싶을 때

#### Controller 타입 선택

- **PureController**: 시스템 레벨 로직, 간단한 매니저
- **ModelController**: UI 컨트롤러, 데이터 관리자
- **EMController**: 게임 캐릭터, 건물, 복잡한 오브젝트

## 반응형 시스템 활용

### RxVar<T> - 기본 반응형 변수

```csharp
var score = new RxVar<int>(0, nameof(score), this);
score.Listen((oldVal, newVal) => Debug.Log($"Score: {oldVal} -> {newVal}"));
score.Value = 100;  // 리스너 자동 호출
```

### RxMod<T> - Modifier 지원

```csharp
var attackPower = new RxMod<float>(10f, nameof(attackPower), this);

// 버프 적용: 10초간 공격력 50% 증가
var buffId = attackPower.AddModifier(
    0.5f,
    ModifierType.AddMultiplier,
    duration: 10f
);

// 나중에 제거
attackPower.RemoveModifier(buffId);
```

**계산 순서**: `(base + OriginAdd) * (1 + AddMultiplier) * Multiplier + FinalAdd`

### RxComputed<T> - 계산된 값

```csharp
// 의존하는 값이 변경되면 자동 재계산
TotalDamage = CreateComputed(() => AttackPower.Value * CriticalRate.Value, nameof(TotalDamage));
```

## 에디터 도구 사용법

### Entity와 Model 생성

1. Project 창에서 폴더 선택 (예: `Scripts/Mercenary/Character/Wizard`)
2. 우클릭 → **Create/Akasha/Entity and Model**
3. `WizardEntity.cs`, `WizardModel.cs` 자동 생성

### Part 생성

1. Project 창에서 폴더 선택
2. 우클릭 → **Create/Akasha/Part**
3. 생성 후 이름 변경 권장 (예: `FolderPart.cs` → `WizardCombatPart.cs`)

### Variation 데이터 임포트

1. Google Sheets에 데이터 작성
2. Unity 에디터: **Tools → Akasha → Import Variations**
3. Sheet ID 입력 및 임포트 실행
4. CSV 파일 생성 및 자동 검증

## 디버깅 팁

### Aggregate 추적

ControllerManager에서 모든 Aggregate 확인 가능:

```csharp
// 모든 활성 Controller 조회
var allControllers = ControllerManager.Instance.GetAllControllers();

// 특정 타입 조회
var player = ControllerManager.Instance.GetController<PlayerController>(instanceId);
```

### RxData 디버깅

```csharp
// RxDebugTools 사용
RxDebugTools.LogAllRxDatas(model);  // Model의 모든 RxData 출력
```

### FSM 시각화

Inspector에서 FSM 상태 실시간 확인 가능

## 중요한 참고 문서

프레임워크 상세 정보는 **Akasha_Framework_Documentation.md** 참조

## 주의 사항

### gitignore 설정

- `Assets/Scripts/Akasha/` - 프레임워크 코드는 무시됨
- `Assets/OutSource/` - 외부 라이브러리는 무시됨
- `.csproj`, `.sln` 파일은 무시됨 (Unity가 자동 생성)

### 버전 관리

- Akasha Framework 버전: 1.0
- 프레임워크 코드를 수정한 경우 별도 브랜치 사용
- Mercenary 게임 로직만 메인 브랜치에 커밋

## FAQ

**Q: Pure/Model/Entity 중 어떤 것을 선택해야 하나요?**
- 시스템 레벨 → PureController
- UI/데이터만 → ModelController
- 게임 오브젝트 → EMController

**Q: Part 없이 Entity만 사용할 수 있나요?**
- 가능합니다. 간단한 경우 Entity에 직접 로직 작성 가능
- 복잡해지면 Part로 분리 권장

**Q: Variation은 필수인가요?**
- 아니요, "default" variation 사용 시 기본값으로 초기화
- 기획 데이터 통합을 위해 사용 권장

**Q: 여러 개의 Model을 가질 수 있나요?**
- 한 Controller는 하나의 Model만 가능
- 여러 데이터 소스 필요 시 다른 Controller 참조하거나 Model 내부에 중첩 구조 사용
