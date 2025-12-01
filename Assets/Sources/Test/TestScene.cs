using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class TestScene : MonoBehaviour
{
    [SerializeField] private Button[] colorChangeButtons;
    [SerializeField] private PlayerSpawner playerSpawner;
    [SerializeField] private NetworkRunner networkRunner;

    private void Start()
    {
        colorChangeButtons[0].onClick.AddListener(() => ChangeColor(0));
        colorChangeButtons[1].onClick.AddListener(() => ChangeColor(1));
    }

    private void ChangeColor(int index)
    {
        bool isLocalPlayer = index == 0;
        if (isLocalPlayer)
        {
            var playerObject = playerSpawner.PlayerObjects[networkRunner.LocalPlayer];
            if (playerObject)
            {
                playerObject.GetComponent<PlayerColor>()?.ChangeColorRpc();
            }
        }
        else
        {
            foreach (var player in playerSpawner.PlayerObjects)
            {
                if (player.Key != networkRunner.LocalPlayer)
                {
                    var playerObject = player.Value;
                    if (playerObject)
                    {
                        playerObject.GetComponent<PlayerColor>()?.ChangeColorRpc();
                    }
                }
            }
        }
    }
}
