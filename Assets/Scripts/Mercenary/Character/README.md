# 캐릭터 시스템 구조

## 개요

이 문서는 M 프로젝트의 캐릭터 시스템 구조를 설명합니다. 모든 캐릭터는 Akasha 프레임워크의 Aggregate 패턴을 따라 설계되었습니다.

## 아키텍처

```
Character/
├── CharModel.cs              (기본 Model - 7개 스탯)
├── CharController.cs         (기본 Controller)
│
├── Commander/
│   ├── CommanderModel.cs    (부대장 Model - 타입별 성장률)
│   ├── CommanderController.cs
│   ├── Player/
│   │   ├── PlayerModel.cs   (플레이어 Model - 정밀/통찰)
│   │   ├── PlayerController.cs
│   │   └── PlayerEntity.cs
│   ├── Boss/
│   │   ├── BossModel.cs     (보스 Model - 페이즈 시스템)
│   │   └── BossController.cs
│   └── Hero/
│       └── HeroController.cs
│
├── Unit/
│   ├── UnitModel.cs         (병사 Model - 병종별 특성)
│   └── UnitController.cs
│
└── Parts/
    ├── CharacterPartBase.cs (Part 기본 클래스)
    ├── MovementPart.cs      (이동 Part)
    └── CombatPart.cs        (전투 Part)
```

## Model 계층

### 1. CharModel (기본)
- **7개 기본 스탯**: 힘, 제어, 지능, 신성, 민첩, 정신, 행운
- **파생 전투 스탯**: 공격력(근접/원거리/마법/신성), 방어력, HP/MP
- **레벨 시스템**: 경험치 획득, 레벨업
- **전투 기능**: 데미지 받기, 치유, MP 소모/회복

#### 스탯별 효과

| 스탯 | 주 효과 | 부 효과 |
|------|---------|---------|
| 힘 (Strength) | 근접 공격력(대량), HP(대량) | 적중력 |
| 제어 (Control) | 원거리 공격력(대량), 적중력(대량) | HP |
| 지능 (Intelligence) | 마법 공격력(대량), 마법 방어력 | MP(대량) |
| 신성 (Divine) | 신성 공격력(대량), 신성 방어력 | MP(대량) |
| 민첩 (Agility) | 근접 방어력, HP 회복력 | 마법 방어력(소량) |
| 정신 (Spirit) | 원거리 방어력, MP 회복력 | 신성 방어력(소량) |
| 행운 (Luck) | 크리티컬, 회피력 | 적중력(소량) |

### 2. PlayerModel (플레이어)
- **CharModel 상속**
- **버프 스탯**:
  - 정밀 (Precision): 제어 + 지능 + 민첩 버프
  - 통찰 (Insight): 힘 + 정신 + 신성 버프
- **계수 시스템**:
  - 정밀 계수: `(정밀 × 2) + (제어 + 지능 + 민첩) × 0.5`
  - 통찰 계수: `(통찰 × 2) + (힘 + 정신 + 신성) × 0.3`
- **전술 영역**: 통찰 × 10m
- **스탯 포인트 할당**: 레벨업 시 기본 5포인트 + 버프 1포인트

#### 역할
- **던전**: 파티 리더, 정밀 계수로 능력치 버프
- **전장**: 전술 영역 제공 (통찰 계수로 병사 버프)

### 3. CommanderModel (부대장)
- **CharModel 상속**
- **타입 시스템**: Warrior, Archer, Mage, Priest
- **환생 시스템**:
  - 레벨 1로 초기화
  - 성장률 영구 증가 (+10%/환생)
  - 비용: `100 × 2^(환생횟수)`
- **타입별 성장률**:

| 타입 | 주 스탯 | 부 스탯 | 기타 |
|------|---------|---------|------|
| Warrior | 힘 +2.5 | 민첩 +1.5 | +0.5 |
| Archer | 제어 +2.5 | 정신 +1.5 | +0.5 |
| Mage | 지능 +2.5 | 민첩 +1.0 | +0.5 |
| Priest | 신성 +2.5 | 정신 +1.5 | +0.5 |

- **스킬 슬롯**: Lv1(1개), Lv10(2개), Lv20(3개), Lv30(4개)

### 4. UnitModel (병사)
- **CharModel 상속**
- **병종 타입**: Warrior, Cavalry, Ranger, Crossbowman, TacticalMage, BattleMage, Priest
- **등급 시스템**:
  - 신병 (Recruit): 기본 능력치
  - 정예병 (Elite): 능력치 2배
- **사기 시스템**:
  - 범위: 0~200 (기본 100)
  - 높은 사기 (150+): 공격력/방어력 +20%, 이동속도 +10%
  - 낮은 사기 (50-): 공격력/방어력 -30%
  - 사기 붕괴 (0): 부대 도주

