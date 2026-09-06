using TMPro;
using UnityEngine;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance { get; private set; }

    [Header("UI монет")]
    [SerializeField] private TextMeshProUGUI _coinText;

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
        if (LevelManager.Instance != null &&
            LevelManager.Instance.HasSavedRunState)
        {
            _coinCount = Mathf.Max(
                0,
                LevelManager.Instance.SavedCoins
            );

            Debug.Log(
                $"Восстановлено монет: {_coinCount}"
            );
        }
        else
        {
            // Новая игра начинается без монет.
            _coinCount = 0;
        }
    }

    public void AddCoin(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

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
}
