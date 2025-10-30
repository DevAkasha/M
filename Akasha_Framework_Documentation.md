# Akasha Framework
### Unity를 위한 엔터프라이즈급 게임 개발 프레임워크

---

## 개요

**Akasha**는 **Domain-Driven Design(DDD)** 원칙과 **반응형 프로그래밍**을 기반으로 설계된 Unity 게임 개발 프레임워크입니다.
복잡한 게임 로직과 대규모 프로젝트를 체계적으로 관리할 수 있도록 설계되었으며, **유지보수성**, **확장성**, **개발 유연성**을 최우선으로 고려합니다.

---

## 핵심 철학

### 1. Aggregate 중심 아키텍처
모든 게임 오브젝트는 **Aggregate**라는 명확한 경계를 가진 단위로 관리됩니다.
이는 DDD의 핵심 개념을 게임 개발에 적용한 것으로, 각 Aggregate는 **독립적인 생명주기와 책임**을 가지며 일관된 규칙에 따라 생성, 관리, 소멸됩니다.

Aggregate 내부는 단일한 **AggregateRoot**와 단일한 **Model**(선택적)을 포함하여:
- **Aggregate 간 상호작용**은 AggregateRoot가 담당
- **상태 관리**는 Model이 담당
- **Aggregate의 확장**은 Part로 지속적으로 확장 가능
- Part를 사용하기 위해서는 Entity를 필요로 함

### 2. 타입 안전성과 명확한 계약
모든 컴포넌트는 명시적인 타입 파라미터를 통해 의존성을 선언하며, 컴파일 타임에 타입 안전성을 보장합니다.

### 3. 반응형 데이터 흐름
상태 변화는 **RxVar**, **RxMod**, **RxComputed** 등의 반응형 프로퍼티를 통해 자동으로 전파되며, 수동 동기화가 필요 없습니다.

### 4. 관심사의 분리
- **Controller**: 게임 로직과 생명주기 관리
- **Model**: 데이터와 상태 관리
- **Entity**: Model 인스턴스 생성 및 Part 조립
- **Part**: 특정 기능의 조립식 확장

---

## Aggregate 시스템

Akasha의 핵심은 **Aggregate 시스템**으로, 게임 오브젝트를 일관된 방식으로 생성, 관리, 소멸합니다.

### AggregateRoot

모든 Aggregate의 기반 클래스로, 다음 책임을 가집니다:

**주요 기능**
- **고유 식별**: 각 Aggregate는 고유한 InstanceId를 가짐
- **자동 등록**: Awake 시 ControllerManager에 자동 등록
- **생명주기 추적**: IsInitialized, IsInPool 등의 상태 관리
- **풀링 지원**: IPoolable 인터페이스 구현으로 오브젝트 풀링 지원
- **Transform 캐싱**: 성능 최적화를 위한 Transform 캐싱

**Aggregate 타입**
- **Pure**: Model 없는 순수 로직 Controller
- **Model**: Model만 있는 Controller
- **Entity**: Model + Entity + Parts 구조
- **Unknown**: 알 수 없는 타입

**생명주기 흐름**
```
생성 → Awake → (PoolInit) → Enable → Start → LateStart
    ↓
(사용)
    ↓
Disable → (PoolDeinit) → Deinit → Destroy
```

### Controller

게임 로직의 중심으로, 세 가지 타입이 존재합니다:

#### 1. PureController
Model 없이 순수 로직만 담당하는 Controller입니다.

**특징**
- 가장 가벼운 형태
- RxVar를 직접 관리 가능
- 시스템 레벨 컨트롤러에 적합
- Prefab 불필요
- 게임 시스템, 매니저 등에 사용

#### 2. ModelController&lt;M&gt;
Model만 가지는 Controller입니다.

**특징**
- 데이터 중심 설계
- Prefab 불필요
- UI Controller, Data Controller에 적합
- 반응형 데이터를 활용한 상태 관리
- 간단한 로직과 데이터 바인딩

#### 3. Controller&lt;E, M&gt; (EMController)
Entity와 Model을 모두 가지는 완전한 형태의 Controller입니다.

**특징**
- Part 시스템 사용 가능
- Prefab 필수
- 게임 캐릭터, 건물 등 복잡한 오브젝트에 적합
- 조립식 설계로 기능 확장
- 생명주기를 Entity와 Part에 자동 전파

