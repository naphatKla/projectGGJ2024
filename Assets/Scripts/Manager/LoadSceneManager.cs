using System;
using DG.Tweening;
using Plugins.Singleton;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Manager
{
    public enum SceneName
    {
        MainMenu,
        GamePlay,
        Credit,
        LoadingScene,
        Exit
    }       
    public class LoadSceneManager : MonoSingleton<LoadSceneManager>
    {
        [Serializable]
        private struct SceneButton
        {
            public Button button;
            public SceneName sceneName;
            public bool useLoadingScene;
        }

        [SerializeField] private SceneButton[] sceneButtons;
        public static SceneName SceneNameAfterLoad {get; set;}
   
        void Start()
        {
            /*Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;*/
            foreach (var sceneButton in sceneButtons)
            {
                if (sceneButton.sceneName == SceneName.Exit)
                {
                    sceneButton.button.onClick.AddListener(() =>
                    {
                        Application.Quit();
                        #if UNITY_EDITOR
                        UnityEditor.EditorApplication.isPlaying = false;
                        #endif
                    });
                    continue;
                }
                
                sceneButton.button.onClick.AddListener(() =>
                {
                    DOTween.KillAll();
                    if (sceneButton.useLoadingScene)
                    {
                        SceneNameAfterLoad = sceneButton.sceneName == SceneName.LoadingScene? SceneName.MainMenu : sceneButton.sceneName;
                        SceneManager.LoadScene(SceneName.LoadingScene.ToString());
                        return;
                    }
                    SceneManager.LoadScene(sceneButton.sceneName.ToString());
                });
            }
        }
    
    }
}
