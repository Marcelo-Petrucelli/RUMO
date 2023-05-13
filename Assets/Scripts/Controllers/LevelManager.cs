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
    public static List<string> itemSpritesNames = new() { "Book", "Controller", "Pets", "IceCream", "Shoes", "Fish", "Camera", "Kid" };

    [SerializeField, BoxGroup("Config")] public int levelIndex;
    [SerializeField, BoxGroup("Config")] public float messagesFadeDuration = 0.8f;
    [SerializeField, BoxGroup("Config")] public float messagesDuration = 4f;
    [SerializeField, BoxGroup("Config")] public float compassInterval = 0.3f;
    [SerializeField, BoxGroup("Config")] public Color compassEndingColor = Color.red;
    [SerializeField, BoxGroup("Config")] public float whaleWaitingInterval = 1.2f;
    [SerializeField, BoxGroup("Config")] public float characterFinalSceneWaitingTime = 10f;

    [SerializeField, BoxGroup("References")] public Camera levelCamera;
    [SerializeField, BoxGroup("References")] public SceneController sceneController;
    [SerializeField, BoxGroup("References")] public RectTransform compassBg;
    [SerializeField, BoxGroup("References")] public RectTransform compassPointer;
    [SerializeField, BoxGroup("References")] public ItemHUDController itemController;
    [SerializeField, BoxGroup("References")] public TextMeshProUGUI tutorialMessage;
    [SerializeField, BoxGroup("References")] public List<TextMeshProUGUI> messages;
    [SerializeField, BoxGroup("References")] public GameObject boat;
    [SerializeField, BoxGroup("References")] public GameObject waterAndReflex;
    [SerializeField, BoxGroup("References")] public GameObject messagesParent;
    [SerializeField, BoxGroup("References")] public GameObject whale;
    [SerializeField, BoxGroup("References")] public GameObject island;
    [SerializeField, BoxGroup("References")] public GameObject character;
    [SerializeField, BoxGroup("References")] public List<Transform> islandSpawns;
    //[SerializeField, BoxGroup("References")] public GameObject bubblePrefab;

    [SerializeField, ReadOnly, TextArea(maxLines:1, minLines:1), BoxGroup("References")] private string descSprites = "Na ordem " + string.Join(", ", LevelManager.itemSpritesNames.ToArray());
    [SerializeField, BoxGroup("References")] public List<Sprite> itemSprites;

    [SerializeField, ReorderableList, ReadOnly] internal List<BubbleController> allBubbles = new();
    [ShowNonSerializedField] internal BoatController player;
    [ShowNonSerializedField] internal bool showingText =  false;

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

        this.tutorialMessage.gameObject.SetActive(true);
        this.waterAndReflex.SetActive(true);
        this.whale.gameObject.SetActive(false);
        this.island.gameObject.SetActive(false);
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

    private void ShowMessage(int index) {
        var msg = this.messages[index];
        var originalColor = msg.color;
        var transp = originalColor - Color.black;
        var colorTeste = Color.blue;

        msg.color = transp;
        msg.gameObject.SetActive(true);

        this.showingText = true;
        var sequence = DOTween.Sequence();
        sequence.Append(msg.DOColor(originalColor, this.messagesFadeDuration));
        sequence.AppendInterval(this.messagesDuration);
        sequence.OnComplete(() => {
            var sequenceFadeOut = DOTween.Sequence();
            var actualDirection = new Vector3(0, 0, 0);
            var actualPosition = msg.rectTransform.position;

            if(this.Player.right) {
                actualDirection = new Vector3(300, 0, 0);
            } else if(this.Player.left) {
                actualDirection = new Vector3(-300, 0, 0);
            } else if(this.Player.up) {
                actualDirection = new Vector3(0, 300, 0);
            } else if(this.Player.down) {
                actualDirection = new Vector3(0, -300, 0);
            }       
            
            sequenceFadeOut.Append(msg.rectTransform.DOMove((actualPosition - actualDirection), this.messagesFadeDuration).SetEase(Ease.InOutCirc));           
            sequenceFadeOut.Join(msg.DOColor(transp, this.messagesFadeDuration));
            sequenceFadeOut.OnComplete(() => {
                msg.gameObject.SetActive(false);
                this.showingText = false;

            });
        });
    }

    public void HideTutorial() {
        var transp = this.tutorialMessage.color - Color.black;
        this.tutorialMessage.DOColor(transp, 0.3f).OnComplete(() => { this.tutorialMessage.gameObject.SetActive(false); });
    }

    public void ObtainNextItem() {
        this.itemController.SpawnAndMoveToIventory();
        this.player.jammed = true;
    }

    public void WhaleItAllUp() {
        this.whaledBubbles = new List<BubbleController>(this.player.chasingBubbles.ToArray());
        this.whale.transform.position = new Vector3(this.player.whalePivot.position.x, this.player.whalePivot.position.y + UnityEngine.Random.Range(-2.2f, 2.2f), this.whale.transform.position.z);
        this.whale.gameObject.SetActive(true);
        this.whale.GetComponentInChildren<Animator>().SetTrigger("Jump");
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

    public void ItemObtained(int index) {
        //Mudar caso seja necessário mudar as mensagens
        switch(index) {
            case 0:
                this.ShowMessage(0); //Livro de receitas
                this.player.WhaleTime();
                break;
            case 1:
                this.ShowMessage(1); //VideoGame controller
                this.player.WhaleTime();
                break;
            case 2:
                this.ShowMessage(2); //Pets
                this.player.WhaleTime();
                break;
            case 3:
                this.ShowMessage(3); //IceCream
                this.player.WhaleTime();
                break;
            case 4:
                this.ShowMessage(4); //Shoes
                this.player.FishTime(5f);
                this.player.WhaleTime();
                break;
            case 5:
                this.player.chaseBlocked = false;
                break;
            case 6:
                this.ShowMessage(5); //Camera
                this.player.KidTime(5f);
                break;
            case 7:
                this.EndTime(6f);
                break;
            case 99:
                this.ShowMessage(6); //Final Text
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
            if(this.islandSpawns.Count > 2) {
                selected = this.islandSpawns[^2];
            } else {
                selected = this.islandSpawns[^1];
            }

            this.compassBg.GetComponent<Image>().color = Color.red;
            this.island.transform.position = selected.position;
            this.island.gameObject.SetActive(true);
            this.ending = true;
        }, durationSec);
    }

    public void EndGame() {
        this.ItemObtained(99);
        this.player.jammed = true;

        this.character.gameObject.SetActive(true);
    }

    public void CharacterReachedEnd() {
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
