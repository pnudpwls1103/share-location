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

### Shared Mode Basics - 네트워크 속성
**링크**: [https://doc.photonengine.com/ko-kr/fusion/current/tutorials/shared-mode-basics/4-network-properties](https://doc.photonengine.com/ko-kr/fusion/current/tutorials/shared-mode-basics/4-network-properties)  
**날짜**: 2024-12-19  
**카테고리**: 네트워크 속성, 데이터 동기화, StateAuthority  
**핵심 내용**:
- **네트워크 속성 기본 개념**:
  - `NetworkTransform`은 위치만 동기화, 다른 변수는 `[Networked]` 속성 필요
  - `[Networked]` 속성: `StateAuthority`에서 다른 모든 클라이언트로 상태 동기화
  - 네트워크 속성은 **속성(property)**이어야 함 - 일반 필드는 지원 안 됨 (`{get; set;}` 필수)
- **StateAuthority 규칙**:
  - `StateAuthority`가 없는 객체의 네트워크 속성 변경 시 로컬 예측으로만 적용되고 재정의될 수 있음
  - 모든 클라이언트에 동기화하려면 **반드시 `StateAuthority`에서만 업데이트**
- **변경 감지 및 렌더링**:
  - `OnChangedRender` 속성: 각 렌더 프레임(Unity Update)에서 변경 감지
  - 네트워크 속성 변경 시 자동으로 콜백 함수 호출
  - 예제: 플레이어 색상 변경 시 모든 클라이언트에 반영

**참고 코드/예제**:
```csharp
using Fusion;
using UnityEngine;

public class PlayerColor : NetworkBehaviour
{
    public MeshRenderer MeshRenderer;
    
    [Networked, OnChangedRender(nameof(ColorChanged))]
    public Color NetworkedColor { get; set; }
    
    void Update()
    {
        if (HasStateAuthority && Input.GetKeyDown(KeyCode.E))
        {
            // StateAuthority에서만 변경해야 모든 클라이언트에 동기화됨
            NetworkedColor = new Color(
                Random.Range(0f, 1f), 
                Random.Range(0f, 1f), 
                Random.Range(0f, 1f), 
                1f
            );
        }
    }
    
    void ColorChanged()
    {
        // 네트워크 속성 변경 시 자동 호출됨
        MeshRenderer.material.color = NetworkedColor;
    }
}
```

**추가 메모**:
- 네트워크 속성은 반드시 `{get; set;}` 형태의 property여야 함
- `StateAuthority`가 아닌 클라이언트에서 변경하면 로컬 예측으로만 적용되고 나중에 덮어씌워질 수 있음
- `OnChangedRender`는 Unity Update 주기에서 변경 감지 (렌더링 프레임)
- 직접 Material 색상을 변경하면 안 됨 - 네트워크 속성을 변경하고 콜백에서 렌더링 업데이트

---

### Shared Mode Basics - 원격 프로시저 호출 (RPC)
**링크**: [https://doc.photonengine.com/ko-kr/fusion/current/tutorials/shared-mode-basics/5-remote-procedure-calls](https://doc.photonengine.com/ko-kr/fusion/current/tutorials/shared-mode-basics/5-remote-procedure-calls)  
**날짜**: 2024-12-19  
**카테고리**: RPC, 원격 호출, 네트워크 상호작용  
**핵심 내용**:
- **RPC 기본 개념**:
  - 다른 플레이어의 네트워크 속성을 수정하려면 RPC 사용 필요
  - `StateAuthority`가 없는 객체는 직접 네트워크 속성 변경 불가
  - RPC는 특정 클라이언트에서 함수를 실행하도록 요청하는 메커니즘
- **RPC 소스와 타겟 설정**:
  - `RpcSources`: RPC를 호출할 수 있는 클라이언트 지정
    - `All`: 누구나 호출 가능 (기본값은 `InputAuthority`만 가능)
    - `InputAuthority`: 객체를 제어하는 클라이언트만 호출 가능
  - `RpcTargets`: RPC를 수신할 클라이언트 지정
    - `StateAuthority`: 상태 권한을 가진 클라이언트에서만 실행
    - `All`: 모든 클라이언트에서 실행
- **RPC 실행 위치**:
  - RPC 내부 코드는 `RpcTarget` 클라이언트에서 실행됨
  - `RpcTargets.StateAuthority`로 설정하면 StateAuthority에서만 실행되어 네트워크 속성 수정 가능
- **사용 사례**:
  - 다른 플레이어에게 피해 입히기 (가장 일반적)
  - 플레이어 간 메시지, 이모티콘 등 휘발성 상호작용
  - 게임 시작, 준비 상태 등 게임 플로우 제어

**참고 코드/예제**:
```csharp
// Health 컴포넌트 - 네트워크 속성과 RPC
using Fusion;
using UnityEngine;

public class Health : NetworkBehaviour
{
    [Networked, OnChangedRender(nameof(HealthChanged))]
    public float NetworkedHealth { get; set; } = 100;
    
    void HealthChanged()
    {
        Debug.Log($"Health changed to: {NetworkedHealth}");
    }
    
    // RPC: 모든 클라이언트에서 호출 가능, StateAuthority에서만 실행
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void DealDamageRpc(float damage)
    {
        // 이 코드는 StateAuthority 클라이언트에서만 실행됨
        // 따라서 네트워크 속성을 안전하게 수정할 수 있음
        NetworkedHealth -= damage;
    }
}

// RaycastAttack - 레이캐스트로 공격
public class RaycastAttack : NetworkBehaviour
{
    public float Damage = 10;
    public PlayerMovement PlayerMovement;
    
    void Update()
    {
        if (HasStateAuthority == false) return;
        
        Ray ray = PlayerMovement.Camera.ScreenPointToRay(Input.mousePosition);
        ray.origin += PlayerMovement.Camera.transform.forward;
        
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            Debug.DrawRay(ray.origin, ray.direction, Color.red, 1f);
            
            // Physics Raycast로 타겟 찾기
            if (Runner.GetPhysicsScene().Raycast(ray.origin, ray.direction, out var hit))
            {
                if (hit.transform.TryGetComponent<Health>(out var health))
                {
                    // RPC 호출로 다른 플레이어에게 피해 입히기
                    health.DealDamageRpc(Damage);
                }
            }
        }
    }
}
```

**추가 메모**:
- RPC는 `[Rpc]` 속성으로 표시된 메서드
- 기본적으로 `InputAuthority`만 RPC 호출 가능, `RpcSources.All`로 모든 클라이언트에서 호출 가능하게 설정
- `RpcTargets.StateAuthority`는 네트워크 속성을 수정해야 할 때 필수
- RPC 내부 코드는 타겟 클라이언트에서 실행되므로 StateAuthority의 네트워크 속성을 안전하게 수정 가능
- 대부분의 경우 네트워크 속성과 변경 감지기만으로도 충분하지만, 다른 플레이어의 상태를 수정할 때는 RPC 필요
- `Runner.GetPhysicsScene().Raycast()` 사용: Fusion의 물리 시뮬레이션과 동기화된 레이캐스트

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