**Controller 생명주기 후크**
- **AtAwake()**: GameObject Awake 시
- **AtInit()**: 초기화 시
- **AtStart()**: Start 시
- **AtLateStart()**: Start 이후
- **AtModelReady()**: Model 준비 완료 시 (Model/Entity 모드)
- **AtEnable()**: Enable 시
- **AtDisable()**: Disable 시
- **AtPoolInit()**: 풀에서 Spawn 시
- **AtPoolDeinit()**: 풀로 Return 시
- **AtSave()**: 저장 시
- **AtLoad()**: 로드 시
- **AtDeinit()**: 정리 시
- **AtDestroy()**: Destroy 시

### Model

데이터와 상태를 관리하는 반응형 데이터 컨테이너입니다.

**주요 기능**
- **RxData 자동 추적**: RegisterRx를 통한 자동 등록
- **Modifier 관리**: IModifiable 필드 자동 인덱싱
- **직렬화 지원**: GetAllRxDatas()를 통한 자동 직렬화
- **필드 접근**: GetRxField&lt;T&gt;(fieldName)으로 동적 접근
- **생명주기 후크**: AtInit(), AtReadyModel()

**구조**
Model은 게임 데이터의 단일 진실 공급원(Single Source of Truth)입니다. 모든 반응형 프로퍼티(RxVar, RxMod, RxComputed)를 포함하며, 이들의 생명주기를 관리합니다. DataManager를 통해 Variation 데이터로 초기화되며, 자동으로 직렬화 가능합니다.

### Entity

Model 인스턴스를 생성하고 Part를 조립하는 조립 계층입니다.

**주요 기능**
- **Model 생성**: SetupModel()을 통한 Model 인스턴스 생성
- **Part 관리**: 자식 Part를 자동으로 찾아 등록
- **Part 접근**: GetPart&lt;T&gt;()로 특정 Part 검색
- **생명주기 전파**: 모든 생명주기 이벤트를 Part에 전파

**[GenerateWrappers] Attribute**
- Entity의 Model 프로퍼티를 EntityPart에서 직접 접근 가능한 래퍼 생성
- Source Generator가 자동으로 프로퍼티 래퍼 생성
- 타입 안전성 보장
- 보일러플레이트 코드 제거

**역할**
Entity는 Controller와 Part 사이의 중재자 역할을 합니다. Model의 생명주기를 제어하고, Part들을 등록하며, Controller로부터 받은 생명주기 이벤트를 모든 Part에 전파합니다.

### Part

특정 기능을 담당하는 조립식 컴포넌트입니다.

**주요 기능**
- **Model 직접 접근**: Entity를 통해 Model에 직접 접근
- **독립적 생명주기**: 자체 생명주기 후크 보유
- **조립식 설계**: 기능별로 Part를 추가/제거하여 확장
- **재사용성**: 다른 Entity에서 동일한 Part 재사용 가능

**Part 생명주기 후크**
- **AtAwake()**: Awake 시
- **AtModelReady()**: Entity의 Model 준비 완료 시
- **AtInit()**: 초기화 시
- **AtStart()**: Start 시
- **AtLateStart()**: LateStart 시
- **AtEnable()**: Enable 시
- **AtDisable()**: Disable 시
- **AtPoolInit()**: 풀에서 Spawn 시
- **AtPoolDeinit()**: 풀로 Return 시
- **AtSave()**: 저장 시
- **AtLoad()**: 로드 시
- **AtDeinit()**: 정리 시
- **AtDestroy()**: Destroy 시

**설계 철학**
Part는 단일 책임 원칙(Single Responsibility Principle)을 따릅니다. 예를 들어, PlayerEntity는 MovementPart, CombatPart, AnimationPart 등으로 분리되어 각각 이동, 전투, 애니메이션 기능을 담당합니다.

---

## 반응형 프로퍼티 시스템

Akasha의 반응형 시스템은 상태 변화를 자동으로 감지하고 전파하는 강력한 도구입니다.

### RxVar&lt;T&gt;

가장 기본적인 반응형 변수입니다.

**특징**
- 값 변경 시 자동으로 리스너 호출
- 리스너 등록 시 즉시 현재 값으로 호출
- Model/Controller에서만 생성 가능 (IRxOwner)
- 타입 안전성 보장
- 메모리 효율적

**사용 시나리오**
플레이어의 점수, 체력, 이름 등 단순한 값을 저장하고 변화를 감지할 때 사용합니다. 값이 변경되면 등록된 모든 리스너가 자동으로 호출되어 UI 업데이트 등의 작업을 수행할 수 있습니다.

### RxMod&lt;T&gt;

Modifier를 지원하는 반응형 변수입니다.

**Modifier 타입**
- **OriginAdd**: 기본값에 더함
- **AddMultiplier**: 합산 후 곱함
- **Multiplier**: 곱셈
- **FinalAdd**: 최종 더함

