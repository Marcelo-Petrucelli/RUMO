using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;

public class PauseMenuController: MonoBehaviour
{
    [SerializeField, BoxGroup("Config")] private float finalY = 0f;
    [SerializeField, BoxGroup("Config")] private float animationDuration = 1f;

    private float initialY;
    private RectTransform myRect;
    private Tween animation;

    private void Start()
    {
        this.myRect = GetComponent<RectTransform>();
        this.initialY = this.myRect.anchoredPosition.y;
    }

    [Button]
    public void OpenMenu() {
        this.animation?.Kill();
        this.animation = this.myRect.DOAnchorPosY(this.finalY, this.animationDuration, true).SetUpdate(UpdateType.Normal, true);
    }

    [Button]
    public void CloseMenu() {
        this.animation?.Kill();
        this.animation = this.myRect.DOAnchorPosY(this.initialY, this.animationDuration, true).SetUpdate(UpdateType.Normal, true);
    }
}
