using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomManager : MonoBehaviour, INetworkRunnerCallbacks
{
    [Header("Network Settings")]
    [SerializeField] private NetworkRunner runnerPrefab;
    [SerializeField] private NetworkRunner runner;
    [SerializeField] private SceneRef gameScene;

    private string currentRoomCode;

    // Events
    public event Action<string> OnRoomCreated; // roomCode
    public event Action OnRoomJoined;
    public event Action<string> OnRoomJoinFailed; // errorMessage
    public event Action OnConnectedToServerAction;
    public event Action<string> OnDisconnected; // reason

    private void Awake()
    {
        if (runnerPrefab == null)
        {
            runnerPrefab = FindFirstObjectByType<NetworkRunner>();
            if (runnerPrefab == null)
            {
                Debug.LogError("NetworkRunner prefab not found!");
            }
        }
    }

    /// <summary>
    /// 6자리 방 코드 생성 (대문자 + 숫자)
    /// </summary>
    public string GenerateRoomCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        System.Random random = new System.Random();
        char[] code = new char[6];

        for (int i = 0; i < 6; i++)
        {
            code[i] = chars[random.Next(chars.Length)];
        }

        return new string(code);
    }

    /// <summary>
    /// NetworkRunner 초기화 (내부 메서드)
    /// </summary>
    private void InitializeRunner()
    {
        // 이미 실행 중인 runner가 있으면 먼저 종료
        if (runner != null && runner.IsRunning)
        {
            runner.Shutdown();
            runner = null;
        }

        // Scene에 NetworkRunner가 있는지 확인
        if (runner == null)
        {
            runner = FindFirstObjectByType<NetworkRunner>();
        }

        // Scene에 없으면 프리팹에서 생성
        if (runner == null)
        {
            if (runnerPrefab != null)
            {
                runner = Instantiate(runnerPrefab);
            }
            else
            {
                // 프리팹도 없으면 새로 생성
                GameObject runnerObj = new GameObject("NetworkRunner");
                runner = runnerObj.AddComponent<NetworkRunner>();
            }
        }

        // runner.name = "NetworkRunner";
        DontDestroyOnLoad(runner);

        // SceneManager와 ObjectProvider 설정
        var sceneManager = runner.GetComponent<INetworkSceneManager>();
        if (sceneManager == null)
        {
            sceneManager = runner.gameObject.AddComponent<NetworkSceneManagerDefault>();
        }

        var objectProvider = runner.GetComponent<INetworkObjectProvider>();
        if (objectProvider == null)
        {
            objectProvider = runner.gameObject.AddComponent<NetworkObjectProviderDefault>();
        }

        // 콜백 등록
        runner.AddCallbacks(this);
    }

    /// <summary>
    /// 방 생성 (Connect + CreateRoom을 함께 처리)
    /// </summary>
    public async Task CreateRoom()
    {
        InitializeRunner();

        // 6자리 방 코드 생성
        currentRoomCode = GenerateRoomCode();

        var sceneInfo = new NetworkSceneInfo();
        if (gameScene.IsValid)
        {
            sceneInfo.AddSceneRef(gameScene, LoadSceneMode.Additive);
        }

        var sceneManager = runner.GetComponent<INetworkSceneManager>();
        var objectProvider = runner.GetComponent<INetworkObjectProvider>();

        var startGameArgs = new StartGameArgs
        {
            GameMode = GameMode.AutoHostOrClient, // Host 모드로 방 생성
            SessionName = currentRoomCode,
            Scene = sceneInfo,
            SceneManager = sceneManager,
            ObjectProvider = objectProvider,
        };

        await runner.StartGame(startGameArgs);
    }

    /// <summary>
    /// 방 참가 (Connect + JoinRoom을 함께 처리)
    /// </summary>
    public async Task JoinRoom(string roomCode)
    {
        if (string.IsNullOrEmpty(roomCode) || roomCode.Length != 6)
        {
            OnRoomJoinFailed?.Invoke("Room code must be 6 characters.");
            return;
        }

        currentRoomCode = roomCode.ToUpper();

        // NetworkRunner 초기화
        InitializeRunner();

        var sceneInfo = new NetworkSceneInfo();
        if (gameScene.IsValid)
        {
            sceneInfo.AddSceneRef(gameScene, LoadSceneMode.Additive);
        }

        var sceneManager = runner.GetComponent<INetworkSceneManager>();
        var objectProvider = runner.GetComponent<INetworkObjectProvider>();

        var startGameArgs = new StartGameArgs
        {
            GameMode = GameMode.AutoHostOrClient, // Client 모드로 방 참가
            SessionName = currentRoomCode,
            Scene = sceneInfo,
            SceneManager = sceneManager,
            ObjectProvider = objectProvider,
        };

        await runner.StartGame(startGameArgs);
    }

    /// <summary>
    /// 연결 해제
    /// </summary>
    public void Disconnect()
    {
        if (runner != null && runner.IsRunning)
        {
            runner.Shutdown();
        }
    }

    // INetworkRunnerCallbacks 구현

    public void OnConnectedToServer(NetworkRunner runner)
    {
        Debug.Log("Connected to server");
        OnConnectedToServerAction?.Invoke();
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log($"Shutdown: {shutdownReason}");

        string errorMessage = GetShutdownReasonMessage(shutdownReason);

        // 방이 없거나 연결 실패한 경우 에러 메시지 전달
        if (shutdownReason == ShutdownReason.GameNotFound)
        {
            OnRoomJoinFailed?.Invoke("Room not found. Please check the room code.");
        }
        else if (shutdownReason != ShutdownReason.Ok)
        {
            OnDisconnected?.Invoke(errorMessage);
        }
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        Debug.Log($"Disconnected from server: {reason}");
        OnDisconnected?.Invoke(reason.ToString());
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        request.Accept();
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        Debug.LogError($"Connect failed: {reason}");
        OnRoomJoinFailed?.Invoke($"Connection failed: {reason}");
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (player == runner.LocalPlayer)
        {
            Debug.Log("Player joined room");

            // 호스트인 경우 (방 생성자) OnRoomCreated 이벤트 호출
            if (runner.IsServer && !string.IsNullOrEmpty(currentRoomCode))
            {
                OnRoomCreated?.Invoke(currentRoomCode);
            }
            else
            {
                // 클라이언트인 경우 (방 참가자) OnRoomJoined 이벤트 호출
                OnRoomJoined?.Invoke();
            }
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"Player left: {player}");
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        // 입력 처리 (필요시 구현)
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
        // 입력 누락 처리
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        Debug.Log("Scene load done");
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
        Debug.Log("Scene load start");
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        // 세션 리스트 업데이트
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
        // 커스텀 인증 응답
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        // 호스트 마이그레이션
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
        // 신뢰할 수 있는 데이터 수신
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
        // 신뢰할 수 있는 데이터 진행률
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        // AOI에서 객체 제거
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        // AOI에 객체 추가
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
        // 사용자 시뮬레이션 메시지
    }

    /// <summary>
    /// ShutdownReason을 사용자 친화적인 메시지로 변환
    /// </summary>
    private string GetShutdownReasonMessage(ShutdownReason reason)
    {
        switch (reason)
        {
            case ShutdownReason.GameNotFound:
                return "Room not found.";
            case ShutdownReason.GameIsFull:
                return "Room is full.";
            case ShutdownReason.GameClosed:
                return "Room is closed.";
            case ShutdownReason.Error:
                return "An error occurred.";
            case ShutdownReason.IncompatibleConfiguration:
                return "Incompatible configuration.";
            case ShutdownReason.ServerInRoom:
                return "Server already exists in room.";
            case ShutdownReason.MaxCcuReached:
                return "Maximum concurrent users reached.";
            case ShutdownReason.InvalidRegion:
                return "Invalid region.";
            case ShutdownReason.GameIdAlreadyExists:
                return "Room already exists.";
            case ShutdownReason.InvalidAuthentication:
                return "Authentication failed.";
            case ShutdownReason.PhotonCloudTimeout:
                return "Server connection timeout.";
            default:
                return reason.ToString();
        }
    }

    public string GetCurrentRoomCode()
    {
        return currentRoomCode;
    }
}

