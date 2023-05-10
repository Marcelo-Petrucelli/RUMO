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
    [SerializeField, BoxGroup("References")] public RectTransform itemController;
    [SerializeField, BoxGroup("References")] public GameObject boat;
    [SerializeField, BoxGroup("References")] public GameObject waterAndReflex;
    [SerializeField, BoxGroup("References")] public GameObject bubblePrefab;
    [SerializeField, BoxGroup("References")] public GameObject itemPrefab;

    [SerializeField, ReadOnly, TextArea(maxLines:1, minLines:1), BoxGroup("References")] private string descSprites = "Na ordem " + string.Join(", ", LevelManager.itemSpritesNames.ToArray());
    [SerializeField, BoxGroup("References")] public List<Sprite> itemSprites;

    [ShowNonSerializedField] internal BoatController player;

    public BoatController Player => this.player;

    public static LevelManager currentInstance;

    private int currentItemIndex = -1;

    void Awake()
    {
        currentInstance = this;
        this.player = this.boat.GetComponent<BoatController>();
        this.waterAndReflex.SetActive(true);
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    public void PoppedBubble(BubbleController bubble) {
        if(this.currentItemIndex > this.itemSprites.Count - 1) {
            return;
        }

        this.currentItemIndex++;

        var bubbleWorldPosition = Camera.current.WorldToViewportPoint(bubble.transform.position);
        Instantiate(this.itemPrefab, bubbleWorldPosition, Quaternion.identity);

        
    }
}
