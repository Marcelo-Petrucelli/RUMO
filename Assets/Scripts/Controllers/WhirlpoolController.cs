using NaughtyAttributes;
using UnityEngine;
using DG.Tweening;

public class WhirlpoolController : MonoBehaviour
{
    [SerializeField, BoxGroup("References")] public Animator animator;

    [SerializeField, BoxGroup("Config")] public float deadRadius = 0.15f;
    [SerializeField, BoxGroup("Config")] public Vector2 centerOffset = new (0f, 0f);
    [SerializeField, BoxGroup("Config")] public Vector2 whirlPoolMaxDragForce = new(0.3f, 0.3f);
    [SerializeField, BoxGroup("Config"), Range(0f, 1f)] public float speedMultiplier = 0.75f;

    [ShowNativeProperty] internal Vector2 Center => (this.transform.position + (Vector3) this.centerOffset);

    [ShowNonSerializedField] internal bool active = true;

    public void DisableFadeAndDestroy() {
        this.active = false;
        var spr = this.animator.GetComponent<SpriteRenderer>();
        
        spr.DOFade(0f, 0.3f).OnComplete(() => {
            Destroy(this.gameObject);
        });
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(this.active && collision.TryGetComponent<BoatController>(out var boat)) {
            if(boat.poolDraggin == null) {
                boat.poolDraggin = this;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if(collision.TryGetComponent<BoatController>(out var boat)) {
            if(boat.poolDraggin == this) {
                boat.poolDraggin = null;
            }
        }
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(this.Center, this.deadRadius);
    }
}
