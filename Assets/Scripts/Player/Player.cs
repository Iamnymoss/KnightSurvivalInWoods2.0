using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[SelectionBase]
public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    public event EventHandler OnPlayerDeath;
    public event EventHandler OnFlashBlink;

    [Header("Передвижение")]
    [SerializeField] private float movingSpeed = 10f;

    [Header("Здоровье")]
    [SerializeField] private int maxHealth = 20;
    [SerializeField] private float damageRecoveryTime = 0.5f;

    [Header("Рывок")]
    [SerializeField] private int dashSpeed = 4;
    [SerializeField] private float dashTime = 0.2f;
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private float dashCoolDownTime = 0.25f;

    [Header("UI")]
    [SerializeField] private GameObject restartPanel;

    private Vector2 _inputVector;

    private Rigidbody2D _rb;
    private KnokBack _knokBack;
    private Camera _mainCamera;
    private HealthSystem _healthSystem;

    private const float MinMovingSpeed = 0.1f;

    private int _currentHealth;
    private bool _canTakeDamage;
    private bool _isAlive;
    private bool _isRunning;
    private bool _isDashing;

    private float _initialMovingSpeed;

    private void Awake()
    {
        Instance = this;

        _rb = GetComponent<Rigidbody2D>();
        _knokBack = GetComponent<KnokBack>();
        _mainCamera = Camera.main;

        _initialMovingSpeed = movingSpeed;
    }

    private void Start()
    {
        RestoreHealth();

        _canTakeDamage = true;
        _isAlive = _currentHealth > 0;

        _healthSystem = HealthSystem.Instance;

        if (_healthSystem == null)
        {
            _healthSystem = FindFirstObjectByType<HealthSystem>();
        }

        SyncHealthUI();

        if (GameInput.Instance != null)
        {
            GameInput.Instance.OnPlayerAttack +=
                GameInput_OnPlayerAttack;

            GameInput.Instance.OnPlayerDash +=
                GameInput_OnPlayerDash;
        }
    }

    private void RestoreHealth()
    {
        if (LevelManager.Instance != null &&
            LevelManager.Instance.HasSavedRunState)
        {
            _currentHealth = Mathf.Clamp(
                LevelManager.Instance.SavedHealth,
                1,
                maxHealth
            );

            Debug.Log(
                $"Восстановлено здоровье игрока: " +
                $"{_currentHealth}/{maxHealth}"
            );
        }
        else
        {
            _currentHealth = maxHealth;
        }
    }

    private void Update()
    {
        if (GameInput.Instance != null)
        {
            _inputVector =
                GameInput.Instance.GetMovementVector();
        }
    }

    private void FixedUpdate()
    {
        if (_knokBack != null &&
            _knokBack.IsGettingKnockedBack)
        {
            return;
        }

        HandleMovement();
    }

    public bool IsAlive()
    {
        return _isAlive;
    }

    public int GetCurrentHealth()
    {
        return _currentHealth;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }

    public void TakeDamage(Transform damageSource, int damage)
    {
        if (_canTakeDamage && _isAlive)
        {
            _canTakeDamage = false;

            _currentHealth = Mathf.Max(
                0,
                _currentHealth - damage
            );

            SyncHealthUI();

            if (_knokBack != null)
            {
                _knokBack.GetKnockedBack(damageSource);
            }

            OnFlashBlink?.Invoke(this, EventArgs.Empty);

            StartCoroutine(DamageRecoveryRoutine());
        }

        DetectDeath();
    }

    private void SyncHealthUI()
    {
        if (_healthSystem != null)
        {
            _healthSystem.SetPlayerHealth(
                _currentHealth,
                maxHealth
            );
        }
    }

    private void DetectDeath()
    {
        if (_currentHealth != 0 || !_isAlive)
        {
            return;
        }

        _isAlive = false;

        if (_knokBack != null)
        {
            _knokBack.StopKnockBackMovement();
        }

        if (GameInput.Instance != null)
        {
            GameInput.Instance.DisableMovement();
        }

        OnPlayerDeath?.Invoke(this, EventArgs.Empty);

        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.SendCoinsToServer();
        }

        if (restartPanel != null)
        {
            restartPanel.SetActive(true);
        }
    }

    private void GameInput_OnPlayerDash(
        object sender,
        EventArgs eventArgs
    )
    {
        Dash();
    }

    private void Dash()
    {
        if (!_isDashing)
        {
            StartCoroutine(DashRoutine());
        }
    }

    private IEnumerator DashRoutine()
    {
        _isDashing = true;
        movingSpeed *= dashSpeed;

        if (trailRenderer != null)
        {
            trailRenderer.emitting = true;
        }

        yield return new WaitForSeconds(dashTime);

        if (trailRenderer != null)
        {
            trailRenderer.emitting = false;
        }

        movingSpeed = _initialMovingSpeed;

        yield return new WaitForSeconds(dashCoolDownTime);

        _isDashing = false;
    }

    private IEnumerator DamageRecoveryRoutine()
    {
        yield return new WaitForSeconds(
            damageRecoveryTime
        );

        _canTakeDamage = true;
    }

    public bool IsRunning()
    {
        return _isRunning;
    }

    private void GameInput_OnPlayerAttack(
        object sender,
        EventArgs eventArgs
    )
    {
        if (ActiveWeapon.Instance == null)
        {
            return;
        }

        Sword currentWeapon =
            ActiveWeapon.Instance.GetActiveWeapon();

        if (currentWeapon != null)
        {
            currentWeapon.Attack();
        }
    }

    private void HandleMovement()
    {
        _rb.MovePosition(
            _rb.position +
            _inputVector *
            (movingSpeed * Time.fixedDeltaTime)
        );

        _isRunning =
            Mathf.Abs(_inputVector.x) > MinMovingSpeed ||
            Mathf.Abs(_inputVector.y) > MinMovingSpeed;
    }

   

    private void OnDestroy()
    {
        if (GameInput.Instance != null)
        {
            GameInput.Instance.OnPlayerAttack -=
                GameInput_OnPlayerAttack;

            GameInput.Instance.OnPlayerDash -=
                GameInput_OnPlayerDash;
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }
}
