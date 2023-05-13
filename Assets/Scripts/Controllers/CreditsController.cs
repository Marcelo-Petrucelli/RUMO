using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CreditsController : MonoBehaviour
{
    private UIDocument doc;
    private Button backButton;
    private SceneController sceneController;

   private void Awake()
    {
        this.sceneController = FindObjectOfType<SceneController>();

        this.doc = this.GetComponent<UIDocument>();
        this.backButton = this.doc.rootVisualElement.Q<Button>("BackButton");
        this.backButton.clicked += this.BackButtonClicked;
    }

    private void BackButtonClicked() {
        AudioController.Instance.ReturnedToMenuFromWinningCredits();
        this.sceneController.BackToMenu();
    }
}
