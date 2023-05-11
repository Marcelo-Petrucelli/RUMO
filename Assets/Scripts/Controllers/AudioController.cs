using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using System;

public class AudioController:MonoBehaviour
{
    public static AudioController Instance = null;

    void Awake() {
        if(Instance == null) {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        } else {
            Destroy(this.gameObject);
        }
    }
}
