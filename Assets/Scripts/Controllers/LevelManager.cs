using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;

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
    [SerializeField, BoxGroup("References")] public Camera levelCamera;
    [SerializeField, BoxGroup("References")] public RectTransform itemController;
    [SerializeField, BoxGroup("References")] public GameObject boat;
    [SerializeField, BoxGroup("References")] public GameObject waterAndReflex;
    [SerializeField, BoxGroup("References")] public GameObject messagesParent;
    [SerializeField, BoxGroup("References")] public GameObject bubblePrefab;

    [SerializeField, ReadOnly, TextArea(maxLines:1, minLines:1), BoxGroup("References")] private string descSprites = "Na ordem " + string.Join(", ", LevelManager.itemSpritesNames.ToArray());
    [SerializeField, BoxGroup("References")] public List<Sprite> itemSprites;
    [SerializeField, BoxGroup("References")] public List<RectTransform> itemTexts;

    [ShowNonSerializedField] internal BoatController player;

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

    public void ObtainNextItem() {
        this.itemController.GetComponent<ItemHUDController>().SpawnAndMoveToIventory();
        this.player.jammed = true;
    }

    public void ItemObtained() {
        //Check if all items were grabbed or even toggle more events;
        this.player.jammed = false;
    }
}
