using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class ProjectileBase: MonoBehaviour
{
    [SerializeField, BoxGroup("References")] public Animator anim;

    [SerializeField, BoxGroup("Config")] public float speed = 2f;
    [SerializeField, BoxGroup("Config")] public float timeToDestroy = 3f;
    [ShowNonSerializedField] internal bool active = true;

    private BoatController player;
    internal FishController fishParent;

    private void Start()
    {
        this.player = LevelManager.currentInstance.Player;
        this.ExecuteAfter(() => { this.Explode();}, this.timeToDestroy);
    }

    private void Update() {
        if (this.active) {
            this.MoveProjectile();
        }                
    }

    public void MoveProjectile() {
        if(this.player.jammed) {
            this.Explode();
        }
        var finalPos = this.speed * Time.deltaTime * (this.player.transform.position - this.transform.position).normalized;
        this.transform.Translate(finalPos.x, finalPos.y, this.transform.position.z);
    }

    //Moved to BoatController logic
    /*private void OnCollisionEnter2D(Collision2D collision) {
        if(collision.transform.TryGetComponent<ProjectileBase>(out var fishProjectile) && fishProjectile.active) {
            //this.player.ToBeDestroyed();
            this.Explode();
        }
    }*/

    public void Explode() {
        this.active = false;
        this.anim.SetTrigger("Explode");
    }

    public void DestroySelf() {
        if(this.fishParent != null) {
            this.fishParent.myBubbles.Remove(this);
        }
        Destroy(this.gameObject);
    }

    private void ExecuteAfter(Action stuff, float waitingTime) {
        this.StartCoroutine(this.ExecuteAfterCR(stuff, waitingTime));
    }

    private IEnumerator ExecuteAfterCR(Action stuff, float waitingTime) {
        yield return new WaitForSeconds(waitingTime);
        stuff?.Invoke();
    }
}
