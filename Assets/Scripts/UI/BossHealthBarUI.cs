using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 보스 체력바 UI - 화면 상단에 표시
/// </summary>
public class BossHealthBarUI : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private Image _healthBarFill;  // 체력바 Fill 이미지
    [SerializeField] private TextMeshProUGUI _bossNameText;  // 보스 이름 텍스트
    [SerializeField] private TextMeshProUGUI _healthText;  // 체력 수치 텍스트 (선택 사항)

    [Header("체력바 색상")]
    [SerializeField] private Color _healthColor = new Color(0.8f, 0f, 1f);  // 보라색 (보스용)
    [SerializeField] private Color _lowHealthColor = Color.red;  // 낮은 체력 색상
    [SerializeField] private float _lowHealthThreshold = 0.3f;  // 낮은 체력 기준 (30%)

    [Header("애니메이션")]
    [SerializeField] private float _fillSpeed = 5f;  // 체력바 감소 속도
    [SerializeField] private bool _smoothFill = true;  // 부드러운 체력바 애니메이션

    private Health _bossHealth;
    private float _targetFillAmount;
    private float _currentFillAmount;

    private void Start()
    {
        // 초기 상태: 보스가 없으면 숨김
        if (_bossHealth == null)
        {
            gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (_smoothFill && _bossHealth != null)
        {
            // 부드러운 체력바 애니메이션
            _currentFillAmount = Mathf.Lerp(_currentFillAmount, _targetFillAmount, Time.deltaTime * _fillSpeed);
            _healthBarFill.fillAmount = _currentFillAmount;
        }
    }

    /// <summary>
    /// 보스 설정
    /// </summary>
    public void SetBoss(Health bossHealth, string bossName)
    {
        _bossHealth = bossHealth;

        if (_bossHealth != null)
        {
            // 이벤트 구독
            _bossHealth.OnHealthChanged += UpdateHealthBar;
            _bossHealth.OnDeath += OnBossDeath;

            // 보스 이름 설정
            if (_bossNameText != null)
            {
                _bossNameText.text = bossName;
            }

            // 초기 체력 설정
            _targetFillAmount = _bossHealth.HealthPercent;
            _currentFillAmount = _targetFillAmount;
            UpdateHealthBar(_bossHealth.CurrentHealth, _bossHealth.MaxHealth);

            // UI 표시
            gameObject.SetActive(true);

            Debug.Log($"BossHealthBarUI: 보스 '{bossName}' 체력바 표시 시작");
        }
    }

    /// <summary>
    /// 체력바 업데이트
    /// </summary>
    private void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (_healthBarFill == null || _bossHealth == null)
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
            _healthBarFill.color = _healthColor;
        }

        // 체력 수치 텍스트 업데이트
        if (_healthText != null)
        {
            _healthText.text = $"{Mathf.CeilToInt(currentHealth)} / {Mathf.CeilToInt(maxHealth)}";
        }
    }

    /// <summary>
    /// 보스 사망 시 처리
    /// </summary>
    private void OnBossDeath()
    {
        Debug.Log("BossHealthBarUI: 보스 사망! 체력바 숨김");

        // 이벤트 구독 해제
        if (_bossHealth != null)
        {
            _bossHealth.OnHealthChanged -= UpdateHealthBar;
            _bossHealth.OnDeath -= OnBossDeath;
        }

        // UI 숨김 (약간의 지연 후)
        Invoke(nameof(HideHealthBar), 1f);
    }

    /// <summary>
    /// 체력바 숨김
    /// </summary>
    private void HideHealthBar()
    {
        gameObject.SetActive(false);
        _bossHealth = null;
    }

    /// <summary>
    /// 보스 제거 (수동 호출용)
    /// </summary>
    public void ClearBoss()
    {
        if (_bossHealth != null)
        {
            _bossHealth.OnHealthChanged -= UpdateHealthBar;
            _bossHealth.OnDeath -= OnBossDeath;
            _bossHealth = null;
        }

        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (_bossHealth != null)
        {
            _bossHealth.OnHealthChanged -= UpdateHealthBar;
            _bossHealth.OnDeath -= OnBossDeath;
        }
    }
}
