using System.Collections.Generic;
using System.Collections;
using NaughtyAttributes;
using UnityEngine.UI;
using UnityEngine;
using DG.Tweening;
using System;
using TMPro;

public class LevelManager : MonoBehaviour
{
    [SerializeField, BoxGroup("Config")] public int levelIndex;
    [SerializeField, BoxGroup("Config")] public float messagesFadeDuration = 0.8f;
    [SerializeField, BoxGroup("Config")] public float messagesDuration = 4f;
    [SerializeField, BoxGroup("Config")] public float compassInterval = 0.3f;
    [SerializeField, BoxGroup("Config")] public float waitingEndingInterval = 1.5f;
    [SerializeField, BoxGroup("Config")] public Color compassEndingColor = Color.red;
    [SerializeField, BoxGroup("Config")] public float characterFinalSceneWaitingTime = 10f;
    [SerializeField, BoxGroup("Config")] public float tutorialIntervalToShow = 0.6f;
    [SerializeField, BoxGroup("Config")] public float tutorialFadeDuration = 0.4f;
    [SerializeField, BoxGroup("Config"), MinMaxSlider(0f, 30f)] public Vector2 specialEventsTimeRange = new (4f, 9f);
    [SerializeField, BoxGroup("Config")] public float whaleWaitingInterval = 1.2f;
    [SerializeField, BoxGroup("Config")] public float loseWaitingInterval = 1.5f;
    [SerializeField, BoxGroup("Config")] public float loseFadeWaitInterval = 1f;
    [SerializeField, BoxGroup("Config"), MinMaxSlider(-100f, 100f)] public Vector2 minMaxAutoFixZRange = new (0f, 15f);

    [SerializeField, BoxGroup("References")] public Camera levelCamera;
    [SerializeField, BoxGroup("References")] public SceneController sceneController;
    [SerializeField, BoxGroup("References")] public PauseMenuController pauseController; 
    [SerializeField, BoxGroup("References")] public RectTransform compassBg;
    [SerializeField, BoxGroup("References")] public RectTransform compassPointer;
    [SerializeField, BoxGroup("References")] public ItemHUDController itemController;
    [SerializeField, BoxGroup("References")] public RectTransform tutorialMessage;
    [SerializeField, BoxGroup("References")] public RectTransform sideMessagesParent;
    [SerializeField, BoxGroup("References")] public GameObject boat;
    [SerializeField, BoxGroup("References")] public GameObject waterAndReflex;
    [SerializeField, BoxGroup("References")] public GameObject whale;
    [SerializeField, BoxGroup("References")] public GameObject island;
    [SerializeField, BoxGroup("References")] public GameObject character;
    [SerializeField, BoxGroup("References")] public List<Transform> islandSpawns;
    //[SerializeField, BoxGroup("References")] public GameObject bubblePrefab;

    [SerializeField, ReorderableList, BoxGroup("Debug")] internal List<BubbleController> allBubbles = new();
    [SerializeField, ReorderableList, ReadOnly, BoxGroup("Debug")] internal List<TextMeshProUGUI> sideMessages = new();
    [ShowNonSerializedField] internal bool showingText =  false;

    internal BoatController player;
    public BoatController Player => this.player;    
    public static LevelManager currentInstance;

    private bool shouldPoolCompass = true;
    private List<BubbleController> whaledBubbles;
    [ShowNonSerializedField] private bool ending = false;
    [ShowNonSerializedField] internal bool ended = false;

    void Awake()
    {
        Application.targetFrameRate = 60;
        currentInstance = this;
        this.player = this.boat.GetComponent<BoatController>();
        this.allBubbles = new List<BubbleController>(FindObjectsOfType<BubbleController>());
        this.waterAndReflex.SetActive(true);
        this.whale.SetActive(false);
        this.island.SetActive(false);

        this.sideMessages = new List<TextMeshProUGUI>(this.sideMessagesParent.GetComponentsInChildren<TextMeshProUGUI>(true));

        this.tutorialMessage.gameObject.SetActive(false);

        this.player.jammed = true;
        this.ExecuteAfter(() => this.ShowTutorial(), this.tutorialIntervalToShow); 
    }

    // Start is called before the first frame update
    void Start() {}

    void Update()
    {
        if(this.shouldPoolCompass) {
            this.shouldPoolCompass = false;
            this.PoolCompassDirection();
            this.ExecuteAfter(() => {
                this.shouldPoolCompass = true;
            }, this.compassInterval);
        }
    }

