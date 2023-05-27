using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using NaughtyAttributes;
using TMPro;
using UnityEngine;

public class Wobble : MonoBehaviour
{
    [SerializeField, BoxGroup("Text effect config")] public TMP_Text tmpText;    
    [SerializeField, BoxGroup("Text effect config")] public float velocityX = 7f;
    [SerializeField, BoxGroup("Text effect config")] public float velocityY = 7f;
    [SerializeField, BoxGroup("Text effect config"), MinMaxSlider(0f, 10f)] public Vector2 intensityX;
    [SerializeField, BoxGroup("Text effect config"), MinMaxSlider(0f, 10f)] public Vector2 intensityY;
    [SerializeField, BoxGroup("Text effect config")] public float wobbleDuration = 1.5f;

    private Mesh mesh;
    private Vector3[] vertices;
    [ShowNonSerializedField] private Vector2 currentIntensity = new (0f, 0f);

    private void Start()
    {
        currentIntensity = new Vector2(intensityX.y, intensityY.y);        
    }

    private void LateUpdate()
    {
        this.tmpText.ForceMeshUpdate();
        this.mesh = tmpText.mesh;
        vertices = mesh.vertices;

        for (int i = 0; i < vertices.Length; i++) {
            Vector3 offset = WoobleWobble(Time.time + i);

            vertices[i] = vertices[i] + offset;
        }
        mesh.vertices = vertices;
        tmpText.canvasRenderer.SetMesh(mesh);
    }

    public Vector2 WoobleWobble(float time) {
        return new Vector2(Mathf.Sin(time * velocityX) * currentIntensity.x, Mathf.Cos(time * velocityY) * currentIntensity.y);
    }

    public void WobbleOut() {        
        DOTween.To(() => currentIntensity.x, ix => currentIntensity.x = ix, intensityX.x, wobbleDuration);
        DOTween.To(() => currentIntensity.y, iy => currentIntensity.y = iy, intensityY.x, wobbleDuration);
    }
}
