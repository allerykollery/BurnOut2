using UnityEngine;

/// <summary>
/// 아이템 타입 정의
/// </summary>
public enum ItemType
{
    Consumable,     // 소모품 (포션 등)
    Equipment,      // 장비
    Special,        // 특수 아이템
    CardKey         // 카드키 (다음 층 입장권)
}

/// <summary>
/// 아이템 희귀도
/// </summary>
public enum ItemRarity
{
    Common,         // 일반
    Uncommon,       // 고급
    Rare,           // 희귀
    Epic,           // 영웅
    Legendary       // 전설
}
