using NaughtyAttributes;
using UnityEngine;

public class ZIndexFix : MonoBehaviour
{
    [SerializeField, BoxGroup("Config")] public bool autoFromLM = true;
    [SerializeField, BoxGroup("Config")] public bool fixing = true;
    [SerializeField, BoxGroup("Config")] internal float minY;
    [SerializeField, BoxGroup("Config")] internal float maxY;
    [SerializeField, BoxGroup("Config")] internal float minZ;
    [SerializeField, BoxGroup("Config")] internal float maxZ;
    
    private Transform attachement;

    // Start is called before the first frame update
    void Start()
    {
        this.attachement = this.GetComponent<Transform>();
        if(this.autoFromLM) {
            LevelManager.currentInstance?.SetWorldMinMaxZIndex(this);
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if(this.fixing) {
            var zPos = this.Remap(this.attachement.position.y, this.minY, this.maxY, this.minZ, this.maxZ);
            this.attachement.position = new Vector3(this.attachement.position.x, this.attachement.position.y, zPos);
        }
    }

    public float Remap(float value, float from1, float to1, float from2, float to2) => (value - from1) / (to1 - from1) * (to2 - from2) + from2;
}
