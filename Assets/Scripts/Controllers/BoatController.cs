using System.Collections.Generic;
using System.Collections;
using NaughtyAttributes;
using UnityEngine;
using FMODUnity;
using System;
using Unity.VisualScripting;
using DG.Tweening;

public class BoatController : MonoBehaviour
{
    [SerializeField, BoxGroup("Config")] public float acceleration = 0f;
    [SerializeField, BoxGroup("Config")] public float deceleration = 0f;
    [SerializeField, BoxGroup("Config")] public float maxSpeed = 0f;
    [SerializeField, BoxGroup("Config")] public int maxChasingBubbles = 1;
    [SerializeField, BoxGroup("Config")] public float whaleTrollDistance = 7f;
    [SerializeField, BoxGroup("Config")] public float warningFadeInOutTime = 0.5f;
    [SerializeField, BoxGroup("Config")] public float warningFadeInterval = 4f;

    [SerializeField, BoxGroup("References")] public Animator trails;
    [SerializeField, BoxGroup("References")] public List<Transform> bubblePivots;
    [SerializeField, BoxGroup("References")] public Transform whalePivots;
    [SerializeField, BoxGroup("References")] public GameObject warning;
    [SerializeField, BoxGroup("Vertical Colider Config")] private Vector2 verticalSize = new Vector2(0.5f, 1.5f);
    [SerializeField, BoxGroup("Vertical Colider Config")] private Vector2 verticalOffset = new Vector2(0.025f, 0f);
    [SerializeField, BoxGroup("Horizontal Colider Config")] private Vector2 horizontalSize = new Vector2(2f, 0.4583282f);
    [SerializeField, BoxGroup("Horizontal Colider Config")] private Vector2 horizontalOffset = new Vector2(-0.01640372f, -0.3568905f);

    [ShowNonSerializedField] private float speed = 0f;
    [ShowNonSerializedField] private bool moving = false;
    [ShowNonSerializedField] private bool whaleTime = false;

    [ShowNonSerializedField] internal bool left = false;
    [ShowNonSerializedField] internal bool right = true;
    [ShowNonSerializedField] internal bool up = false;
    [ShowNonSerializedField] internal bool down = false;
    [ShowNonSerializedField] internal bool jammed = false;
    [ShowNonSerializedField] internal bool chaseBlocked = false;

    [ShowNativeProperty] internal int MayPopListSize => this.mayPopBubbles.Count;

    [ShowNativeProperty] internal int ChaseListSize => this.chasingBubbles.Count;
    internal bool MayBeBubbleChasedBy(BubbleController bubble) => (this.chasingBubbles.Count < this.maxChasingBubbles || this.chasingBubbles.Contains(bubble)) && !this.chaseBlocked;

    private Animator anim;
    private Rigidbody2D boatBody;
    private CapsuleCollider2D capsuleCollider;
    private List<FMODUnity.StudioEventEmitter> soundEmitter;
    private List<BubbleController> mayPopBubbles = new List<BubbleController>();
    internal List<BubbleController> chasingBubbles = new List<BubbleController>();

    // Start is called before the first frame update
    void Start() {
        this.anim = this.GetComponent<Animator>();
        this.boatBody = this.GetComponent<Rigidbody2D>();
        this.capsuleCollider = this.GetComponent<CapsuleCollider2D>();
        this.warning.gameObject.SetActive(false);
        this.soundEmitter = new List<FMODUnity.StudioEventEmitter>(this.GetComponents<FMODUnity.StudioEventEmitter>());
        this.capsuleCollider.direction = CapsuleDirection2D.Horizontal;
        this.capsuleCollider.size = horizontalSize;        
        this.capsuleCollider.offset = horizontalOffset;
    }

    // Update is called once per frame
    void Update() {
        this.CheckWhale();
        this.CheckPop();
    }

    void FixedUpdate()
    {
        this.Move();
    }

    private void CheckWhale() {
        if(this.whaleTime) {
            foreach(var b in this.chasingBubbles) {
                if(Vector2.Distance(b.transform.position, this.transform.position) < this.whaleTrollDistance) {
                    this.jammed = true;
                    LevelManager.currentInstance.WhaleItAllUp();
                    this.whaleTime = false;
                    break;
                }
            }
        }
    }

    private void CheckPop() {
        if(this.jammed) {
            return;
        }

        if(this.MayPopListSize > 0 && Input.GetKeyDown(KeyCode.Space) && !LevelManager.currentInstance.showingText) {
            //this.jammed = true; //Will be set by LevelManager.ObtainNextItem()
            this.speed = 0;
            this.mayPopBubbles[0].Pop();
            this.chasingBubbles.RemoveAt(0);
            this.mayPopBubbles.RemoveAt(0);
        }
    }

