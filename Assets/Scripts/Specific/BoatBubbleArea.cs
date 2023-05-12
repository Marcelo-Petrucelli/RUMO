using System.Collections.Generic;
using System.Collections;
using UnityEngine.Events;
using UnityEngine;
using Unity.VisualScripting;

public class BoatBubbleArea : MonoBehaviour
{
    public UnityEvent<Collider2D> onTriggerEnter;
    public UnityEvent<Collider2D> onTriggerStay;
    public UnityEvent<Collider2D> onTriggerExit;

    private void OnTriggerStay2D(Collider2D collision) => this.onTriggerStay?.Invoke(collision);

    private void OnTriggerEnter2D(Collider2D collision)  => this.onTriggerEnter?.Invoke(collision);

    private void OnTriggerExit2D(Collider2D collision) => this.onTriggerExit?.Invoke(collision);
}
