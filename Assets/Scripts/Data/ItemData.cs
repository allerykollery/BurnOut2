using UnityEngine;

/// <summary>
/// 아이템 데이터 ScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "New Item", menuName = "Burnout/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("기본 정보")]
    [SerializeField] private string _itemId;
    [SerializeField] private string _itemName;
    [TextArea(2, 4)]
    [SerializeField] private string _description;
    [SerializeField] private Sprite _icon;

    [Header("아이템 속성")]
    [SerializeField] private ItemType _itemType;
    [SerializeField] private ItemRarity _rarity;
    [SerializeField] private int _maxStackCount = 1;  // 최대 중첩 개수 (1 = 중첩 불가)
    [SerializeField] private bool _isConsumable = false;  // 사용 시 소모되는가
    [SerializeField] private float _duration = 0f;  // 지속시간 (초, 0 = 무제한)

    [Header("가격")]
    [SerializeField] private int _buyPrice = 100;
    [SerializeField] private int _sellPrice = 50;

    // 프로퍼티
    public string ItemId => _itemId;
    public string ItemName => _itemName;
    public string Description => _description;
    public Sprite Icon => _icon;
    public ItemType ItemType => _itemType;
    public ItemRarity Rarity => _rarity;
    public int MaxStackCount => _maxStackCount;
    public bool IsConsumable => _isConsumable;
    public float Duration => _duration;  // 지속시간 (0 = 무제한)
    public int BuyPrice => _buyPrice;
    public int SellPrice => _sellPrice;

    /// <summary>
    /// 아이템이 중첩 가능한지 확인
    /// </summary>
    public bool IsStackable => _maxStackCount > 1;

    /// <summary>
    /// 아이템이 시간 제한이 있는지 확인
    /// </summary>
    public bool HasDuration => _duration > 0f;
}
