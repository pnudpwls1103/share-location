using UnityEngine;
using System;
using System.Collections;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif

public class GPSController : MonoBehaviour
{
    private readonly int MAX_WAIT = 20;

    private float latitude;
    private float longitude;
    private bool isGPSReady = false;

    public event Action<float, float> OnGPSUpdated; // latitude, longitude
    public event Action<string> OnGPSFailed; // error message

    #region Getter
    public float GetLatitude() => latitude;
    public float GetLongitude() => longitude;
    public bool IsGPSReady() => isGPSReady;
    #endregion

    public IEnumerator InitGPS()
    {
        isGPSReady = false;

        // 플랫폼 별 권한 요청
#if UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
            Permission.RequestUserPermission(Permission.FineLocation);

        while (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
            yield return null;
#endif

        // WebGL은 브라우저가 물어봄
#if UNITY_WEBGL
        WebGLInput.captureAllKeyboardInput = false;
#endif

        // iOS는 Info.plist만 있으면 자동 팝업

        // GPS 시작
        Input.location.Start();
        int waitCount = MAX_WAIT;
        while (Input.location.status == LocationServiceStatus.Initializing && waitCount > 0)
        {
            yield return new WaitForSeconds(1);
            waitCount--;
        }

        if (Input.location.status != LocationServiceStatus.Running)
        {
            string errorMessage = "GPS Failed or Disable.";
            Debug.LogWarning(errorMessage);
            OnGPSFailed?.Invoke(errorMessage);
            yield break;
        }

        latitude = Input.location.lastData.latitude;
        longitude = Input.location.lastData.longitude;
        isGPSReady = true;

        Debug.Log($"GPS Ready. Lat: {latitude}, Lon: {longitude}");
        OnGPSUpdated?.Invoke(latitude, longitude);
        Input.location.Stop();
    }
}
