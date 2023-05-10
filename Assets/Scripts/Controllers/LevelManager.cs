using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;

/*[Serializable]
class ColorFilterFinalProperties
{
    [SerializeField] float finalHUE;
    [SerializeField] float finalBrightness;
    [SerializeField] float finalContrast;
    [SerializeField] float finalSaturation;
    [SerializeField] Vector3 finalSaturationIntensity;
}*/

public class LevelManager : MonoBehaviour
{
    [SerializeField, BoxGroup("References")] public GameObject boat;
    [SerializeField, BoxGroup("References")] public GameObject waterAndReflex;

    [ShowNonSerializedField] internal BoatController player;

    public BoatController Player => this.player;

    public static LevelManager currentInstance;

    void Awake()
    {
        currentInstance = this;
        this.player = this.boat.GetComponent<BoatController>();
        this.waterAndReflex.SetActive(true);
    }

    // Start is called before the first frame update
    void Start()
    {

    }
}