**계산 순서**
```
result = (base + OriginAdd) * (1 + AddMultiplier) * Multiplier + FinalAdd
```

**주요 기능**
- 기본 값과 계산된 값을 자동으로 분리
- 시간 제한이 있는 Modifier 지원
- 스택 가능한 Modifier 지원
- Modifier Policy를 통한 유연한 적용 전략
- 자동 재계산 및 리스너 알림

**Modifier Policy**
- **TakeMax**: 최대값 선택
- **TakeMin**: 최소값 선택
- **Add**: 누적
- **Replace**: 교체
- **KeepFirst**: 첫 번째 값 유지
- **Average**: 평균

**사용 시나리오**
공격력, 이동속도, 방어력 등 버프/디버프의 영향을 받는 스탯에 사용합니다. 여러 버프가 동시에 적용되어도 계산 순서가 보장되며, 지속 시간이 끝나면 자동으로 제거됩니다.

### RxComputed&lt;T&gt;

다른 반응형 변수에 의존하는 계산된 값입니다.

**특징**
- 의존하는 값이 변경되면 자동으로 재계산
- 순환 의존성 감지 및 경고
- Lazy 평가 (처음 접근 시 계산)
- Model 내부에서만 생성 가능
- 캐싱으로 불필요한 재계산 방지

**사용 시나리오**
여러 스탯을 조합한 최종 데미지, 총 체력, 이동 가능 여부 등 다른 값에 의존하는 계산된 값에 사용합니다. 의존하는 값이 변경되면 자동으로 재계산되므로 수동 동기화가 필요 없습니다.

### IRxCaller와 IRxOwner

반응형 시스템의 타입 안전성을 보장하는 인터페이스입니다.

**IRxOwner**: RxData를 소유할 수 있는 타입
- **BaseModel**: 모든 Rx 타입 소유 가능 (IsRxAllOwner = true)
- **PureController**: RxVar만 소유 가능 (IsRxVarOwner = true)

**IRxCaller**: RxData의 값을 설정할 수 있는 타입
- **IsLogicalCaller**: 로직 계층 (Controller, Model)
- **IsFunctionalCaller**: 기능 계층 (Entity, Part)
- **IsMultiRolesCaller**: 다중 역할 (PureController, Model)

**설계 의도**
이 시스템은 잘못된 위치에서의 상태 변경을 컴파일 타임에 방지합니다. 예를 들어, Part에서 RxVar를 직접 생성하거나, Controller가 아닌 곳에서 특정 값을 변경하는 것을 막아 아키텍처 일관성을 보장합니다.

---

## Effect 및 Modifier 시스템

게임 효과를 정의하고 관리하는 시스템입니다.

### EffectSystem

Effect 정의를 등록하고 관리합니다.

**주요 기능**
- Effect 정의 등록 및 조회
- 카테고리별 Effect 검색
- Effect 정의 유효성 검증
- 중앙 집중식 Effect 관리

**역할**
EffectSystem은 게임에서 사용되는 모든 효과(버프, 디버프, 상태이상 등)의 정의를 중앙에서 관리합니다. 효과의 ID, 카테고리, 기본 설정 등을 저장하고, 런타임에 빠르게 조회할 수 있도록 캐싱합니다.

### Modifier 시스템

RxMod&lt;T&gt;에 내장된 Modifier 시스템으로, 게임 효과를 구현합니다.

**적용 방식**
- **버프**: 지속 시간 동안 스탯 증가
- **디버프**: 지속 시간 동안 스탯 감소
- **스택**: 동일 효과를 여러 번 적용
- **영구**: 지속 시간 없는 효과

**특징**
- 자동 타이머 관리
- 효과 종료 시 자동 제거
- 실시간 재계산
- 스택 가능 여부 설정
- Policy 기반 유연한 적용

**사용 예시**
- 힘의 물약: 10초간 공격력 50% 증가 (AddMultiplier)
- 슬로우: 5초간 이동속도 30% 감소 (Multiplier)
- 화상: 5초간 초당 2 데미지 (FinalAdd, stackable)
- 신성한 축복: 최종 공격력에 20 추가 (FinalAdd)

---

## FSM 및 RxStateFlagSet 시스템

상태 기반 로직을 구현하는 시스템입니다.

### FSM&lt;TState&gt;

타입 안전한 유한 상태 머신입니다.

**주요 기능**
- **타입 안전**: Enum 기반으로 컴파일 타임 안전성 보장
- **전이 규칙**: AddTransitionRule로 복잡한 전이 조건 정의
- **우선순위**: 여러 후보 중 우선순위가 높은 상태로 자동 전이
- **콜백**: OnEnter/OnExit로 상태 진입/이탈 시 로직 실행
- **상태 변경 리스너**: 상태 변경 시 자동 알림

