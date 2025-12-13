using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 아이템 슬롯 UI - 개별 아이템 슬롯 표시
/// InventoryManager와 연동하여 아이템 표시
/// </summary>
public class ItemSlotUI : MonoBehaviour
{
    [Header("슬롯 설정")]
    [SerializeField] private int _slotIndex = 0;  // 슬롯 인덱스 (0~4)

    [Header("UI 참조")]
    [SerializeField] private Image _itemIcon;  // 아이템 아이콘
    [SerializeField] private TextMeshProUGUI _countText;  // 개수 텍스트 (x2, x3 등)
    [SerializeField] private TextMeshProUGUI _timerText;  // 타이머 텍스트 (선택, 하단 작은 글씨)
    [SerializeField] private Image _timerBar;  // 타이머 바 (선택, Fill Image)
    [SerializeField] private GameObject _emptySlotIndicator;  // 빈 슬롯 표시 (선택)

    [Header("시각 효과")]
    [SerializeField] private Color _emptySlotColor = new Color(1f, 1f, 1f, 0.3f);  // 빈 슬롯 색상
    [SerializeField] private Color _filledSlotColor = Color.white;  // 채워진 슬롯 색상

    [Header("타이머 설정")]
    [SerializeField] private bool _showTimerText = true;  // 타이머 텍스트 표시
    [SerializeField] private bool _showTimerBar = false;  // 타이머 바 표시 (선택)
    [SerializeField] private Color _timerNormalColor = Color.white;  // 타이머 정상 색상
    [SerializeField] private Color _timerWarningColor = Color.yellow;  // 타이머 경고 색상 (30% 이하)
    [SerializeField] private Color _timerCriticalColor = Color.red;  // 타이머 위험 색상 (10% 이하)

    private InventoryItem _currentItem;

    private void Start()
    {
        if (InventoryManager.Instance != null)
        {
            // 슬롯 변경 이벤트 구독
            InventoryManager.Instance.OnItemSlotChanged += OnItemSlotChanged;

            // 초기 상태 설정
            UpdateSlotUI(InventoryManager.Instance.GetItem(_slotIndex));
        }
        else
        {
            Debug.LogError("ItemSlotUI: InventoryManager를 찾을 수 없습니다!");
        }
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnItemSlotChanged -= OnItemSlotChanged;
        }
    }

    /// <summary>
    /// 슬롯 변경 이벤트 핸들러
    /// </summary>
    private void OnItemSlotChanged(int slotIndex, InventoryItem item)
    {
        // 이 슬롯의 변경인지 확인
        if (slotIndex != _slotIndex)
            return;

        UpdateSlotUI(item);
    }

    /// <summary>
    /// 슬롯 UI 업데이트
    /// </summary>
    private void UpdateSlotUI(InventoryItem item)
    {
        _currentItem = item;

        if (item == null || item.IsEmpty)
        {
            // 빈 슬롯
            ShowEmptySlot();
        }
        else
        {
            // 아이템 있음
            ShowFilledSlot(item);
        }
    }

    /// <summary>
    /// 빈 슬롯 표시
    /// </summary>
    private void ShowEmptySlot()
    {
        if (_itemIcon != null)
        {
            _itemIcon.sprite = null;
            _itemIcon.color = _emptySlotColor;
            _itemIcon.enabled = false;  // 아이콘 숨기기
        }

        if (_countText != null)
        {
            _countText.text = "";
            _countText.enabled = false;
        }

        // 타이머 UI 숨김
        if (_timerText != null)
        {
            _timerText.enabled = false;
        }

        if (_timerBar != null)
        {
            _timerBar.enabled = false;
        }

        if (_emptySlotIndicator != null)
        {
            _emptySlotIndicator.SetActive(true);
        }
    }

    /// <summary>
    /// 아이템이 있는 슬롯 표시
    /// </summary>
    private void ShowFilledSlot(InventoryItem item)
    {
        if (_itemIcon != null)
        {
            _itemIcon.sprite = item.ItemData.Icon;
            _itemIcon.color = _filledSlotColor;
            _itemIcon.enabled = true;
        }

        // 개수 표시 (2개 이상일 때만)
        if (_countText != null)
        {
            if (item.Count > 1)
            {
                _countText.text = $"x{item.Count}";
                _countText.enabled = true;
            }
            else
            {
                _countText.text = "";
                _countText.enabled = false;
            }
        }

        // 타이머 표시
        UpdateTimerDisplay(item);

        if (_emptySlotIndicator != null)
        {
            _emptySlotIndicator.SetActive(false);
        }
    }

    /// <summary>
    /// 타이머 표시 업데이트
    /// </summary>
    private void UpdateTimerDisplay(InventoryItem item)
    {
        if (item == null || !item.HasTimer)
        {
            // 타이머가 없는 아이템 - 타이머 UI 숨김
            if (_timerText != null)
                _timerText.enabled = false;
            if (_timerBar != null)
                _timerBar.enabled = false;
            return;
        }

        float remainingTime = item.RemainingTime;
        float timePercent = item.TimePercent;

        // 타이머 텍스트 표시
        if (_timerText != null && _showTimerText)
        {
            _timerText.enabled = true;
            _timerText.text = FormatTime(remainingTime);

            // 시간에 따른 색상 변경
            if (timePercent <= 0.1f)
                _timerText.color = _timerCriticalColor;
            else if (timePercent <= 0.3f)
                _timerText.color = _timerWarningColor;
            else
                _timerText.color = _timerNormalColor;
        }

        // 타이머 바 표시
        if (_timerBar != null && _showTimerBar)
        {
            _timerBar.enabled = true;
            _timerBar.fillAmount = timePercent;

            // 시간에 따른 색상 변경
            if (timePercent <= 0.1f)
                _timerBar.color = _timerCriticalColor;
            else if (timePercent <= 0.3f)
                _timerBar.color = _timerWarningColor;
            else
                _timerBar.color = _timerNormalColor;
        }
    }

    /// <summary>
    /// 시간을 문자열로 포맷 (예: 5초 → "5", 90초 → "1:30")
    /// </summary>
    private string FormatTime(float seconds)
    {
        int totalSeconds = Mathf.CeilToInt(seconds);

        if (totalSeconds >= 60)
        {
            int minutes = totalSeconds / 60;
            int secs = totalSeconds % 60;
            return $"{minutes}:{secs:D2}";
        }
        else
        {
            return totalSeconds.ToString();
        }
    }

    /// <summary>
    /// 슬롯 클릭 처리 (외부에서 호출)
    /// </summary>
    public void OnSlotClicked()
    {
        if (_currentItem != null && !_currentItem.IsEmpty)
        {
            // 아이템 사용
            InventoryManager.Instance?.UseItem(_slotIndex);
        }
    }

    /// <summary>
    /// 슬롯 인덱스 설정 (프로그래밍 방식)
    /// </summary>
    public void SetSlotIndex(int index)
    {
        _slotIndex = index;
    }

    /// <summary>
    /// 현재 아이템 가져오기
    /// </summary>
    public InventoryItem GetCurrentItem()
    {
        return _currentItem;
    }
}
