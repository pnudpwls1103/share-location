using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

public class UILobby : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RoomManager roomManager;
    [SerializeField] private UIMain uiMain;

    [Header("UI Elements")]
    [SerializeField] private Button btnCreateRoom;
    [SerializeField] private Button btnJoinRoom;
    [SerializeField] private TMP_InputField inputRoomCode;
    [SerializeField] private GameObject panelLobby;

    private bool isConnecting = false;

    private void Awake()
    {
        if (!roomManager)
        {
            roomManager = FindFirstObjectByType<RoomManager>();
        }

        if (!roomManager)
        {
            Debug.LogError("RoomManager not found!");
            return;
        }

        if (!uiMain)
        {
            uiMain = FindFirstObjectByType<UIMain>();
        }

        if (!uiMain)
        {
            Debug.LogWarning("UIMain not found!");
        }

        if (!panelLobby)
        {
            panelLobby = this.gameObject;
        }

        // 이벤트 구독
        roomManager.OnRoomCreated += OnRoomCreated;
        roomManager.OnRoomJoined += OnRoomJoined;
        roomManager.OnRoomJoinFailed += OnRoomJoinFailed;
        roomManager.OnConnectedToServerAction += OnConnectedToServer;
        roomManager.OnDisconnected += OnDisconnected;
    }

    private void OnDestroy()
    {
        if (roomManager)
        {
            // 이벤트 구독 해제
            roomManager.OnRoomCreated -= OnRoomCreated;
            roomManager.OnRoomJoined -= OnRoomJoined;
            roomManager.OnRoomJoinFailed -= OnRoomJoinFailed;
            roomManager.OnConnectedToServerAction -= OnConnectedToServer;
            roomManager.OnDisconnected -= OnDisconnected;
        }
    }

    private void Start()
    {
        // 버튼 이벤트 연결
        if (btnCreateRoom)
        {
            btnCreateRoom.onClick.AddListener(OnCreateRoomClicked);
        }

        if (btnJoinRoom)
        {
            btnJoinRoom.onClick.AddListener(OnJoinRoomClicked);
        }

        // 초기 UI 상태 설정
        if (uiMain)
        {
            uiMain.ClearErrorMessage();
            uiMain.SetRoomCode("");
            uiMain.SetWaitingPanelActive(false);
        }
    }

    /// <summary>
    /// 방 생성 버튼 클릭
    /// </summary>
    private async void OnCreateRoomClicked()
    {
        if (isConnecting)
        {
            return;
        }

        ClearErrorMessage();
        SetConnectingState(true);

        try
        {
            if (roomManager)
            {
                await roomManager.CreateRoom();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to create room: {e}");
            ShowErrorMessage($"Failed to create room: {e.Message}");
            SetConnectingState(false);
        }
    }

    /// <summary>
    /// 방 참가 버튼 클릭
    /// </summary>
    private async void OnJoinRoomClicked()
    {
        if (isConnecting)
        {
            return;
        }
        ClearErrorMessage();

        if (!inputRoomCode.gameObject.activeSelf)
        {
            inputRoomCode.gameObject.SetActive(true);
            return;
        }

        // 방 코드 입력 확인
        string roomCode = inputRoomCode ? inputRoomCode.text.Trim().ToUpper() : "";
        if (string.IsNullOrEmpty(roomCode))
        {
            ShowErrorMessage("Please enter a room code.");
            return;
        }

        if (roomCode.Length != 6)
        {
            ShowErrorMessage("Room code must be 6 characters.");
            return;
        }

        SetConnectingState(true);
        try
        {
            if (roomManager)
            {
                await roomManager.JoinRoom(roomCode);
                if (inputRoomCode)
                {
                    inputRoomCode.gameObject.SetActive(false);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to join room: {e}");
            ShowErrorMessage($"Failed to join room: {e.Message}");
            SetConnectingState(false);
        }
    }

    /// <summary>
    /// 방 생성 성공 시 호출
    /// </summary>
    private void OnRoomCreated(string roomCode)
    {
        Debug.Log($"Room created: {roomCode}");

        if (uiMain)
        {
            uiMain.SetRoomCode(roomCode);
        }

        SetConnectingState(false);
        panelLobby?.SetActive(false);
    }

    /// <summary>
    /// 방 참가 성공 시 호출
    /// </summary>
    private void OnRoomJoined()
    {
        Debug.Log("Room joined successfully");
        SetConnectingState(false);

        // 로비 UI 숨기기 (게임 씬으로 전환될 예정)
        if (panelLobby)
        {
            panelLobby.SetActive(false);
        }
    }

    /// <summary>
    /// 방 참가 실패 시 호출
    /// </summary>
    private void OnRoomJoinFailed(string errorMessage)
    {
        Debug.LogWarning($"Room join failed: {errorMessage}");
        ShowErrorMessage(errorMessage);
        SetConnectingState(false);
    }

    /// <summary>
    /// 서버 연결 성공 시 호출
    /// </summary>
    private void OnConnectedToServer()
    {
        Debug.Log("Connected to server");
    }

    /// <summary>
    /// 연결 해제 시 호출
    /// </summary>
    private void OnDisconnected(string reason)
    {
        Debug.Log($"Disconnected: {reason}");
        SetConnectingState(false);
    }

    /// <summary>
    /// 에러 메시지 표시
    /// </summary>
    private void ShowErrorMessage(string message)
    {
        if (uiMain)
        {
            uiMain.ShowErrorMessage(message);
        }
    }

    /// <summary>
    /// 에러 메시지 지우기
    /// </summary>
    private void ClearErrorMessage()
    {
        if (uiMain)
        {
            uiMain.ClearErrorMessage();
        }
    }

    /// <summary>
    /// 연결 중 상태 설정
    /// </summary>
    private void SetConnectingState(bool connecting)
    {
        isConnecting = connecting;

        if (btnCreateRoom)
        {
            btnCreateRoom.interactable = !connecting;
        }

        if (btnJoinRoom)
        {
            btnJoinRoom.interactable = !connecting;
        }

        if (inputRoomCode)
        {
            inputRoomCode.interactable = !connecting;
        }

        if (uiMain)
        {
            uiMain.SetWaitingPanelActive(connecting);
        }
    }
}

