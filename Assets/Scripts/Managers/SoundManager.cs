using System.Collections.Generic;
using DG.Tweening;
using Plugins.Singleton;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;

namespace Managers
{
    public enum AfterFadeAction
    {
        Nothing = 0,
        Stop,
        Pause,
    }
    public class SoundManager : PersistentMonoSingleton<SoundManager>
    {
        [SerializeField] private AudioSource musicAudioSource;
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private List<AudioSource> fxAudioPool;
        
        [Title("Debug")]
        [SerializeField][ReadOnly] private List<AudioSource> freeSources = new List<AudioSource>(); 
        [SerializeField][ReadOnly] private List<AudioSource> reservedSources = new List<AudioSource>();

        private GameObject _baseAudioSource;
        private readonly string _musicVolumeParameter = "BGMVolume";
        private readonly string _fxVolumeParameter = "FXVolume";
        private float _initialMusicVolume;
        private float _initialFxVolume;
        private Tween _musicFadeTween;
        private Tween _fxFadeTween;
        // Start is called before the first frame update
        protected override void Awake()
        {
            base.Awake();
            audioMixer.GetFloat(_musicVolumeParameter, out _initialMusicVolume);
            audioMixer.GetFloat(_fxVolumeParameter, out _initialFxVolume);
            _baseAudioSource = fxAudioPool[0].gameObject;
            freeSources = fxAudioPool;
        }
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
           
        }
        
        private void FreeWhenFinished()
        {
            List<int> indicesToRemove = new List<int>();
            reservedSources.ForEach(x =>
            {
                if (x.isPlaying) return;
                freeSources.Add(x);
                indicesToRemove.Add(reservedSources.IndexOf(x));
            });
            indicesToRemove.Reverse();
            indicesToRemove.ForEach(x => reservedSources.RemoveAt(x));
        }
        
        public void PlayMusic(AudioClip musicClip, bool loop = true)
        {
            musicAudioSource.clip = musicClip;
            musicAudioSource.loop = loop;
            musicAudioSource.Play();
        }
        
        public void PauseMusic()
        {
            musicAudioSource.Pause();
        }
        
        public void StopMusic()
        {
            musicAudioSource.Stop();
        }

        public void FadeOutMusic(float duration, AfterFadeAction afterFadeAction = AfterFadeAction.Nothing)
        {
            if (_musicFadeTween.IsActive()) _musicFadeTween.Kill();
            _musicFadeTween = audioMixer.DOSetFloat(_musicVolumeParameter, -80f, duration).OnComplete(() =>
            {
                switch (afterFadeAction)
                {
                    case AfterFadeAction.Nothing:
                        break;
                    case AfterFadeAction.Stop:
                        StopMusic();
                        break;
                    case AfterFadeAction.Pause:
                        PauseMusic();
                        break;
                }
            });
            
        }
        
        public void FadeInMusic(float duration, bool fromZero = false)
        {
            if (_musicFadeTween.IsActive()) _musicFadeTween.Kill();
            if (fromZero) audioMixer.SetFloat(_musicVolumeParameter, -80f);
            musicAudioSource.Play();
            _musicFadeTween = audioMixer.DOSetFloat(_musicVolumeParameter, _initialMusicVolume, duration);
        }
        
        public void PlayFx(AudioClip fxClip, out AudioSource selectedSource, bool loop = false)
        {
            FreeWhenFinished();
            if (freeSources.Count <= 0)
            {
                Debug.LogWarning("No free audio sources available!, creating new one but this is not optimal! Please increase the pool size!");
                AudioSource newAudioSource = Instantiate(_baseAudioSource, transform.position, Quaternion.identity, transform)
                    .GetComponent<AudioSource>();
                freeSources.Add(newAudioSource);
            }
            AudioSource audioSource = freeSources[0];
            freeSources.RemoveAt(0);
            reservedSources.Add(audioSource);
            audioSource.clip = fxClip;
            audioSource.loop = loop;
            audioSource.Play();
            selectedSource = audioSource;
        }
        
        public void PauseFx(AudioSource audioSource)
        {
            audioSource.Pause();
        }
        
        public void StopFx(AudioSource audioSource)
        {
            audioSource.Stop();
            freeSources.Add(audioSource);
            reservedSources.Remove(audioSource);
        }
        
        public void PlayAllFx()
        {
            foreach (AudioSource audioSource in reservedSources)
            {
                audioSource.Play();
            }
        }
        
        public void PauseAllFx()
        {
            foreach (AudioSource audioSource in reservedSources)
            {
                audioSource.Pause();
            }
        }
        
        public void StopAllFx()
        {
            foreach (AudioSource audioSource in reservedSources)
            {
                audioSource.Stop();
                freeSources.Add(audioSource);
            }
            reservedSources.Clear();
        }
        
        public void FadeOutFx(float duration, AfterFadeAction afterFadeAction = AfterFadeAction.Nothing)
        {
            if (_fxFadeTween.IsActive()) _fxFadeTween.Kill();
            _fxFadeTween = audioMixer.DOSetFloat(_fxVolumeParameter, -80f, duration).OnComplete(() =>
            {
                switch (afterFadeAction)
                {
                    case AfterFadeAction.Nothing:
                        break;
                    case AfterFadeAction.Stop:
                        StopAllFx();
                        break;
                    case AfterFadeAction.Pause:
                        PauseAllFx();
                        break;
                }
            });
        }
        
        public void FadeInFx(float duration, bool fromZero = false)
        {
            if (_fxFadeTween.IsActive()) _fxFadeTween.Kill();
            if (fromZero) audioMixer.SetFloat(_fxVolumeParameter, -80f);
            foreach (AudioSource audioSource in reservedSources)
            {
                audioSource.Play();
            }
            _fxFadeTween = audioMixer.DOSetFloat(_fxVolumeParameter, _initialFxVolume, duration);
        }
    }
}
