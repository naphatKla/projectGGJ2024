using System;
using DG.Tweening;
using Managers;
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

        [SerializeField] private Image loadSceneTransition;
        [SerializeField] private SceneButton[] sceneButtons;
        public static SceneName SceneNameAfterLoad {get; set;}
   
        void Start()
        {
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
                    SoundManager.Instance.FadeOutMusic(2f, AfterFadeAction.Stop);
                    SoundManager.Instance.FadeOutFx(2f, AfterFadeAction.Stop);
                    loadSceneTransition.GetComponent<Animator>().SetTrigger("ChangeScene");
                    DOVirtual.DelayedCall(0.6f, (() =>
                    {
                        if (sceneButton.useLoadingScene)
                        {
                            SceneNameAfterLoad = sceneButton.sceneName == SceneName.LoadingScene? SceneName.MainMenu : sceneButton.sceneName;
                            SceneManager.LoadScene(SceneName.LoadingScene.ToString());
                            SoundManager.Instance.FadeInMusic(0.5f, true);
                            SoundManager.Instance.FadeInFx(0.5f, true);
                            return;
                        }
                        SceneManager.LoadScene(sceneButton.sceneName.ToString());
                        SoundManager.Instance.FadeInMusic(0.5f, true);
                        SoundManager.Instance.FadeInFx(0.5f, true);
                    }));
                });
            }
        }
    
    }
}
