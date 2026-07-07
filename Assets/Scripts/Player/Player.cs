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

    [Header("Движение и Физика")]
    [SerializeField] private float movingSpeed = 10f;
    [SerializeField] private float damageRecoveryTime = 0.5f;
    [Space(10)]
    [SerializeField] private int dashSpeed = 4;
    [SerializeField] private float dashTime = 0.2f;
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private float dashCoolDownTime = 0.25f;

    [Header("Интеграция с TinyHealthSystem")]
    [SerializeField] private int maxHealth = 20;
    [SerializeField] private HealthSystem healthSystem;
    [SerializeField] public GameObject restartPanel;

    private Vector2 _inputVector;
    private Rigidbody2D _rb;
    private KnokBack _knokBack;

    private readonly float _minMovingSpeed = 0.1f;
    private bool _isRunning = false;

    private int _currentHealth;
    private bool _canTakeDamage;
    private bool _isAlive;
    private bool _isDashing;
    private float _initialMovingSpeed;

    private Camera _mainCamera;

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
        _canTakeDamage = true;
        _isAlive = true;

        if (healthSystem != null)
        {
            _currentHealth = (int)healthSystem.hitPoint;
        }
        else
        {
            _currentHealth = maxHealth;
            Debug.LogWarning("[Player] Забыл перетащить TinyHealthSystem в инспектор Игрока!");
        }

        GameInput.Instance.OnPlayerAttack += GameInput_OnPlayerAttack;
        GameInput.Instance.OnPlayerDash += GameInput_OnPlayerDash;
    }

    private void Update()
    {
        _inputVector = GameInput.Instance.GetMovementVector();
    }

    private void FixedUpdate()
    {
        if (_knokBack.IsGettingKnockedBack)
            return;

        HandleMovement();
    }

    public bool IsAlive() => _isAlive;

    public void TakeDamage(Transform damageSource, int damage)
    {
        if (_canTakeDamage && _isAlive)
        {
            _canTakeDamage = false;

            if (healthSystem != null)
            {
                healthSystem.TakeDamage(damage);
                _currentHealth = Mathf.Max(0, (int)healthSystem.hitPoint);
            }
            else
            {
                _currentHealth = Mathf.Max(0, _currentHealth -= damage);
            }

            _knokBack.GetKnockedBack(damageSource);
            OnFlashBlink?.Invoke(this, EventArgs.Empty);

            StartCoroutine(DamageRecoveryRoutine());
        }

        DetectDeath();
    }

    private void DetectDeath()
    {
        if (_currentHealth == 0 && _isAlive)
        {
            _isAlive = false;
            _knokBack.StopKnockBackMovement();
            GameInput.Instance.DisableMovement();

            if (restartPanel != null)
            {
                restartPanel.SetActive(true);
            }

            Time.timeScale = 0f;

            //int coinsEarned = 0;
            //if (CoinManager.Instance != null)
            //{

            //    coinsEarned = CoinManager.Instance.coins;
            //}

            //if (TelegramSender.Instance != null)
            //{
            //    TelegramSender.Instance.SendStats(coinsEarned);
            //}

            OnPlayerDeath?.Invoke(this, EventArgs.Empty);
        }
    }

    private void GameInput_OnPlayerDash(object sender, EventArgs e)
    {
        Dash();
    }

    private void Dash()
    {
        if (!_isDashing)
            StartCoroutine(DashRoutine());
    }

    private IEnumerator DashRoutine()
    {
        _isDashing = true;
        movingSpeed *= dashSpeed;
        trailRenderer.emitting = true;
        yield return new WaitForSeconds(dashTime);

        trailRenderer.emitting = false;
        movingSpeed = _initialMovingSpeed;

        yield return new WaitForSeconds(dashCoolDownTime);
        _isDashing = false;
    }

    private IEnumerator DamageRecoveryRoutine()
    {
        yield return new WaitForSeconds(damageRecoveryTime);
        _canTakeDamage = true;
    }

    public bool IsRunning()
    {
        return _isRunning;
    }

    private void GameInput_OnPlayerAttack(object sender, EventArgs e)
    {
        ActiveWeapon.Instance.GetActiveWeapon().Attack();
    }

    private void HandleMovement()
    {
        _rb.MovePosition(_rb.position + _inputVector * (movingSpeed * Time.fixedDeltaTime));
        if (Mathf.Abs(_inputVector.x) > _minMovingSpeed || Mathf.Abs(_inputVector.y) > _minMovingSpeed)
        {
            _isRunning = true;
        }
        else
        {
            _isRunning = false;
        }
    }

    public Vector3 GetPlayerScreenPosition()
    {
        Vector3 playerScreenPosition = _mainCamera.WorldToScreenPoint(transform.position);
        return playerScreenPosition;
    }

    private void OnDestroy()
    {
        if (GameInput.Instance != null)
        {
            GameInput.Instance.OnPlayerAttack -= GameInput_OnPlayerAttack;
            GameInput.Instance.OnPlayerDash -= GameInput_OnPlayerDash;
        }
    }

    public void LocalRestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LocalGoToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }
}