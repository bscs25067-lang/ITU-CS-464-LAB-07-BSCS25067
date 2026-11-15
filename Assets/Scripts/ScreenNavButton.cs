using UnityEngine;

public class SceneNavButton : MonoBehaviour
{
    [Tooltip("To quit/exit game enter \"EXIT\"")]
    [SerializeField] private string targetSceneName = "02_MainMenu";

    public void Navigate()
    {
        if (targetSceneName == "EXIT")
        {
            Application.Quit();
            return;
        }

        SceneTransition.TransitionTo(targetSceneName);
    }
}