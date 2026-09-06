using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Текущий уровень сложности")]
    public int currentLevel = 1;

    [Header("Название процедурной сцены")]
    [SerializeField] private string proceduralSceneName = "Level_Procedural";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Объект не умрет при смене сцен
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += SceneManager_OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= SceneManager_OnSceneLoaded;
    }

    private void SceneManager_OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Menu")
        {
            currentLevel = 1;
        }
    }

    public void AdvanceToNextLevel()
    {
        currentLevel++;
        Debug.Log("Переход на уровень: " + currentLevel);

        // Первый портал ведёт в процедурную сцену, следующие перезагружают её с новым seed.
        string activeSceneName = SceneManager.GetActiveScene().name;
        string nextSceneName = activeSceneName == proceduralSceneName
            ? activeSceneName
            : proceduralSceneName;
        SceneManager.LoadScene(nextSceneName);
    }
}
