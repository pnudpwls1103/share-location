using Fusion;
using UnityEngine;

public class PlayerColor : NetworkBehaviour
{
    public MeshRenderer MeshRenderer;

    [Networked, OnChangedRender(nameof(ColorChanged))] public Color NetworkedColor { get; set; }

    void Update()
    {
        if (HasStateAuthority && Input.GetKeyDown(KeyCode.E))
        {
            ChangeOtherPlayersColor();
        }
    }

    void ChangeOtherPlayersColor()
    {
        var allPlayers = Runner.GetAllNetworkObjects();
        foreach (var player in allPlayers)
        {
            if (!player.HasStateAuthority)
            {
                var playerColor = player.GetComponent<PlayerColor>();
                if (playerColor != null)
                {
                    playerColor.ChangeColorRpc();
                }
            }
        }
    }

    public void ColorChanged()
    {
        MeshRenderer.material.color = NetworkedColor;
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void ChangeColorRpc()
    {
        NetworkedColor = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1f);
    }
}