**우선순위 시스템**
여러 상태 후보가 있을 때, 우선순위가 가장 높고 전이 조건을 만족하는 상태로 자동 전이합니다. 예를 들어, Attack(우선순위 10) > Chase(우선순위 5) > Patrol(우선순위 1)로 설정하면, 공격 조건이 만족되면 다른 상태보다 우선적으로 공격 상태로 전이합니다.

**사용 시나리오**
캐릭터 AI, 애니메이션 상태, 게임 모드 등 명확한 상태를 가진 시스템에 사용합니다. 각 상태에서 허용되는 다음 상태를 명시적으로 정의하여 잘못된 상태 전이를 방지합니다.

### RxStateFlagSet&lt;TEnum&gt;

여러 개의 boolean 플래그를 관리하는 시스템입니다.

**주요 기능**
- **조건 기반 플래그**: SetCondition으로 함수 기반 자동 평가
- **수동 설정**: SetValue로 직접 설정
- **집합 연산**: AnyActive, AllSatisfied, NoneActive
- **스냅샷**: Snapshot()으로 현재 상태 저장
- **리스너**: 개별 플래그 변경 감지

**집합 연산**
- **AnyActive**: 주어진 플래그 중 하나라도 true인지 확인
- **AllSatisfied**: 주어진 플래그가 모두 true인지 확인
- **NoneActive**: 주어진 플래그가 모두 false인지 확인
- **ActiveFlags**: 현재 true인 모든 플래그 반환

**사용 시나리오**
캐릭터 상태 (착지 여부, 이동 중, 전투 중, 기절 등), 게임 조건 (퀘스트 완료, 아이템 획득 등), 복잡한 조건 검사 등에 사용합니다. 여러 boolean 상태를 하나의 집합으로 관리하여 복잡한 조건 로직을 단순화합니다.

---

## Manager 시스템

프레임워크의 핵심 관리자들입니다.

### FactoryManager

Aggregate 생성을 담당하는 팩토리 매니저입니다.

**주요 기능**
- **Addressable 통합**: 비동기 리소스 로딩
- **Prefab 지원**: GameObject Prefab에서 생성
- **클래스 생성**: Pure/ModelController 직접 생성
- **배치 생성**: 프레임 분산으로 부하 감소
- **자동 설정**: IsSceneCreated = false 자동 설정

**생성 방식**
- **CreateFromAddressableAsync**: Addressable 키로 비동기 생성
- **CreateFromPrefabAsync**: Prefab 참조로 비동기 생성
- **CreateFromClass**: Pure/ModelController를 클래스에서 직접 생성
- **CreateMultipleAsync**: 여러 개 동시 생성
- **CreateMultipleBatchedAsync**: 프레임 분산 배치 생성

**제약 사항**
- EMController는 반드시 Prefab 또는 Addressable로 생성
- 모든 Aggregate는 FactoryManager를 통해서만 생성 권장

**설계 의도**
일관된 생성 전략을 강제하여 오브젝트 풀링, Addressable 통합, 생명주기 관리를 자동화합니다. 모든 Aggregate가 동일한 방식으로 생성되므로 유지보수가 쉽고 버그를 줄일 수 있습니다.

### DataManager

Model의 데이터 초기화를 담당합니다.

**Variation 시스템**
- Google Sheets에서 데이터 임포트
- Model 타입별 여러 Variation 지원
- CSV 형식으로 관리
- 자동 타입 변환 및 검증

**주요 기능**
- **자동 초기화**: Model 생성 시 Variation 데이터 자동 적용
- **검증**: Sheet와 Model 필드 비교 및 경고
- **타입 안전**: 자동 타입 변환 및 오류 처리
- **캐싱**: 빠른 조회를 위한 메모리 캐싱

**Variation 정의 예시 (Google Sheets)**
```
ModelType   | VariationName | MoveSpeed | AttackPower | Health
PlayerModel | default       | 5.0       | 10.0        | 100
PlayerModel | Warrior       | 4.0       | 15.0        | 150
PlayerModel | Mage          | 6.0       | 8.0         | 80
```

**워크플로우**
1. 기획자가 Google Sheets에 데이터 입력
2. Unity 에디터에서 Import 실행
3. CSV로 변환 및 VariationRegistry에 저장
4. Model 생성 시 자동으로 Variation 적용
5. 필드 불일치 시 경고 출력

