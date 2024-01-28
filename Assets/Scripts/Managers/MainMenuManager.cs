using System;
using UnityEngine;
using UnityEngine.UI;

namespace Managers
{
    public class MainMenuManager : MonoBehaviour
    {
        [SerializeField] private Image languageImage;
        [SerializeField] private Sprite[] languageSprites;
        private readonly string _defaultLanguage = "EN";

        private void Awake()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        public void ToggleLanguage()
        {
            if (!PlayerPrefs.HasKey("Language"))
            {
                PlayerPrefs.SetString("Language", "TH");
                languageImage.sprite = languageSprites[1];
                return;
            }
            string currentLanguage = PlayerPrefs.GetString("Language", _defaultLanguage);
            switch (currentLanguage)
            {
                case "EN":
                    PlayerPrefs.SetString("Language", "TH");
                    languageImage.sprite = languageSprites[1];
                    break;
                case "TH":
                    PlayerPrefs.SetString("Language", "EN");
                    languageImage.sprite = languageSprites[0];
                    break;
            }
        }
    }
}