    public void RemoveBubbleReferences(BubbleController bubble) {
        this.chasingBubbles.Remove(bubble);
        this.mayPopBubbles.Remove(bubble);
    }

    private void Move() {
        if(this.jammed) {
            return;
        }

        this.moving = (Input.GetKey(KeyCode.LeftArrow) && this.left)      || 
                        (Input.GetKey(KeyCode.RightArrow) && this.right) || 
                        (Input.GetKey(KeyCode.UpArrow) && this.up)       || 
                        (Input.GetKey(KeyCode.DownArrow) && this.down);
        this.anim.SetBool("Moving", this.moving);

        if(Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow)) {
            this.soundEmitter[0].SetParameter("Push button", 0.0f);
            if(!this.soundEmitter[0].IsPlaying()) {
                this.soundEmitter[0].Play();
            }
        }

        if(this.moving) {
            if(this.right) {                
                this.trails.SetTrigger("Horizontal");                
                this.trails.ResetTrigger("Idle");
                this.trails.ResetTrigger("Vertical");
            } else if(this.left) {                
                this.trails.SetTrigger("Horizontal");
                this.trails.ResetTrigger("Idle");
                this.trails.ResetTrigger("Vertical");
            } else if(this.up) {                
                this.trails.SetTrigger("Vertical");
                this.trails.ResetTrigger("Idle");
                this.trails.ResetTrigger("Horizontal");
            } else if(this.down) {                
                this.trails.SetTrigger("Vertical");
                this.trails.ResetTrigger("Idle");
                this.trails.ResetTrigger("Horizontal");
            }
        } else if(this.speed == 0) {
            this.trails.SetTrigger("Idle");
            this.trails.ResetTrigger("Vertical");
            this.trails.ResetTrigger("Horizontal");

            this.trails.transform.localScale = new Vector3((this.trails.transform.localScale.x), Mathf.Abs(this.trails.transform.localScale.y), this.trails.transform.localScale.z);

        }

        if(Input.GetKeyDown(KeyCode.LeftArrow) && !this.left) {
            this.left = true;
            this.right = this.up = this.down = false;
            this.speed = 0f;

            this.capsuleCollider.direction = CapsuleDirection2D.Horizontal;
            this.capsuleCollider.size = horizontalSize;
            this.capsuleCollider.offset = horizontalOffset;            

            this.anim.SetTrigger("Left");
            this.anim.ResetTrigger("Right");
            this.anim.ResetTrigger("Up");
            this.anim.ResetTrigger("Down");

            this.trails.transform.localScale = new Vector3((this.trails.transform.localScale.x * -1), Mathf.Abs(this.trails.transform.localScale.y), this.trails.transform.localScale.z);

            this.soundEmitter[1].Play();
        } else if(Input.GetKeyDown(KeyCode.RightArrow) && !this.right) {
            this.right = true;
            this.left = this.up = this.down = false;
            this.speed = 0f;

            this.capsuleCollider.direction = CapsuleDirection2D.Horizontal;
            this.capsuleCollider.size = horizontalSize;
            this.capsuleCollider.offset = horizontalOffset;

            this.anim.SetTrigger("Right");
            this.anim.ResetTrigger("Left");
            this.anim.ResetTrigger("Up");
            this.anim.ResetTrigger("Down");

            this.trails.transform.localScale = new Vector3(Mathf.Abs(this.trails.transform.localScale.x), Mathf.Abs(this.trails.transform.localScale.y), this.trails.transform.localScale.z);


            this.soundEmitter[1].Play();
        } else if(Input.GetKeyDown(KeyCode.UpArrow) && !this.up) {
            this.up = true;
            this.left = this.right = this.down = false;
            this.speed = 0f;

            this.capsuleCollider.direction = CapsuleDirection2D.Vertical;
            this.capsuleCollider.size = verticalSize;
            this.capsuleCollider.offset = verticalOffset;

            this.anim.SetTrigger("Up");
            this.anim.ResetTrigger("Left");
            this.anim.ResetTrigger("Right");
            this.anim.ResetTrigger("Down");

            this.trails.transform.localScale = new Vector3(Mathf.Abs(this.trails.transform.localScale.x), Mathf.Abs(this.trails.transform.localScale.y), this.trails.transform.localScale.z);

            this.soundEmitter[1].Play();
        } else if(Input.GetKeyDown(KeyCode.DownArrow) && !this.down) {
            this.down = true;
            this.left = this.right = this.up = false;
            this.speed = 0f;

            this.capsuleCollider.direction = CapsuleDirection2D.Vertical;
            this.capsuleCollider.size = verticalSize;
            this.capsuleCollider.offset = verticalOffset;

            this.anim.SetTrigger("Down");
            this.anim.ResetTrigger("Left");
            this.anim.ResetTrigger("Right");
            this.anim.ResetTrigger("Up");

            this.trails.transform.localScale = new Vector3(Mathf.Abs(this.trails.transform.localScale.x), (this.trails.transform.localScale.y * -1), this.trails.transform.localScale.z);            

            this.soundEmitter[1].Play();
        }

