using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Текущий уровень")]
    [Min(1)]
    public int currentLevel = 1;

    [Header("Название процедурной сцены")]
    [SerializeField]
    private string proceduralSceneName = "Level_Procedural";

    private int _savedHealth;
    private int _savedCoins;
    private bool _hasSavedRunState;
    private bool _isSceneLoading;

    public int SavedHealth => _savedHealth;
    public int SavedCoins => _savedCoins;
    public bool HasSavedRunState => _hasSavedRunState;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // LevelManager не уничтожается при переходе между сценами.
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _isSceneLoading = false;

        // Возвращение в меню полностью сбрасывает прогресс.
        if (scene.name == "Menu")
        {
            ResetRunState();
        }
    }

    public void AdvanceToNextLevel()
    {
        if (_isSceneLoading)
        {
            return;
        }

        // Сохраняем состояние до уничтожения текущей сцены.
        SaveCurrentRunState();

        _isSceneLoading = true;
        currentLevel++;

        Debug.Log(
            $"Переход на уровень {currentLevel}. " +
            $"Сохранено HP: {_savedHealth}, " +
            $"монет: {_savedCoins}"
        );

        // LevelGenerator в этой сцене создаст новую случайную карту.
        SceneManager.LoadScene(proceduralSceneName);
    }

    private void SaveCurrentRunState()
    {
        if (Player.Instance != null)
        {
            _savedHealth = Player.Instance.GetCurrentHealth();
        }
        else
        {
            Debug.LogWarning(
                "LevelManager не смог найти Player для сохранения HP."
            );
        }

        if (CoinManager.Instance != null)
        {
            _savedCoins = CoinManager.Instance.GetCurrentCoins();
        }
        else
        {
            Debug.LogWarning(
                "LevelManager не смог найти CoinManager."
            );
        }

        _hasSavedRunState = true;
    }

    private void ResetRunState()
    {
        currentLevel = 1;

        _savedHealth = 0;
        _savedCoins = 0;
        _hasSavedRunState = false;

        Debug.Log("Прогресс прохождения сброшен.");
    }
}
