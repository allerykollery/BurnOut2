using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 플레이어 체력바 UI - 이미지 기반 체력 표시
/// Health.cs와 연동하여 체력 변화를 시각적으로 표시
/// </summary>
public class PlayerHealthBarUI : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private Health _playerHealth;

    [Header("체력바 UI (Image 컴포넌트만 연결)")]
    [Tooltip("체력바 Fill - Image 컴포넌트에 Front_Player_HP_0 스프라이트 설정하고 드래그")]
    [SerializeField] private Image _healthBarFill;
    [Tooltip("체력바 배경 - Image 컴포넌트에 Back_Norm, Player_0 스프라이트 설정하고 드래그")]
    [SerializeField] private Image _healthBarBack;

    [Header("쉴드바 UI (옵션)")]
    [Tooltip("쉴드바 Fill - Image 컴포넌트에 Front_Player_Shield_0 스프라이트 설정하고 드래그")]
    [SerializeField] private Image _shieldBarFill;
    [Tooltip("쉴드바 배경")]
    [SerializeField] private Image _shieldBarBack;

    [Header("아이콘 UI")]
    [Tooltip("체력 아이콘 - Image 컴포넌트에 HP_Color_0 스프라이트 설정하고 드래그")]
    [SerializeField] private Image _healthIcon;
    [Tooltip("쉴드 아이콘 - Image 컴포넌트에 Shield_Color_0 스프라이트 설정하고 드래그")]
    [SerializeField] private Image _shieldIcon;

    [Header("텍스트 (옵션)")]
    [SerializeField] private TextMeshProUGUI _healthText;  // 예: "100/100"
    [SerializeField] private bool _showHealthText = false;

    [Header("애니메이션 설정")]
    [SerializeField] private float _smoothSpeed = 8f;
    [SerializeField] private bool _enableSmoothAnimation = true;

    [Header("체력 감소 효과 (옵션)")]
    [SerializeField] private bool _enableDamageEffect = true;
    [SerializeField] private float _damageFlashDuration = 0.2f;
    [SerializeField] private Color _damageFlashColor = new Color(1f, 0.3f, 0.3f, 1f);

    private float _targetFillAmount;
    private float _currentFillAmount;
    private Color _originalHealthColor;
    private float _damageFlashTimer = 0f;

    private void Awake()
    {
        // Fill 이미지 설정 (Filled 타입으로 자동 변경)
        SetupFillImages();

        // 체력바 색상 저장
        if (_healthBarFill != null)
        {
            _originalHealthColor = _healthBarFill.color;
        }
    }

    /// <summary>
    /// Fill 이미지를 Filled 타입으로 설정
    /// </summary>
    private void SetupFillImages()
    {
        if (_healthBarFill != null)
        {
            _healthBarFill.type = Image.Type.Filled;
            _healthBarFill.fillMethod = Image.FillMethod.Horizontal;
            _healthBarFill.fillOrigin = (int)Image.OriginHorizontal.Left;
        }

        if (_shieldBarFill != null)
        {
            _shieldBarFill.type = Image.Type.Filled;
            _shieldBarFill.fillMethod = Image.FillMethod.Horizontal;
            _shieldBarFill.fillOrigin = (int)Image.OriginHorizontal.Left;
        }
    }

    private void Start()
    {
        // 플레이어 Health 컴포넌트 자동 찾기
        if (_playerHealth == null)
        {
            Debug.Log("PlayerHealthBarUI: PlayerController 찾는 중...");
            PlayerController player = FindFirstObjectByType<PlayerController>();

            if (player != null)
            {
                Debug.Log($"PlayerHealthBarUI: PlayerController 찾음! {player.name}");
                _playerHealth = player.Health;

                if (_playerHealth == null)
                {
                    Debug.LogError("PlayerHealthBarUI: PlayerController를 찾았지만 Health 컴포넌트가 없습니다!");
                }
                else
                {
                    Debug.Log("PlayerHealthBarUI: Health 컴포넌트 연결 성공!");
                }
            }
            else
            {
                Debug.LogError("PlayerHealthBarUI: Scene에 PlayerController를 찾을 수 없습니다!");
            }
        }

        if (_playerHealth != null)
        {
            // Health 이벤트 구독
            _playerHealth.OnHealthChanged += OnHealthChanged;
            _playerHealth.OnDamageTaken += OnDamageTaken;

            // 초기 체력 설정
            UpdateHealthBar(_playerHealth.CurrentHealth, _playerHealth.MaxHealth);
            Debug.Log($"PlayerHealthBarUI: 초기 체력 설정 완료 - {_playerHealth.CurrentHealth}/{_playerHealth.MaxHealth}");
        }
        else
        {
            Debug.LogError("PlayerHealthBarUI: Health 컴포넌트를 찾을 수 없습니다!");
        }
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (_playerHealth != null)
        {
            _playerHealth.OnHealthChanged -= OnHealthChanged;
            _playerHealth.OnDamageTaken -= OnDamageTaken;
        }
    }

    private void Update()
    {
        // 부드러운 애니메이션
        if (_enableSmoothAnimation && _healthBarFill != null)
        {
            _currentFillAmount = Mathf.Lerp(_currentFillAmount, _targetFillAmount, Time.deltaTime * _smoothSpeed);
            _healthBarFill.fillAmount = _currentFillAmount;
        }

        // 데미지 플래시 효과
        if (_damageFlashTimer > 0f)
        {
            _damageFlashTimer -= Time.deltaTime;
            if (_damageFlashTimer <= 0f && _healthBarFill != null)
            {
                _healthBarFill.color = _originalHealthColor;
            }
        }
    }

    /// <summary>
    /// Health.OnHealthChanged 이벤트 핸들러
    /// </summary>
    private void OnHealthChanged(float currentHealth, float maxHealth)
    {
        UpdateHealthBar(currentHealth, maxHealth);
    }

    /// <summary>
    /// Health.OnDamageTaken 이벤트 핸들러
    /// </summary>
    private void OnDamageTaken(float damageAmount)
    {
        // 데미지 플래시 효과
        if (_enableDamageEffect && _healthBarFill != null)
        {
            _healthBarFill.color = _damageFlashColor;
            _damageFlashTimer = _damageFlashDuration;
        }
    }

    /// <summary>
    /// 체력바 UI 업데이트
    /// </summary>
    private void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        float healthPercent = Mathf.Clamp01(currentHealth / maxHealth);

        // 체력바 Fill Amount 설정
        _targetFillAmount = healthPercent;

        if (!_enableSmoothAnimation && _healthBarFill != null)
        {
            _healthBarFill.fillAmount = _targetFillAmount;
            _currentFillAmount = _targetFillAmount;
        }

        // 텍스트 업데이트 (옵션)
        if (_showHealthText && _healthText != null)
        {
            _healthText.text = $"{Mathf.CeilToInt(currentHealth)}/{Mathf.CeilToInt(maxHealth)}";
        }
    }

    /// <summary>
    /// 체력바 색상 변경 (외부에서 호출 가능)
    /// </summary>
    public void SetHealthBarColor(Color color)
    {
        if (_healthBarFill != null)
        {
            _healthBarFill.color = color;
            _originalHealthColor = color;
        }
    }

    /// <summary>
    /// 체력바 표시/숨김
    /// </summary>
    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }
}
