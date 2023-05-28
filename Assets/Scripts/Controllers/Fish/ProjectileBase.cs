using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class ProjectileBase: MonoBehaviour
{
    [SerializeField, BoxGroup("References")] public Animator anim;

    [SerializeField, BoxGroup("Projectile config")] public float timeToDestroy = .5f;
    [SerializeField, BoxGroup("Projectile config")] public float side = 1f;
    [ShowNonSerializedField] private bool active = true;

    private BoatController player;    

    private void Start()
    {
        this.player = LevelManager.currentInstance.Player;
        this.ExecuteAfter(() => { Explode();}, this.timeToDestroy);
    }

    private void Update() {
        if (this.active) {
            MoveProjectile();
        }                
    }

    public void MoveProjectile() {
        this.transform.Translate((this.player.transform.position - this.transform.position) * Time.deltaTime * this.side);
    }

    private void OnCollisionEnter2D(Collision2D collision) {        
        if(active && collision.transform.TryGetComponent<BoatController>(out var player)) {
            Explode();
        }
    }

    public void Explode() {
        active = false;
        this.anim.SetTrigger("Explode");
    }

    public void DestroySelf() => Destroy(this.gameObject);

    private void ExecuteAfter(Action stuff, float waitingTime) {
        StartCoroutine(this.ExecuteAfterCR(stuff, waitingTime));
    }

    private IEnumerator ExecuteAfterCR(Action stuff, float waitingTime) {
        yield return new WaitForSeconds(waitingTime);
        stuff?.Invoke();
    }
}
