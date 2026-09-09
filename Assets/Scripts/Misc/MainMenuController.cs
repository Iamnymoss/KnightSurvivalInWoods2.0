using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    private const string BotUsername = "KnightSurvivalInWoodsBot";

    public void StartGame()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void OpenTelegramLink()
    {
        string unityId = SystemInfo.deviceUniqueIdentifier;
        string url = $"https://t.me/{BotUsername}?start={unityId}";
        Application.OpenURL(url);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}