    void PoolCompassDirection() {
        var min = Mathf.Infinity;
        GameObject found = null;

        if(!this.ending) { 
            foreach(var b in this.allBubbles) {
                var dist = Vector2.Distance(this.player.transform.position, b.transform.position);
                if(dist < min) {
                    min = dist;
                    found = b.gameObject;
                }
            }
        } else {
            found = this.island.gameObject;
        }
        if(found == null) {
            this.compassPointer.DORotate(new Vector3(0f, 0f, -90f), this.compassInterval, RotateMode.Fast).SetRelative(true).SetEase(Ease.Linear);
        } else {
            var angle = this.AngleLocal(found.transform.position, this.player.transform.position) + 90;
            this.compassPointer.DORotate(new Vector3(0f, 0f, angle), this.compassInterval, RotateMode.Fast);
        }
    }

    public void ShowSideMessage(int index) {
        var msg = this.sideMessages[index];
        var originalColor = msg.color;
        var transp = originalColor - Color.black;

        msg.color = transp;
        msg.gameObject.SetActive(true);

        this.showingText = true;
        var sequence = DOTween.Sequence();
        sequence.Append(msg.DOColor(originalColor, this.messagesFadeDuration));
        sequence.AppendInterval(this.messagesDuration);
        sequence.OnComplete(() => {
            var sequenceFadeOut = DOTween.Sequence();
            var actualPosition = msg.rectTransform.position;
        
            sequenceFadeOut.Append(msg.DOColor(transp, this.messagesFadeDuration));
            sequenceFadeOut.OnComplete(() => {
                msg.gameObject.SetActive(false);
                this.showingText = false;
            });
        });
    }

    public void ShowTutorial() {
        var texts = this.tutorialMessage.GetComponentsInChildren<TextMeshProUGUI>(true);
        var bgs = this.tutorialMessage.GetComponentsInChildren<Image>(true);

        Tween tw = null;
        foreach(var text in texts) {
            var initialTextColor = text.color - Color.black;
            var finalTextColor = new Color(text.color.r, text.color.g, text.color.b, 1);

            text.color = initialTextColor;
            tw = text.DOColor(finalTextColor, this.tutorialFadeDuration);
        }
        tw?.OnComplete(() => {
            this.player.jammed = false;
        });

        foreach(var bg in bgs) {
            var initialBGColor = bg.color - Color.black;
            var finalBGColor = new Color(bg.color.r, bg.color.g, bg.color.b, 1);

            bg.color = initialBGColor;
            bg.DOColor(finalBGColor, this.tutorialFadeDuration);
        }

        this.tutorialMessage.gameObject.SetActive(true);
    }

    public void HideTutorial() {
        var texts = this.tutorialMessage.GetComponentsInChildren<TextMeshProUGUI>();
        var bgs = this.tutorialMessage.GetComponentsInChildren<Image>();

        Tween tw = null;
        foreach(var text in texts) {
            var finalTextColor = text.color - Color.black;
            tw = text.DOColor(finalTextColor, this.tutorialFadeDuration);
        }
        tw?.OnComplete(() => { this.tutorialMessage.gameObject.SetActive(false); });

        foreach(var bg in bgs) {
            var finalBGColor = bg.color - Color.black;
            bg.DOColor(finalBGColor, this.tutorialFadeDuration).OnComplete(() => { });
        }
    }

    public void WhaleItAllUp() {
        this.whaledBubbles = new List<BubbleController>(this.player.chasingBubbles.ToArray());
        this.whale.transform.position = new Vector3(this.player.whalePivot.position.x, this.player.whalePivot.position.y + UnityEngine.Random.Range(-2.2f, 2.2f), this.whale.transform.position.z);
        this.whale.gameObject.SetActive(true);
        this.whale.GetComponentInChildren<Animator>().SetTrigger("Jump");
        this.whale.GetComponentInChildren<FMODUnity.StudioEventEmitter>().Play();
    }

    public void Whaled() {
        this.ExecuteAfter(() => {
            foreach(var b in this.whaledBubbles) {
                b.Pop(true);
                this.player.RemoveBubbleReferences(b);
            }

            this.player.chaseBlocked = false;
            this.player.jammed = false;
        }, this.whaleWaitingInterval);
    }

    public void ObtainNextItem(bool isPartItem = false) {
        this.itemController.SpawnAndShowItem(isPartItem);
        this.player.jammed = true;
    }

    public void ItemWaitForInput() {
        //Assuming player is already Jammed
        this.player.waitingItemDismiss = true;
    }

    public void ItemInputRecieved() {
        this.itemController.FromFrameToSlot();
    }

