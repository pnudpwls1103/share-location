using TMPro;
using UnityEngine;

public class UIMain : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GPSController gpsController;
    [SerializeField] private RoomManager roomManager;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI textRoomCode;
    [SerializeField] private TextMeshProUGUI textErrorMessage;
    [SerializeField] private TextMeshProUGUI textGPS;
    [SerializeField] private GameObject panelWaiting;

    private void Awake()
    {
        if (!gpsController)
        {
            gpsController = FindFirstObjectByType<GPSController>();
        }

        if (!roomManager)
        {
            roomManager = FindFirstObjectByType<RoomManager>();
        }
    }

    private void OnEnable()
    {
        if (gpsController)
        {
            gpsController.OnGPSUpdated += OnGPSUpdated;
            gpsController.OnGPSFailed += OnGPSFailed;
        }
    }

    private void OnDisable()
    {
        if (gpsController)
        {
            gpsController.OnGPSUpdated -= OnGPSUpdated;
            gpsController.OnGPSFailed -= OnGPSFailed;
        }
    }

    /// <summary>
    /// GPS 값 업데이트 시 호출
    /// </summary>
    private void OnGPSUpdated(float latitude, float longitude)
    {
        if (textGPS)
        {
            Vector3 coordinate = BSCoordinate.LatLonToWorld(latitude, longitude, 100);
            textGPS.text = $"GPS: {coordinate.x:F6}, {coordinate.y:F6}, {coordinate.z:F6}";
        }
    }

    /// <summary>
    /// GPS 실패 시 호출
    /// </summary>
    private void OnGPSFailed(string errorMessage)
    {
        ShowErrorMessage(errorMessage);
    }

    public void SetRoomCode(string roomCode)
    {
        if (textRoomCode)
        {
            textRoomCode.text = string.IsNullOrEmpty(roomCode) ? "" : $"RoomId: {roomCode}";
        }
    }

    public void ShowErrorMessage(string message)
    {
        if (textErrorMessage)
        {
            textErrorMessage.text = message;
            textErrorMessage.gameObject.SetActive(true);
        }
    }

    public void ClearErrorMessage()
    {
        if (textErrorMessage)
        {
            textErrorMessage.text = "";
            textErrorMessage.gameObject.SetActive(false);
        }
    }

    public void SetWaitingPanelActive(bool active)
    {
        if (panelWaiting)
        {
            panelWaiting.SetActive(active);
        }
    }

    public void OnSceneLoadButtonClicked()
    {
        if (roomManager == null)
        {
            Debug.LogError("RoomManager is not assigned. Cannot load scene.");
            ShowErrorMessage("RoomManager를 찾을 수 없습니다.");
            return;
        }

        roomManager.LoadGameScene();
    }
}

