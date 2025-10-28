using UnityEngine;

namespace Akasha
{
    /// <summary>
    /// GameManager 프로젝트 확장
    /// Akasha 프레임워크의 GameManager에 Mercenary 프로젝트 초기화 로직 추가
    /// </summary>
    public partial class GameManager
    {
        partial void InitializeProject()
        {
            Debug.Log("=== Mercenary Game Initializing ===");

            // Akasha 프레임워크가 자동으로 다음 Manager들을 우선순위 순서로 초기화:
            // 1. DataManager (Priority 0) - 아카샤 프레임워크
            // 2. PlayerManager (Priority 10) - 플레이어 관리
            // 3. CommanderManager (Priority 20) - 부대장 관리
            // 4. SquadManager (Priority 30) - 부대 관리
            // 5. SkillManager (Priority 40) - 스킬 시스템
            // 6. DungeonManager (Priority 100) - 던전 시스템
            // 7. BattleManager (Priority 110) - 전장 시스템

            // 추가 초기화 로직이 필요하면 여기에 작성
            // 예: 저장 데이터 로드, 초기 리소스 설정 등

            Debug.Log("=== Mercenary Game Initialized ===");
        }

        partial void ShutdownProject()
        {
            Debug.Log("=== Mercenary Game Shutting Down ===");

            // Akasha 프레임워크가 자동으로 모든 Manager 종료
            // 추가 종료 로직이 필요하면 여기에 작성

            Debug.Log("=== Mercenary Game Shutdown Complete ===");
        }
    }

}