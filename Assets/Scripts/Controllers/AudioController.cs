using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using System;

public class AudioController:MonoBehaviour
{
    internal static AudioController Instance = null;
    [ShowNonSerializedField] internal bool isMusicMuted = false;
    [ShowNonSerializedField] internal bool isSFXMuted = false;
    [ShowNonSerializedField] internal bool isDubMuted = false;

    private FMODUnity.StudioEventEmitter musicEmitter;

    void Awake() {        
        if(Instance == null) {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        } else {
            Destroy(this.gameObject);
        }
        this.musicEmitter = this.GetComponent<FMODUnity.StudioEventEmitter>();
    }

    public void GameStartedMusic() {
        this.musicEmitter.SetParameter("Transition", 1.1f); //1~1.5
    }

    public void FirstAdvanceMusic() {
        this.musicEmitter.SetParameter("Transition", 2.1f); //2~2.5
    }

    public void SecondAdvanceMusic() {
        this.musicEmitter.SetParameter("Transition", 3.1f); //3~3.5
    }

    public void FinalizeNormalAndStartWOWMusic() {
        this.musicEmitter.SetParameter("Transition", 4.1f); //4~4.5
    }

    public void StartEUFOMusic() {
        this.musicEmitter.SetParameter("Transition", 5.1f); //5~5.5
    }

    public void FoundIslandMusicEnding() {
        this.musicEmitter.SetParameter("Transition", 6.1f); //5~5.5
    }

    public void ReturnedToMenuFromWinningCredits() {
        this.musicEmitter.SetParameter("Transition", 7.1f); //7~7.1
    }
}
