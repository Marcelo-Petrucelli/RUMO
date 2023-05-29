using System.Collections.Generic;
using System.Collections;
using NaughtyAttributes;
using UnityEngine;
using DG.Tweening;

public class FishController : MonoBehaviour
{
    [SerializeField, BoxGroup("References")] public Transform prefabProjectile;    
    [SerializeField, BoxGroup("References")] public Animator animator;

    [SerializeField, BoxGroup("Configs")] public float minDistToShoot;
    [SerializeField, BoxGroup("Configs"), Range(2f, 10f)] public float timeBetweenShoot = 2f;
    [SerializeField, BoxGroup("Configs")] public Vector2 projectileSpawnOffset = Vector2.zero;

    [SerializeField, BoxGroup("Spawn Configs")] public float allProjectileSpeed = 2f;
    [SerializeField, BoxGroup("Spawn Configs")] public float allProjectileDuration = 3f;

    [SerializeField, ReorderableList, ReadOnly, BoxGroup("Debug")] internal List<ProjectileBase> myBubbles = new();

    [ShowNonSerializedField] internal bool active = true;
    [ShowNonSerializedField] private bool isShooting;
    [ShowNativeProperty] private Vector3 ProjectileSpawnPoint => this.transform.position + (Vector3) this.projectileSpawnOffset;
    [ShowNonSerializedField] private float distanceBetweenBoatAndFish;

    private BoatController player;
    private Coroutine currentCorrotine;

    private void Start()
    {
        this.player = LevelManager.currentInstance.Player;
    }

    private void Update()
    {
        if(this.active) {
            this.distanceBetweenBoatAndFish = Vector3.Distance(this.transform.position, this.player.transform.position);

            if(this.distanceBetweenBoatAndFish < this.minDistToShoot && !this.player.jammed) {
                if(!this.isShooting) {
                    this.isShooting = true;
                    //AudioController.Instance.EnterBattle();
                    this.currentCorrotine = this.StartCoroutine(this.StartShoot());
                }
            } else if(this.isShooting) {
                //AudioController.Instance.ExitBattle();
                this.StopCoroutine(this.currentCorrotine);
                this.isShooting = false;
            }
        } else {
            if(this.currentCorrotine != null) {
                this.StopCoroutine(this.currentCorrotine);
            }
            this.isShooting = false;
        }
    }  

    public void FishShoot() {         
        var obj = Instantiate(this.prefabProjectile, this.transform);
        var projectile = obj.GetComponent<ProjectileBase>();
        
        //Config
        projectile.transform.position = this.ProjectileSpawnPoint;
        projectile.speed = this.allProjectileSpeed;
        projectile.timeToDestroy = this.allProjectileDuration;
        projectile.fishParent = this;

        this.myBubbles.Add(projectile);
    }

    IEnumerator StartShoot() {
        while(this.isShooting) {
            this.animator.SetTrigger("Attack");
            yield return new WaitForSeconds(this.timeBetweenShoot);
        }
    }

    public void DisableFadeAndDestroy() {
        this.active = false;
        var spr = this.animator.GetComponent<SpriteRenderer>();

        spr.DOFade(0f, 0.3f).OnComplete(() => {
            Destroy(this.gameObject);
        });
    }

    private void OnDestroy()
    {
        foreach(var bubble in this.myBubbles) {
            bubble.Explode();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(this.transform.position, this.minDistToShoot);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(this.ProjectileSpawnPoint, 0.5f);
    }
}
