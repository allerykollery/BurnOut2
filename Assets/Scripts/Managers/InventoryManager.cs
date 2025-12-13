using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 인벤토리 관리자 - 플레이어의 아이템 관리
/// 5개의 아이템 슬롯 + 카드키 슬롯
/// </summary>
public class InventoryManager : MonoBehaviour
{
    // 싱글톤
    public static InventoryManager Instance { get; private set; }

    [Header("인벤토리 설정")]
    [SerializeField] private int _maxItemSlots = 5;  // 일반 아이템 슬롯 개수

    // 아이템 슬롯 (인덱스 0~4)
    private InventoryItem[] _itemSlots;

    // 카드키 (별도 관리)
    private int _cardKeyCount = 0;

    // 이벤트
    public event Action<int, InventoryItem> OnItemSlotChanged;  // (slotIndex, item)
    public event Action<int> OnCardKeyCountChanged;  // (cardKeyCount)
    public event Action<ItemData> OnItemAdded;  // 아이템 추가 성공
    public event Action<ItemData> OnItemRemoved;  // 아이템 제거
    public event Action<ItemData, int> OnItemUsed;  // (itemData, slotIndex)

    // 프로퍼티
    public int MaxItemSlots => _maxItemSlots;
    public int CardKeyCount => _cardKeyCount;

    private void Awake()
    {
        // 싱글톤 설정
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 아이템 슬롯 초기화
        _itemSlots = new InventoryItem[_maxItemSlots];
    }

    private void Update()
    {
        // 모든 아이템의 타이머 업데이트
        UpdateItemTimers();
    }

    /// <summary>
    /// 아이템 타이머 업데이트 (지속시간이 지나면 자동 제거)
    /// </summary>
    private void UpdateItemTimers()
    {
        for (int i = 0; i < _maxItemSlots; i++)
        {
            if (_itemSlots[i] == null)
                continue;

            int beforeCount = _itemSlots[i].Count;
            int beforeBatchCount = _itemSlots[i].GetBatchCount();

            // 타이머 업데이트 (모든 배치가 사라졌으면 true 반환)
            bool allExpired = _itemSlots[i].UpdateTimer(Time.deltaTime);

            int afterCount = _itemSlots[i].Count;
            int afterBatchCount = _itemSlots[i].GetBatchCount();

            if (allExpired)
            {
                // 모든 배치 만료 - 슬롯 전체 제거
                ItemData expiredItem = _itemSlots[i].ItemData;
                _itemSlots[i] = null;
                OnItemSlotChanged?.Invoke(i, null);
                OnItemRemoved?.Invoke(expiredItem);
                Debug.Log($"InventoryManager: {expiredItem.ItemName} 모든 배치 만료로 제거됨 (슬롯 {i})");
            }
            else if (beforeCount != afterCount || beforeBatchCount != afterBatchCount)
            {
                // 일부 배치만 만료 - UI 갱신
                OnItemSlotChanged?.Invoke(i, _itemSlots[i]);
                Debug.Log($"InventoryManager: {_itemSlots[i].ItemData.ItemName} 배치 만료 (슬롯 {i}, {beforeCount} → {afterCount}개, 배치: {beforeBatchCount} → {afterBatchCount}개)");
            }
            else if (_itemSlots[i].HasTimer)
            {
                // 타이머만 업데이트 - UI 갱신
                OnItemSlotChanged?.Invoke(i, _itemSlots[i]);
            }
        }
    }

    /// <summary>
    /// 아이템 추가
    /// </summary>
    /// <returns>추가 성공 여부</returns>
    public bool AddItem(ItemData itemData, int count = 1)
    {
        if (itemData == null)
        {
            Debug.LogWarning("InventoryManager: ItemData가 null입니다.");
            return false;
        }

        // 카드키는 별도 처리
        if (itemData.ItemType == ItemType.CardKey)
        {
            AddCardKey(count);
            return true;
        }

        // 같은 아이템이 이미 있는지 확인 (무제한 중첩)
        for (int i = 0; i < _maxItemSlots; i++)
        {
            if (_itemSlots[i] != null && _itemSlots[i].ItemData == itemData)
            {
                // 같은 아이템 발견 - 새 배치로 추가 (독립적인 타이머)
                _itemSlots[i].AddNewBatch(count);
                OnItemSlotChanged?.Invoke(i, _itemSlots[i]);
                OnItemAdded?.Invoke(itemData);
                Debug.Log($"InventoryManager: {itemData.ItemName} x{count} 추가 (슬롯 {i}, 총 {_itemSlots[i].Count}개, 배치: {_itemSlots[i].GetBatchCount()}개)");
                return true;
            }
        }

        // 같은 아이템이 없으면 빈 슬롯에 추가
        int emptySlot = FindEmptySlot();
        if (emptySlot == -1)
        {
            Debug.LogWarning("InventoryManager: 인벤토리가 가득 찼습니다.");
            return false;
        }

        // 새 슬롯에 추가
        _itemSlots[emptySlot] = new InventoryItem(itemData, count);
        OnItemSlotChanged?.Invoke(emptySlot, _itemSlots[emptySlot]);
        OnItemAdded?.Invoke(itemData);
        Debug.Log($"InventoryManager: {itemData.ItemName} x{count} 추가 (슬롯 {emptySlot})");
        return true;
    }

