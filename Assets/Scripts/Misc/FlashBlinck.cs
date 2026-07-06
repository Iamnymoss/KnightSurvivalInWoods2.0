using UnityEngine;

public class FlashBlinck : MonoBehaviour
{
    [SerializeField] private MonoBehaviour damagableObject;
    [SerializeField] private Material blinkMaterial;
    [SerializeField] private float blinkDuration = 0.2f;

    private float _blinkTimer;
    private Material _defaultMaterial;
    private SpriteRenderer _spriteRenderer;
    private bool _isBlinking;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _defaultMaterial = _spriteRenderer.material;

        _isBlinking = true;
    }

    private void DamagableObject_OnFlashBlink(object sender, System.EventArgs e)
    {
        SetBlinkingMaterial();
    }

    private void Start()
    {
        if (damagableObject is Player)
        {
            (damagableObject as Player).OnFlashBlink += DamagableObject_OnFlashBlink;
        }
    }

    private void Update()
    {
        if (_isBlinking)
        {
            _blinkTimer -= Time.deltaTime;
            if (_blinkTimer < 0)
            {
                SetDefaultMaterial();
            }
        }
    }

    private void SetBlinkingMaterial()
    {
        _blinkTimer = blinkDuration;
        _spriteRenderer.material = blinkMaterial;
    }

    private void SetDefaultMaterial()
    {
        _spriteRenderer.material = _defaultMaterial;
    }

    public void StopBlinking()
    {
        SetDefaultMaterial();
        _isBlinking = false;
    }

    private void OnDestroy()
    {
        if (damagableObject is Player player)
        {
            player.OnFlashBlink -= DamagableObject_OnFlashBlink;
        }
    }
}
