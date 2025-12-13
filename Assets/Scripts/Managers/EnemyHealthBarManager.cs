using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 적 체력바 관리자 - 적 생성 시 자동으로 체력바 생성 및 관리
/// </summary>
public class EnemyHealthBarManager : MonoBehaviour
{
    public static EnemyHealthBarManager Instance { get; private set; }

    [Header("프리팹 참조")]
    [SerializeField] private GameObject _enemyHealthBarPrefab;  // 일반 몬스터 체력바 프리팹
    [SerializeField] private BossHealthBarUI _bossHealthBarUI;  // 보스 체력바 UI (화면 상단 고정)

    [Header("부모 Transform")]
    [SerializeField] private Transform _healthBarsContainer;  // 체력바들의 부모 (Canvas 하위)

    private Dictionary<Health, EnemyHealthBarUI> _activeHealthBars = new Dictionary<Health, EnemyHealthBarUI>();

    private void Awake()
    {
        // 싱글톤 설정
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("EnemyHealthBarManager: 이미 인스턴스가 존재합니다! 파괴합니다.");
            Destroy(gameObject);
            return;
        }

        // 컨테이너 자동 생성
        if (_healthBarsContainer == null)
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                GameObject container = new GameObject("EnemyHealthBars");
                container.transform.SetParent(canvas.transform, false);
                _healthBarsContainer = container.transform;
            }
            else
            {
                Debug.LogError("EnemyHealthBarManager: Canvas를 찾을 수 없습니다!");
            }
        }
    }

    /// <summary>
    /// 일반 몬스터 체력바 생성
    /// </summary>
    public void CreateEnemyHealthBar(Health enemyHealth, Transform enemyTransform)
    {
        if (enemyHealth == null || enemyTransform == null)
        {
            Debug.LogWarning("EnemyHealthBarManager: Health 또는 Transform이 null입니다!");
            return;
        }

        // 이미 체력바가 있으면 무시
        if (_activeHealthBars.ContainsKey(enemyHealth))
        {
            Debug.LogWarning("EnemyHealthBarManager: 이미 체력바가 존재합니다!");
            return;
        }

        // 프리팹이 없으면 생성 불가
        if (_enemyHealthBarPrefab == null)
        {
            Debug.LogError("EnemyHealthBarManager: 체력바 프리팹이 설정되지 않았습니다!");
            return;
        }

        // 체력바 생성
        GameObject healthBarObj = Instantiate(_enemyHealthBarPrefab, _healthBarsContainer);
        EnemyHealthBarUI healthBarUI = healthBarObj.GetComponent<EnemyHealthBarUI>();

        if (healthBarUI != null)
        {
            healthBarUI.SetEnemy(enemyHealth, enemyTransform);
            _activeHealthBars.Add(enemyHealth, healthBarUI);

            // 몬스터 사망 시 체력바 제거
            enemyHealth.OnDeath += () => RemoveEnemyHealthBar(enemyHealth);

            Debug.Log($"EnemyHealthBarManager: 일반 몬스터 '{enemyTransform.name}' 체력바 생성");
        }
        else
        {
            Debug.LogError("EnemyHealthBarManager: 프리팹에 EnemyHealthBarUI 컴포넌트가 없습니다!");
            Destroy(healthBarObj);
        }
    }

    /// <summary>
    /// 보스 체력바 표시
    /// </summary>
    public void ShowBossHealthBar(Health bossHealth, string bossName)
    {
        if (bossHealth == null)
        {
            Debug.LogWarning("EnemyHealthBarManager: 보스 Health가 null입니다!");
            return;
        }

        if (_bossHealthBarUI == null)
        {
            Debug.LogError("EnemyHealthBarManager: BossHealthBarUI가 설정되지 않았습니다!");
            return;
        }

        _bossHealthBarUI.SetBoss(bossHealth, bossName);
        Debug.Log($"EnemyHealthBarManager: 보스 '{bossName}' 체력바 표시");
    }

    /// <summary>
    /// 일반 몬스터 체력바 제거
    /// </summary>
    public void RemoveEnemyHealthBar(Health enemyHealth)
    {
        if (enemyHealth != null && _activeHealthBars.ContainsKey(enemyHealth))
        {
            EnemyHealthBarUI healthBarUI = _activeHealthBars[enemyHealth];
            _activeHealthBars.Remove(enemyHealth);

            if (healthBarUI != null)
            {
                Destroy(healthBarUI.gameObject);
            }

            Debug.Log($"EnemyHealthBarManager: 몬스터 체력바 제거");
        }
    }

    /// <summary>
    /// 보스 체력바 숨김
    /// </summary>
    public void HideBossHealthBar()
    {
        if (_bossHealthBarUI != null)
        {
            _bossHealthBarUI.ClearBoss();
            Debug.Log("EnemyHealthBarManager: 보스 체력바 숨김");
        }
    }

    /// <summary>
    /// 모든 체력바 제거
    /// </summary>
    public void ClearAllHealthBars()
    {
        foreach (var healthBar in _activeHealthBars.Values)
        {
            if (healthBar != null)
            {
                Destroy(healthBar.gameObject);
            }
        }

        _activeHealthBars.Clear();
        HideBossHealthBar();

        Debug.Log("EnemyHealthBarManager: 모든 체력바 제거");
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
