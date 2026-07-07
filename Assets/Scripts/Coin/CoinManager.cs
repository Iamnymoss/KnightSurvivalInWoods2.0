using UnityEngine;
using TMPro;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance { get; private set; }

    [Header("UI Элементы")]
    [SerializeField] private TextMeshProUGUI _coinText;

    private int _coinCount = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UpdateUI();
    }

    public void AddCoin(int amount)
    {
        _coinCount += amount;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (_coinText != null)
        {
            _coinText.text = _coinCount.ToString();
        }
    }
}