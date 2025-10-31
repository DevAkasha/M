# Mercenary 폴더 작업 시 참고 문서

## 주요 참고 문서
- **용병단 육성 게임 통합 기획서**: `Assets/Scripts/Mercenary/용병단 육성 게임 통합 기획서.md`
- **Akasha Framework 문서**: `Akasha_Framework_Documentation.md`
- **프로젝트 개요**: `CLAUDE.md`

## 작업 규칙

### Mercenary 폴더에서 작업할 때
1. **필수 참조**: 항상 `Assets/Scripts/Mercenary/용병단 육성 게임 통합 기획서.md` 파일을 참고하여 작업
2. **아키텍처 준수**: Akasha Framework의 Aggregate 시스템(Controller/Model/Entity/Part) 구조 따르기
3. **네이밍 컨벤션**:
   - Controller: `{Name}Controller.cs`
   - Model: `{Name}Model.cs`
   - Entity: `{Name}Entity.cs`
   - Part: `{Name}{Feature}Part.cs`

### 캐릭터 시스템 구조
```
CharController<E,M>
├─ CommanderController<E,M>
│  ├─ PlayerController
│  ├─ HeroController<E,M>
│  │  ├─ WarriorHeroController
│  │  ├─ ArcherHeroController
│  │  ├─ MageHeroController
│  │  └─ ClericHeroController
│  └─ BossController<E,M>
└─ UnitController<E,M>
   ├─ DungeonEnemyController
   └─ BattlefieldSoldierController
```

### 개발 시 체크리스트
- [ ] 기획서의 관련 섹션 확인
- [ ] 스탯 시스템 연동 확인 (7개 기본 스탯 + 정밀/통찰)
- [ ] RxMod를 사용한 버프/디버프 시스템 적용
- [ ] FactoryManager를 통한 생성
- [ ] DataManager를 통한 Model 초기화
- [ ] 생명주기 후크 메서드 사용 (AtAwake, AtStart, AtModelReady 등)

## 컨텍스트 로딩 방법

이 파일을 Claude Code가 자동으로 인식하도록 하려면:
1. Mercenary 폴더에서 작업 시작 시 "용병단 육성 게임 통합 기획서.md를 참고해줘"라고 요청
2. 또는 작업 전에 기획서의 관련 섹션을 직접 질문
