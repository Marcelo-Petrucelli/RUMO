using UnityEngine;
using UnityEngine.Events;

public class CallIndexedEvent:MonoBehaviour
{
    //Generic script that has several functions that can be called in different moments of the game.
    [SerializeField] private bool callOnAwake = false;
    [SerializeField] private UnityEvent awakeEvent;
    [SerializeField] private bool callOnStart = false;
    [SerializeField] private UnityEvent startEvent;
    [SerializeField] private bool callOnUpdate = false;
    [SerializeField] private UnityEvent updateEvent;
    [SerializeField] private bool callOnEnable = false;
    [SerializeField] private UnityEvent onEnableEvent;
    [SerializeField] private bool callOnDisable = false;
    [SerializeField] private UnityEvent onDisableEvent;

    [SerializeField] private UnityEvent[] enumeratedEvents;

    private void Awake() {
        if(this.callOnAwake) {
            this.awakeEvent?.Invoke();
        }
    }

    private void Start() {
        if(this.callOnStart) {
            this.startEvent?.Invoke();
        }
    }

    private void Update() {
        if(this.callOnUpdate) {
            this.updateEvent?.Invoke();
        } else {
            this.enabled = false;
        }
    }

    private void OnEnable() {
        if(this.callOnEnable) {
            this.onEnableEvent?.Invoke();
        }
    }

    private void OnDisable() {
        if(this.callOnDisable) {
            this.onDisableEvent?.Invoke();
        }
    }

    //This is a public function to be called by other scripts or through animation.
    public void CallEnumeratedEvents(int eventNumber) => this.enumeratedEvents[eventNumber].Invoke();
}
