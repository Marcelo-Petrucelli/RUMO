using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine;
using TMPro;
using System;
using Unity.VisualScripting;

public class ItemHUDController : MonoBehaviour
{
    [SerializeField, BoxGroup("References")] public GameObject itemPrefab;
    [SerializeField, BoxGroup("References")] public RectTransform screenItemFrame;
    [SerializeField, BoxGroup("References")] public RectTransform itemMessagesParent;
    [SerializeField, BoxGroup("References")] public RectTransform slotsParent;

    //[SerializeField, BoxGroup("AnimationConfig")] private float xVariation = -300f;
    [SerializeField, BoxGroup("AnimationConfig")] private float itemMultiplier = 1.3f;
    [SerializeField, BoxGroup("AnimationConfig")] private float frameAnimationDuration = 0.3f;
    [SerializeField, BoxGroup("AnimationConfig")] private float waitAnimationDuration = 1f;
    [SerializeField, BoxGroup("AnimationConfig")] private float itemAnimationDuration = 0.3f;

    [SerializeField, ReadOnly, ReorderableList, BoxGroup("Debug")] private List<RectTransform> slots;
    [SerializeField, ReorderableList, ReadOnly, BoxGroup("Debug")] internal List<TextMeshProUGUI> itemMessages = new();
    [ShowNonSerializedField, BoxGroup("Debug")] internal int currentItemIndex = -1;

    void Start()
    {
        this.itemMessages = new List<TextMeshProUGUI>(this.itemMessagesParent.GetComponentsInChildren<TextMeshProUGUI>(true));
        this.slots = new List<RectTransform>(this.slotsParent.GetComponentsInChildren<RectTransform>(true));
        this.slots.Remove(this.slotsParent);
        this.slots.Reverse();

        this.screenItemFrame.localScale = Vector3.zero;
        this.screenItemFrame.gameObject.SetActive(false);
    }

    void Update() {

    }

    public void SpawnAndShowItem() {
        if(this.currentItemIndex >= this.slots.Count) {
            print("Request to Spawn an item on with index " + this.currentItemIndex + " but there are only " + this.slots.Count + " slots...");
            return;
        }

        this.currentItemIndex++;

        var msg = this.itemMessages[this.currentItemIndex];
        var originalColor = new Color(msg.color.r, msg.color.g, msg.color.b, 1);
        var transp = originalColor - Color.black;

        var newItem = Instantiate(this.itemPrefab, this.screenItemFrame.transform, false);
        var item = newItem.GetComponent<RectTransform>();
        var image = newItem.GetComponent<Image>();

        image.sprite = LevelManager.currentInstance.itemSprites[this.currentItemIndex];
        item.GetComponent<Image>().SetNativeSize();

        msg.color = transp;
        msg.gameObject.SetActive(true);
        this.screenItemFrame.gameObject.SetActive(true);

        var sequence = DOTween.Sequence();
        sequence.Append(this.screenItemFrame.DOScale(Vector3.one, this.frameAnimationDuration));
        sequence.Join(msg.DOColor(originalColor, this.frameAnimationDuration));
        sequence.AppendInterval(this.waitAnimationDuration);

        LevelManager.currentInstance.ItemWaitForInput();
    }

    public void FromFrameToSlot() {
        var msg = this.itemMessages[this.currentItemIndex];
        var transp = msg.color - Color.black;
        var item = this.screenItemFrame.GetComponentsInChildren<RectTransform>(true)[1];

        item.SetParent(this.slots[this.currentItemIndex], true);

        var frameSequence = DOTween.Sequence();
        frameSequence.Append(item.DOScale(new Vector3(
            this.slots[0].rect.width * this.itemMultiplier / item.rect.width,
            this.slots[0].rect.height * this.itemMultiplier / item.rect.height,
            1
        ), this.frameAnimationDuration));
        frameSequence.Join(this.screenItemFrame.DOScale(Vector3.zero, this.frameAnimationDuration));
        frameSequence.Join(msg.DOColor(transp, this.frameAnimationDuration)).OnComplete(() => {
            this.screenItemFrame.gameObject.SetActive(false);
            msg.gameObject.SetActive(false);
        });

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
