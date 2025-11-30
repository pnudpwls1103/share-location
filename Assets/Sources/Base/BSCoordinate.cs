using UnityEngine;

public static class BSCoordinate
{
    /// <summary>
    /// GPS 좌표(위도/경도)를 Unity 3D 구 좌표로 변환
    /// </summary>
    /// <param name="latitude">위도 (도 단위, -90 ~ 90)</param>
    /// <param name="longitude">경도 (도 단위, -180 ~ 180)</param>
    /// <param name="radius">구의 반지름</param>
    /// <returns>Unity 3D 월드 좌표 (Vector3)</returns>
    public static Vector3 LatLonToWorld(float latitude, float longitude, float radius)
    {
        // 도를 라디안으로 변환
        float latRad = latitude * Mathf.Deg2Rad;
        float lonRad = longitude * Mathf.Deg2Rad;

        // 구 좌표 변환
        // X: 동서 방향 (경도)
        // Y: 남북 방향 (위도)
        // Z: 앞뒤 방향
        float x = radius * Mathf.Cos(latRad) * Mathf.Sin(lonRad);
        float y = radius * Mathf.Sin(latRad);
        float z = radius * Mathf.Cos(latRad) * Mathf.Cos(lonRad);

        return new Vector3(x, y, z);
    }

    /// <summary>
    /// Unity 3D 구 좌표를 GPS 좌표(위도/경도)로 변환
    /// </summary>
    /// <param name="worldPosition">Unity 3D 월드 좌표</param>
    /// <param name="radius">구의 반지름</param>
    /// <returns>위도와 경도 (도 단위)</returns>
    public static Vector2 WorldToLatLon(Vector3 worldPosition, float radius)
    {
        // 정규화
        Vector3 normalized = worldPosition.normalized;

        // 위도 계산 (Y축 기준)
        float latitude = Mathf.Asin(normalized.y) * Mathf.Rad2Deg;

        // 경도 계산 (X, Z축 기준)
        float longitude = Mathf.Atan2(normalized.x, normalized.z) * Mathf.Rad2Deg;

        return new Vector2(latitude, longitude);
    }
}