**이점**
기획자가 코드 수정 없이 데이터를 관리할 수 있으며, 자동 검증으로 오타를 방지합니다. Hot Reload를 통해 게임 재시작 없이 데이터 변경이 가능합니다.

### ControllerManager

모든 Aggregate의 생명주기와 풀링을 관리합니다.

**주요 기능**
- **자동 등록**: AggregateRoot.Awake 시 자동 등록
- **타입별 컨테이너**: Pure/Model/Entity 각각 별도 관리
- **오브젝트 풀링**: Prefab 기반 풀링 시스템
- **Prewarm**: 미리 생성하여 초기 부하 감소
- **통계**: 등록/활성/풀링 오브젝트 수 추적

**ObjectContainer 구조**
```
ControllerManager
  ├─ PureContainer
  ├─ ModelContainer
  └─ EntityContainer
      ├─ ObjectPool (Prefab1)
      ├─ ObjectPool (Prefab2)
      └─ ...
```

**풀링 전략**
- Prefab별로 독립적인 풀 유지
- 사용하지 않는 오브젝트를 풀에 보관
- Spawn 시 풀에서 재사용, 없으면 새로 생성
- Return 시 풀로 반환
- 최대 풀 크기 설정 가능

**Prewarm 기능**
게임 시작 시 미리 오브젝트를 생성하여 풀에 보관합니다. 런타임에 처음 생성할 때의 부하를 줄이고, 안정적인 프레임률을 유지할 수 있습니다.

---

## Save/Load 시스템

Aggregate와 Model의 상태를 저장하고 복원하는 시스템입니다.

### SaveLoadSystem

직렬화 및 파일 관리를 담당합니다.

**저장 데이터 구조**
- **SaveData**: 단일 Aggregate 저장
  - AggregateData: Transform, 타입 정보
  - ModelData: Model의 RxData 값들
  - timestamp: 저장 시각
  - version: 버전 정보

- **FullGameSave**: 전체 게임 상태 저장
  - saveName: 저장 이름
  - gameStateData: 게임 전체 상태
  - aggregateSaves: 모든 Aggregate 데이터

**주요 기능**
- **자동 직렬화**: Model의 모든 RxData 자동 직렬화
- **Reflection 캐싱**: 성능 최적화된 Reflection 사용
- **암호화 옵션**: 간단한 Base64 암호화 지원
- **버전 관리**: 저장 데이터 버전 추적
- **일괄 저장**: 여러 Aggregate 동시 저장

**파일 관리**
- 저장 경로: Application.persistentDataPath/SaveData
- 파일 확장자: .save (설정 가능)
- 파일 목록 조회
- 파일 삭제

### RestoreManager

저장된 데이터로부터 Aggregate를 복원합니다.

**주요 기능**
- **비동기 복원**: Addressable 리소스 비동기 로딩
- **Transform 복원**: 위치, 회전, 스케일 복원
- **Model 데이터 복원**: 저장된 RxData 값 복원
- **Prefab 매핑**: 클래스명 → Addressable Key 매핑

**복원 프로세스**
1. SaveData에서 클래스명과 Aggregate 타입 확인
2. FactoryManager를 통해 Aggregate 생성
3. Transform 복원
4. Model의 RxData 값 복원
5. 생명주기 후크 호출

**제약 사항**
- 복원 시 Addressable Key가 유효해야 함
- Prefab이 변경되었을 경우 복원 실패 가능
- SaveData 버전 호환성 확인 필요

---

## 프레임워크 사용 규칙

프레임워크의 일관성과 안정성을 보장하기 위한 필수 규칙입니다.

### 1. FactoryManager를 통한 생성

모든 Aggregate는 반드시 FactoryManager를 통해 생성되어야 합니다.

**이점**
- 생성 전략의 일관성 보장
- 비동기 로딩 및 Addressable 활용
- 오브젝트 풀링 통합
- IsSceneCreated 자동 설정

**잘못된 사용**
Unity의 Instantiate를 직접 호출하면 ControllerManager에 등록되지 않고, 풀링 시스템을 활용할 수 없으며, 생명주기가 보장되지 않습니다.

### 2. EMController는 Prefab 필수

Entity와 Part 기반 구조를 보장하기 위해 Prefab으로 생성되어야 합니다.

**이유**
EMController는 Entity와 Part를 포함하는 복잡한 구조이므로, Prefab으로 미리 구성되어야 합니다. CreateFromClass로 생성하면 Entity와 Part가 없어 프레임워크가 정상 작동하지 않습니다.

### 3. DataManager를 통한 Model 초기화

Variation 데이터 기반 초기화 및 기획 데이터 통합을 보장합니다.

