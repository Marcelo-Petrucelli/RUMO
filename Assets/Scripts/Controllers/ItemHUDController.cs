using System.Collections.Generic;
using System.Collections;
using NaughtyAttributes;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine;
using TMPro;
using System;

[Serializable]
public class ItemOnHud
{
    [SerializeField] public Sprite sprite;
    [SerializeField] public TextMeshProUGUI text;
}
public class ItemHUDController : MonoBehaviour
{
    [SerializeField, BoxGroup("References")] public GameObject itemPrefab;
    [SerializeField, BoxGroup("References")] public RectTransform screenItemFrame;
    [SerializeField, BoxGroup("References")] public RectTransform slotsParent;
    [SerializeField, BoxGroup("References")] public RectTransform partSlotsParent;
    [SerializeField, BoxGroup("References")] public List<ItemOnHud> items;

    //[SerializeField, BoxGroup("AnimationConfig")] private float xVariation = -300f;
    [SerializeField, BoxGroup("AnimationConfig")] private float itemMultiplier = 1.3f;
    [SerializeField, BoxGroup("AnimationConfig")] private float frameAnimationDuration = 0.3f;
    [SerializeField, BoxGroup("AnimationConfig")] private float waitAnimationDuration = 1f;
    [SerializeField, BoxGroup("AnimationConfig")] private float itemAnimationDuration = 0.3f;
    [SerializeField, BoxGroup("AnimationConfig")] private float partItemWaitingDuration = 3f;
    [SerializeField, BoxGroup("AnimationConfig")] private float partItemFadeDuration = 0.4f;

    [SerializeField, ReadOnly, ReorderableList, BoxGroup("Debug")] private List<RectTransform> slots;
    [SerializeField, ReadOnly, BoxGroup("Debug")] private RectTransform partSlot;
    [ShowNonSerializedField, BoxGroup("Debug")] internal int currentItemIndex = -1;
    [ShowNonSerializedField, BoxGroup("Debug")] internal int currentSlotIndex = -1;
    [ShowNonSerializedField, BoxGroup("Debug")] internal bool nextIsPart = false;

    void Start()
    {
        this.slots = new List<RectTransform>(this.slotsParent.GetComponentsInChildren<RectTransform>(true));
        this.slots.Remove(this.slotsParent);
        //this.slots.Reverse();
        this.partSlot = this.partSlotsParent.GetComponentsInChildren<RectTransform>(true)[1];
        this.partSlotsParent.gameObject.SetActive(false);
        this.FadePart(true, true);

        this.screenItemFrame.localScale = Vector3.zero;
        this.screenItemFrame.gameObject.SetActive(false);
    }

    void Update() {

    }

    public void SpawnAndShowItem(bool isPartItem = false) {
        this.nextIsPart = isPartItem;
        this.currentItemIndex++;
        if(!isPartItem) {
            this.currentSlotIndex++;
        }

        var msg = this.items[this.currentItemIndex].text;

        var originalColor = Color.white;
        var transp = Color.white - Color.black;
        if(msg != null) {
            originalColor = new Color(msg.color.r, msg.color.g, msg.color.b, 1);
            transp = originalColor - Color.black;
        }

        var newItem = Instantiate(this.itemPrefab, this.screenItemFrame.transform, false);
        var item = newItem.GetComponent<RectTransform>();
        var image = newItem.GetComponent<Image>();

        image.sprite = this.items[this.currentItemIndex].sprite;
        item.GetComponent<Image>().SetNativeSize();

        if(msg != null) {
            msg.color = transp;
            msg.gameObject.SetActive(true);
        }
        this.screenItemFrame.gameObject.SetActive(true);

        var sequence = DOTween.Sequence();
        sequence.Append(this.screenItemFrame.DOScale(Vector3.one, this.frameAnimationDuration));
        if(msg != null) {
            sequence.Join(msg.DOColor(originalColor, this.frameAnimationDuration));
        }
        sequence.AppendInterval(this.waitAnimationDuration);

        LevelManager.currentInstance.ItemWaitForInput();
    }

    public void FromFrameToSlot() {
        var msg = this.items[this.currentItemIndex].text;
        var transp = Color.white - Color.black;
        if(msg != null) {
            transp = msg.color - Color.black;
        }
        var item = this.screenItemFrame.GetComponentsInChildren<RectTransform>(true)[1];

        var xPart = 1;
        var yPart = 1;
        if(this.nextIsPart) {
            item.SetParent(this.partSlot, true);
            yPart = 2;
            this.FadePart();
        } else {
            item.SetParent(this.slots[this.currentSlotIndex], true);
            xPart = 2;
        }

        var frameSequence = DOTween.Sequence();
        if(!this.nextIsPart) {
            frameSequence.Append(item.DOScale(new Vector3(
                this.slots[0].rect.width * this.itemMultiplier / item.rect.width,
                this.slots[0].rect.height * this.itemMultiplier / item.rect.height,
                1
            ), this.frameAnimationDuration));
        }
        frameSequence.Join(this.screenItemFrame.DOScale(Vector3.zero, this.frameAnimationDuration));
        if(msg != null) {
            frameSequence.Join(msg.DOColor(transp, this.frameAnimationDuration));
        }
        
        frameSequence.OnComplete(() => {
            this.screenItemFrame.gameObject.SetActive(false);
            if(msg != null) {
                msg.gameObject.SetActive(false);
            }
        });

        var itemSequence = DOTween.Sequence();
        itemSequence.AppendInterval(this.frameAnimationDuration / 3);
        itemSequence.Append(item.DOLocalMoveY(0, this.itemAnimationDuration / yPart, true)).SetEase(Ease.OutSine);
        itemSequence.Join(item.DOLocalMoveX(0, this.itemAnimationDuration / xPart).SetEase(Ease.OutSine));
        /*itemSequence.Join(item.DOLocalMoveX(this.xVariation, this.itemAnimationDuration / 2).SetEase(Ease.OutSine).OnComplete(() => {
            item.DOLocalMoveX(0, this.itemAnimationDuration / 2, true).SetEase(Ease.InSine);
        }));*/

        itemSequence.OnComplete(() => {
            item = null;
            if(this.nextIsPart) {
                this.ExecuteAfter(() => { this.FadePart(true); }, this.partItemWaitingDuration);
            }
            LevelManager.currentInstance.ItemObtained(this.currentItemIndex);
        });
    }

    private void FadePart(bool fadeOut = false, bool fadeInstant = false) {
        var parts = this.partSlotsParent.GetComponentsInChildren<Image>();

        if(!fadeOut) {
            this.partSlotsParent.gameObject.SetActive(true);
        }

        var doOnComplete = true;
        foreach(var part in parts) {
            var color = fadeOut ? part.color - Color.black : new Color(part.color.r, part.color.g, part.color.b, 1);
            var tween = part.DOColor(color, fadeInstant ? 0.01f : this.partItemFadeDuration);

            if(fadeOut && doOnComplete) {
                tween.OnComplete(() => { this.partSlotsParent.gameObject.SetActive(false); });
                doOnComplete = false;
            }
        }
    }

    private void ExecuteAfter(Action stuff, float waitingTime) {
        StartCoroutine(this.ExecuteAfterCR(stuff, waitingTime));
    }

    private IEnumerator ExecuteAfterCR(Action stuff, float waitingTime) {
        yield return new WaitForSeconds(waitingTime);
        stuff?.Invoke();
    }
}
