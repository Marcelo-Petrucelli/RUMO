using UnityEngine.UIElements;
using UnityEngine;

public class GameSceneBtnController : MonoBehaviour
{
    private UIDocument doc;
    private Button backButton;
    private SceneController sceneController;

   private void Awake()
    {
        this.sceneController = FindObjectOfType<SceneController>();

        this.doc = this.GetComponent<UIDocument>();
        
        this.backButton = this.doc.rootVisualElement.Q<Button>("ExitButton");
        this.backButton.clicked += this.ExitButtonClicked;
    }

    private void ExitButtonClicked() => this.sceneController.BackToMenu();
}
