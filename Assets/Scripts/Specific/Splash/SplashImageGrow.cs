using UnityEngine.UI;
using UnityEngine;
using DG.Tweening;
using NaughtyAttributes;

public class SplashImageGrow : MonoBehaviour
{
    [SerializeField] Ease easy = Ease.Linear;
    [SerializeField, MinMaxSlider(0f, 10f)] Vector2 xVariation = new(0.8f, 1.2f);
    [SerializeField, MinMaxSlider(0f, 10f)] Vector2 yVariation = new(0.8f, 1.2f);
    [SerializeField, Range(0f, 10f)] float duration = 0.8f;
    
    private Image toGrow;
    private Tween growingTween;

    private void Awake()
    {
        this.toGrow = this.GetComponent<Image>();
        if(this.toGrow != null) {
            this.toGrow.rectTransform.localScale = new Vector2(this.xVariation.x, this.yVariation.x);
        }
    }
    void Start() {
        if(this.toGrow != null){
            this.growingTween = this.toGrow.rectTransform.DOScale(new Vector2(this.xVariation.y, this.yVariation.y), this.duration).SetEase(this.easy);
        }
    }

    private void OnDestroy() => this.growingTween?.Kill();
}
