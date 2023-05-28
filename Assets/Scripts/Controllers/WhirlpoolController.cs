using NaughtyAttributes;
using UnityEngine;

public class WhirlpoolController : MonoBehaviour
{
    [SerializeField] public bool active = true;
    [SerializeField] public Vector2 centerOffset = new (0f, 0f);
    [SerializeField] public float deadRadius = 0.15f;

    [ShowNativeProperty] internal Vector3 Center => this.transform.position + (Vector3) this.centerOffset;

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
