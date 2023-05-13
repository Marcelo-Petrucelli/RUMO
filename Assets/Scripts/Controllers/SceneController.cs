using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using DG.Tweening;

public class SceneController:MonoBehaviour
{
    [SerializeField] private GameObject frontDrop;
    [SerializeField] private Vector2 fadeInAndOutDuration = new(0.5f, 0.5f);

    private Image backDropImage;
    private Image BackDropImage {
        get {
            if(this.backDropImage == null) {
                this.backDropImage = this.frontDrop.GetComponent<Image>();
            }
            return this.backDropImage;
        }
    }

    void Start() {
        this.BackDropImage.color = Color.black;
        this.frontDrop.SetActive(true);

        this.BackDropImage.DOFade(0f, fadeInAndOutDuration.y).SetEase(Ease.Linear).OnComplete(() => {
            this.frontDrop.SetActive(false);
        });

        var scaneName = SceneManager.GetActiveScene().name;

        var isSplash1 = scaneName == "0_Splash0";
        var isSplash2 = scaneName == "1_Splash1";
        if(isSplash1 || isSplash2) {
            this.StartCoroutine(this.waitSplashScreen(isSplash1));
        }
        //if (SceneManager.GetActiveScene().name == "FinalCutscene") StartCoroutine(FinalGameCoroutine());

        //Music in Scenes
        switch(scaneName) {
            case "2_Menu":
                //AudioManager.Instance?.Play("MenuMusic", true);
                break;
                /*case "4_Level":
                    AudioManager.Instance?.Play("GameMusic", true);
                    break;*/
        }
    }

    IEnumerator waitSplashScreen(bool isSplash1, float duration = 2.5f) {
        yield return new WaitForSeconds(duration);
        this.End(isSplash1 ? "1_Splash1" : "2_Menu");
    }

    void End(string scene) {
        this.BackDropImage.color = Color.black - Color.black;
        this.frontDrop.SetActive(true);

        this.BackDropImage.DOFade(1f, this.fadeInAndOutDuration.x).SetEase(Ease.Linear).OnComplete(() => {
            SceneManager.LoadScene(scene);
        });
    }

    public void ButtonPressed() { } //AudioController.Instance?.Play("ButtonSFX");

    public void PlayGame() { this.End("4_Level_1"); this.ButtonPressed(); }

    public void GotoCredits() { this.End("3_Credits"); this.ButtonPressed(); }

    public void BackToMenu() { this.End("2_Menu"); this.ButtonPressed(); }

    public void GoToLevel(int levelIndex){
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        switch (levelIndex) {
            case 0:
                this.End("4_Level_1");
                break;
            /*case 1:
                this.End("5_Level_2");
                break;
            case 2:
                this.End("6_Level_3");
                break;
            case 3:
                this.End("7_Level_4");
                break;
            case 4:
                this.End("8_Level_5");
                break;
            case 5:
                this.End("9_Level_6");
                break;*/
            default:
                this.End("2_Menu");
                break;
        }
    }
}
