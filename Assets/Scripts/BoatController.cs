using System.Collections.Generic;
using System.Collections;
using NaughtyAttributes;
using UnityEngine;

public class BoatController : MonoBehaviour
{
    private Animator anim;
    [ShowNonSerializedField] private float speed = 0f;

    // Start is called before the first frame update
    void Start() => this.anim = this.GetComponent<Animator>();

    // Update is called once per frame
    void Update() {
        this.Move();
    }

    private void Move() {

    }
}