**이유**
DataManager를 사용하면 기획 데이터와 코드가 분리되어 기획자가 직접 데이터를 수정할 수 있습니다. 또한 자동 검증으로 필드 불일치를 방지합니다.

**잘못된 사용**
Model을 직접 new로 생성하면 Variation이 적용되지 않아 기본값으로 초기화됩니다.

### 4. ControllerManager 자동 등록

모든 Aggregate는 생성 시 ControllerManager에 자동 등록됩니다.

**메커니즘**
- AggregateRoot.Awake에서 자동 등록
- OnDestroy에서 자동 해제
- 수동 등록/해제 불필요

**이점**
중앙에서 모든 Aggregate를 추적하여 통계, 조회, 관리가 용이합니다. 개발자가 등록을 잊어버리는 실수를 방지합니다.

### 5. 생명주기 후크 사용

Unity 기본 메서드 대신 프레임워크의 후크 메서드를 사용합니다.

**이유**
Unity의 Start, OnEnable 등을 직접 오버라이드하면 프레임워크의 생명주기 흐름이 깨집니다. 후크 메서드를 사용하면 프레임워크가 생명주기를 제어하고, Entity와 Part에 이벤트를 자동으로 전파합니다.

**권장 사용**
AtAwake, AtInit, AtStart, AtModelReady, AtEnable, AtDisable 등의 후크 메서드를 사용합니다.

---

## Aggregate 사용의 이점

### 아키텍처 및 구조
- **예측 가능한 생명주기**: 일관된 생명주기 후크로 초기화 순서 보장
- **중앙 집중식 관리**: ControllerManager를 통한 모든 Aggregate 추적
- **Part 시스템을 통한 조립식 설계**: 기능별로 Part를 추가하여 유연한 확장
- **타입 안전성**: 컴파일 타임에 타입 검증

### 데이터 및 상태 관리
- **RxVar, RxMod, RxComputed 등 반응형 시스템**: 자동 상태 전파
- **Variation 기반 초기화**: Google Sheets 연동으로 기획 데이터 통합
- **자동 직렬화 및 SaveLoad 기능**: RxData 자동 저장/로드
- **Modifier 시스템**: 복잡한 게임 효과를 간단히 구현

### 성능 및 최적화
- **오브젝트 풀링**: Prefab 기반 자동 풀링
- **Reflection 캐싱**: ReflectionCache로 성능 최적화
- **Transform 캐싱**: Transform 접근 최적화
- **Lazy Evaluation**: RxComputed의 지연 계산

### 개발 생산성
- **Source Generator**: [GenerateWrappers]로 보일러플레이트 제거
- **에디터 도구**: EntityModelGenerator로 빠른 스캐폴딩
- **검증 시스템**: 컴파일 타임 + 런타임 검증
- **디버깅 지원**: Inspector에서 FSM, RxStateFlagSet 시각화

### 유지보수성
- **관심사의 분리**: Controller/Model/Entity/Part 명확한 책임
- **테스트 용이성**: 의존성 주입 및 인터페이스 기반 설계
- **확장성**: 새로운 Part 추가로 기능 확장
- **코드 재사용**: Part를 다른 Entity에 재사용 가능

---

## 에디터 도구

### EntityModelGenerator

Entity와 Model을 빠르게 생성하는 에디터 도구입니다.

**사용 방법**
1. Project 창에서 폴더 선택
2. 우클릭 → Create/Akasha/Entity and Model
3. 폴더명Entity.cs와 폴더명Model.cs 생성됨

**생성되는 파일**
- **Entity 파일**: GenerateWrappers Attribute 포함, SetupModel 메서드 구현
- **Model 파일**: BaseModel 상속, 빈 클래스

**이점**
수동으로 파일을 만들고 보일러플레이트를 작성하는 시간을 절약합니다. 일관된 코드 스타일을 유지하고 오타를 방지합니다.

### PartGenerator

Part를 빠르게 생성하는 에디터 도구입니다.

**사용 방법**
1. Project 창에서 폴더 선택
2. 우클릭 → Create/Akasha/Part
3. 폴더명Part.cs 생성됨

**생성되는 파일**
- **Part 파일**: GenerateWrappers Attribute 포함, BasePart 상속
- 주석: "파트의 이름은 변경하여 사용하는 것을 권장합니다."

**권장 사항**
생성된 Part는 임시 이름이므로, 기능에 맞게 이름을 변경하는 것이 좋습니다. 예: HeroPart → HeroMovementPart, HeroCombatPart 등

### PropertyWrapperGenerator (Source Generator)

