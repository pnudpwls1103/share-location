using Fusion;
using UnityEngine;

public class PlayerColor : NetworkBehaviour
{
    public MeshRenderer MeshRenderer;

    [Networked, OnChangedRender(nameof(ColorChanged))] public Color NetworkedColor { get; set; }

    void Update()
    {
        if (HasInputAuthority && Input.GetKeyDown(KeyCode.E))
        {
            ChangeColorRpc();
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void ChangeColorRpc()
    {
        NetworkedColor = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1f);
    }

    void ColorChanged()
    {
        MeshRenderer.material.color = NetworkedColor;
    }
}