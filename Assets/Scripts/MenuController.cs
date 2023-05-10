using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
   private UIDocument _doc;
   private Button _playButton;
   private Button _creditsButton;
   private Button _exitButton;

   private void Awake()
    {
        _doc = GetComponent<UIDocument>();
        _playButton = _doc.rootVisualElement.Q<Button>("PlayButton");
        _playButton.clicked += PlayButtonClicked;
        _creditsButton = _doc.rootVisualElement.Q<Button>("CreditsButton");
        _creditsButton.clicked += CreditsButtonClicked;
        _exitButton = _doc.rootVisualElement.Q<Button>("ExitButton");
        _exitButton.clicked += ExitButtonClicked;
    }

    private void PlayButtonClicked()
    {
        SceneManager.LoadScene("2_Menu");
    }

    private void ExitButtonClicked()
    {
        Application.Quit();
    }

    private void CreditsButtonClicked()
    {
        SceneManager.LoadScene("3_Credits");
    }
   
}