    public void ItemObtained(int index) {
        var preRandomValue = UnityEngine.Random.Range(this.specialEventsTimeRange.x, this.specialEventsTimeRange.y);
        switch(index) {
            case 0: //Nothing special
                break;
            case 1: //Nothing special
                break;
            case 2:
                AudioController.Instance.FirstAdvanceMusic();
                this.player.PartTime(preRandomValue); //First Part will be picked
                break;
            case 3:
                this.ShowSideMessage(0); //Part Text - 0
                this.player.chaseBlocked = false; //Reactivates bubble chases
                break;
            case 4: //Nothing special
                break;
            case 5: //Nothing special
                break;
            case 6:
                AudioController.Instance.SecondAdvanceMusic();
                this.player.PartTime(preRandomValue); //Second Part will be picked
                break;
            case 7:
                this.ShowSideMessage(1); //Part Text - 1
                this.player.chaseBlocked = false; //Reactivates bubble chases
                this.player.WhaleTime();
                break;
            case 8: //Nothing special
                this.player.WhaleTime();
                break;
            case 9: //Nothing special
                this.player.WhaleTime();
                break;
            case 10:
                AudioController.Instance.SecondAdvanceMusic();
                this.player.PartTime(preRandomValue); //Last Part will be picked
                break;
            case 11:
                //this.player.chaseBlocked = false; //Not needed
                this.ShowSideMessage(2); //Part Text - 2
                AudioController.Instance.FinalizeNormalAndStartWOWMusic();
                this.EndTime();
                break;
            case 99:
                this.ShowSideMessage(3); //Final Text
                break;
        }

        //Check if all items were grabbed or even toggle more events;
        this.player.jammed = false;
    }

    public void EndTime() {
        this.player.chaseBlocked = true;
        this.player.jammed = true;

        this.DisableHazards();

        this.player.ObtainedCharacter();
        foreach(var b in this.allBubbles.ToArray()) {
            b.Pop(true);
            this.player.RemoveBubbleReferences(b);
        }

        this.ExecuteAfter(() => {
            this.islandSpawns.Sort((a, b) => Vector2.Distance(a.transform.position, this.player.transform.position) < Vector2.Distance(b.transform.position, this.player.transform.position) ? -1 : 1);

            Transform selected;
            if(this.islandSpawns.Count > 3) {
                selected = this.islandSpawns[^3];
            } else {
                selected = this.islandSpawns[^1];
            }

            this.compassBg.GetComponent<Image>().color = Color.red;
            this.island.transform.position = selected.position;
            this.island.SetActive(true);
            this.ending = true;
            AudioController.Instance.StartEUFOMusic();
        }, this.waitingEndingInterval);
    }

    public void DisableHazards() {
        var allFishs = FindObjectsOfType<FishController>();
        var allWhirlpool = FindObjectsOfType<WhirlpoolController>();

        foreach(var fish in allFishs) {
            fish.DisableFadeAndDestroy();
        }

        foreach(var whirlpool in allWhirlpool) {
            whirlpool.DisableFadeAndDestroy();
        }
    }

    public void EndGame() {
        this.ItemObtained(99);
        this.player.ToBeDestroyed(false);
        this.player.FadeAndDestroyTrail();
        this.pauseController.ForceDisableLeave();
        AudioController.Instance.FoundIslandMusicEnding();

        this.character.SetActive(true);
    }

    public void CharacterReachedEnd() {
        this.whale.GetComponentInChildren<FMODUnity.StudioEventEmitter>().Play();
        this.ExecuteAfter(() => {
            this.sceneController.GotoCredits();
        }, this.characterFinalSceneWaitingTime);
    }

    public void BoatDied() {
        if(!this.ending) { //This only works if all enemies and hazards are DeSpawned after getting all items.
            this.ExecuteAfter(() => {
                this.sceneController.FakeEnd(
                    this.loseWaitingInterval,
                    () => {
                        this.player.ResetAll();
                    },
                    () => {
                        this.player.jammed = false;
                    }
                );
            }, this.loseWaitingInterval);
        } //else {} //Else it's the end of the game, boat is supposed to die.
    }

    public void LeaveGame() {
        this.player.jammed = true;
        Time.timeScale = 1;
        this.sceneController.BackToMenu();
    }

    public void SetWorldMinMaxZIndex(ZIndexFix fixData) {
        if(fixData != null) {
            var camController = this.levelCamera.GetComponent<CameraController>();
            fixData.minY = camController.farDown;
            fixData.maxY = camController.farUp;
            fixData.minZ = this.minMaxAutoFixZRange.x;
            fixData.maxZ = this.minMaxAutoFixZRange.y;
        }
    }

    private float AngleLocal(Vector2 p1, Vector2 p2) => Mathf.Atan2(p2.y - p1.y, p2.x - p1.x) * 180 / Mathf.PI;

    private void ExecuteAfter(Action stuff, float waitingTime) {
        StartCoroutine(this.ExecuteAfterCR(stuff, waitingTime));
    }

    private IEnumerator ExecuteAfterCR(Action stuff, float waitingTime) {
        yield return new WaitForSeconds(waitingTime);
        stuff?.Invoke();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        foreach(var sSpot in this.islandSpawns) {
            Gizmos.DrawWireCube(sSpot.transform.position, Vector3.one * 3);
        }
    }
}
