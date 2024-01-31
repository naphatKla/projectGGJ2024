using System;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Managers
{
    public class MainMenuManager : MonoBehaviour
    {
        [SerializeField] private Image languageImage;
        [SerializeField] private Sprite[] languageSprites;
        [SerializeField] private AudioClip bgmSound;
        [SerializeField] private GameObject achievement;
        [SerializeField] private AudioClip achievementSound;
        public static bool IsPlayAchievementNotification { get; set; }
        private readonly string _defaultLanguage = "EN";

        private void Awake()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            SoundManager.Instance.PlayMusic(bgmSound);
            Debug.Log("Call");
            if (!PlayerPrefs.HasKey("Language"))
            {
                PlayerPrefs.SetString("Language", "EN");
            }
            languageImage.sprite = PlayerPrefs.GetString("Language") == "EN" ? languageSprites[0] : languageSprites[1];
            
            if (!IsPlayAchievementNotification) return;
            achievement.SetActive(true);
            achievement.GetComponentInChildren<TextMeshProUGUI>().text = $"{GameManager.endingTypeUnlocked.Count} / 4 ending unlocked";
            SoundManager.Instance.PlayFx(achievementSound, out _);
            IsPlayAchievementNotification = false;
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
