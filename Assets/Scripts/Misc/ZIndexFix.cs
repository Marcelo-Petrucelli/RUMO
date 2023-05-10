using UnityEngine;

public class ZIndexFix : MonoBehaviour
{
    [SerializeField] public bool fixing = true;
    [SerializeField] private float minY;
    [SerializeField] private float maxY;

    [SerializeField] private float minZ;
    [SerializeField] private float maxZ;
    
    private Transform attachement;

    // Start is called before the first frame update
    void Start()
    {
        this.attachement = this.GetComponent<Transform>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if(this.fixing) {
            var newY = this.Remap(this.attachement.position.y, this.minY, this.maxY, this.minZ, this.maxZ);
            this.attachement.position = new Vector3(this.attachement.position.x, this.attachement.position.y, newY);
        }
    }

    public float Remap(float value, float from1, float to1, float from2, float to2) => (value - from1) / (to1 - from1) * (to2 - from2) + from2;
}
