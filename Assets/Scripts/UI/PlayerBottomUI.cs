using UnityEngine;

/// <summary>
/// 플레이어 하단 UI 통합 관리자
/// - 체력바/쉴드바 (PlayerHealthBarUI)
/// - 5개 아이템 슬롯 (ItemSlotUI)
/// - 카드키 (CardKeyUI)
/// </summary>
public class PlayerBottomUI : MonoBehaviour
{
    [Header("하단 UI 컴포넌트 참조")]
    [SerializeField] private PlayerHealthBarUI _healthBarUI;
    [SerializeField] private ItemSlotUI[] _itemSlots = new ItemSlotUI[5];
    [SerializeField] private CardKeyUI _cardKeyUI;

    [Header("표시/숨김")]
    [SerializeField] private bool _showOnStart = true;

    private void Awake()
    {
        // 아이템 슬롯 자동 인덱스 설정
        for (int i = 0; i < _itemSlots.Length; i++)
        {
            if (_itemSlots[i] != null)
            {
                _itemSlots[i].SetSlotIndex(i);
            }
        }
    }

    private void Start()
    {
        // 초기 표시 설정
        SetVisible(_showOnStart);

        // 컴포넌트 유효성 검사
        ValidateComponents();
    }

    /// <summary>
    /// 컴포넌트 유효성 검사
    /// </summary>
    private void ValidateComponents()
    {
        if (_healthBarUI == null)
        {
            Debug.LogWarning("PlayerBottomUI: PlayerHealthBarUI가 연결되지 않았습니다.");
        }

        if (_cardKeyUI == null)
        {
            Debug.LogWarning("PlayerBottomUI: CardKeyUI가 연결되지 않았습니다.");
        }

        for (int i = 0; i < _itemSlots.Length; i++)
        {
            if (_itemSlots[i] == null)
            {
                Debug.LogWarning($"PlayerBottomUI: ItemSlot {i}가 연결되지 않았습니다.");
            }
        }
    }

    /// <summary>
    /// 전체 하단 UI 표시/숨김
    /// </summary>
    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }

    /// <summary>
    /// 체력바 UI 가져오기
    /// </summary>
    public PlayerHealthBarUI GetHealthBarUI()
    {
        return _healthBarUI;
    }

    /// <summary>
    /// 아이템 슬롯 UI 가져오기
    /// </summary>
    public ItemSlotUI GetItemSlot(int index)
    {
        if (index >= 0 && index < _itemSlots.Length)
        {
            return _itemSlots[index];
        }
        return null;
    }

    /// <summary>
    /// 카드키 UI 가져오기
    /// </summary>
    public CardKeyUI GetCardKeyUI()
    {
        return _cardKeyUI;
    }

    /// <summary>
    /// 모든 아이템 슬롯 가져오기
    /// </summary>
    public ItemSlotUI[] GetAllItemSlots()
    {
        return _itemSlots;
    }
}
