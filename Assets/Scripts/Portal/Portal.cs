using UnityEngine;

public class Portal : MonoBehaviour
{
    private Animator _animator;
    private BoxCollider2D _collider;
    private bool _isActive = false;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _collider = GetComponent<BoxCollider2D>();
    }

    private void Start()
    {
        // В начале уровня портал спит и невидим (или полупрозрачен)
        SetPortalState(false);
    }

    private void Update()
    {
        if (!_isActive)
        {
            // Ищем, есть ли на сцене объекты с тегом "Enemy"
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

            // Если все скелеты убиты — активируем портал!
            if (enemies.Length == 0)
            {
                SetPortalState(true);
            }
        }
    }

    private void SetPortalState(bool state)
    {
        _isActive = state;

        // Включаем/выключаем коллайдер, чтобы нельзя было уйти раньше времени
        if (_collider != null) _collider.enabled = state;

        // Управляем видимостью: если выключен — делаем прозрачным
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color c = sr.color;
            c.a = state ? 1f : 0.2f; // 20% прозрачности, если враги еще живы
            sr.color = c;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Проверяем, что в портал зашел именно Игрок
        if (_isActive && collision.CompareTag("Player"))
        {
            // Даем команду менеджеру уровней перезагрузить сцену и повысить сложность
            LevelManager.Instance.AdvanceToNextLevel();
        }
    }
}