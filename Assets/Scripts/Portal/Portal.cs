using UnityEngine;

public class Portal : MonoBehaviour
{
    private Collider2D[] _portalColliders;
    private SpriteRenderer _spriteRenderer;

    private bool _isActive;
    private bool _isLoadingNextLevel;

    private void Awake()
    {
        // Получаем любые 2D-коллайдеры портала.
        // Это работает и с CapsuleCollider2D, и с BoxCollider2D.
        _portalColliders = GetComponents<Collider2D>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        SetPortalActive(false);
    }

    private void Update()
    {
        if (_isLoadingNextLevel)
        {
            return;
        }

        // При смерти EnemyEntity меняет тег врага на Untagged.
        bool hasLivingEnemies =
            GameObject.FindGameObjectWithTag("Enemy") != null;

        bool shouldBeActive = !hasLivingEnemies;

        if (_isActive != shouldBeActive)
        {
            SetPortalActive(shouldBeActive);
        }
    }

    private void SetPortalActive(bool active)
    {
        _isActive = active;

        if (_portalColliders != null)
        {
            foreach (Collider2D portalCollider in _portalColliders)
            {
                if (portalCollider != null)
                {
                    portalCollider.enabled = active;
                }
            }
        }

        if (_spriteRenderer != null)
        {
            Color portalColor = _spriteRenderer.color;
            portalColor.a = active ? 1f : 0.2f;
            _spriteRenderer.color = portalColor;
        }

        if (active)
        {
            Debug.Log("Все враги убиты. Портал активирован.");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!_isActive || _isLoadingNextLevel)
        {
            return;
        }

        Player player = other.GetComponentInParent<Player>();

        if (player == null)
        {
            return;
        }

        if (LevelManager.Instance == null)
        {
            Debug.LogError(
                "Портал не может загрузить уровень: LevelManager не найден."
            );
            return;
        }

        _isLoadingNextLevel = true;
        SetPortalActive(false);

        Debug.Log("Игрок вошёл в портал. Загружается следующий уровень.");

        LevelManager.Instance.AdvanceToNextLevel();
    }
}
