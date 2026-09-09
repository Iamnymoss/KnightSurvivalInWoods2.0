using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance { get; private set; }

    [Header("UI монет")]
    [SerializeField] private TextMeshProUGUI _coinText;

    [Header("Настройки сервера")]
    [SerializeField] private string serverUrl = "http://127.0.0.1:8000/add_coins";

    private int _coinCount;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        RestoreCoins();
        UpdateUI();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void RestoreCoins()
    {
        if (LevelManager.Instance != null && LevelManager.Instance.HasSavedRunState)
        {
            _coinCount = Mathf.Max(0, LevelManager.Instance.SavedCoins);
        }
        else
        {
            _coinCount = 0;
        }
    }

    public void AddCoin(int amount)
    {
        if (amount <= 0) return;

        _coinCount += amount;
        UpdateUI();
    }

    public int GetCurrentCoins()
    {
        return _coinCount;
    }

    private void UpdateUI()
    {
        if (_coinText != null)
        {
            _coinText.text = _coinCount.ToString();
        }
    }

    public void SendCoinsToServer()
    {
        string unityId = SystemInfo.deviceUniqueIdentifier;

        if (_coinCount <= 0)
        {
            Debug.Log("[CoinManager] Монет 0, отправка пропущена.");
            return;
        }

        Debug.Log($"[CoinManager] Отправка {_coinCount} монет на сервер для ID: {unityId}");
        StartCoroutine(SendCoinsRoutine(unityId, _coinCount));
    }

    private IEnumerator SendCoinsRoutine(string unityId, int coinsToSend)
    {
        WWWForm form = new WWWForm();
        form.AddField("unity_id", unityId);
        form.AddField("coins", coinsToSend.ToString());

        using (UnityWebRequest www = UnityWebRequest.Post(serverUrl, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"[CoinManager] УСПЕХ! Ответ сервера: {www.downloadHandler.text}");
                _coinCount = 0;
                UpdateUI();
            }
            else
            {
                Debug.LogError($"[CoinManager] ОШИБКА: {www.error} | Код: {www.responseCode}");
            }
        }
    }
}