using System.Collections.Generic;
using System.Collections;
using NaughtyAttributes;
using UnityEngine.UI;
using UnityEngine;
using DG.Tweening;
using System;
using TMPro;
using UnityEngine.SceneManagement;

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
    [SerializeField, BoxGroup("Config")] public int levelIndex;
    [SerializeField, BoxGroup("Config")] public float messagesFadeDuration = 0.8f;
    [SerializeField, BoxGroup("Config")] public float messagesDuration = 4f;
    [SerializeField, BoxGroup("Config")] public float compassInterval = 0.3f;
    [SerializeField, BoxGroup("Config")] public Color compassEndingColor = Color.red;
    [SerializeField, BoxGroup("Config")] public float characterFinalSceneWaitingTime = 10f;
    [SerializeField, BoxGroup("Config")] public float tutorialIntervalToShow = 0.6f;
    [SerializeField, BoxGroup("Config")] public float tutorialFadeDuration = 0.4f;
    [SerializeField, BoxGroup("Config"), MinMaxSlider(0f, 30f)] public Vector2 specialEventsTimeRange = new (4f, 9f);
    [SerializeField, BoxGroup("Config")] public float whaleWaitingInterval = 1.2f;

    [SerializeField, BoxGroup("References")] public Camera levelCamera;
    [SerializeField, BoxGroup("References")] public SceneController sceneController;
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

    [SerializeField, ReorderableList, ReadOnly, BoxGroup("Debug")] internal List<BubbleController> allBubbles = new();
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
            //var actualDirection = new Vector3(0, 0, 0);
            var actualPosition = msg.rectTransform.position;

            /*if(this.Player.right) {
                actualDirection = new Vector3(300, 0, 0);
            } else if(this.Player.left) {
                actualDirection = new Vector3(-300, 0, 0);
            } else if(this.Player.up) {
                actualDirection = new Vector3(0, 300, 0);
            } else if(this.Player.down) {
                actualDirection = new Vector3(0, -300, 0);
            }*/  
            
            //sequenceFadeOut.Append(msg.rectTransform.DOMove((actualPosition - actualDirection), this.messagesFadeDuration).SetEase(Ease.InOutCirc));           
            sequenceFadeOut.Append(msg.DOColor(transp, this.messagesFadeDuration));
            sequenceFadeOut.OnComplete(() => {
                msg.gameObject.SetActive(false);
                this.showingText = false;
            });
        });
    }

    public void ShowTutorial() {
        var text = this.tutorialMessage.GetComponentInChildren<TextMeshProUGUI>(true);
        var bg = this.tutorialMessage.GetComponentInChildren<Image>(true);

        var initialTextColor = text.color - Color.black;
        var finalTextColor = new Color(text.color.r, text.color.g, text.color.b, 1);
        var initialBGColor = bg.color - Color.black;
        var finalBGColor = new Color(bg.color.r, bg.color.g, bg.color.b, 1);

        this.tutorialMessage.gameObject.SetActive(true);

        text.color = initialTextColor;
        text.DOColor(finalTextColor, this.tutorialFadeDuration).OnComplete(() => {
            this.player.jammed = false;
        });

        bg.color = initialBGColor;
        bg.DOColor(finalBGColor, this.tutorialFadeDuration);
    }

    public void HideTutorial() {
        var text = this.tutorialMessage.GetComponentInChildren<TextMeshProUGUI>();
        var bg = this.tutorialMessage.GetComponentInChildren<Image>();

        var finalTextColor = text.color - Color.black;
        var finalBGColor = bg.color - Color.black;

        text.DOColor(finalTextColor, this.tutorialFadeDuration).OnComplete(() => { });
        bg.DOColor(finalBGColor, this.tutorialFadeDuration).OnComplete(() => { this.tutorialMessage.gameObject.SetActive(false); });
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
        //Mudar caso seja necessário mudar as mensagens
        //this.player.FishTime(5f);
        //this.player.WhirlpoolTime(5f);
        //this.player.WhaleTime();

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
                this.player.chaseBlocked = false; //Reactivates bubble chases
                break;
            case 8: //Nothing special
                break;
            case 9: //Nothing special
                break;
            case 10:
                AudioController.Instance.SecondAdvanceMusic();
                this.player.PartTime(preRandomValue); //Last Part will be picked
                break;
            case 11:
                //this.player.chaseBlocked = false; //Not needed
                AudioController.Instance.FinalizeNormalAndStartWOWMusic();
                this.EndTime(0.5f);
                break;
            case 99:
                this.ShowSideMessage(0); //Final Text
                break;
        }

        //Check if all items were grabbed or even toggle more events;
        this.player.jammed = false;
    }

    public void EndTime(float durationSec) {
        this.player.chaseBlocked = true;
        this.player.jammed = true;
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
        }, durationSec);
    }

    public void EndGame() {
        this.ItemObtained(99);
        this.player.jammed = true;
        AudioController.Instance.FoundIslandMusicEnding();

        this.character.SetActive(true);
    }

    public void CharacterReachedEnd() {
        this.whale.GetComponentInChildren<FMODUnity.StudioEventEmitter>().Play();
        this.ExecuteAfter(() => {
            this.sceneController.GotoCredits();
        }, this.characterFinalSceneWaitingTime);
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
