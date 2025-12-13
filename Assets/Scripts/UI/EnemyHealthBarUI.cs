using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 일반 몬스터 체력바 UI - 몬스터 머리 위에 표시
/// </summary>
public class EnemyHealthBarUI : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private Image _healthBarFill;  // 체력바 Fill 이미지
    [SerializeField] private Image _healthBarBackground;  // 체력바 배경 (선택 사항)

    [Header("체력바 색상")]
    [SerializeField] private Color _normalHealthColor = new Color(1f, 0.4f, 0.7f);  // 분홍색
    [SerializeField] private Color _lowHealthColor = Color.red;  // 낮은 체력 색상
    [SerializeField] private float _lowHealthThreshold = 0.3f;  // 낮은 체력 기준 (30%)

    [Header("표시 설정")]
    [SerializeField] private Vector3 _offset = new Vector3(0, 1.5f, 0);  // 몬스터로부터의 오프셋
    [SerializeField] private bool _hideWhenFull = false;  // 체력이 가득 차면 숨김
    [SerializeField] private bool _hideWhenDead = true;  // 사망 시 숨김

    [Header("애니메이션")]
    [SerializeField] private float _fillSpeed = 10f;  // 체력바 감소 속도
    [SerializeField] private bool _smoothFill = true;  // 부드러운 체력바 애니메이션

    private Health _enemyHealth;
    private Transform _enemyTransform;
    private Camera _mainCamera;
    private Canvas _canvas;
    private RectTransform _rectTransform;

    private float _targetFillAmount;
    private float _currentFillAmount;

    private void Awake()
    {
        _mainCamera = Camera.main;
        _canvas = GetComponentInParent<Canvas>();
        _rectTransform = GetComponent<RectTransform>();

        // 초기 상태: 숨김
        if (_hideWhenFull)
        {
            gameObject.SetActive(false);
        }
    }

    private void LateUpdate()
    {
        // 몬스터 위치 추적
        if (_enemyTransform != null && _mainCamera != null)
        {
            UpdatePosition();
        }

        // 부드러운 체력바 애니메이션
        if (_smoothFill && _enemyHealth != null)
        {
            _currentFillAmount = Mathf.Lerp(_currentFillAmount, _targetFillAmount, Time.deltaTime * _fillSpeed);
            if (_healthBarFill != null)
            {
                _healthBarFill.fillAmount = _currentFillAmount;
            }
        }
    }

    /// <summary>
    /// 몬스터 설정
    /// </summary>
    public void SetEnemy(Health enemyHealth, Transform enemyTransform)
    {
        _enemyHealth = enemyHealth;
        _enemyTransform = enemyTransform;

        if (_enemyHealth != null)
        {
            // 이벤트 구독
            _enemyHealth.OnHealthChanged += UpdateHealthBar;
            _enemyHealth.OnDeath += OnEnemyDeath;

            // 초기 체력 설정
            _targetFillAmount = _enemyHealth.HealthPercent;
            _currentFillAmount = _targetFillAmount;
            UpdateHealthBar(_enemyHealth.CurrentHealth, _enemyHealth.MaxHealth);

            // 체력이 가득 차면 숨김 (옵션)
            if (_hideWhenFull && _enemyHealth.HealthPercent >= 1f)
            {
                gameObject.SetActive(false);
            }
            else
            {
                gameObject.SetActive(true);
            }
        }
    }

    /// <summary>
    /// 월드 좌표를 UI 좌표로 변환하여 위치 업데이트
    /// </summary>
    private void UpdatePosition()
    {
        Vector3 worldPosition = _enemyTransform.position + _offset;
        Vector3 screenPosition = _mainCamera.WorldToScreenPoint(worldPosition);

        // 화면 밖이면 숨김
        if (screenPosition.z < 0 || screenPosition.x < 0 || screenPosition.x > Screen.width ||
            screenPosition.y < 0 || screenPosition.y > Screen.height)
        {
            if (gameObject.activeSelf)
            {
                gameObject.SetActive(false);
            }
            return;
        }

        // Canvas가 Screen Space - Overlay인 경우
        if (_canvas != null && _canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            _rectTransform.position = screenPosition;
        }
        // Canvas가 Screen Space - Camera인 경우
        else if (_canvas != null && _canvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvas.transform as RectTransform,
                screenPosition,
                _canvas.worldCamera,
                out localPoint
            );
            _rectTransform.localPosition = localPoint;
        }

        // 체력바 표시 (화면 안이면)
        if (!gameObject.activeSelf && !(_hideWhenFull && _enemyHealth.HealthPercent >= 1f))
        {
            gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 체력바 업데이트
    /// </summary>
    private void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (_healthBarFill == null || _enemyHealth == null)
            return;

        float healthPercent = currentHealth / maxHealth;
        _targetFillAmount = healthPercent;

        // 즉시 업데이트 (부드러운 애니메이션 비활성화 시)
        if (!_smoothFill)
        {
            _currentFillAmount = _targetFillAmount;
            _healthBarFill.fillAmount = _currentFillAmount;
        }

        // 체력바 색상 변경 (낮은 체력일 때)
        if (healthPercent <= _lowHealthThreshold)
        {
            _healthBarFill.color = _lowHealthColor;
        }
        else
        {
            _healthBarFill.color = _normalHealthColor;
        }

        // 체력이 가득 차면 숨김 (옵션)
        if (_hideWhenFull && healthPercent >= 1f)
        {
            gameObject.SetActive(false);
        }
        else if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 몬스터 사망 시 처리
    /// </summary>
    private void OnEnemyDeath()
    {
        if (_hideWhenDead)
        {
            gameObject.SetActive(false);
        }

        // 이벤트 구독 해제
        if (_enemyHealth != null)
        {
            _enemyHealth.OnHealthChanged -= UpdateHealthBar;
            _enemyHealth.OnDeath -= OnEnemyDeath;
        }
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (_enemyHealth != null)
        {
            _enemyHealth.OnHealthChanged -= UpdateHealthBar;
            _enemyHealth.OnDeath -= OnEnemyDeath;
        }
    }
}
