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
    [ShowNonSerializedField] internal bool chasing;
    private BoatController player;

    private List<string> BubbleTypes => LevelManager.itemSpritesNames;

    private void Start()
    {
        if(active) {
            this.player = LevelManager.currentInstance.Player;
        }
    }

    private void FixedUpdate()
    {
        if(this.active) {
            var dist = Vector3.Distance(this.transform.position, this.player.transform.position);
            if(dist < this.minDistToFollow && this.player.MayBeBubbleChasedBy(this)) {
                if(!this.chasing) {
                    this.chasing = true;
                    this.player.TryAddToChasing(this);
                }
                this.transform.position = Vector3.MoveTowards(this.transform.position, this.player.GetBubblePivot(), this.maxMoveDelta * Time.fixedDeltaTime);
            } else if(this.chasing){
                this.player.TryRemoveFromChasing(this);
                this.chasing = false;
            }
        }
    }

    public void Pop(bool fake = false) {
        if(this.active) {
            this.active = false;
            var popEmitter = this.GetComponent<FMODUnity.StudioEventEmitter>();
            popEmitter.Play();
            this.bubbleInner.GetComponent<Animator>().SetTrigger("Explode");
            if(!fake) {
                this.ShowItem(); //this.Invoke(nameof(ShowItem), 0.5f);
            }
        }
    }

    private void ShowItem() => LevelManager.currentInstance.ObtainNextItem();

    public void DestroySelf() => Destroy(this.gameObject);

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(this.transform.position, this.minDistToFollow);
    }
}
