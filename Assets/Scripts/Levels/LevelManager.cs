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

    public void AdvanceToNextLevel()
    {
        currentLevel++;
        Debug.Log("Переход на уровень: " + currentLevel);

        // Если мы переходим с 1-го уровня, загружаем процедурную сцену.
        // Если мы уже на ней (уровень 2, 3, 4...), перезагружаем её же, и карта перегенерируется.
        if (currentLevel == 2)
        {
            SceneManager.LoadScene(proceduralSceneName);
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}