using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using NaughtyAttributes;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class CutSceneText : MonoBehaviour
{
    [SerializeField, BoxGroup("Texts")] public List<TextMeshProUGUI> texts;    

    [SerializeField, BoxGroup("Text transition config")] public float initialWaiting = 1f;
    [SerializeField, BoxGroup("Text transition config")] public float textWaitingInterval = 1f;
    [SerializeField, BoxGroup("Text transition config")] public float textFadeInDuration = 1f;   

    private void Start()
    {        
        var sequence = DOTween.Sequence();
        sequence.AppendInterval(initialWaiting);
        foreach (var text in this.texts) {
            sequence.Append(TextAnimation(text));
            sequence.AppendInterval(textWaitingInterval);
        }       
    }

    public Tween TextAnimation(TextMeshProUGUI currentText) {        
        currentText.gameObject.SetActive(true);        
        var originalColor = currentText.color;
        var transp = currentText.color - Color.black;
        currentText.color = transp;

        return currentText.DOColor(originalColor, textFadeInDuration).OnComplete(() => {
            currentText.GetComponent<Wobble>().WobbleOut();
        });
    }
}
