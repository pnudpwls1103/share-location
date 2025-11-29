using UnityEngine;
using UnityEngine.Android;
using System.Collections;

public class GPSController : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(InitGPS());
    }

    public IEnumerator InitGPS()
    {
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
        Input.location.Start(1f, 1f);

        // 초기화 기다리기
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (Input.location.status != LocationServiceStatus.Running)
        {
            Debug.LogWarning("GPS Failed or Disabled");
            yield break;
        }

        Debug.Log($"GPS Ready. Lat: {Input.location.lastData.latitude}, Lon: {Input.location.lastData.longitude}");
    }
}
