using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 아이템 배치 - 같은 시간에 획득한 아이템들
/// </summary>
[System.Serializable]
public class ItemBatch
{
    public int Count;
    public float RemainingTime;
    public float MaxDuration;

    public ItemBatch(int count, float duration)
    {
        Count = count;
        MaxDuration = duration;
        RemainingTime = duration;
    }

    public float TimePercent => MaxDuration > 0 ? RemainingTime / MaxDuration : 1f;
}

/// <summary>
/// 인벤토리 아이템 인스턴스
/// ItemData를 참조하고 여러 배치를 관리
/// </summary>
[System.Serializable]
public class InventoryItem
{
    public ItemData ItemData { get; private set; }

    // 여러 배치 관리 (각각 독립적인 타이머)
    private List<ItemBatch> _batches;

    // 총 개수
    public int Count => _batches.Sum(b => b.Count);

    // 가장 빨리 만료되는 배치의 시간 (UI 표시용)
    public float RemainingTime => _batches.Count > 0 ? _batches.Min(b => b.RemainingTime) : 0f;
    public float MaxDuration => ItemData.Duration;
    public float TimePercent => MaxDuration > 0 ? RemainingTime / MaxDuration : 1f;
    public bool HasTimer => MaxDuration > 0f;
    public bool IsExpired => _batches.Count == 0;  // 모든 배치가 사라지면 만료

    public InventoryItem(ItemData itemData, int count = 1)
    {
        ItemData = itemData;
        _batches = new List<ItemBatch>();

        // 첫 배치 추가
        AddNewBatch(count);
    }

    /// <summary>
    /// 새 배치 추가 (아이템을 새로 획득했을 때)
    /// </summary>
    public void AddNewBatch(int count)
    {
        if (count <= 0) return;

        ItemBatch newBatch = new ItemBatch(count, ItemData.Duration);
        _batches.Add(newBatch);
    }

    /// <summary>
    /// 아이템 개수 감소 (가장 오래된 배치부터 제거)
    /// </summary>
    /// <returns>감소 후 남은 총 개수</returns>
    public int RemoveCount(int amount)
    {
        int remaining = amount;

        // 가장 빨리 만료되는 배치부터 제거 (FIFO)
        for (int i = 0; i < _batches.Count && remaining > 0; i++)
        {
            ItemBatch batch = _batches[i];
            int removeFromBatch = Math.Min(batch.Count, remaining);

            batch.Count -= removeFromBatch;
            remaining -= removeFromBatch;

            if (batch.Count <= 0)
            {
                _batches.RemoveAt(i);
                i--;  // 인덱스 조정
            }
        }

        return Count;
    }

    /// <summary>
    /// 아이템이 비어있는지 확인
    /// </summary>
    public bool IsEmpty => _batches.Count == 0 || Count <= 0;

    /// <summary>
    /// 타이머 업데이트 (모든 배치의 타이머 업데이트)
    /// </summary>
    /// <param name="deltaTime">프레임 시간</param>
    /// <returns>만료된 배치가 있는지 여부</returns>
    public bool UpdateTimer(float deltaTime)
    {
        if (!HasTimer || _batches.Count == 0)
            return false;

        bool anyExpired = false;

        // 모든 배치의 타이머 감소
        for (int i = _batches.Count - 1; i >= 0; i--)
        {
            ItemBatch batch = _batches[i];
            batch.RemainingTime -= deltaTime;

            // 배치 만료
            if (batch.RemainingTime <= 0f)
            {
                _batches.RemoveAt(i);
                anyExpired = true;
            }
        }

        // 모든 배치가 사라졌으면 슬롯 전체 만료
        return anyExpired && _batches.Count == 0;
    }

    /// <summary>
    /// 배치 개수 가져오기 (디버그용)
    /// </summary>
    public int GetBatchCount() => _batches.Count;

    /// <summary>
    /// 모든 배치 정보 가져오기 (디버그용)
    /// </summary>
    public List<ItemBatch> GetBatches() => new List<ItemBatch>(_batches);
}
