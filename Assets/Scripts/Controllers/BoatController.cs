using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class BoatController : MonoBehaviour
{
    private Animator anim;
    [SerializeField] public float acceleration = 0f;
    [SerializeField] public float deceleration = 0f;
    [SerializeField] public float maxSpeed = 0f;
    [SerializeField] public List<Transform> bubblePivots;

    [ShowNonSerializedField] private float speed = 0f;
    [ShowNonSerializedField] private bool left = false;
    [ShowNonSerializedField] private bool right = true;
    [ShowNonSerializedField] private bool up = false;
    [ShowNonSerializedField] private bool down = false;
    [ShowNonSerializedField] internal bool jammed = false;
    [ShowNativeProperty] private int MayPopListSize => this.mayPopBubble.Count;

    private List<BubbleController> mayPopBubble = new List<BubbleController>();

    // Start is called before the first frame update
    void Start() {
        this.anim = this.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update() {
        this.Move();
        this.CheckPop();
    }

    private void CheckPop() {
        if(this.jammed) {
            return;
        }

        if(this.MayPopListSize > 0 && Input.GetKeyDown(KeyCode.Space)) {
            this.jammed = true;
            this.mayPopBubble[0].Pop();
            this.mayPopBubble.RemoveAt(0);
        }
    }

    private void Move() {
        if(this.jammed) {
            return;
        }

        var moving = Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow);
        this.anim.SetBool("Moving", moving);

        if(Input.GetKeyDown(KeyCode.LeftArrow) && !this.left) {
            this.left = true;
            this.right = this.up = this.down = false;
            this.speed = 0f;

            this.anim.SetTrigger("Left");
            this.anim.ResetTrigger("Right");
            this.anim.ResetTrigger("Up");
            this.anim.ResetTrigger("Down");
        } else if(Input.GetKeyDown(KeyCode.RightArrow) && !this.right) {
            this.right = true;
            this.left = this.up = this.down = false;
            this.speed = 0f;

            this.anim.SetTrigger("Right");
            this.anim.ResetTrigger("Left");
            this.anim.ResetTrigger("Up");
            this.anim.ResetTrigger("Down");
        } else if(Input.GetKeyDown(KeyCode.UpArrow) && !this.up) {
            this.up = true;
            this.left = this.right = this.down = false;
            this.speed = 0f;

            this.anim.SetTrigger("Up");
            this.anim.ResetTrigger("Left");
            this.anim.ResetTrigger("Right");
            this.anim.ResetTrigger("Down");
        } else if(Input.GetKeyDown(KeyCode.DownArrow) && !this.down) {
            this.down = true;
            this.left = this.right = this.up = false;
            this.speed = 0f;

            this.anim.SetTrigger("Down");
            this.anim.ResetTrigger("Left");
            this.anim.ResetTrigger("Right");
            this.anim.ResetTrigger("Up");
        }

        if(moving) {
            if(Mathf.Abs(this.speed) < this.maxSpeed) { //Not at Max Speed
                this.speed += (this.left || this.down ? -1 : 1) * this.acceleration * Time.deltaTime;
            }
        } else {
            if(
                ((this.left || this.down) && this.speed < 0) ||
                ((this.right || this.up) && this.speed > 0))
            {
                this.speed += (this.left || this.down ? 1 : -1) * this.deceleration * Time.deltaTime;
            } else {
                this.speed = 0;
            }
        }

        if(this.speed != 0) {
            this.transform.position += new Vector3(
                this.left || this.right ? this.speed : 0f,
                this.up || this.down ? this.speed : 0f, 
                0f
            );
        }
    }

    public Vector3 GetBubblePivot() {
        if(this.up) {
            return this.bubblePivots[0].position;
        } else if(this.right) {
            return this.bubblePivots[1].position;
        } else if(this.down) {
            return this.bubblePivots[2].position;
        } else { //Left
            return this.bubblePivots[3].position;
        }
    }

    //Upon collision with another GameObject, this GameObject will reverse direction
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.transform.parent.TryGetComponent<BubbleController>(out var bubble)) {
            this.mayPopBubble.Add(bubble);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.transform.parent.TryGetComponent<BubbleController>(out var bubble)) {
            this.mayPopBubble.Remove(bubble);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        foreach(Transform t in bubblePivots) {
            Gizmos.DrawWireSphere(t.position, 0.3f);
        }
    }
}
