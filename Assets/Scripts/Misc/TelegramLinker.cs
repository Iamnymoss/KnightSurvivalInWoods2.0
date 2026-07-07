using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class TelegramLinker : MonoBehaviour
{
    private string _serverUrl = "http://127.0.0.1:8000";
    private string _botUsername = "KnightSurvivalInWoodsBot";

    public void LinkTelegram()
    {
        string unityId = SystemInfo.deviceUniqueIdentifier;
        StartCoroutine(RegisterIdOnServer(unityId));
    }

    private IEnumerator RegisterIdOnServer(string unityId)
    {
        string url = _serverUrl + "/register_id/" + unityId;
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();
            string botUrl = "https://t.me/" + _botUsername + "?start=" + unityId;
            Application.OpenURL(botUrl);
        }
    }
}