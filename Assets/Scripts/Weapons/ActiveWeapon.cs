using UnityEngine;

public class ActiveWeapon : MonoBehaviour
{
    public static ActiveWeapon Instance { get; private set; }

    [SerializeField] private Sword sword;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (Player.Instance == null ||
            GameInput.Instance == null)
        {
            return;
        }

        if (!Player.Instance.IsAlive())
        {
            return;
        }

        FollowMousePosition();
    }

    public Sword GetActiveWeapon()
    {
        return sword;
    }

    private void FollowMousePosition()
    {
        bool mouseIsOnLeft =
            GameInput.Instance.IsMouseLeftOfWorldPosition(
                Player.Instance.transform.position
            );

        transform.rotation = mouseIsOnLeft
            ? Quaternion.Euler(0f, 180f, 0f)
            : Quaternion.Euler(0f, 0f, 0f);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