        if(this.moving) {
            if(Mathf.Abs(this.speed) < this.maxSpeed) { //Not at Max Speed
                this.speed += (this.left || this.down ? -1 : 1) * this.acceleration * Time.fixedDeltaTime;
            }/* else {
                this.speed = (this.left || this.down ? -1 : 1) * this.maxSpeed;
            }*/
        } else {
            if(
                ((this.left || this.down) && this.speed < 0) ||
                ((this.right || this.up) && this.speed > 0))
            {
                if(this.soundEmitter[0].IsPlaying()) {
                    this.soundEmitter[0].SetParameter("Push button", 1f);
                }
                this.speed += (this.left || this.down ? 1 : -1) * this.deceleration * Time.fixedDeltaTime;
            } else { 
                this.speed = 0;
                
            }
        }

        if(this.speed != 0) {
             var speedVect = new Vector3( //this.transform.position
                this.left || this.right ? this.speed * Time.fixedDeltaTime : 0f,
                this.up || this.down ? this.speed * Time.fixedDeltaTime : 0f,
            0f
            );

            //this.transform.position = LevelManager.currentInstance.levelCamera.GetComponent<CameraController>().ClampMapPosition(this.transform.position);
            var clamped = LevelManager.currentInstance.levelCamera.GetComponent<CameraController>().ClampMapPosition(this.transform.position + speedVect);
            this.boatBody.MovePosition(clamped);
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

    public void ShowWarning() {
        this.warning.gameObject.SetActive(true);
        var sprite = this.warning.GetComponent<SpriteRenderer>();
        var originalColor = sprite.color;
        var transpWarning = sprite.color - Color.black;          
        sprite.color = transpWarning;

        var sequence = DOTween.Sequence();
        sequence.Append(sprite.DOColor(originalColor, warningFadeInOutTime));
        sequence.AppendInterval(warningFadeInterval);
        sequence.Append(sprite.DOColor(transpWarning, warningFadeInOutTime));
        sequence.OnComplete(() => {
            sprite.color = originalColor;
            sprite.gameObject.SetActive(false);
        });
    }


    public void TryAddToChasing(BubbleController bubble) {
        if(!this.chasingBubbles.Contains(bubble)) {
            this.chasingBubbles.Add(bubble);
        }
    }

    public void TryRemoveFromChasing(BubbleController bubble) {
        if(this.chasingBubbles.Contains(bubble)) {
            this.chasingBubbles.Remove(bubble);
        }
    }

    //Upon collision with another GameObject, this GameObject will reverse direction
    public void TriggerEnter2D(Collider2D collision)
    {
        if(collision.transform.parent.TryGetComponent<BubbleController>(out var bubble)) {
            if(!this.mayPopBubbles.Contains(bubble) && bubble.chasing) {
                this.soundEmitter[2].Play();
                this.ShowWarning();
                this.mayPopBubbles.Add(bubble);
            }
        }
    }

    public void TriggerExit2D(Collider2D collision)
    {
        if(collision.transform.parent.TryGetComponent<BubbleController>(out var bubble)) {
            //We create a delay to remove the bubble, to avoid it reentering right after being removed.
            this.StartCoroutine(nameof(InstaRemoveFromPopList), bubble);         
        }
    }

    private IEnumerator InstaRemoveFromPopList(BubbleController bubble) {
        yield return new WaitForSeconds(0.2f);
        if(this.mayPopBubbles.Contains(bubble)) {
            this.mayPopBubbles.Remove(bubble);
        }
    }

    public void WhaleTime() => this.whaleTime = true;

    public void FishTime(float durationSec) {
        this.chaseBlocked = true;
        this.ExecuteAfter(() => {
            this.jammed = true;

            this.anim.ResetTrigger("Down");
            this.anim.ResetTrigger("Left");
            this.anim.ResetTrigger("Right");
            this.anim.ResetTrigger("Up");

            //this.chaseBlocked = false;
        }, durationSec);
    }

    public void KidTime(float durationSec) {
        this.chaseBlocked = true;
        //DO STUFF
    }

    private void ExecuteAfter(Action stuff, float waitingTime) {
        StartCoroutine(this.ExecuteAfterCR(stuff, waitingTime));
    }

    private IEnumerator ExecuteAfterCR(Action stuff, float waitingTime) {
        yield return new WaitForSeconds(waitingTime);
        stuff?.Invoke();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        foreach(Transform t in this.bubblePivots) {
            Gizmos.DrawWireSphere(t.position, 0.3f);
        }
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(this.whalePivots.position, 0.3f);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(this.transform.position, this.whaleTrollDistance);
    }
}
