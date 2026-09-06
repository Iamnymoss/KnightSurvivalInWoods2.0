using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }

    private PlayerInputActions _playerInputActions;
    private Camera _mainCamera;

    public event EventHandler OnPlayerAttack;
    public event EventHandler OnPlayerDash;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        _playerInputActions = new PlayerInputActions();
        _playerInputActions.Enable();

        _playerInputActions.Combat.Attack.started +=
            PlayerAttackStarted;

        _playerInputActions.Player.Dash.performed +=
            PlayerDashPerformed;
    }

    public Vector2 GetMovementVector()
    {
        if (_playerInputActions == null)
        {
            return Vector2.zero;
        }

        return _playerInputActions.Player.Move.ReadValue<Vector2>();
    }

    public Vector3 GetMousePosition()
    {
        Camera activeCamera = GetMainCamera();

        Rect allowedRect;

        if (activeCamera != null)
        {
            allowedRect = activeCamera.pixelRect;
        }
        else
        {
            allowedRect = new Rect(
                0f,
                0f,
                Mathf.Max(1, Screen.width),
                Mathf.Max(1, Screen.height)
            );
        }

        Vector2 mousePosition = allowedRect.center;

        if (Mouse.current != null)
        {
            mousePosition = Mouse.current.position.ReadValue();
        }

        // Защита от NaN и Infinity.
        if (!IsFinite(mousePosition.x) ||
            !IsFinite(mousePosition.y))
        {
            mousePosition = allowedRect.center;
        }

        // Не разрешаем позиции мыши выйти за пределы камеры.
        float maximumX = Mathf.Max(
            allowedRect.xMin,
            allowedRect.xMax - 1f
        );

        float maximumY = Mathf.Max(
            allowedRect.yMin,
            allowedRect.yMax - 1f
        );

        mousePosition.x = Mathf.Clamp(
            mousePosition.x,
            allowedRect.xMin,
            maximumX
        );

        mousePosition.y = Mathf.Clamp(
            mousePosition.y,
            allowedRect.yMin,
            maximumY
        );

        return new Vector3(
            mousePosition.x,
            mousePosition.y,
            0f
        );
    }

    public bool IsMouseLeftOfWorldPosition(
        Vector3 worldPosition
    )
    {
        Camera activeCamera = GetMainCamera();
        Vector3 mousePosition = GetMousePosition();

        if (activeCamera == null)
        {
            // Без камеры используем центр экрана.
            return mousePosition.x < Screen.width * 0.5f;
        }

        Rect cameraRect = activeCamera.pixelRect;

        if (cameraRect.width <= 0f)
        {
            return false;
        }

        float targetScreenX;

        if (activeCamera.orthographic)
        {
            // Ширина видимой камерой области в мировых координатах.
            float worldWidth =
                activeCamera.orthographicSize *
                2f *
                activeCamera.aspect;

            if (worldWidth <= 0f || !IsFinite(worldWidth))
            {
                targetScreenX = cameraRect.center.x;
            }
            else
            {
                float offsetFromCamera =
                    worldPosition.x -
                    activeCamera.transform.position.x;

                targetScreenX =
                    cameraRect.center.x +
                    offsetFromCamera /
                    worldWidth *
                    cameraRect.width;
            }
        }
        else
        {
            // Проект является 2D и использует Orthographic Camera.
            // Этот вариант нужен только как безопасный запасной.
            targetScreenX = cameraRect.center.x;
        }

        if (!IsFinite(targetScreenX))
        {
            targetScreenX = cameraRect.center.x;
        }

        return mousePosition.x < targetScreenX;
    }

    public void DisableMovement()
    {
        if (_playerInputActions != null)
        {
            _playerInputActions.Disable();
        }
    }

    private Camera GetMainCamera()
    {
        if (_mainCamera == null ||
            !_mainCamera.isActiveAndEnabled)
        {
            _mainCamera = Camera.main;
        }

        return _mainCamera;
    }

    private static bool IsFinite(float value)
    {
        return !float.IsNaN(value) &&
               !float.IsInfinity(value);
    }

    private void PlayerAttackStarted(
        InputAction.CallbackContext context
    )
    {
        OnPlayerAttack?.Invoke(this, EventArgs.Empty);
    }

    private void PlayerDashPerformed(
        InputAction.CallbackContext context
    )
    {
        OnPlayerDash?.Invoke(this, EventArgs.Empty);
    }

    private void OnDestroy()
    {
        if (_playerInputActions != null)
        {
            _playerInputActions.Combat.Attack.started -=
                PlayerAttackStarted;

            _playerInputActions.Player.Dash.performed -=
                PlayerDashPerformed;

            _playerInputActions.Disable();
            _playerInputActions.Dispose();
            _playerInputActions = null;
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }
}
