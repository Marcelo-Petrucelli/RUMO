using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using DG.Tweening;
using TMPro;
using System;
using System.Collections;

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

    [SerializeField, BoxGroup("References")] public Camera levelCamera;
    [SerializeField, BoxGroup("References")] public RectTransform compassPointer;
    [SerializeField, BoxGroup("References")] public ItemHUDController itemController;
    [SerializeField, BoxGroup("References")] public List<TextMeshProUGUI> messages;
    [SerializeField, BoxGroup("References")] public GameObject boat;
    [SerializeField, BoxGroup("References")] public GameObject waterAndReflex;
    [SerializeField, BoxGroup("References")] public GameObject messagesParent;
    //[SerializeField, BoxGroup("References")] public GameObject bubblePrefab;

    [SerializeField, ReadOnly, TextArea(maxLines:1, minLines:1), BoxGroup("References")] private string descSprites = "Na ordem " + string.Join(", ", LevelManager.itemSpritesNames.ToArray());
    [SerializeField, BoxGroup("References")] public List<Sprite> itemSprites;

    [SerializeField, ReorderableList, ReadOnly] internal List<BubbleController> allBubbles = new();
    [ShowNonSerializedField] internal BoatController player;
    [ShowNonSerializedField] internal bool showingText =  false;

    public BoatController Player => this.player;    
    public static LevelManager currentInstance;

    private bool shouldPoolCompass = true;

    void Awake()
    {
        Application.targetFrameRate = 60;
        currentInstance = this;
        this.player = this.boat.GetComponent<BoatController>();
        this.waterAndReflex.SetActive(true);
        this.allBubbles = new List<BubbleController>(FindObjectsOfType<BubbleController>());
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
        BubbleController found = null;
        foreach(var b in this.allBubbles) {
            var dist = Vector2.Distance(this.player.transform.position, b.transform.position);
            if(dist < min) {
                min = dist;
                found = b;
            }
        }
        if(found == null) {
            //No more bubbles

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

        if(Player.right) {
            actualDirection = new Vector3(300, 0, 0);
        } else if(Player.left) {
            actualDirection = new Vector3(-300, 0, 0);
        } else if(Player.up) {
            actualDirection = new Vector3(0, 300, 0);
        } else if(Player.down) {
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

    public void ObtainNextItem() {
        this.itemController.SpawnAndMoveToIventory();
        this.player.jammed = true;
    }

    public void ItemObtained(int index) {
        //Mudar caso seja necessário mudar as mensagens
        switch(index) {
            case 0:
                this.ShowMessage(0); //Livro de receitas
                break;
            case 1:
                this.ShowMessage(1); //VideoGame controller
                break;
            case 2:
                this.ShowMessage(2); //Pets
                this.player.WhaleTime(3f);
                break;
            case 3:
                this.ShowMessage(3); //IceCream
                break;
            case 4:
                this.ShowMessage(4); //Shoes
                this.player.FishTime(3f);
                break;
            case 5:
                this.ShowMessage(5); //Camera
                this.player.KidTime(3f); //Also Spawn BIRD
                break;
            case 99:
                this.ShowMessage(5); //Final Text
                break;
        }

        //Check if all items were grabbed or even toggle more events;
        this.player.jammed = false;
    }

    private float AngleLocal(Vector2 p1, Vector2 p2) => Mathf.Atan2(p2.y - p1.y, p2.x - p1.x) * 180 / Mathf.PI;

    private void ExecuteAfter(Action stuff, float waitingTime) {
        StartCoroutine(this.ExecuteAfterCR(stuff, waitingTime));
    }

    private IEnumerator ExecuteAfterCR(Action stuff, float waitingTime) {
        yield return new WaitForSeconds(waitingTime);
        stuff?.Invoke();
    }
}
