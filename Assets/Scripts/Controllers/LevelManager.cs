using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using DG.Tweening;
using TMPro;
using System;

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

    [SerializeField, BoxGroup("References")] public Camera levelCamera;
    [SerializeField, BoxGroup("References")] public ItemHUDController itemController;
    [SerializeField, BoxGroup("References")] public List<TextMeshProUGUI> messages;
    [SerializeField, BoxGroup("References")] public GameObject boat;
    [SerializeField, BoxGroup("References")] public GameObject waterAndReflex;
    [SerializeField, BoxGroup("References")] public GameObject messagesParent;
    [SerializeField, BoxGroup("References")] public GameObject bubblePrefab;

    [SerializeField, ReadOnly, TextArea(maxLines:1, minLines:1), BoxGroup("References")] private string descSprites = "Na ordem " + string.Join(", ", LevelManager.itemSpritesNames.ToArray());
    [SerializeField, BoxGroup("References")] public List<Sprite> itemSprites;

    [ShowNonSerializedField] internal BoatController player;
    [ShowNonSerializedField] internal bool showingText =  false;

    public BoatController Player => this.player;

    public static LevelManager currentInstance;

    void Awake()
    {
        Application.targetFrameRate = 60;
        currentInstance = this;
        this.player = this.boat.GetComponent<BoatController>();
        this.waterAndReflex.SetActive(true);
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    private void ShowMessage(int index) {
        var msg = this.messages[index];
        var originalColor = msg.color;
        var transp = originalColor - Color.black;

        msg.color = transp;
        msg.gameObject.SetActive(true);

        this.showingText = true;
        var sequence = DOTween.Sequence();
        sequence.Append(msg.DOColor(originalColor, this.messagesFadeDuration));
        sequence.AppendInterval(this.messagesDuration);
        sequence.Append(msg.DOColor(transp, this.messagesFadeDuration));
        sequence.OnComplete(() => {
            msg.gameObject.SetActive(false);
            this.showingText = false;
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
                this.ShowMessage(2);
                break;
            case 3:
                this.ShowMessage(3);
                break;
            case 4:
                this.ShowMessage(4);
                break;
            case 99:
                this.ShowMessage(5);
                break;
        }

        //Check if all items were grabbed or even toggle more events;
        this.player.jammed = false;
    }
}
