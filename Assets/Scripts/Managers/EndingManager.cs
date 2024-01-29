using System;
using System.Linq;
using DG.Tweening;
using Manager;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

namespace Managers
{
    [Serializable]
    public struct EndingSettings
    {
        [SerializeField] private ParameterType endingType;
        [SerializeField] private VideoClip videoClip;
        public ParameterType EndingType => endingType;
        public VideoClip VideoClip => videoClip;
    }
    public class EndingManager : MonoBehaviour
    {
        [SerializeField] private AudioClip endingMusic;
        [SerializeField] private AudioClip endingFx;
        [SerializeField] private VideoPlayer videoPlayer;
        [SerializeField] private EndingSettings[] endingSettings;
        private bool _goingToCredit;
        
        public static ParameterType endingType = ParameterType.Ignorant;
        
        
        // Start is called before the first frame update
        void Awake()
        {
            PlayEnding();
        }

        private void Start()
        {
            SoundManager.Instance.FadeInMusic(2f,true);
            SoundManager.Instance.StopAllFx();
            SoundManager.Instance.FadeInFx(1, true);
            SoundManager.Instance.PlayMusic(endingMusic);
            SoundManager.Instance.PlayFx(endingFx,out _,true);
        }

        private void PlayEnding()
        {
            videoPlayer.clip = endingSettings.First(x => x.EndingType == endingType).VideoClip;
            videoPlayer.Play();
        }

        // Update is called once per frame
        void Update()
        {
            if (!videoPlayer.isPrepared) return;
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                GoToCredit();
            }
            if (videoPlayer.frame >= (long)videoPlayer.frameCount - 1)
            {
                GoToCredit();
            }
        }
        
        private void GoToCredit()
        {
            if (_goingToCredit) return;
            _goingToCredit = true;
            DOVirtual.DelayedCall(1f, () =>
            {
                SceneManager.LoadScene("Credit");
            });
        }
    }
}
