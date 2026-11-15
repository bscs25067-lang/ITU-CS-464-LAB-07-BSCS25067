using UnityEditor;
using UnityEngine;

public class SceneNavButton : MonoBehaviour
{
    [Tooltip("To exit game: \"EXIT\" \nTo do nothing: \"NOTHING\"")]
    [SerializeField] private string targetSceneName = "02_MainMenu";

    public void Navigate()
    {
        if (targetSceneName == "NOTHING") return;
        if (targetSceneName == "EXIT")
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
            Application.Quit();
            return;
        }

        SceneTransition.TransitionTo(targetSceneName);
    }
}