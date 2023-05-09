﻿using NaughtyAttributes;
using UnityEngine;

public class CameraController:MonoBehaviour
{
    public Transform cameraFollow;
    public Transform cameraLimits; //limits for the camera movement
    public Vector2 smoothSpeed = new(1f, 1f);
    public float smoothDuration = 0.3f;
    public Vector2 offset = new(0f, 0f); //how much the camera will move past the player

    private Camera myCamera;
    private Vector2 halfCam;
    private Vector3 actualSmoothSpeed;
    private float farLeft => this.cameraLimits.position.x - (this.cameraLimits.localScale.x / 2f);
    private float farRight => this.cameraLimits.position.x + (this.cameraLimits.localScale.x / 2f);
    private float farUp => this.cameraLimits.position.y + (this.cameraLimits.localScale.y / 2f);
    private float farDown => this.cameraLimits.position.y - (this.cameraLimits.localScale.y / 2f);

    // Start is called before the first frame update
    void Start() {
        this.myCamera = this.GetComponent<Camera>();
        this.transform.position = new Vector3(this.cameraFollow.transform.position.x, this.cameraFollow.transform.position.y, this.transform.position.z);
        this.halfCam = new Vector2(this.myCamera.orthographicSize * this.myCamera.aspect, this.myCamera.orthographicSize);
        this.actualSmoothSpeed = this.smoothSpeed;
    }

    // Update is called once per frame
    void Update() {
        //gets follow position in x axis + offset
        var anchorPosition = new Vector3(
            this.cameraFollow.position.x + (Mathf.Sign(this.cameraFollow.localScale.x) * this.offset.x),
            this.cameraFollow.position.y + (Mathf.Sign(this.cameraFollow.localScale.y) * this.offset.y),
            this.transform.position.z
        );

        //this.transform.position = Vector3.Lerp(this.transform.position, anchorPosition, Time.deltaTime); //smooths camera movement
        //var dist = Vector2.Distance(this.transform.position, anchorPosition);
        this.transform.position = Vector3.SmoothDamp(this.transform.position, anchorPosition, ref this.actualSmoothSpeed, this.smoothDuration);
        //this.transform.position = new Vector3(this.cameraFollow.position.x, this.cameraFollow.position.y, this.transform.position.z);

        //can't pass the limits
        if(this.transform.position.x < this.farLeft + this.halfCam.x) {
            this.transform.position = new Vector3(this.farLeft + this.halfCam.x, this.transform.position.y, this.transform.position.z);
        }
        if(this.transform.position.x > this.farRight - this.halfCam.x) {
            this.transform.position = new Vector3(this.farRight - this.halfCam.x, this.transform.position.y, this.transform.position.z);
        }
        if(this.transform.position.y < this.farDown + this.halfCam.y) {
            this.transform.position = new Vector3(this.transform.position.x, this.farDown + this.halfCam.y, this.transform.position.z);
        }
        if(this.transform.position.y > this.farUp - this.halfCam.y) {
            this.transform.position = new Vector3(this.transform.position.x, this.farUp - this.halfCam.y, this.transform.position.z);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawLine(new Vector2(this.farLeft, this.farUp), new Vector2(this.farRight, this.farUp)); //TopLeft, TopRight
        Gizmos.DrawLine(new Vector2(this.farLeft, this.farUp), new Vector2(this.farLeft, this.farDown)); //TopLeft, BottomLeft

        Gizmos.DrawLine(new Vector2(this.farRight, this.farDown), new Vector2(this.farRight, this.farUp)); //BottomRight, TopRight
        Gizmos.DrawLine(new Vector2(this.farRight, this.farDown), new Vector2(this.farLeft, this.farDown)); //BottomRight, BottomLeft
    }
}
