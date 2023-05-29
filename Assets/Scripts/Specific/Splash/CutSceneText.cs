using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using NaughtyAttributes;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class CutSceneText : MonoBehaviour
{
    [SerializeField, BoxGroup("References")] public List<TextMeshProUGUI> texts;    

    [SerializeField, BoxGroup("Config")] public float initialWaiting = 1f;
    [SerializeField, BoxGroup("Config")] public float textWaitingInterval = 1f;
    [SerializeField, BoxGroup("Config")] public float textFadeInDuration = 1f;
    [SerializeField, BoxGroup("Config")] public float endSceneWaitingTime = 5f;

    private SceneController sceneController;

    private void Awake()
    {
        this.sceneController = FindObjectOfType<SceneController>();
    }

    private void Start()
    {        
        var sequence = DOTween.Sequence();
        sequence.AppendInterval(this.initialWaiting);
        foreach (var text in this.texts) {
            sequence.Append(this.TextAnimation(text));
            sequence.AppendInterval(this.textWaitingInterval);
        }
        sequence.AppendInterval(this.endSceneWaitingTime).OnComplete(() => {
            this.sceneController.GoToLevel(0);
        });

        AudioController.Instance?.PlayDub(0, this.initialWaiting + (this.textWaitingInterval * 2));
    }

    public Tween TextAnimation(TextMeshProUGUI currentText) {        
        currentText.gameObject.SetActive(true);        
        var originalColor = currentText.color;
        var transp = currentText.color - Color.black;
        currentText.color = transp;

        return currentText.DOColor(originalColor, this.textFadeInDuration).OnComplete(() => {
            currentText.GetComponent<Wobble>().WobbleOut();
        });
    }
}
