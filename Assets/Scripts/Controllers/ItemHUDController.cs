using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class ItemHUDController : MonoBehaviour
{
    [SerializeField, ReadOnly, ReorderableList] private List<RectTransform> slots;

    void Start()
    {
        this.slots = new List<RectTransform>(this.transform.GetComponentsInChildren<RectTransform>());
        this.slots.Remove(this.GetComponent<RectTransform>());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
