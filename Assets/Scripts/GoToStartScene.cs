using UGESystem;
using UnityEngine.SceneManagement;

public class GoToStartScene : AbstractEventReward
{
    public override void GrantReward(UGEEventTaskRunner runner)
    {
        SceneManager.LoadScene(1);
    }
}
