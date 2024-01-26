using System.Collections;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Manager
{
    public class LoadingSceneController : MonoBehaviour
    {
        [SerializeField] 
        private TextMeshProUGUI loadingPercentText;
        [SerializeField] 
        private TextMeshProUGUI loadingText;
        
        [SerializeField] 
        private TextMeshProUGUI tipsText;
        
        [SerializeField][TextArea]
        private string[] tips;
        
        [SerializeField]
        [PropertyTooltip("Time between each dot of the loading text")]
        private float dotInterval = 0.5f;
        
        [SerializeField]
        [PropertyTooltip("Minimum time to show the loading text even if the scene is already finished loading")]
        private float minimumLoadingTime = 1.0f;
        
        private int _currentTipsIndex;
        private AsyncOperation _asyncOperation;
        private bool _isDoneLoading;
        
        void Start()
        {
            //_currentTipsIndex = Random.Range(0, tips.Length);
            //tipsText.text = tips[_currentTipsIndex];
            StartCoroutine(LoadSceneAsync());
        }

        private void Update()
        {
            if (!_isDoneLoading) return;
            if (!(Time.timeSinceLevelLoad > minimumLoadingTime) || !Input.anyKeyDown) return;
            _asyncOperation.allowSceneActivation = true;
        }

        private IEnumerator LoadSceneAsync()
        {
            SceneName sceneToLoad = LoadSceneManager.SceneNameAfterLoad;
            _asyncOperation = SceneManager.LoadSceneAsync(sceneToLoad.ToString());
            _asyncOperation.allowSceneActivation = false;
         
            while (!_asyncOperation.isDone || Time.timeSinceLevelLoad < minimumLoadingTime)
            {
                loadingPercentText.text = $"{(_asyncOperation.progress+0.1) *100f :0}%";
                if (_asyncOperation.progress >= 0.9f && Time.timeSinceLevelLoad > minimumLoadingTime)
                {
                    loadingText.text = "Any key to continue";
                    _isDoneLoading = true;
                    yield break;
                }
                yield return null;
            }
        }
    }
}
