using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 층별 Y 범위 설정
/// </summary>
[System.Serializable]
public class FloorYRange
{
    public float MinY;  // 층의 최소 Y 좌표
    public float MaxY;  // 층의 최대 Y 좌표

    public FloorYRange(float minY, float maxY)
    {
        MinY = minY;
        MaxY = maxY;
    }

    public bool ContainsY(float y)
    {
        return y >= MinY && y <= MaxY;
    }
}

/// <summary>
/// 미니맵 UI - 층별 맵 이미지 표시 및 플레이어 위치 추적
/// </summary>
public class MinimapUI : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private Image _minimapImage;  // 층별 맵 이미지
    [SerializeField] private RectTransform _playerMarker;  // 플레이어 마커 (빨간 점)

    [Header("맵 설정")]
    [SerializeField] private Sprite[] _floorMapSprites;  // 층별 맵 이미지 배열 (인덱스 = 층 번호)
    [SerializeField] private int _currentFloor = 0;  // 현재 층 (0부터 시작)

    [Header("층별 Y 범위 설정")]
    [SerializeField] private FloorYRange[] _floorYRanges;  // 층별 Y 좌표 범위
    [SerializeField] private bool _autoDetectFloor = true;  // 플레이어 Y 좌표로 층 자동 감지

    [Header("월드 좌표 범위 (X축)")]
    [SerializeField] private float _worldMinX = -50f;  // 월드 최소 X 좌표
    [SerializeField] private float _worldMaxX = 50f;  // 월드 최대 X 좌표
    [SerializeField] private bool _showDebugInfo = true;  // 디버그 정보 표시

    [Header("플레이어 추적")]
    [SerializeField] private Transform _playerTransform;  // 플레이어 Transform (비워두면 자동 검색)
    [SerializeField] private bool _trackPlayer = true;  // 플레이어 위치 추적 활성화

    [Header("마커 설정")]
    [SerializeField] private Color _playerMarkerColor = Color.red;  // 플레이어 마커 색상
    [SerializeField] private float _markerSize = 8f;  // 마커 크기
    [SerializeField] private float _markerYPosition = -0.4f;  // 마커의 Y 위치 (미니맵 내, -0.5~0.5 = 하단~상단)

    private RectTransform _minimapRect;
    private float _worldSizeX;
    private int _lastDetectedFloor = -1;

    private void Start()
    {
        _minimapRect = _minimapImage.GetComponent<RectTransform>();

        // 플레이어 자동 검색
        if (_playerTransform == null)
        {
            PlayerController player = FindFirstObjectByType<PlayerController>();
            if (player != null)
            {
                _playerTransform = player.transform;
                Debug.Log("MinimapUI: PlayerController 자동 검색 완료");
            }
            else
            {
                Debug.LogWarning("MinimapUI: PlayerController를 찾을 수 없습니다!");
            }
        }

        // 플레이어 마커 설정
        if (_playerMarker != null)
        {
            Image markerImage = _playerMarker.GetComponent<Image>();
            if (markerImage != null)
            {
                markerImage.color = _playerMarkerColor;
            }
            _playerMarker.sizeDelta = new Vector2(_markerSize, _markerSize);
        }
        else
        {
            Debug.LogWarning("MinimapUI: Player Marker가 연결되지 않았습니다!");
        }

        // 월드 크기 계산 (X축만)
        _worldSizeX = _worldMaxX - _worldMinX;

        // 초기 층 설정
        SetFloor(_currentFloor);
    }

    private void Update()
    {
        if (_trackPlayer && _playerTransform != null && _playerMarker != null)
        {
            // 층 자동 감지
            if (_autoDetectFloor)
            {
                DetectCurrentFloor();
            }

            UpdatePlayerMarkerPosition();
        }
    }

    /// <summary>
    /// 플레이어 Y 좌표로 현재 층 감지
    /// </summary>
    private void DetectCurrentFloor()
    {
        if (_floorYRanges == null || _floorYRanges.Length == 0)
            return;

        float playerY = _playerTransform.position.y;

        for (int i = 0; i < _floorYRanges.Length; i++)
        {
            if (_floorYRanges[i].ContainsY(playerY))
            {
                if (_currentFloor != i)
                {
                    SetFloor(i);
                    _lastDetectedFloor = i;
                    Debug.Log($"MinimapUI: 플레이어 층 변경 감지 - {i}층 (Y: {playerY})");
                }
                return;
            }
        }

        // 범위 밖이면 경고
        if (_showDebugInfo && _lastDetectedFloor != -1)
        {
            Debug.LogWarning($"MinimapUI: 플레이어가 정의된 층 범위 밖에 있습니다! (Y: {playerY})");
            _lastDetectedFloor = -1;
        }
    }

    /// <summary>
    /// 플레이어 마커 위치 업데이트 (X축만 사용)
    /// </summary>
    private void UpdatePlayerMarkerPosition()
    {
        // 플레이어 X 좌표만 사용
        float playerX = _playerTransform.position.x;

        // X 좌표를 0~1 범위로 정규화
        float normalizedX = Mathf.InverseLerp(_worldMinX, _worldMaxX, playerX);

        // 미니맵 로컬 좌표로 변환 (X축만, Y축은 고정)
        Vector2 minimapLocalPos = new Vector2(
            (normalizedX - 0.5f) * _minimapRect.rect.width,
            _markerYPosition * _minimapRect.rect.height
        );

        // 마커 위치 설정
        _playerMarker.anchoredPosition = minimapLocalPos;

        // 디버그 정보 출력 (1초마다)
        if (_showDebugInfo && Time.frameCount % 60 == 0)
        {
            Debug.Log($"[Minimap] Player X: {playerX} | Y: {_playerTransform.position.y} | Floor: {_currentFloor}");
            Debug.Log($"[Minimap] Normalized X: {normalizedX} | Marker Local Pos: {minimapLocalPos}");
        }
    }

    /// <summary>
    /// 층 변경
    /// </summary>
    public void SetFloor(int floorIndex)
    {
        _currentFloor = floorIndex;

        if (_floorMapSprites != null && floorIndex >= 0 && floorIndex < _floorMapSprites.Length)
        {
            if (_floorMapSprites[floorIndex] != null)
            {
                _minimapImage.sprite = _floorMapSprites[floorIndex];
                Debug.Log($"MinimapUI: {floorIndex}층 맵 이미지 설정");
            }
            else
            {
                Debug.LogWarning($"MinimapUI: {floorIndex}층 맵 이미지가 null입니다!");
            }
        }
        else
        {
            Debug.LogWarning($"MinimapUI: 층 인덱스 {floorIndex}가 범위를 벗어났습니다! (최대: {_floorMapSprites?.Length ?? 0})");
        }
    }

    /// <summary>
    /// X축 경계 수동 설정
    /// </summary>
    public void SetWorldBoundsX(float minX, float maxX)
    {
        _worldMinX = minX;
        _worldMaxX = maxX;
        _worldSizeX = _worldMaxX - _worldMinX;
        Debug.Log($"MinimapUI: X축 경계 설정 - Min: {_worldMinX}, Max: {_worldMaxX}");
    }

    /// <summary>
    /// 층별 Y 범위 설정
    /// </summary>
    public void SetFloorYRanges(FloorYRange[] ranges)
    {
        _floorYRanges = ranges;
        Debug.Log($"MinimapUI: {ranges.Length}개 층의 Y 범위 설정 완료");
    }

    /// <summary>
    /// 플레이어 추적 활성화/비활성화
    /// </summary>
    public void SetTrackPlayer(bool track)
    {
        _trackPlayer = track;
    }

    /// <summary>
    /// 플레이어 Transform 설정
    /// </summary>
    public void SetPlayerTransform(Transform player)
    {
        _playerTransform = player;
    }

    /// <summary>
    /// 미니맵 표시/숨김
    /// </summary>
    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }

    /// <summary>
    /// Scene 뷰에서 층별 경계 시각화 (디버그용)
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!_showDebugInfo)
            return;

        // 층별 Y 범위를 Scene 뷰에 그리기
        if (_floorYRanges != null && _floorYRanges.Length > 0)
        {
            for (int i = 0; i < _floorYRanges.Length; i++)
            {
                FloorYRange floor = _floorYRanges[i];

                // 층별로 다른 색상
                Gizmos.color = new Color(1f, 1f, 0f, 0.3f + (i * 0.2f));

                Vector3 bottomLeft = new Vector3(_worldMinX, floor.MinY, 0);
                Vector3 bottomRight = new Vector3(_worldMaxX, floor.MinY, 0);
                Vector3 topLeft = new Vector3(_worldMinX, floor.MaxY, 0);
                Vector3 topRight = new Vector3(_worldMaxX, floor.MaxY, 0);

                // 층 경계 그리기
                Gizmos.DrawLine(bottomLeft, bottomRight);
                Gizmos.DrawLine(bottomRight, topRight);
                Gizmos.DrawLine(topRight, topLeft);
                Gizmos.DrawLine(topLeft, bottomLeft);

                // 층 번호 표시 (왼쪽 상단)
                #if UNITY_EDITOR
                UnityEditor.Handles.Label(topLeft + Vector3.up * 0.5f, $"Floor {i}");
                #endif
            }
        }

        // X축 범위 표시
        Gizmos.color = Color.cyan;
        if (_floorYRanges != null && _floorYRanges.Length > 0)
        {
            float minY = _floorYRanges[0].MinY;
            float maxY = _floorYRanges[_floorYRanges.Length - 1].MaxY;
            Gizmos.DrawLine(new Vector3(_worldMinX, minY, 0), new Vector3(_worldMinX, maxY, 0));
            Gizmos.DrawLine(new Vector3(_worldMaxX, minY, 0), new Vector3(_worldMaxX, maxY, 0));
        }

        // 플레이어 위치 표시
        if (_playerTransform != null && Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_playerTransform.position, 0.5f);

            // 플레이어 X 좌표에 수직선 표시
            if (_floorYRanges != null && _floorYRanges.Length > 0)
            {
                float minY = _floorYRanges[0].MinY;
                float maxY = _floorYRanges[_floorYRanges.Length - 1].MaxY;
                Gizmos.color = Color.red;
                Gizmos.DrawLine(
                    new Vector3(_playerTransform.position.x, minY, 0),
                    new Vector3(_playerTransform.position.x, maxY, 0)
                );
            }
        }
    }
}
