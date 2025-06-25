using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    private void Start()
    {

    }

    public void LoadRoadsterSimulation()
    {
        SceneManager.LoadScene("RoadsterSimulation");
    }

    public void LoadLaunchesBrowser()
    {
        SceneManager.LoadScene("LaunchesBrowser");
    }

    public static void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}