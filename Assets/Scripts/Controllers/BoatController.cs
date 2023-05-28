using System.Collections.Generic;
using System.Collections;
using NaughtyAttributes;
using UnityEngine;
using DG.Tweening;
using FMODUnity;
using System;
using TMPro;

public class BoatController : MonoBehaviour
{
    [SerializeField, BoxGroup("Config")] public float speed = 4f;
    [SerializeField, BoxGroup("Config")] public float smoothInputSpeed = 0.3f;
    [SerializeField, BoxGroup("Config")] public Vector2 smoothInputDumpSnap = new(0.1f, 0.1f);
    [SerializeField, BoxGroup("Config")] public int maxChasingBubbles = 1;
    [SerializeField, BoxGroup("Config")] public float whaleTrollDistance = 7f;
    [SerializeField, BoxGroup("Config")] public float warningFadeInOutTime = 0.5f;
    [SerializeField, BoxGroup("Config")] public float warningFadeInterval = 4f;
    [SerializeField, BoxGroup("Config")] public float destroyPreInterval = 0.5f;
    [SerializeField, BoxGroup("Config")] public float destroyWarningDuration = 1f;

    [SerializeField, BoxGroup("References")] public Animator trails;
    [SerializeField, BoxGroup("References")] public Transform whalePivot;
    [SerializeField, BoxGroup("References")] public List<Transform> bubblePivots;
    [SerializeField, BoxGroup("References")] public GameObject warning;
    [SerializeField, BoxGroup("Vertical Colider Config")] private Vector2 verticalSize = new(0.5f, 1.5f);
    [SerializeField, BoxGroup("Vertical Colider Config")] private Vector2 verticalOffset = new(0.025f, 0f);
    [SerializeField, BoxGroup("Horizontal Colider Config")] private Vector2 horizontalSize = new(2f, 0.4583282f);
    [SerializeField, BoxGroup("Horizontal Colider Config")] private Vector2 horizontalOffset = new(-0.01640372f, -0.3568905f);

    [ShowNonSerializedField] private bool moving = false;
    [ShowNonSerializedField] private bool whaleTime = false;
    [ShowNonSerializedField] private Vector2 currentMovement;
    [ShowNonSerializedField] private Vector2 currentVelocity;
    [ShowNonSerializedField] private BoatDirection currentDirection = BoatDirection.east;

    [ShowNonSerializedField] internal bool jammed = false;
    [ShowNonSerializedField] internal bool chaseBlocked = false;
    [ShowNonSerializedField] internal bool waitingAnchor = false;
    [ShowNonSerializedField] internal bool waitingItemDismiss = false;

    [ShowNativeProperty] internal int MayPopListSize => this.mayPopBubbles.Count;

    [ShowNativeProperty] internal int ChaseListSize => this.chasingBubbles.Count;
    internal bool MayBeBubbleChasedBy(BubbleController bubble) => (this.chasingBubbles.Count < this.maxChasingBubbles || this.chasingBubbles.Contains(bubble)) && !this.chaseBlocked;

    private Animator anim;
    private Vector3 initialPosition;
    private Rigidbody2D boatBody;
    private CapsuleCollider2D capsuleCollider;
    private List<FMODUnity.StudioEventEmitter> soundEmitter;
    private List<BubbleController> mayPopBubbles = new List<BubbleController>();
    internal List<BubbleController> chasingBubbles = new List<BubbleController>();
    internal WhirlpoolController poolDraggin;

    // Start is called before the first frame update
    void Start() {
        this.initialPosition = this.transform.position;
        this.anim = this.GetComponent<Animator>();
        this.boatBody = this.GetComponent<Rigidbody2D>();
        this.capsuleCollider = this.GetComponent<CapsuleCollider2D>();
        this.soundEmitter = new List<FMODUnity.StudioEventEmitter>(this.GetComponents<FMODUnity.StudioEventEmitter>());
        this.capsuleCollider.direction = CapsuleDirection2D.Horizontal;
        this.capsuleCollider.size = this.horizontalSize;        
        this.capsuleCollider.offset = this.horizontalOffset;
        this.warning.SetActive(false);
    }

