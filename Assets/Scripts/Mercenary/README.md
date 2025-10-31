# Mercenary - 용병단 게임 로직

이 폴더는 M(Mercenary) 게임의 핵심 게임 로직을 담고 있습니다.

## 필수 참고 문서

**작업 전 반드시 확인**: [`용병단 육성 게임 통합 기획서.md`](./용병단 육성 게임 통합 기획서.md)

이 기획서에는 다음 내용이 포함되어 있습니다:
- 던전 시스템 (전투, 파티 편성, 스킬)
- 본거지 시스템 (시설, 생산, 자원)
- 전장 시스템 (RTS, 전술 영역)
- 스탯 시스템 (7개 기본 스탯 + 정밀/통찰)
- 성장 시스템 (레벨, 환생, 장비)

## 폴더 구조

```
Mercenary/
├── Character/              # 캐릭터 관련
│   ├── CharController.cs        # 최상위 캐릭터 베이스
│   ├── Commander/               # 지휘관 타입
│   │   ├── CommanderController.cs
│   │   ├── Player/             # 플레이어
│   │   ├── Hero/               # 부대장 영웅
│   │   └── Boss/               # 던전 보스
│   └── Unit/                   # 일반 유닛
│       ├── UnitController.cs
│       ├── DungeonEnemy/       # 던전 적
│       └── BattlefieldSoldier/ # 전장 병사
├── Dungeon/               # 던전 시스템
├── Barracks/              # 본거지 시스템
├── Battlefield/           # 전장 시스템
└── GameManager/           # 게임 매니저
```

## 개발 가이드

### 새 캐릭터 추가 시

1. **기획서 확인**: 해당 캐릭터의 스탯, 스킬, 역할 확인
2. **계층 구조 파악**: CharController → CommanderController/UnitController
3. **Akasha 패턴 적용**: Controller/Model/Entity/Part 구조
4. **생성 도구 사용**: Unity 에디터의 Akasha 메뉴 활용

### 코딩 컨벤션

- **RxData는 public PascalCase**: `public RxMod<int> Health;`
- **sealed 키워드**: 구체 클래스는 sealed로 선언
- **partial 키워드**: Source Generator 사용 시 필수
- **생명주기 후크 사용**: Unity 메서드 대신 AtAwake(), AtStart() 등 사용

### 스탯 시스템

**7개 기본 스탯** (모든 캐릭터):
- Strength (힘), Control (제어), Intelligence (지능), Divine (신성)
- Agility (민첩), Spirit (정신), Luck (행운)

**버프 스탯** (플레이어 전용):
- Precision (정밀): 던전 파티 능력치 버프
- Insight (통찰): 전장 전술 영역 크기/효과

### 필수 규칙

1. ✅ **FactoryManager로 생성**: `Instantiate()` 직접 사용 금지
2. ✅ **DataManager로 Model 초기화**: Variation 데이터 활용
3. ✅ **생명주기 후크 사용**: Unity 메서드 오버라이드 금지
4. ✅ **RxMod 사용**: 버프/디버프가 필요한 스탯은 RxMod<T> 사용

## 참고 자료

- **프레임워크 문서**: `/Akasha_Framework_Documentation.md`
- **프로젝트 개요**: `/CLAUDE.md`
- **기획 문서**: `/Assets/Doc/` 폴더 내 상세 기획서들
