﻿using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using FMODUnity;

public class BoatController : MonoBehaviour
{
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

    private Animator anim;
    private Rigidbody2D boatBody;
    private List<FMODUnity.StudioEventEmitter> soundEmitter;
    private List<BubbleController> mayPopBubble = new List<BubbleController>();

    // Start is called before the first frame update
    void Start() {
        this.anim = this.GetComponent<Animator>();
        this.boatBody = this.GetComponent<Rigidbody2D>();
        this.soundEmitter = new List<FMODUnity.StudioEventEmitter>(this.GetComponents<FMODUnity.StudioEventEmitter>());
    }

    // Update is called once per frame
    void Update() {
        this.CheckPop();
    }

    void FixedUpdate()
    {
        this.Move();
    }

    private void CheckPop() {
        if(this.jammed) {
            return;
        }

        if(this.MayPopListSize > 0 && Input.GetKeyDown(KeyCode.Space)) {
            //this.jammed = true; //Will be set by LevelManager.ObtainNextItem()
            this.speed = 0;
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

        if(Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow)) {
            this.soundEmitter[0].SetParameter("Push button", 0.0f);
            if(!this.soundEmitter[0].IsPlaying()) {
                this.soundEmitter[0].Play();
            }
        }

        if(Input.GetKeyDown(KeyCode.LeftArrow) && !this.left) {
            this.left = true;
            this.right = this.up = this.down = false;
            this.speed = 0f;

            this.anim.SetTrigger("Left");
            this.anim.ResetTrigger("Right");
            this.anim.ResetTrigger("Up");
            this.anim.ResetTrigger("Down");
            
            this.soundEmitter[1].Play();
        } else if(Input.GetKeyDown(KeyCode.RightArrow) && !this.right) {
            this.right = true;
            this.left = this.up = this.down = false;
            this.speed = 0f;

            this.anim.SetTrigger("Right");
            this.anim.ResetTrigger("Left");
            this.anim.ResetTrigger("Up");
            this.anim.ResetTrigger("Down");
            
            this.soundEmitter[1].Play();
        } else if(Input.GetKeyDown(KeyCode.UpArrow) && !this.up) {
            this.up = true;
            this.left = this.right = this.down = false;
            this.speed = 0f;

            this.anim.SetTrigger("Up");
            this.anim.ResetTrigger("Left");
            this.anim.ResetTrigger("Right");
            this.anim.ResetTrigger("Down");

            this.soundEmitter[1].Play();
        } else if(Input.GetKeyDown(KeyCode.DownArrow) && !this.down) {
            this.down = true;
            this.left = this.right = this.up = false;
            this.speed = 0f;

            this.anim.SetTrigger("Down");
            this.anim.ResetTrigger("Left");
            this.anim.ResetTrigger("Right");
            this.anim.ResetTrigger("Up");

            this.soundEmitter[1].Play();
        }

        if(moving) {
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

    //Upon collision with another GameObject, this GameObject will reverse direction
    public void TriggerEnter2D(Collider2D collision)
    {
        if(collision.transform.parent.TryGetComponent<BubbleController>(out var bubble)) {
            if(!this.mayPopBubble.Contains(bubble)) {
                this.mayPopBubble.Add(bubble);
            }
        }
    }

    public void TriggerExit2D(Collider2D collision)
    {
        if(collision.transform.parent.TryGetComponent<BubbleController>(out var bubble)) {
            this.mayPopBubble.Remove(bubble);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        foreach(Transform t in this.bubblePivots) {
            Gizmos.DrawWireSphere(t.position, 0.3f);
        }
    }
}