GenerateWrappers Attribute가 붙은 클래스에 자동으로 프로퍼티 래퍼를 생성합니다.

**동작 방식**
- Source Generator가 빌드 시 자동 실행
- Entity의 Model 프로퍼티를 분석
- 각 프로퍼티에 대한 래퍼 프로퍼티 생성
- Generated.cs 파일로 저장

**이점**
- 보일러플레이트 코드 제거
- Part에서 Entity.MoveSpeed 처럼 간편하게 접근 가능
- 타입 안전성 보장
- 코드 중복 감소

**주의사항**
Generated 파일은 자동 생성되므로 직접 수정하지 않아야 합니다. Model의 프로퍼티를 변경하면 자동으로 재생성됩니다.

---

## 프레임워크 강점

### 1. 대규모 프로젝트 적합성
- **명확한 아키텍처**: DDD 원칙으로 복잡도 관리
- **타입 안전성**: 컴파일 타임 검증으로 버그 감소
- **확장성**: Part 시스템으로 기능 확장 용이

복잡한 게임 로직을 체계적으로 구조화하여 수백 개의 클래스가 있어도 쉽게 관리할 수 있습니다. 각 컴포넌트의 역할이 명확하여 새로운 팀원도 빠르게 이해할 수 있습니다.

### 2. 팀 협업 효율성
- **일관된 코드 스타일**: 프레임워크가 강제하는 패턴
- **명확한 책임 분리**: Controller/Model/Entity/Part
- **문서화된 규칙**: 학습 곡선 단축

모든 개발자가 동일한 패턴을 따르므로 코드 리뷰가 쉽고, 다른 사람의 코드를 이해하는 시간이 줄어듭니다. 프레임워크 자체가 베스트 프랙티스를 제공합니다.

### 3. 유지보수성
- **반응형 시스템**: 상태 변화 자동 전파로 버그 감소
- **중앙 집중식 관리**: Manager 시스템으로 통합 관리
- **테스트 용이성**: 의존성 주입 및 인터페이스 기반

상태 변화를 수동으로 추적할 필요가 없어 버그가 줄어듭니다. 각 컴포넌트를 독립적으로 테스트할 수 있어 품질 보증이 용이합니다.

### 4. 성능 최적화
- **오브젝트 풀링**: 자동 메모리 관리
- **Reflection 캐싱**: 런타임 성능 최적화
- **Lazy Evaluation**: 필요할 때만 계산

메모리 할당/해제를 최소화하고, 불필요한 계산을 피하여 안정적인 프레임률을 유지합니다. 프로파일링 없이도 기본적인 최적화가 적용됩니다.

### 5. 개발 생산성
- **에디터 도구**: 빠른 스캐폴딩
- **Source Generator**: 보일러플레이트 제거
- **Variation 시스템**: 기획 데이터 통합

반복적인 작업을 자동화하여 개발 시간을 단축합니다. 기획자가 직접 데이터를 수정할 수 있어 프로그래머의 병목을 줄입니다.

### 6. 기획 데이터 통합
- **Google Sheets 연동**: 기획자 친화적
- **자동 검증**: 필드 불일치 자동 감지
- **Hot Reload**: 게임 재시작 없이 데이터 변경 가능

기획자가 Google Sheets에서 직접 데이터를 관리하므로 프로그래머의 개입이 최소화됩니다. 자동 검증으로 오타와 필드 불일치를 방지합니다.

---

## 사용 시나리오

### 플레이어 캐릭터 구현

**구조**
- PlayerController: 전체 제어 및 생명주기 관리
- PlayerModel: 체력, 공격력, 이동속도 등 스탯 관리
- PlayerEntity: Model 생성 및 Part 조립
- PlayerMovementPart: 이동 로직
- PlayerCombatPart: 전투 로직
- PlayerAnimationPart: 애니메이션 제어

**흐름**
1. FactoryManager로 PlayerController 생성
2. PlayerEntity가 DataManager를 통해 PlayerModel 생성
3. PlayerEntity가 자식 Part들을 자동으로 찾아 등록
4. PlayerController의 AtStart에서 초기 설정
5. 각 Part가 독립적으로 기능 수행
6. Model의 반응형 프로퍼티로 UI 자동 업데이트

### 적 AI 구현

**구조**
- EnemyController: AI 제어
- EnemyModel: 스탯 및 FSM
- EnemyEntity: Model 생성
- EnemyAIPart: AI 로직 (순찰, 추적, 공격)

**FSM 상태**
- Patrol: 순찰 중
- Chase: 플레이어 추적
- Attack: 공격
- Dead: 사망

