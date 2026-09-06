using System;
using UnityEngine;

public class PlayerVisual : MonoBehaviour
{
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private FlashBlinck _flashBlink;
    private Player _player;

    private const string IsRunning = "IsRunning";
    private const string IsDie = "IsDie";

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _flashBlink = GetComponent<FlashBlinck>();
    }

    private void Start()
    {
        _player = Player.Instance;

        if (_player != null)
        {
            _player.OnPlayerDeath += OnPlayerDeath;
        }
    }

    private void Update()
    {
        if (_player == null)
        {
            return;
        }

        if (_animator != null)
        {
            _animator.SetBool(
                IsRunning,
                _player.IsRunning()
            );
        }

        if (_player.IsAlive())
        {
            AdjustPlayerFacingDirection();
        }
    }

    private void AdjustPlayerFacingDirection()
    {
        if (GameInput.Instance == null ||
            _spriteRenderer == null)
        {
            return;
        }

        _spriteRenderer.flipX =
            GameInput.Instance.IsMouseLeftOfWorldPosition(
                _player.transform.position
            );
    }

    private void OnPlayerDeath(
        object sender,
        EventArgs eventArgs
    )
    {
        if (_animator != null)
        {
            _animator.SetBool(IsDie, true);
        }

        if (_flashBlink != null)
        {
            _flashBlink.StopBlinking();
        }
    }

    private void OnDestroy()
    {
        if (_player != null)
        {
            _player.OnPlayerDeath -= OnPlayerDeath;
        }
    }
}
