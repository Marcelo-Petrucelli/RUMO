using UnityEngine.UIElements;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    private UIDocument doc;
    private Button playButton;
    private Button creditsButton;
    private Button exitButton;
    private SceneController sceneController;

   private void Awake()
    {
        this.sceneController = FindObjectOfType<SceneController>();

        this.doc = this.GetComponent<UIDocument>();
        this.playButton = this.doc.rootVisualElement.Q<Button>("PlayButton");
        this.playButton.clicked += this.PlayButtonClicked;

        this.creditsButton = this.doc.rootVisualElement.Q<Button>("CreditsButton");
        this.creditsButton.clicked += this.CreditsButtonClicked;

        this.exitButton = this.doc.rootVisualElement.Q<Button>("ExitButton");
        this.exitButton.clicked += this.ExitButtonClicked;
    }

    private void PlayButtonClicked() => this.sceneController.PlayGame();

    private void CreditsButtonClicked() => this.sceneController.GotoCredits();

    private void ExitButtonClicked() {
        Application.Quit();
    }
}
