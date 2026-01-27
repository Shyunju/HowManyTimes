using UnityEngine;
using UGESystem;
using Unity.VisualScripting;

public class GiveReward : AbstractEventReward
{
    
        // [핵심] 실제 보상을 지급하는 함수
        // 이벤트가 종료(EndCommand)될 때 시스템에 의해 자동으로 호출됩니다.
    public override void GrantReward(UGEEventTaskRunner runner)
    {
        // [사용자 구현 필요]
        // 여러분의 게임 프로젝트에 있는 골드 매니저나 인벤토리 시스템을 호출하세요.
        // 예시 코드:
        // if (GameManager.Instance != null)
        // {
        // GameManager.Instance.AddGold(Amount);
        // }
        // 테스트용 로그 출력
        GameManager.Instance.AddLikeablility();

        //Debug.Log("호감도가 상승하였습니다.");
        //Debug.Log(GameManager.Instance.Likeability);
    }
}
