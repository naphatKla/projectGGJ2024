using System;
using System.Linq;
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
        [SerializeField] private VideoPlayer videoPlayer;
        [SerializeField] private EndingSettings[] endingSettings;
        
        public static ParameterType endingType;
        
        
        // Start is called before the first frame update
        void Start()
        {
            PlayEnding();
        }
        
        private void PlayEnding()
        {
            videoPlayer.clip = endingSettings.First(x => x.EndingType == endingType).VideoClip;
            videoPlayer.Play();
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                SceneManager.LoadScene("Credit");
            }
            if (videoPlayer.isPlaying) return;
            SceneManager.LoadScene("Credit");
        }
    }
}