    // Update is called once per frame
    void Update() {
        this.CheckWhale();
        this.CheckPop();
        this.CheckAnchor();
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
                    this.chaseBlocked = true;

                    LevelManager.currentInstance.WhaleItAllUp();
                    this.whaleTime = false;
                    break;
                }
            }
        }
    }

    private void CheckPop() {
        if(this.jammed || this.waitingAnchor) {
            return;
        }

        if(this.MayPopListSize > 0 && Input.GetKeyDown(KeyCode.Space) && !LevelManager.currentInstance.showingText) {
            //this.jammed = true; //Will be set by LevelManager.ObtainNextItem()
            //this.speed = 0;
            this.mayPopBubbles[0].Pop();
            this.chasingBubbles.RemoveAt(0);
            this.mayPopBubbles.RemoveAt(0);
        }
    }

    public void RemoveBubbleReferences(BubbleController bubble) {
        this.chasingBubbles.Remove(bubble);
        this.mayPopBubbles.Remove(bubble);
    }

    private void CheckAnchor() {
        if(this.waitingAnchor && Input.GetKeyDown(KeyCode.A)) {
            this.waitingAnchor = false;
            LevelManager.currentInstance.ObtainNextItem(true);
        }
    }

    private void Move() {
        if(this.jammed) {
            this.anim.SetBool("Moving", false);
            if(this.waitingItemDismiss && Input.anyKeyDown) { //Upgrade to any key on Gamepad
                LevelManager.currentInstance.ItemInputRecieved();
                this.waitingItemDismiss = false;
            }
            return;
        }

        var drag = new Vector2(0f, 0f);
        var boatSpeed = this.speed;
        if(this.poolDraggin != null) {
            boatSpeed /= 2;
            drag = this.Vector3Clamp((this.poolDraggin.Center - this.transform.position).normalized, new Vector3(-0.3f, -0.3f, 0f), new Vector3(0.3f, 0.3f, 0f));

            if(Vector3.Distance(this.poolDraggin.Center, this.transform.position) < this.poolDraggin.deadRadius) {
                this.ToBeDestroyed();
            }
        }

        var input = new Vector2(Input.GetAxis("Horizontal") + drag.x, Input.GetAxis("Vertical") + drag.y).normalized;
        this.currentMovement = Vector2.SmoothDamp(this.currentMovement, input, ref this.currentVelocity, this.smoothInputSpeed);
        if(this.currentMovement.x <= this.smoothInputDumpSnap.x) {
            this.currentMovement.x = input.x;
        }
        if(this.currentMovement.y <= this.smoothInputDumpSnap.y) {
            this.currentMovement.y = input.y;
        }

        var movement = boatSpeed * Time.fixedDeltaTime * this.currentMovement;

        var direction = this.FindDirection(movement, this.currentDirection);
        var changedDirection = this.currentDirection != direction;
        var moving = Mathf.Abs(movement.x) + Mathf.Abs(movement.y) > 0;
        var startedMoving = !this.moving && moving;
        var stopedMoving = this.moving && !moving;

        this.moving = moving;
        this.currentDirection = direction;

        this.anim.SetBool("Moving", this.moving);
        if(changedDirection) {
            this.anim.SetTrigger("Changed");
        }
        this.anim.SetInteger("Direction", (int) this.currentDirection);

        if(changedDirection) {
            this.soundEmitter[1].Play();

            this.ChangeCollision();
        }

        if(startedMoving) {
            this.FadeTutorial();
        }/* else if(stopedMoving) {

        }*/
        this.ChangeTrail(moving);

        if(this.currentMovement.sqrMagnitude == 0 && this.currentVelocity.sqrMagnitude <= 0.01f) {
            if(this.soundEmitter[0].IsPlaying()) {
                this.soundEmitter[0].SetParameter("Push button", 1f);
            }
        } else if(moving) {
            this.soundEmitter[0].SetParameter("Push button", 0.0f);
            if(!this.soundEmitter[0].IsPlaying()) {
                this.soundEmitter[0].Play();
            }
        }

        var clamped = LevelManager.currentInstance.levelCamera.GetComponent<CameraController>().ClampMapPosition(this.transform.position + new Vector3(movement.x, movement.y));
        this.boatBody.MovePosition(clamped);
    }

    private void ChangeCollision() {
        var horizontal = true;
        switch(this.currentDirection) {
            case BoatDirection.north:
            case BoatDirection.south:
                horizontal = false;
                break;
            /*case BoatDirection.west:
            case BoatDirection.northWest:
            case BoatDirection.southWest:
            case BoatDirection.east:
            case BoatDirection.northEast:
            case BoatDirection.southEast:
                break;*/
        }
        if(horizontal) {
            this.capsuleCollider.direction = CapsuleDirection2D.Horizontal;
            this.capsuleCollider.size = this.horizontalSize;
            this.capsuleCollider.offset = this.horizontalOffset;
        } else {
            this.capsuleCollider.direction = CapsuleDirection2D.Vertical;
            this.capsuleCollider.size = this.verticalSize;
            this.capsuleCollider.offset = this.verticalOffset;
        }
    }

    private void ChangeTrail(bool moving) {
        var spriteR = this.trails.GetComponent<SpriteRenderer>();
        if(!moving) {
            this.trails.SetTrigger("Idle");
            spriteR.flipX = false;
            spriteR.flipY = false;
        } else {
            switch(this.currentDirection) {
                case BoatDirection.north:
                    this.trails.SetTrigger("Vertical");
                    this.trails.ResetTrigger("Horizontal");
                    this.trails.ResetTrigger("Idle");
                    spriteR.flipX = false;
                    spriteR.flipY = false;
                    break;
                case BoatDirection.south:
                    this.trails.SetTrigger("Vertical");
                    this.trails.ResetTrigger("Horizontal");
                    this.trails.ResetTrigger("Idle");
                    spriteR.flipX = false;
                    spriteR.flipY = true;
                    break;
                case BoatDirection.west:
                case BoatDirection.northWest:
                case BoatDirection.southWest:
                    this.trails.SetTrigger("Horizontal");
                    this.trails.ResetTrigger("Vertical");
                    this.trails.ResetTrigger("Idle");
                    spriteR.flipX = true;
                    spriteR.flipY = false;
                    break;
                case BoatDirection.east:
                case BoatDirection.northEast:
                case BoatDirection.southEast:
                    this.trails.SetTrigger("Horizontal");
                    this.trails.ResetTrigger("Vertical");
                    this.trails.ResetTrigger("Idle");
                    spriteR.flipX = false;
                    spriteR.flipY = false;
                    break;
            }
        }
    }

    private BoatDirection FindDirection(Vector2 movement, BoatDirection lastDirection) {
        if(movement.y > 0) { //north
            if(movement.x > 0) { //northEast
                return BoatDirection.northEast;
            } else if(movement.x < 0) { //northWest
                return BoatDirection.northWest;
            } else { //only north
                return BoatDirection.north;
            }
        } else if (movement.y < 0){ //south
            if(movement.x > 0) { //southEast
                return BoatDirection.southEast;
            } else if(movement.x < 0) { //southWest
                return BoatDirection.southWest;
            } else { //only south
                return BoatDirection.south;
            }
        } else { //neutral
            if(movement.x > 0) { //northEast
                return BoatDirection.east;
            } else if(movement.x < 0) { //northWest
                return BoatDirection.west;
            } else { //only north
                return lastDirection;
            }
        }
    }

    public Vector3 GetBubblePivot() {
        if(this.currentDirection == BoatDirection.north) {
            return this.bubblePivots[0].position;
        } else if(this.currentDirection == BoatDirection.east) {
            return this.bubblePivots[1].position;
        } else if(this.currentDirection == BoatDirection.south) {
            return this.bubblePivots[2].position;
        } else { //this.currentDirection == BoatDirection.west
            return this.bubblePivots[3].position;
        }
    }

    public void FadeTutorial() => LevelManager.currentInstance.HideTutorial();

    public void ShowWarning(bool showAnchorText = false, float? duration = null) {
        this.warning.SetActive(true);
        var sprite = this.warning.GetComponent<SpriteRenderer>();
        var originalColor = sprite.color;
        var transpWarning = sprite.color - Color.black;          
        sprite.color = transpWarning;

        var sequence = DOTween.Sequence();
        sequence.Append(sprite.DOColor(originalColor, this.warningFadeInOutTime));
        sequence.AppendInterval(duration ?? this.warningFadeInterval);
        sequence.Append(sprite.DOColor(transpWarning, this.warningFadeInOutTime));
        sequence.OnComplete(() => {
            sprite.color = originalColor;
            sprite.gameObject.SetActive(false);
        });

        if(showAnchorText) {
            var text = this.warning.GetComponentInChildren<TextMeshPro>(true);

            text.gameObject.SetActive(true);
            var originalTextColor = text.color;
            var transpTextWarning = text.color - Color.black;
            text.color = transpTextWarning;

            var sequenceText = DOTween.Sequence();
            sequenceText.Append(text.DOColor(originalTextColor, this.warningFadeInOutTime));
            sequenceText.AppendInterval(duration ?? this.warningFadeInterval);
            sequenceText.Append(text.DOColor(transpTextWarning, this.warningFadeInOutTime));
            sequenceText.OnComplete(() => {
                text.color = originalColor;
                text.gameObject.SetActive(false);
            });
        }
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Finish") && !LevelManager.currentInstance.ended) {
            LevelManager.currentInstance.EndGame();
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
            this.soundEmitter[2].Play(); //Warning Sound
            this.ShowWarning(true);

            this.jammed = true;
            this.waitingAnchor = true;
        }, durationSec);
    }

    public void WhirlpoolTime(float durationSec) {
        this.chaseBlocked = true;
        this.ExecuteAfter(() => {
            this.soundEmitter[2].Play(); //Warning Sound
            this.ShowWarning(true);

            this.jammed = true;
            this.waitingAnchor = true;
        }, durationSec);
    }

    public void PartTime(float durationSec) {
        this.chaseBlocked = true;
        this.ExecuteAfter(() => {
            this.soundEmitter[2].Play(); //Warning Sound
            this.ShowWarning(true);

            this.jammed = true;
            this.waitingAnchor = true;
        }, durationSec);
    }

    public void ToBeDestroyed() {
        this.jammed = true;
        this.ShowWarning(false, this.destroyWarningDuration);
        this.ExecuteAfter(() => { 
            this.anim.SetTrigger("Destroy"); 
        }, this.destroyPreInterval); //Animation EVENT! Calls Destroyed at end.
    }

    public void Destroyed() => LevelManager.currentInstance.BoatDied();

    public void ResetAll()
    {
        this.jammed = true; //Guarantee
        this.anim.SetTrigger("Changed");
        this.anim.SetInteger("Direction", 1);
        this.chaseBlocked = false;
        this.waitingAnchor = false;
        this.waitingItemDismiss = false;
        this.currentMovement = Vector2.zero;
        this.transform.position = this.initialPosition;
    }

    private Vector3 Vector3Clamp(Vector3 current, Vector3 min, Vector3 max) => new (Mathf.Clamp(current.x, min.x, max.x), Mathf.Clamp(current.y, min.y, max.y), Mathf.Clamp(current.z, min.z, max.z));

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
        Gizmos.DrawWireSphere(this.whalePivot.position, 0.3f);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(this.transform.position, this.whaleTrollDistance);
    }
}

internal enum BoatDirection {
    north = 0,
    northEast = 1,
    east = 2,
    southEast = 3,
    south = 4,
    southWest = 5,
    west = 6,
    northWest = 7,
}
