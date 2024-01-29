using System;
using Sirenix.OdinInspector;
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
            if (!PlayerPrefs.HasKey("Language"))
            {
                PlayerPrefs.SetString("Language", "EN");
            }
            languageImage.sprite = PlayerPrefs.GetString("Language") == "EN" ? languageSprites[0] : languageSprites[1];
        }

        public void ToggleLanguage()
        {
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
        
        [Title("Debug")]
        [Button("Delete Keys")]
        private void DeleteKeys()
        {
            PlayerPrefs.DeleteKey("Language");
        }
    }
}
