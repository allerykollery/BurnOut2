using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// 카드키 UI - 카드키 개수 표시 및 다음 스테이지 이동
/// InventoryManager와 연동
/// </summary>
public class CardKeyUI : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private Image _cardKeyIcon;  // 카드키 아이콘
    [SerializeField] private TextMeshProUGUI _countText;  // 개수 텍스트
    [SerializeField] private GameObject _emptyIndicator;  // 빈 상태 표시 (선택)
    [SerializeField] private Button _button;  // 클릭 버튼 (선택)

    [Header("아이콘 설정")]
    [SerializeField] private Sprite _cardKeySprite;  // 카드키 스프라이트

    [Header("시각 효과")]
    [SerializeField] private Color _emptyColor = new Color(1f, 1f, 1f, 0.3f);  // 카드키 없을 때
    [SerializeField] private Color _hasCardKeyColor = Color.white;  // 카드키 있을 때
    [SerializeField] private bool _showCountWhenZero = false;  // 0개일 때도 개수 표시
    [SerializeField] private bool _hideIconWhenEmpty = true;  // 카드키 없을 때 아이콘 숨기기

    [Header("다음 스테이지 설정")]
    [SerializeField] private string _nextSceneName = "";  // 다음 씬 이름 (비어있으면 현재 씬 인덱스+1)
    [SerializeField] private bool _useSceneIndex = true;  // 씬 인덱스 사용 (true) vs 씬 이름 사용 (false)

    private int _currentCardKeyCount = 0;

    private void Start()
    {
        if (InventoryManager.Instance != null)
        {
            // 카드키 변경 이벤트 구독
            InventoryManager.Instance.OnCardKeyCountChanged += OnCardKeyCountChanged;

            // 초기 상태 설정
            UpdateCardKeyUI(InventoryManager.Instance.CardKeyCount);
        }
        else
        {
            Debug.LogError("CardKeyUI: InventoryManager를 찾을 수 없습니다!");
        }

        // 카드키 아이콘 설정
        if (_cardKeyIcon != null && _cardKeySprite != null)
        {
            _cardKeyIcon.sprite = _cardKeySprite;
        }

        // 버튼 클릭 이벤트 연결
        if (_button != null)
        {
            _button.onClick.AddListener(OnCardKeyClicked);
        }
        else
        {
            Debug.LogWarning("CardKeyUI: Button이 연결되지 않았습니다. CardKeyPanel에 Button 컴포넌트를 추가하세요.");
        }
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnCardKeyCountChanged -= OnCardKeyCountChanged;
        }

        // 버튼 이벤트 해제
        if (_button != null)
        {
            _button.onClick.RemoveListener(OnCardKeyClicked);
        }
    }

    /// <summary>
    /// 카드키 개수 변경 이벤트 핸들러
    /// </summary>
    private void OnCardKeyCountChanged(int count)
    {
        UpdateCardKeyUI(count);
    }

    /// <summary>
    /// 카드키 UI 업데이트
    /// </summary>
    private void UpdateCardKeyUI(int count)
    {
        _currentCardKeyCount = count;
        bool hasCardKey = count > 0;

        // 아이콘 표시/숨김
        if (_cardKeyIcon != null)
        {
            if (_hideIconWhenEmpty && !hasCardKey)
            {
                // 카드키 없을 때 아이콘 완전히 숨김
                _cardKeyIcon.enabled = false;
            }
            else
            {
                // 카드키 있거나 숨김 옵션 꺼져있을 때 표시
                _cardKeyIcon.enabled = true;
                _cardKeyIcon.color = hasCardKey ? _hasCardKeyColor : _emptyColor;
            }
        }

        // 개수 텍스트 업데이트 - 카드키는 개수 표시 안 함
        if (_countText != null)
        {
            _countText.enabled = false;  // 항상 숨김
        }

        // 빈 상태 표시기
        if (_emptyIndicator != null)
        {
            _emptyIndicator.SetActive(!hasCardKey);
        }

        // 버튼 활성화/비활성화
        if (_button != null)
        {
            _button.interactable = hasCardKey;
        }
    }

    /// <summary>
    /// 카드키 클릭 이벤트 핸들러
    /// </summary>
    private void OnCardKeyClicked()
    {
        // 카드키가 없으면 무시
        if (_currentCardKeyCount <= 0)
        {
            Debug.LogWarning("CardKeyUI: 카드키가 없습니다!");
            return;
        }

        // 카드키 사용
        if (InventoryManager.Instance != null && InventoryManager.Instance.UseCardKey(1))
        {
            Debug.Log("CardKeyUI: 카드키 사용! 다음 스테이지로 이동...");
            LoadNextStage();
        }
        else
        {
            Debug.LogWarning("CardKeyUI: 카드키 사용 실패!");
        }
    }

    /// <summary>
    /// 다음 스테이지 로드
    /// </summary>
    private void LoadNextStage()
    {
        if (_useSceneIndex)
        {
            // 현재 씬 인덱스 + 1
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            int nextSceneIndex = currentSceneIndex + 1;

            // 씬 인덱스 범위 체크
            if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                Debug.Log($"CardKeyUI: 씬 인덱스 {nextSceneIndex}로 이동");
                SceneManager.LoadScene(nextSceneIndex);
            }
            else
            {
                Debug.LogWarning($"CardKeyUI: 씬 인덱스 {nextSceneIndex}가 범위를 벗어났습니다! (최대: {SceneManager.sceneCountInBuildSettings - 1})");
            }
        }
        else
        {
            // 씬 이름으로 로드
            if (!string.IsNullOrEmpty(_nextSceneName))
            {
                Debug.Log($"CardKeyUI: 씬 '{_nextSceneName}'로 이동");
                SceneManager.LoadScene(_nextSceneName);
            }
            else
            {
                Debug.LogError("CardKeyUI: Next Scene Name이 설정되지 않았습니다!");
            }
        }
    }

    /// <summary>
    /// 카드키 사용 애니메이션 (외부에서 호출 가능)
    /// </summary>
    public void PlayUseAnimation()
    {
        // TODO: 카드키 사용 시 애니메이션/효과 추가
        Debug.Log("CardKeyUI: 카드키 사용 애니메이션 재생");
    }

    /// <summary>
    /// 카드키 보유 여부 확인 (외부에서 호출 가능)
    /// </summary>
    public bool HasCardKey()
    {
        return _currentCardKeyCount > 0;
    }
}
