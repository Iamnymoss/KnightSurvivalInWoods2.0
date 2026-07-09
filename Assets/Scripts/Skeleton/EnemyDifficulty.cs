using UnityEngine;

public class EnemyDifficulty : MonoBehaviour
{
    [Header("Базовые настройки (для 1-го уровня)")]
    [SerializeField] private int baseMaxHealth = 30;
    [SerializeField] private int baseDamage = 2;

    // Сюда будут записываться уже измененные параметры для текущего уровня
    public int scaledMaxHealth;
    public int scaledDamage;

    void Awake()
    {
        // Проверяем, существует ли менеджер уровней
        int level = 1;
        if (LevelManager.Instance != null)
        {
            level = LevelManager.Instance.currentLevel;
        }

        // Математика прогрессии: каждый уровень увеличивает характеристики на 30%
        scaledMaxHealth = Mathf.RoundToInt(baseMaxHealth * Mathf.Pow(1.3f, level - 1));
        scaledDamage = Mathf.RoundToInt(baseDamage * Mathf.Pow(1.3f, level - 1));

        Debug.Log($"[Скелет] Спавн на уровне {level}. Здоровье: {scaledMaxHealth}, Урон: {scaledDamage}");

        // TODO: Передай эти переменные в свой основной скрипт здоровья/атаки скелета
        // Пример: 
        // GetComponent<EnemyHealth>().SetHealth(scaledMaxHealth);
    }
}