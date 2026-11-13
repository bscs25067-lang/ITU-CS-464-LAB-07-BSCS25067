using UnityEngine;

public class SceneNavButton : MonoBehaviour
{
    [SerializeField] private string targetSceneName = "02_MainMenu";

    public void Navigate()
    {
        SceneTransition.TransitionTo(targetSceneName);
    }
}