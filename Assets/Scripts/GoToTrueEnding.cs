using UGESystem;
using UnityEngine.SceneManagement;

public class GoToTrueEnding : AbstractEventReward
{
    public override void GrantReward(UGEEventTaskRunner runner)
    {
        if(GameManager.Instance.Likeability >= 60)
        {
            SceneManager.LoadScene(2);
        }
    }
}
