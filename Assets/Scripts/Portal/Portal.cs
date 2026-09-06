using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    private BoxCollider2D _collider;
    private SpriteRenderer _spriteRenderer;
    private bool _isActive = false;

    private void Awake()
    {
        _collider = GetComponent<BoxCollider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        // При спавне портала всегда ставим его в полупрозрачное состояние
        SetPortalState(false);
    }

    private void Update()
    {
        // Находим всех врагов на сцене
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        // Портал должен быть активен ТОЛЬКО если врагов на карте 0
        bool shouldBeActive = (enemies.Length == 0);

        // Обновляем состояние, если оно изменилось
        if (_isActive != shouldBeActive)
        {
            SetPortalState(shouldBeActive);
        }
    }

    private void SetPortalState(bool state)
    {
        _isActive = state;

        if (_collider != null)
        {
            _collider.enabled = state;
        }

        if (_spriteRenderer != null)
        {
            Color c = _spriteRenderer.color;
            // 0.2f — тусклый/полупрозрачный, 1f — яркий активный
            c.a = state ? 1f : 0.2f;
            _spriteRenderer.color = c;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_isActive && collision.GetComponentInParent<Player>() != null)
        {
            Debug.Log("Портал сработал! Переход на следующий уровень...");

            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.AdvanceToNextLevel();
            }
            else
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
    }
}