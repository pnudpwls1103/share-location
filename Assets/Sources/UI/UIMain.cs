using UnityEngine;
using TMPro;

public class UIMain : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GPSController gpsController;

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
        UpdateGPSDisplay(latitude, longitude);
    }

    /// <summary>
    /// GPS 실패 시 호출
    /// </summary>
    private void OnGPSFailed(string errorMessage)
    {
        ShowErrorMessage(errorMessage);
    }

    /// <summary>
    /// GPS 값 UI에 표시
    /// </summary>
    private void UpdateGPSDisplay(float latitude, float longitude)
    {
        if (textGPS)
        {
            textGPS.text = $"GPS: {latitude:F6}, {longitude:F6}";
        }
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
}

