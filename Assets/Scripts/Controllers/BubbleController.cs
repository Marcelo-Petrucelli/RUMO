using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class BubbleController : MonoBehaviour
{
    [SerializeField, Dropdown("BubbleTypes")] public string type;
    [SerializeField] public bool active;
    [SerializeField] public float maxMoveDelta;
    [SerializeField] public float minDistToFollow;
    [SerializeField] public Transform bubbleInner;
    private BoatController player;

    private List<string> BubbleTypes => LevelManager.itemSpritesNames;

    private void Start()
    {
        if(active) {
            this.player = LevelManager.currentInstance.Player;
        }
    }

    private void Update()
    {
        if(this.active) {
            var dist = Vector3.Distance(this.transform.position, this.player.transform.position);
            if(dist < this.minDistToFollow ) {
                this.transform.position = Vector3.MoveTowards(this.transform.position, this.player.GetBubblePivot(), this.maxMoveDelta * Time.deltaTime);
            }
        }
    }

    public void Pop() {
        this.bubbleInner.GetComponent<Animator>().SetTrigger("Explode");
        this.ShowItem(); //this.Invoke(nameof(ShowItem), 0.5f);
    }

    private void ShowItem() => LevelManager.currentInstance.ObtainNextItem();

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(this.transform.position, this.minDistToFollow);
    }
}
