using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class FishController : MonoBehaviour
{
    [SerializeField, BoxGroup("Fish shoot configs")] public Transform prefabProjectile;    
    [SerializeField, BoxGroup("Fish shoot configs")] public float timeBetweenShoot = 1f;
    [SerializeField, BoxGroup("Fish shoot configs")] public float timeToDestroyProjectile = .5f;
    [SerializeField, BoxGroup("Fish shoot configs")] public float minDistToShoot;
    [SerializeField, BoxGroup("Fish shoot configs")] public Transform positionToShoot;
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

    private void FishShoot() {        
        if(distanceBetweenBoatAndFish < this.minDistToShoot) {
            var projectile = Instantiate(prefabProjectile, this.transform);
            projectile.transform.position = positionToShoot.position;            
        }
    }

    IEnumerator StartShoot() {
        while(isShooting) {
            FishShoot();            
            yield return new WaitForSeconds(timeBetweenShoot);
        }
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(this.transform.position, this.minDistToShoot);
    }
}
