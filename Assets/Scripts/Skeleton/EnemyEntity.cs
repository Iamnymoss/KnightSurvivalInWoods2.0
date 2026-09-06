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
    private int _currentDamage;

    private PolygonCollider2D _polygonCollider2D;
    private BoxCollider2D _boxCollider2D;
    private EnemyAI _enemyAI;
    private EnemyDifficulty _enemyDifficulty;

    private bool _attackHitboxActive;
    private bool _isDead;

    private void Awake()
    {
        _polygonCollider2D = GetComponent<PolygonCollider2D>();
        _boxCollider2D = GetComponent<BoxCollider2D>();
        _enemyAI = GetComponent<EnemyAI>();
        _enemyDifficulty = GetComponent<EnemyDifficulty>();
    }

    private void Start()
    {
        if (_enemySO == null)
        {
            Debug.LogError(
                $"” врага {name} не назначен EnemySO."
            );

            _currentHealth = 1;
            _currentDamage = 1;
            return;
        }

        if (_enemyDifficulty != null)
        {
            _currentHealth =
                _enemyDifficulty.CalculateHealth(
                    _enemySO.enemyHelth
                );

            _currentDamage =
                _enemyDifficulty.CalculateDamage(
                    _enemySO.enemyDamageAmount
                );
        }
        else
        {
            _currentHealth = _enemySO.enemyHelth;
            _currentDamage = _enemySO.enemyDamageAmount;
        }

        int level = LevelManager.Instance != null
            ? LevelManager.Instance.currentLevel
            : 1;

        Debug.Log(
            $"{name}: уровень {level}, " +
            $"здоровье {_currentHealth}, " +
            $"урон {_currentDamage}"
        );
    }

    public void TakeDamage(int damage)
    {
        if (_isDead)
        {
            return;
        }

        _currentHealth -= damage;
        OnTakeHit?.Invoke(this, EventArgs.Empty);

        DetectDeath();
    }

    public void PolygonColliderTurnOff()
    {
        _attackHitboxActive = false;

        if (_polygonCollider2D != null)
        {
            _polygonCollider2D.enabled = false;
        }
    }

    public void PolygonColliderTurnOn()
    {
        if (_isDead)
        {
            return;
        }

        _attackHitboxActive = true;

        if (_polygonCollider2D != null)
        {
            _polygonCollider2D.enabled = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDamagePlayer(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryDamagePlayer(other);
    }

    private void TryDamagePlayer(Collider2D other)
    {
        if (!_attackHitboxActive || _isDead)
        {
            return;
        }

        Player player = other.GetComponentInParent<Player>();

        if (player != null)
        {
            player.TakeDamage(transform, _currentDamage);
        }
    }

    private void DetectDeath()
    {
        if (_currentHealth > 0 || _isDead)
        {
            return;
        }

        _isDead = true;

        // Portal.cs перестаЄт считать этого врага живым.
        gameObject.tag = "Untagged";

        _attackHitboxActive = false;

        if (_boxCollider2D != null)
        {
            _boxCollider2D.enabled = false;
        }

        if (_polygonCollider2D != null)
        {
            _polygonCollider2D.enabled = false;
        }

        if (TryGetComponent(out LootSpawner lootSpawner))
        {
            lootSpawner.SpawnLoot();
        }

        if (_enemyAI != null)
        {
            _enemyAI.SetDeathState();
        }

        OnDeath?.Invoke(this, EventArgs.Empty);
    }
}