**AI 로직**
플레이어와의 거리에 따라 FSM 상태를 우선순위 기반으로 전이합니다. Attack(우선순위 10) > Chase(우선순위 5) > Patrol(우선순위 1)로 설정하여, 공격 거리 안에 있으면 항상 공격 상태로 전이합니다.

### 게임 시스템 구현

**구조**
- GameSystemController: PureController로 구현
- RxVar를 직접 관리하여 점수, 게임 시간 등 추적
- 다른 Controller들과 통신하여 게임 진행 제어

**특징**
Prefab이 필요 없고, Model도 없어 가장 가벼운 형태입니다. 시스템 레벨 로직에 적합하며, RxVar로 상태를 관리하여 UI와 자동으로 동기화됩니다.

---

## 라이선스 및 지원

### 라이선스
이 프레임워크는 Akasha 프로젝트의 일부입니다.

### 기술 지원
- GitHub Issues: 프로젝트 저장소
- 문서: 본 문서 및 코드 주석 참조

### 버전 정보
- 현재 버전: 1.0
- Unity 버전: 2021.3 이상 권장
- .NET 버전: .NET Standard 2.1

---

## 부록

### A. 용어 사전

- **Aggregate**: DDD의 집합체 개념, 독립적 생명주기를 가진 게임 오브젝트
- **AggregateRoot**: Aggregate의 진입점
- **Controller**: 로직과 생명주기 관리
- **Model**: 데이터와 상태 관리
- **Entity**: Model 인스턴스화 및 Part 조립
- **Part**: 조립식 기능 컴포넌트
- **RxVar**: 반응형 변수
- **RxMod**: Modifier를 지원하는 반응형 변수
- **RxComputed**: 계산된 반응형 값
- **Variation**: Model의 초기 데이터 프리셋
- **IRxOwner**: RxData를 소유할 수 있는 타입
- **IRxCaller**: RxData의 값을 설정할 수 있는 타입

### B. 네이밍 컨벤션

**파일명**
- Controller: `{Name}Controller.cs`
- Model: `{Name}Model.cs`
- Entity: `{Name}Entity.cs`
- Part: `{Name}Part.cs` *임시이름 변경권장

**클래스명**
- sealed를 사용하여 상속 방지 (필요시)
- partial 키워드 사용 (Source Generator 사용 시)

**필드명**
- RxData는 PascalCase 사용
- private 필드는 camelCase 사용

### C. 자주 묻는 질문 (FAQ)

**Q1. Pure/Model/Entity 중 어떤 것을 선택해야 하나요?**
- **PureController**: 시스템 레벨 로직, 간단한 매니저
- **ModelController**: UI 컨트롤러, 데이터 관리자
- **EMController**: 게임 캐릭터, 건물, 복잡한 오브젝트

**Q2. Part는 언제 사용하나요?**
- 기능을 모듈화하고 싶을 때
- 여러 Entity에서 재사용하고 싶을 때
- 특정 기능을 선택적으로 추가/제거하고 싶을 때

**Q3. Variation은 필수인가요?**
- 아니요, "default" variation을 사용하면 기본값으로 초기화됩니다.
- 하지만 기획 데이터 통합을 위해 사용을 권장합니다.

**Q4. FactoryManager 없이 생성하면 안되나요?**
- 작동은 하지만 권장하지 않습니다.
- FactoryManager를 사용하면 풀링, Addressable, 일관된 생성 전략의 이점이 있습니다.

**Q5. Model의 RxData를 private으로 만들 수 있나요?**
- 권장하지 않습니다. public으로 두고 [GenerateWrappers]로 접근 제어하세요.
- DataManager의 Variation 시스템이 public RxData를 필요로 합니다.

**Q6. 기존 Unity 프로젝트에 Akasha를 적용할 수 있나요?**
- 가능하지만 점진적으로 적용하는 것을 권장합니다.
- 새로운 기능부터 프레임워크를 적용하고, 기존 코드는 필요시 리팩토링하세요.

**Q7. Part 없이 Entity만 사용할 수 있나요?**
- 가능합니다. Entity 자체에 모든 로직을 작성할 수 있습니다.
- 하지만 기능이 복잡해지면 Part로 분리하는 것을 권장합니다.

**Q8. 여러 개의 Model을 가질 수 있나요?**
- 한 Controller는 하나의 Model만 가질 수 있습니다.
- 여러 데이터 소스가 필요하면 다른 Controller를 참조하거나, Model 내부에 중첩된 구조를 만드세요.

---

*이 문서는 Akasha Framework v1.0 기준으로 작성되었습니다.*
*최종 수정일: 2025-10-31*