    /// <summary>
    /// 아이템 제거
    /// </summary>
    public bool RemoveItem(int slotIndex, int count = 1)
    {
        if (!IsValidSlotIndex(slotIndex) || _itemSlots[slotIndex] == null)
        {
            Debug.LogWarning($"InventoryManager: 슬롯 {slotIndex}이 비어있거나 유효하지 않습니다.");
            return false;
        }

        InventoryItem item = _itemSlots[slotIndex];
        int remainingCount = item.RemoveCount(count);

        if (remainingCount <= 0)
        {
            // 아이템 완전 제거
            ItemData removedItemData = item.ItemData;
            _itemSlots[slotIndex] = null;
            OnItemSlotChanged?.Invoke(slotIndex, null);
            OnItemRemoved?.Invoke(removedItemData);
            Debug.Log($"InventoryManager: {removedItemData.ItemName} 제거 (슬롯 {slotIndex} 비움)");
        }
        else
        {
            // 개수만 감소
            OnItemSlotChanged?.Invoke(slotIndex, item);
            Debug.Log($"InventoryManager: {item.ItemData.ItemName} x{count} 제거 (슬롯 {slotIndex}, 남은 개수: {remainingCount})");
        }

        return true;
    }

    /// <summary>
    /// 아이템 사용
    /// </summary>
    public bool UseItem(int slotIndex)
    {
        if (!IsValidSlotIndex(slotIndex) || _itemSlots[slotIndex] == null)
        {
            Debug.LogWarning($"InventoryManager: 슬롯 {slotIndex}이 비어있거나 유효하지 않습니다.");
            return false;
        }

        InventoryItem item = _itemSlots[slotIndex];
        ItemData itemData = item.ItemData;

        // 아이템 사용 이벤트 발생
        OnItemUsed?.Invoke(itemData, slotIndex);
        Debug.Log($"InventoryManager: {itemData.ItemName} 사용 (슬롯 {slotIndex})");

        // 소모품인 경우 제거
        if (itemData.IsConsumable)
        {
            RemoveItem(slotIndex, 1);
        }

        return true;
    }

    /// <summary>
    /// 슬롯의 아이템 가져오기
    /// </summary>
    public InventoryItem GetItem(int slotIndex)
    {
        if (!IsValidSlotIndex(slotIndex))
            return null;

        return _itemSlots[slotIndex];
    }

    /// <summary>
    /// 모든 아이템 가져오기
    /// </summary>
    public InventoryItem[] GetAllItems()
    {
        return (InventoryItem[])_itemSlots.Clone();
    }

    /// <summary>
    /// 빈 슬롯 찾기
    /// </summary>
    /// <returns>빈 슬롯 인덱스 (없으면 -1)</returns>
    private int FindEmptySlot()
    {
        for (int i = 0; i < _maxItemSlots; i++)
        {
            if (_itemSlots[i] == null)
                return i;
        }
        return -1;
    }

    /// <summary>
    /// 슬롯 인덱스 유효성 검사
    /// </summary>
    private bool IsValidSlotIndex(int slotIndex)
    {
        return slotIndex >= 0 && slotIndex < _maxItemSlots;
    }

    /// <summary>
    /// 인벤토리가 가득 찼는지 확인
    /// </summary>
    public bool IsFull()
    {
        return FindEmptySlot() == -1;
    }

    /// <summary>
    /// 특정 아이템이 있는지 확인
    /// </summary>
    public bool HasItem(ItemData itemData, int minCount = 1)
    {
        int totalCount = 0;
        for (int i = 0; i < _maxItemSlots; i++)
        {
            if (_itemSlots[i] != null && _itemSlots[i].ItemData == itemData)
            {
                totalCount += _itemSlots[i].Count;
                if (totalCount >= minCount)
                    return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 인벤토리 비우기
    /// </summary>
    public void ClearInventory()
    {
        for (int i = 0; i < _maxItemSlots; i++)
        {
            if (_itemSlots[i] != null)
            {
                _itemSlots[i] = null;
                OnItemSlotChanged?.Invoke(i, null);
            }
        }
        Debug.Log("InventoryManager: 인벤토리를 비웠습니다.");
    }

    #region 카드키 관리

    /// <summary>
    /// 카드키 추가 (최대 1개)
    /// </summary>
    public void AddCardKey(int count = 1)
    {
        // 카드키는 최대 1개만 보유 가능
        if (_cardKeyCount >= 1)
        {
            Debug.LogWarning("InventoryManager: 카드키는 이미 보유하고 있습니다. (최대 1개)");
            return;
        }

        _cardKeyCount = 1;  // 항상 1로 설정
        OnCardKeyCountChanged?.Invoke(_cardKeyCount);
        Debug.Log($"InventoryManager: 카드키 획득 (총 {_cardKeyCount}개)");
    }

    /// <summary>
    /// 카드키 사용 (제거)
    /// </summary>
    public bool UseCardKey(int count = 1)
    {
        if (_cardKeyCount < count)
        {
            Debug.LogWarning($"InventoryManager: 카드키가 부족합니다. (필요: {count}, 보유: {_cardKeyCount})");
            return false;
        }

        _cardKeyCount -= count;
        OnCardKeyCountChanged?.Invoke(_cardKeyCount);
        Debug.Log($"InventoryManager: 카드키 x{count} 사용 (남은 개수: {_cardKeyCount})");
        return true;
    }

    /// <summary>
    /// 카드키 보유 여부 확인
    /// </summary>
    public bool HasCardKey(int minCount = 1)
    {
        return _cardKeyCount >= minCount;
    }

    #endregion
}
