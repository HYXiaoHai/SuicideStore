using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueInterac : BaseInteractable
{
    public RowManage level3mange;
    public SpriteRenderer spriteRenderer;
    public Transform player;
    public Transform level3Start;
    public override void OnInteract()
    {
        spriteRenderer.DOFade(1f, 0.5f).OnComplete(() => {
            player.DOMove(level3Start.position, 1f).OnComplete(() => { level3mange.BeginGame(); });
        });
    }
}
