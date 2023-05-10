using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using DG.Tweening;
using UnityEngine;

public class ItemHUDController : MonoBehaviour
{
    [SerializeField, BoxGroup("References")] public GameObject itemPrefab;
    [SerializeField, BoxGroup("References")] public GameObject screenItemFrame;
    [SerializeField, BoxGroup("Variation")] private float itemAnimationDuration;
    [SerializeField, BoxGroup("Variation")] private float xVariation;

    [SerializeField, ReadOnly, ReorderableList, BoxGroup("Debug")] private List<RectTransform> slots;

    void Start()
    {
        this.slots = new List<RectTransform>(this.transform.GetComponentsInChildren<RectTransform>());
        this.slots.Remove(this.GetComponent<RectTransform>());
        this.slots.Reverse();
    }

    public void SpawnAndMoveToIventory(int slotIndex) { //Vector2 initialPosition, 
        /*print(initialPosition);
        var newItem = Instantiate(this.itemPrefab, this.transform.parent, false);
        var item = newItem.GetComponent<RectTransform>();

        item.localPosition = initialPosition;
        item.localRotation = Quaternion.identity;
        item.localScale = Vector3.zero;

        item.SetParent(this.slots[slotIndex], true);

        var sequence = DOTween.Sequence();
        sequence.Append(item.DOScale(Vector3.one, 18f / 60f));
        sequence.AppendInterval(1f);
        sequence.Append(item.DOLocalMoveX(- this.xVariation, this.itemAnimationDuration / 2));
        sequence.Join(item.DOLocalMoveX(0, this.itemAnimationDuration / 2));
        sequence.Join(item.DOLocalMoveY(0, this.itemAnimationDuration));*/

        var newItem = Instantiate(this.itemPrefab, this.screenItemFrame.transform, false);
        var item = newItem.GetComponent<RectTransform>();

        item.localPosition = this.screenItemFrame.transform.position;
        item.localRotation = Quaternion.identity;
        item.localScale = Vector3.zero;

        item.SetParent(this.slots[slotIndex], true);

        var sequence = DOTween.Sequence();
        sequence.Append(item.DOScale(Vector3.one, 18f / 60f));
        sequence.AppendInterval(1f);
        sequence.Append(item.DOLocalMoveX(-this.xVariation, this.itemAnimationDuration / 2));
        sequence.Join(item.DOLocalMoveX(0, this.itemAnimationDuration / 2));
        sequence.Join(item.DOLocalMoveY(0, this.itemAnimationDuration));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