### 5. BossModel (보스)
- **CharModel 상속**
- **난이도**: Beginner, Intermediate, Advanced, Nightmare
- **페이즈 시스템**:
  - Phase1 (100%~66% HP): 기본 패턴 3개
  - Phase2 (66%~33% HP): 패턴 5개 + 졸개 소환
  - Phase3 (33%~0% HP): 패턴 10개 + 광폭화 (공격력 +50%)
- **부활 시스템**: HP 30% 회복 (1회)

## Part 시스템

### CharacterPartBase<TEntity, TModel>
- **목적**: 모든 캐릭터 Part의 기본 클래스
- **기능**: Model 스탯에 쉽게 접근할 수 있는 프로퍼티 제공

### MovementPart<TEntity, TModel>
- **목적**: 캐릭터 이동 처리
- **기능**:
  - Move(direction): 방향 이동
  - MoveToPosition(target): 특정 위치로 이동
  - Jump(): 점프
  - Teleport(): 텔레포트
  - Stop(): 정지
- **지원**: CharacterController, Rigidbody

### CombatPart<TEntity, TModel>
- **목적**: 전투 처리
- **기능**:
  - Attack(target): 기본 공격
  - CalculateDamage(): 데미지 계산 (크리티컬 포함)
  - FindTargetsInRange(): 범위 내 적 탐색
  - FindNearestTarget(): 가장 가까운 적 찾기
  - UseSkill(): 스킬 사용 (MP 소모)
- **공격 타입**: Melee, Ranged, Magic, Divine

## 사용 예시

### 플레이어 생성
```csharp
// FactoryManager를 통해 생성
var player = await FactoryManager.Instance.CreateFromAddressableAsync<PlayerController>(
    "Player",
    Vector3.zero,
    Quaternion.identity
);

// 스탯 포인트 할당
player.Model.AllocateStatPoint(StatType.Strength, 5);
player.Model.AllocateBuffStatPoint(BuffStatType.Precision, 1);

// 정밀 계수 확인
float precisionCoeff = player.Model.PrecisionCoefficient.Value;
Debug.Log($"Precision Coefficient: {precisionCoeff}");
```

### 부대장 생성 및 환생
```csharp
var commander = await FactoryManager.Instance.CreateFromAddressableAsync<CommanderController>(
    "Commander_Warrior"
);

// 경험치 획득
commander.Model.GainExperience(1000);

// 환생 비용 확인
int cost = commander.Model.GetReincarnationCost();

// 환생 실행
commander.Model.Reincarnate();
```

### 병사 정예화
```csharp
// 신병 5명을 정예병 1명으로
List<UnitModel> recruits = new List<UnitModel>();
for (int i = 0; i < 5; i++)
{
    recruits.Add(/* ... */);
}

// 정예화 처리
var elite = CreateEliteUnit(recruits);
elite.Model.Promote();
```

### Part 사용
```csharp
// Entity에서 Part 가져오기
var movementPart = player.Entity.GetPart<MovementPart<PlayerEntity, PlayerModel>>();
var combatPart = player.Entity.GetPart<CombatPart<PlayerEntity, PlayerModel>>();

// 이동
movementPart.Move(Vector3.forward);

// 전투 (타겟은 Entity)
var targetEntity = combatPart.FindNearestTarget(10f);
if (targetEntity != null)
{
    combatPart.Attack(targetEntity);
}
```

## 주의사항

1. **Model 생성**: 반드시 `DataManager.Instance.CreateModel<T>()` 사용
2. **Aggregate 생성**: 반드시 `FactoryManager` 사용
3. **스탯 수정**: 직접 Set하지 말고 Modifier 사용 권장
4. **Part 추가**: Entity Prefab에 컴포넌트로 추가
5. **생명주기**: Akasha의 생명주기 훅 메서드 활용 (`AtModelReady`, `AtInit` 등)

## 확장 가이드

### 새로운 Part 추가
1. `CharacterPartBase<TEntity, TModel>` 상속
2. 필요한 기능 구현
3. Entity Prefab에 컴포넌트로 추가

### 새로운 캐릭터 타입 추가
1. `CharModel` 또는 하위 클래스 상속
2. 타입별 특성 구현
3. Controller, Entity 생성
4. DataManager에 Variation 등록

## 관련 문서
- [M_Project_Design_Plan1.md](../../../M_Project_Design_Plan1.md) - 기획 문서
- [M_Project_Design_Plan2.md](../../../M_Project_Design_Plan2.md) - 기획 문서
- [Akasha_API.md](../../../Akasha_API.md) - Akasha API 문서
- [Akasha_README.md](../../../Akasha_README.md) - Akasha 개요
