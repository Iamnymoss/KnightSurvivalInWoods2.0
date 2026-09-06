using UnityEngine;

[DisallowMultipleComponent]
public class EnemyDifficulty : MonoBehaviour
{
    [Header("Рост характеристик за уровень")]

    [Tooltip("0.25 означает увеличение здоровья на 25% за уровень")]
    [Range(0f, 2f)]
    [SerializeField] private float healthIncreasePerLevel = 0.25f;

    [Tooltip("0.15 означает увеличение урона на 15% за уровень")]
    [Range(0f, 2f)]
    [SerializeField] private float damageIncreasePerLevel = 0.15f;

    private int GetCurrentLevel()
    {
        if (LevelManager.Instance == null)
        {
            return 1;
        }

        return Mathf.Max(1, LevelManager.Instance.currentLevel);
    }

    public int CalculateHealth(int baseHealth)
    {
        int completedLevels = GetCurrentLevel() - 1;

        float multiplier =
            1f + healthIncreasePerLevel * completedLevels;

        return Mathf.Max(
            1,
            Mathf.CeilToInt(baseHealth * multiplier)
        );
    }

    public int CalculateDamage(int baseDamage)
    {
        int completedLevels = GetCurrentLevel() - 1;

        float multiplier =
            1f + damageIncreasePerLevel * completedLevels;

        return Mathf.Max(
            1,
            Mathf.RoundToInt(baseDamage * multiplier)
        );
    }
}
