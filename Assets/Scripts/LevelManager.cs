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
    [SerializeField, BoxGroup("DEBUG")] public MaterialPropTransitioner test;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.T)) {
            this.test.Transition();
        }
    }
}
