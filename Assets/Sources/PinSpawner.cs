using Fusion;
using UnityEngine;

public class PinSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GPSController gpsController;
    [SerializeField] private NetworkRunner networkRunner;

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

        if (!networkRunner)
        {
            networkRunner = FindFirstObjectByType<NetworkRunner>();
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

        // GPS 좌표를 Unity 월드 좌표로 변환
        // 핀의 회전을 구 표면에 수직으로 설정
        Vector3 worldPosition = BSCoordinate.LatLonToWorld(latitude, longitude, earthRadius);
        Vector3 direction = (worldPosition - pinParent.position).normalized;
        // Capsule의 Y축(up)이 표면 법선 방향과 일치하도록 회전 설정
        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, direction);
        // 네트워크 오브젝트인지 확인
        NetworkObject networkPinPrefab = pinPrefab.GetComponent<NetworkObject>();
        if (networkPinPrefab != null && networkRunner != null && networkRunner.IsRunning)
        {
            // 네트워크 오브젝트로 스폰
            if (networkRunner.IsServer)
            {
                networkRunner.Spawn(networkPinPrefab, worldPosition, rotation);
                Debug.Log($"Pin spawned at GPS: {latitude:F6}, {longitude:F6} -> World: {worldPosition}");
                hasSpawnedPin = true;
            }
        }
        else
        {
            // 일반 오브젝트로 생성
            GameObject pinInstance = Instantiate(pinPrefab, worldPosition, rotation);
            Debug.Log($"Pin spawned at GPS: {latitude:F6}, {longitude:F6} -> World: {worldPosition}");
            hasSpawnedPin = true;
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

