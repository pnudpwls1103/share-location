# 포톤(Photon) 문서 정리

이 문서는 포톤 문서를 따라하며 참고한 링크와 핵심 내용을 정리합니다.

---

## 문서 링크 목록

### Shared Mode Basics - 씬 그리고 플레이어
**링크**: [https://doc.photonengine.com/ko-kr/fusion/current/tutorials/shared-mode-basics/2-scene-and-player](https://doc.photonengine.com/ko-kr/fusion/current/tutorials/shared-mode-basics/2-scene-and-player)  
**날짜**: 2024-12-19  
**카테고리**: 씬 설정, 플레이어 스폰, 네트워킹 기본  
**핵심 내용**:
- **씬 설정**: `Prototype Runner`와 `Prototype Network Start` 추가로 Fusion 네트워킹 활성화
  - `Network Runner`: Fusion 시뮬레이션 실행 핵심 컴포넌트
  - `Prototype Network Start`: Fusion 룸 가입용 부트스트랩 GUI
- **플레이어 프리팹 구성**:
  - `NetworkObject`: 네트워크 ID 부여로 모든 피어가 참조 가능
  - `CharacterController`: 플레이어 이동 제어
  - `NetworkTransform`: 위치 자동 동기화
- **플레이어 스폰**: `IPlayerJoined` 인터페이스로 플레이어 참여 시 자동 스폰
  - `Runner.Spawn()`으로 생성하면 모든 클라이언트에 자동 복제
  - 로컬 플레이어만 스폰하면 됨 (다른 플레이어는 자동으로 복제됨)

**참고 코드/예제**:
```csharp
using Fusion;
using UnityEngine;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
{
    public GameObject PlayerPrefab;

    public void PlayerJoined(PlayerRef player)
    {
        if (player == Runner.LocalPlayer)
        {
            Runner.Spawn(PlayerPrefab, new Vector3(0, 1, 0), Quaternion.identity); 
        }
    }
}
```

**추가 메모**:
- `SimulationBehaviour`는 `NetworkRunner`에 접근하여 세션 정보를 얻을 수 있게 해줌
- `PlayerJoined`는 자신뿐만 아니라 다른 플레이어가 참여할 때도 호출됨
- 플레이어 프리팹은 프로젝트 창에 저장하고 씬에서는 삭제해야 함

---

### Shared Mode Basics - 이동 & 카메라
**링크**: [https://doc.photonengine.com/ko-kr/fusion/current/tutorials/shared-mode-basics/3-movement-and-camera](https://doc.photonengine.com/ko-kr/fusion/current/tutorials/shared-mode-basics/3-movement-and-camera)  
**날짜**: 2024-12-19  
**카테고리**: 플레이어 이동, 카메라, 네트워크 동기화  
**핵심 내용**:
- **플레이어 이동 구현**:
  - `NetworkBehaviour`를 상속받아 네트워크 동기화 가능하게 함
  - `FixedUpdateNetwork()` 사용: 네트워크 틱에 맞춰 실행되는 업데이트 메서드
  - `HasStateAuthority`: 로컬 플레이어만 이동 제어 (다른 플레이어는 자동 동기화)
  - `CharacterController`로 이동 처리
  - 점프는 `Update()`에서 입력 감지, `FixedUpdateNetwork()`에서 실행
- **카메라 설정**:
  - `FirstPersonCamera` 스크립트: 마우스로 회전하는 1인칭 카메라
  - `Spawned()` 메서드 사용: `NetworkObject` 초기화 시 호출 (Awake/Start 대신 사용 필수)
  - `HasStateAuthority`로 로컬 플레이어만 카메라 연결
  - 카메라 회전에 따라 이동 방향 조정 (카메라 Y축 회전만 사용)

**참고 코드/예제**:
```csharp
// PlayerMovement - 카메라 연결 및 이동 방향 조정
public Camera Camera;

public override void Spawned()
{
    if (HasStateAuthority)
    {
        Camera = Camera.main;
        Camera.GetComponent<FirstPersonCamera>().Target = transform;
    }
}

// 카메라 회전에 따른 이동
Quaternion cameraRotationY = Quaternion.Euler(0, Camera.transform.rotation.eulerAngles.y, 0);
Vector3 move = cameraRotationY * new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")) * Runner.DeltaTime * PlayerSpeed;

// FirstPersonCamera - 간단한 1인칭 카메라
public class FirstPersonCamera : MonoBehaviour
{
    public Transform Target;
    public float MouseSensitivity = 10f;
    
    private float verticalRotation;
    private float horizontalRotation;
    
    void LateUpdate()
    {
        if (Target == null) return;
        
        transform.position = Target.position;
        
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        
        verticalRotation -= mouseY * MouseSensitivity;
        verticalRotation = Mathf.Clamp(verticalRotation, -70f, 70f);
        horizontalRotation += mouseX * MouseSensitivity;
        
        transform.rotation = Quaternion.Euler(verticalRotation, horizontalRotation, 0);
    }
}
```

**추가 메모**:
- `NetworkObjects` 초기화는 반드시 `Spawned()` 사용 (Awake/Start에서는 NetworkObject 준비 안 됨)
- `HasStateAuthority`는 플레이어가 제어하는 객체에만 true (로컬 플레이어만)
- 멀티플레이어 테스트: 빌드 설정에서 Windowed 모드로 설정 후 빌드하여 여러 클라이언트 실행
- 카메라는 씬에 미리 배치하고 런타임에 로컬 플레이어에게 연결하는 방식 권장

---

## 템플릿

새로운 링크를 추가할 때 아래 템플릿을 사용하세요:

```markdown
### [링크 제목]
**링크**: [URL](링크주소)  
**날짜**: YYYY-MM-DD  
**카테고리**: 
**핵심 내용**:
- 
- 

**참고 코드/예제**:
```csharp
// 코드 예제
```

**추가 메모**:
- 
```

---

