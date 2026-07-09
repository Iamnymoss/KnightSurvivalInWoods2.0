using System;
using UnityEngine;

[RequireComponent(typeof(PolygonCollider2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class EnemyEntity : MonoBehaviour
{
    [SerializeField] private EnemySO _enemySO;

    public event EventHandler OnTakeHit;
    public event EventHandler OnDeath;

    private int _currentHealth;

    private PolygonCollider2D _polygonCollider2D;
    private BoxCollider2D _boxCollider2D;
    private EnemyAI _enemyAI;

    private void Awake()
    {
        _polygonCollider2D = GetComponent<PolygonCollider2D>();
        _boxCollider2D = GetComponent<BoxCollider2D>();
        _enemyAI = GetComponent<EnemyAI>();
    }

    private void Start()
    {
        _currentHealth = _enemySO.enemyHelth;
    }

    public void TakeDamage(int damage)
    {
        _currentHealth -= damage;
        OnTakeHit?.Invoke(this, EventArgs.Empty); // Добавил знак `?`, чтобы игра не вылетала, если на событие никто не подписался
        DetectDeath();
    }

    public void PolygonColliderTurnOff()
    {
        _polygonCollider2D.enabled = false;
    }

    public void PolygonColliderTurnOn()
    {
        _polygonCollider2D.enabled = true;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.transform.TryGetComponent(out Player player))
        {
            player.TakeDamage(transform, _enemySO.enemyDamageAmount);
        }
    }

    private void DetectDeath()
    {
        if (_currentHealth <= 0)
        {
            // ==========================================
            // ТОТ САМЫЙ ФИКС: Снимаем тег "Enemy"
            // Теперь портал перестанет считать этого врага живым
            // ==========================================
            gameObject.tag = "Untagged";

            _boxCollider2D.enabled = false;
            _polygonCollider2D.enabled = false;

            // --- КОД ДЛЯ СПАВНА МОНЕТОК ТУТ ---
            // Пытаемся найти компонент LootSpawner на этом враге
            if (TryGetComponent(out LootSpawner lootSpawner))
            {
                lootSpawner.SpawnLoot(); // Если нашли — выбрасываем монеты!
            }
            // ----------------------------------

            _enemyAI.SetDeathState();

            OnDeath?.Invoke(this, EventArgs.Empty);
        }
    }
}