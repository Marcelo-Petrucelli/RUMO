﻿using NaughtyAttributes;
using UnityEngine;

public class BoatController : MonoBehaviour
{
    private Animator anim;
    [SerializeField] public float acceleration = 0f;
    [SerializeField] public float deceleration = 0f;
    [SerializeField] public float maxSpeed = 0f;

    [ShowNonSerializedField] private float speed = 0f;
    [ShowNonSerializedField] private bool left = false;
    [ShowNonSerializedField] private bool right = true;
    [ShowNonSerializedField] private bool up = false;
    [ShowNonSerializedField] private bool down = false;

    // Start is called before the first frame update
    void Start() => this.anim = this.GetComponent<Animator>();

    // Update is called once per frame
    void Update() {
        this.Move();
    }

    private void Move() {
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
            this.transform.position += new Vector3(this.left || this.right ? this.speed : 0f, this.up || this.down ? this.speed : 0f, 0f);
        }
    }
}
