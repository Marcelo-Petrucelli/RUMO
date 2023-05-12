using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine;
using static UnityEditor.Progress;

public class ItemHUDController : MonoBehaviour
{
    [SerializeField, BoxGroup("References")] public GameObject itemPrefab;
    [SerializeField, BoxGroup("References")] public RectTransform screenItemFrame;

    //[SerializeField, BoxGroup("AnimationConfig")] private float xVariation = -300f;
    [SerializeField, BoxGroup("AnimationConfig")] private float itemMultiplier = 1.3f;
    [SerializeField, BoxGroup("AnimationConfig")] private float frameAnimationDuration = 0.3f;
    [SerializeField, BoxGroup("AnimationConfig")] private float waitAnimationDuration = 1f;
    [SerializeField, BoxGroup("AnimationConfig")] private float itemAnimationDuration = 0.3f;

    [SerializeField, ReadOnly, ReorderableList, BoxGroup("Debug")] private List<RectTransform> slots;
    [ShowNonSerializedField, BoxGroup("Debug")] internal int currentItemIndex = -1;

    void Start()
    {
        this.slots = new List<RectTransform>(this.transform.GetComponentsInChildren<RectTransform>());
        this.slots.Remove(this.GetComponent<RectTransform>());
        this.slots.Reverse();

        this.screenItemFrame.localScale = Vector3.zero;
        this.screenItemFrame.gameObject.SetActive(false);
    }

    void Update() {

    }

    public void SpawnAndMoveToIventory() {
        if(this.currentItemIndex >= this.slots.Count) {
            return;
        }

        this.currentItemIndex++;

        var newItem = Instantiate(this.itemPrefab, this.screenItemFrame.transform, false);
        var item = newItem.GetComponent<RectTransform>();
        var image = newItem.GetComponent<Image>();
        image.sprite = LevelManager.currentInstance.itemSprites[this.currentItemIndex];
        item.GetComponent<Image>().SetNativeSize();

        this.screenItemFrame.gameObject.SetActive(true);

        var sequence = DOTween.Sequence();
        sequence.Append(this.screenItemFrame.DOScale(Vector3.one, this.frameAnimationDuration));
        sequence.AppendInterval(this.waitAnimationDuration);
        sequence.OnComplete(() => this.FromFrameToSlot(item));
    }

    private void FromFrameToSlot(RectTransform item) {
        item.SetParent(this.slots[this.currentItemIndex], true);

        var frameSequence = DOTween.Sequence();
        frameSequence.Append(item.DOScale(new Vector3(
            this.slots[0].rect.width * this.itemMultiplier / item.rect.width,
            this.slots[0].rect.height * this.itemMultiplier / item.rect.height,
            1
        ), this.frameAnimationDuration));
        frameSequence.Join(this.screenItemFrame.DOScale(Vector3.zero, this.frameAnimationDuration));

        var itemSequence = DOTween.Sequence();
        itemSequence.AppendInterval(this.frameAnimationDuration / 3);
        itemSequence.Append(item.DOLocalMoveY(0, this.itemAnimationDuration, true));
        itemSequence.Join(item.DOLocalMoveX(0, this.itemAnimationDuration / 2).SetEase(Ease.OutSine));
        /*itemSequence.Join(item.DOLocalMoveX(this.xVariation, this.itemAnimationDuration / 2).SetEase(Ease.OutSine).OnComplete(() => {
            item.DOLocalMoveX(0, this.itemAnimationDuration / 2, true).SetEase(Ease.InSine);
        }));*/

        itemSequence.OnComplete(() => {
            item = null;
            LevelManager.currentInstance.ItemObtained(this.currentItemIndex);
        });
    }
}
