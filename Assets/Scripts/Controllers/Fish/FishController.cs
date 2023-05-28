using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class FishController : MonoBehaviour
{
    [SerializeField, BoxGroup("Fish References")] public Transform prefabProjectile;    
    [SerializeField, BoxGroup("Fish References")] public Transform positionToShoot;
    [SerializeField, BoxGroup("Fish References")] public Animator animator;

    [SerializeField, BoxGroup("Fish shoot configs"), Range(2f, 10f)] public float timeBetweenShoot = 2f;
    [SerializeField, BoxGroup("Fish shoot configs")] public float timeToDestroyProjectile = .5f;
    [SerializeField, BoxGroup("Fish shoot configs")] public float minDistToShoot;
    [SerializeField, BoxGroup("Fish shoot configs")] public string fishAttackTrigger = "Attack";

    [ShowNonSerializedField] private float distanceBetweenBoatAndFish;
    [ShowNonSerializedField] private bool isShooting;

    private Coroutine currentCorrotine;
    private BoatController player;

    private void Start()
    {
        this.player = LevelManager.currentInstance.Player;
    }

    private void Update()
    {
        distanceBetweenBoatAndFish = Vector3.Distance(this.transform.position, this.player.transform.position);

        if(distanceBetweenBoatAndFish < this.minDistToShoot) {
            if(!isShooting) {
                isShooting = true;
                currentCorrotine = StartCoroutine(StartShoot());
            }
        } else if(isShooting) {            
            StopCoroutine(currentCorrotine);
            isShooting = false;
        }
    }  

    public void FishShoot() {         
        var projectile = Instantiate(prefabProjectile, this.transform);
        projectile.transform.position = positionToShoot.position;          
    }

    IEnumerator StartShoot() {
        while(isShooting) {
            animator.SetTrigger(fishAttackTrigger);
            yield return new WaitForSeconds(timeBetweenShoot);
        }
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(this.transform.position, this.minDistToShoot);
    }
}
