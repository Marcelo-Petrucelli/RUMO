using System.Collections.Generic;
using System.Collections;
using NaughtyAttributes;
using UnityEngine.UI;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class PauseMenuController: MonoBehaviour
{
    [SerializeField, BoxGroup("References")] private RectTransform animatedMenu;
    [SerializeField, BoxGroup("References")] private Button menuToggleButton;
    [SerializeField, BoxGroup("References")] private Toggle musicToggle;
    [SerializeField, BoxGroup("References")] private Toggle sfxToggle;
    [SerializeField, BoxGroup("References")] private Toggle dubToggle;
    [SerializeField, BoxGroup("References")] private RectTransform pausedBg;
    [SerializeField, BoxGroup("References")] private RectTransform pausedContent;
    [SerializeField, BoxGroup("References")] private RectTransform pausedConsent;

    [SerializeField, BoxGroup("Config")] private float finalY = 0f;
    [SerializeField, BoxGroup("Config")] private float animationDuration = 1f;
    [SerializeField, BoxGroup("Config")] private float pausedBackgroundFinalAlpha = 138f;

    private float initialY;
    private Tween currentAnimation;
    [ShowNonSerializedField] private bool opened;

    private void Start()
    {
        this.initialY = this.animatedMenu.anchoredPosition.y;

        this.pausedBg.gameObject.SetActive(false);
        this.pausedContent.gameObject.SetActive(false);
        this.pausedConsent.gameObject.SetActive(false);

        this.musicToggle.isOn = AudioController.Instance.isMusicMuted;
        this.sfxToggle.isOn = AudioController.Instance.isSFXMuted;
        this.dubToggle.isOn = !AudioController.Instance.isDubMuted;
    }

    public void MenuToggleMusic(bool value) {
        if(AudioController.Instance != null) {
            AudioController.Instance.isMusicMuted = value;
        }
    }

    public void MenuToggleSFX(bool value) {
        if(AudioController.Instance != null) {
            AudioController.Instance.isSFXMuted = value;
        }
    }

    public void MenuToggleDub(bool value) {
        if(AudioController.Instance != null) {
            AudioController.Instance.isDubMuted = !value;
        }
    }

    public void MenuToggleClicked() {
        if(!this.opened) {
            this.OpenMenu();
        } else {
            this.CloseMenu();
        }
    }

    public void OpenMenu() {
        this.opened = true;
        var graphs = this.menuToggleButton.GetComponentsInChildren<Graphic>(true);
        graphs[0].gameObject.SetActive(false);
        graphs[1].gameObject.SetActive(true);
        this.menuToggleButton.targetGraphic = graphs[1];

        this.currentAnimation?.Kill();
        Time.timeScale = 0f;
        this.ShowChildren(this.pausedBg, this.animationDuration / 2, this.pausedBackgroundFinalAlpha / 255f);
        this.ShowChildren(this.pausedContent, this.animationDuration/2);
        this.currentAnimation = this.animatedMenu.DOAnchorPosY(this.finalY, this.animationDuration, true).SetUpdate(UpdateType.Normal, true);
    }

    public void CloseMenu() {
        this.opened = false;
        var graphs = this.menuToggleButton.GetComponentsInChildren<Graphic>(true);
        graphs[0].gameObject.SetActive(true);
        graphs[1].gameObject.SetActive(false);
        this.menuToggleButton.targetGraphic = graphs[0];

        this.currentAnimation?.Kill();
        this.HideChildren(this.pausedBg, this.animationDuration / 2);
        this.HideChildren(this.pausedContent, this.animationDuration / 2);
        this.currentAnimation = this.animatedMenu.DOAnchorPosY(this.initialY, this.animationDuration, true).SetUpdate(UpdateType.Normal, true).OnComplete(
            () => {
                Time.timeScale = 1f;
            }
        );
    }

    public void ShowLeaveConsent(Button clickedButton) {
        this.menuToggleButton.interactable = false;
        clickedButton.interactable = false;
        this.HideChildren(this.pausedContent, this.animationDuration / 2);
        this.ShowChildren(this.pausedConsent, this.animationDuration / 2).OnComplete(() => {
            var buttons = this.pausedConsent.GetComponentsInChildren<Button>(true);
            foreach(var button in buttons) {
                button.interactable = true;
            }
        });
    }

    public void HideLeaveConsent(Button leaveButton) {
        var buttons = this.pausedConsent.GetComponentsInChildren<Button>(true);
        foreach(var button in buttons) {
            button.interactable = false;
        }

        this.HideChildren(this.pausedConsent, this.animationDuration / 2);
        this.ShowChildren(this.pausedContent, this.animationDuration / 2).OnComplete(() => {
            leaveButton.interactable = true;
            this.menuToggleButton.interactable = true;
        });
    }

    public void LeaveGame() => LevelManager.currentInstance.sceneController.BackToMenu();

    public Tween ShowChildren(RectTransform parent, float duration, float finalAlphaValue = 1f) {
        var texts = parent.GetComponentsInChildren<TextMeshProUGUI>(true);
        var bgs = parent.GetComponentsInChildren<Image>(true);

        Tween tw = null;
        foreach(var text in texts) {
            var initialTextColor = text.color - Color.black;
            var finalTextColor = new Color(text.color.r, text.color.g, text.color.b, finalAlphaValue);

            text.color = initialTextColor;
            tw = text.DOColor(finalTextColor, duration).SetUpdate(UpdateType.Normal, true);
        }
        
        foreach(var bg in bgs) {
            var initialBGColor = bg.color - Color.black;
            var finalBGColor = new Color(bg.color.r, bg.color.g, bg.color.b, finalAlphaValue);

            bg.color = initialBGColor;
            bg.DOColor(finalBGColor, duration).SetUpdate(UpdateType.Normal, true);
        }

        parent.gameObject.SetActive(true);
        return tw;
    }

    public Tween HideChildren(RectTransform parent, float duration) {
        var texts = parent.GetComponentsInChildren<TextMeshProUGUI>(true);
        var bgs = parent.GetComponentsInChildren<Image>(true);

        Tween tw = null;
        foreach(var text in texts) {
            var finalTextColor = new Color(text.color.r, text.color.g, text.color.b, 0f);
            tw = text.DOColor(finalTextColor, duration).SetUpdate(UpdateType.Normal, true);
        }

        foreach(var bg in bgs) {
            var finalBGColor = new Color(bg.color.r, bg.color.g, bg.color.b, 0f);
            bg.DOColor(finalBGColor, duration).SetUpdate(UpdateType.Normal, true);
        }

        tw?.OnComplete(() => { parent.gameObject.SetActive(false); });
        return tw;
    }
}
