using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    [SerializeField] private PlayerHealth _playerHealth;

    [SerializeField] RectTransform _barRect;

    [SerializeField] RectMask2D _mask;

    private float _maxRightMask;
    private float _initialRightMask;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // x = left, w = top, y = bottom, z = right
        _maxRightMask = _barRect.rect.width - _mask.padding.x - _mask.padding.z;
        _initialRightMask = _mask.padding.z;

    }

    public void SetValue(float newValue)
    {
        var targetWidth = newValue * _maxRightMask / _playerHealth.GetMaxHealth();
        var newRightMask = _maxRightMask + _initialRightMask - targetWidth;
        var padding = _mask.padding;
        padding.z = newRightMask;
        _mask.padding = padding;
    }

    void Update()
    {
        if (_playerHealth != null)
        {
            SetValue(_playerHealth.GetCurrentHealth());
        }
    }
}
