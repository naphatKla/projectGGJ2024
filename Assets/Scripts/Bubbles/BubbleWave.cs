using System.Collections.Generic;
using DG.Tweening;
using Managers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Bubbles
{
    
    public class BubbleWave : MonoBehaviour
    {
        [SerializeField] private GameObject bubblePrefab;
        [SerializeField] private bool isEnding;
        [SerializeField][HideIf(nameof(isEnding))] private bool playOnLevelStart;
        [SerializeField] private float startDelay;
        [SerializeField][HideIf(nameof(isEnding))] private bool randomizeOrder;
        [SerializeField][HideIf(nameof(isEnding))] private bool loop;
        [SerializeField][ShowIf(nameof(isEnding))] private ParameterType endingType;
        [SerializeField] private List<BubbleSettings> bubbleSettings;


        [Title("Debug")]
        [SerializeField][ReadOnly] private int currentBubbleIndex;
        [SerializeField][ReadOnly] private int currentRandomizedBubbleIndex;
        
        private BubbleSettings _previousBubbleSettings;
        private List<BubbleSettings> _randomizedBubbleSettings = new List<BubbleSettings>();
        private List<BubbleSettings> _currentRandomizedBundle = new List<BubbleSettings>();
        private bool _isPlaying;
        private bool _nextBubble = true;
        private float _currentInterval;

        public BubbleManager BubbleManager => BubbleManager.Instance;

        public bool PlayOnLevelStart => playOnLevelStart;
        public List<BubbleSettings> BubbleSettings {get => bubbleSettings; set => bubbleSettings = value;}
        public bool RandomizeOrder => randomizeOrder;
        public bool Loop => loop;
        public bool IsEnding => isEnding;
        public ParameterType EndingType => endingType;
        public int CurrentBubbleIndex => currentBubbleIndex;
        
        private void Awake()
        {
            currentBubbleIndex = 0;
            _currentInterval = 0;
            if (randomizeOrder)
            {
                _randomizedBubbleSettings = bubbleSettings.FindAll(x => x.HasAnswer);
            }
        }
        
        private void Update()
        {
            if (!_isPlaying || !_nextBubble) return;
            Debug.Log("Pass 1");
            _currentInterval -= Time.deltaTime;
            if (_currentInterval > 0) return;
            Debug.Log("Pass 2");
            if (randomizeOrder)
            {
                Debug.Log("Pass 3");
                if (currentRandomizedBubbleIndex == 0 && _randomizedBubbleSettings.Count > 0)
                {
                    int randomIndex = Random.Range(0, _randomizedBubbleSettings.Count);
                    BubbleSettings randomBubbleSettings = _randomizedBubbleSettings[randomIndex];
                    _currentRandomizedBundle.Add(randomBubbleSettings);
                    int index = bubbleSettings.IndexOf(randomBubbleSettings);
                    SelectRandomizedBubble(index);
                }
                if (currentRandomizedBubbleIndex >= _currentRandomizedBundle.Count)
                {
                    currentRandomizedBubbleIndex = 0;
                    _currentRandomizedBundle = new List<BubbleSettings>();
                }
                else
                {
                    SpawnBubble(bubbleSettings.IndexOf(_currentRandomizedBundle[currentRandomizedBubbleIndex]));
                    _previousBubbleSettings = _currentRandomizedBundle[currentRandomizedBubbleIndex];
                    _randomizedBubbleSettings.Remove(_currentRandomizedBundle[^1]);
                    currentRandomizedBubbleIndex++;
                    _nextBubble = false;
                }
                
                if (_randomizedBubbleSettings.Count <= 0 &&
                    currentRandomizedBubbleIndex >= _currentRandomizedBundle.Count)
                {
                    if (loop)
                    {
                        _randomizedBubbleSettings = bubbleSettings.FindAll(x => x.HasAnswer);
                        currentRandomizedBubbleIndex = 0;
                        _currentRandomizedBundle = new List<BubbleSettings>();
                        _isPlaying = true;
                        return;
                    }
                    _isPlaying = false;
                }
                
            }
            else if (!randomizeOrder)
            {
                SpawnBubble(currentBubbleIndex);
                _previousBubbleSettings = bubbleSettings[currentBubbleIndex];
                currentBubbleIndex++;
                _nextBubble = false;
            }
            _currentInterval = 0;
            if (currentBubbleIndex < bubbleSettings.Count) return;
            if (loop)
            {
                currentBubbleIndex = 0;
                _isPlaying = true;
            }
            else
            {
                _isPlaying = false;
            }
        }
        
        private void SelectRandomizedBubble(int index)
        {
            for (int i = index - 1; i >= 0; i--)
            {
                if (!bubbleSettings[i].HasAnswer)
                {
                    _currentRandomizedBundle.Add(bubbleSettings[i]);
                }
                else break;
            }
            _currentRandomizedBundle.Reverse();
            Debug.Log($"Current Randomized Bundle: {_currentRandomizedBundle.Count}");
            foreach (BubbleSettings bubbleSettings in _currentRandomizedBundle)
            {
                Debug.Log($"Bubble Settings: {bubbleSettings.DialogueString}");
            }
        }

        public void PlayWave()
        {
            DOVirtual.DelayedCall(startDelay, () =>
            {
                _isPlaying = true;
            });
        }
        
        public void StopWave()
        {
            _isPlaying = false;
        }
        
        public void SetNextBubble()
        {
            _nextBubble = true;
            _currentInterval = _previousBubbleSettings.Interval;
        }
        
        private void SpawnBubble(int index)
        {
            int randomIndex = Random.Range(0, BubbleManager.AvailableSpawnPoints.Count - 1);
            Transform spawnPoint = BubbleManager.AvailableSpawnPoints[randomIndex];
            BubbleManager.ReserveSpawnPoint(spawnPoint);
            Bubble bubble = Instantiate(bubblePrefab, spawnPoint.position, Quaternion.identity, BubbleManager.BubbleCanvas).GetComponent<Bubble>();
            bubble.CurrentSpawnPoint = spawnPoint;
            bubble.Init(this, bubbleSettings[index]);
        }
    }
}
