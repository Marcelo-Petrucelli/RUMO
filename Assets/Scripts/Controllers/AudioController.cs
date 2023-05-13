using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using System;
using UnityEditorInternal;

public class AudioController:MonoBehaviour
{
    public static AudioController Instance = null;
    private FMOD.Studio.EventInstance FMODinstance;

    [SerializeField, Range(0f, 10f)] private float transitionValue;
   
    void Awake() {        
        if(Instance == null) {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        } else {
            Destroy(this.gameObject);
        }
    }

    public void ChangeAudioTransition() {
        FMODinstance.setParameterByName("transition", transitionValue);
    }
    
}
