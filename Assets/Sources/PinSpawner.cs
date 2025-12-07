using Fusion;
using UnityEngine;

public class PinSpawner : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private GPSController gpsController;

    [Header("Pin Settings")]
    [SerializeField] private GameObject pinPrefab;
    [SerializeField] private Transform pinParent;
    [SerializeField] private float earthRadius = 100f;

    private bool hasSpawnedPin = false;

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
        }
    }

    private void OnDisable()
    {
        if (gpsController)
        {
            gpsController.OnGPSUpdated -= OnGPSUpdated;
        }
    }

    /// <summary>
    /// GPS 좌표 업데이트 시 핀 생성
    /// </summary>
    private void OnGPSUpdated(float latitude, float longitude)
    {
        if (hasSpawnedPin)
        {
            return; // 이미 핀을 생성했으면 무시
        }

        if (pinPrefab == null)
        {
            Debug.LogWarning("Pin prefab is not assigned!");
            return;
        }

        // 네트워크 오브젝트인지 확인
        NetworkObject networkPinPrefab = pinPrefab.GetComponent<NetworkObject>();
        if (networkPinPrefab != null && Runner != null && Runner.IsRunning)
        {
            // 네트워크 오브젝트로 스폰 - RPC를 통해 서버에 요청
            RequestSpawnPinRpc(latitude, longitude);
            // RPC 호출 후 로컬에서도 플래그 설정 (중복 호출 방지)
            hasSpawnedPin = true;
        }
        else
        {
            // 일반 오브젝트로 생성 (네트워크가 없을 때)
            Vector3 worldPosition = BSCoordinate.LatLonToWorld(latitude, longitude, earthRadius);
            Vector3 direction = (worldPosition - pinParent.position).normalized;
            Quaternion rotation = Quaternion.FromToRotation(Vector3.up, direction);
            GameObject pinInstance = Instantiate(pinPrefab, worldPosition, rotation);
            Debug.Log($"Pin spawned (local) at GPS: {latitude:F6}, {longitude:F6} -> World: {worldPosition}");
            hasSpawnedPin = true;
        }
    }

    /// <summary>
    /// 서버에 핀 스폰 요청 (RPC)
    /// </summary>
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RequestSpawnPinRpc(float latitude, float longitude, RpcInfo info = default)
    {
        if (pinPrefab == null)
        {
            Debug.LogWarning("Pin prefab is not assigned!");
            return;
        }

        // GPS 좌표를 Unity 월드 좌표로 변환
        Vector3 worldPosition = BSCoordinate.LatLonToWorld(latitude, longitude, earthRadius);
        Vector3 direction = (worldPosition - pinParent.position).normalized;
        // Capsule의 Y축(up)이 표면 법선 방향과 일치하도록 회전 설정
        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, direction);

        NetworkObject networkPinPrefab = pinPrefab.GetComponent<NetworkObject>();
        if (networkPinPrefab != null && Runner != null)
        {
            // 서버에서 네트워크 오브젝트로 스폰 (모든 클라이언트에 동기화됨)
            Runner.Spawn(networkPinPrefab, worldPosition, rotation, info.Source);
            Debug.Log($"Pin spawned (network) for player {info.Source} at GPS: {latitude:F6}, {longitude:F6} -> World: {worldPosition}");
        }
    }

    /// <summary>
    /// 핀 생성 상태 리셋 (방을 나갔다가 다시 들어올 때 사용)
    /// </summary>
    public void ResetPinSpawn()
    {
        hasSpawnedPin = false;
    }
}

