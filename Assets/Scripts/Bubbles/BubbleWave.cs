using System.Collections.Generic;
using DG.Tweening;
using Managers;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

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
        [SerializeField] private List<BubbleSettings> bubbleSettings = new List<BubbleSettings>();


        [Title("Debug")]
        [SerializeField][ReadOnly] private int currentBubbleIndex;
        [SerializeField][ReadOnly] private int currentRandomizedBubbleIndex;
        [SerializeField] private List<BubbleSettings> randomizedBubbleSettings = new List<BubbleSettings>();
        [SerializeField] private List<BubbleSettings> currentRandomizedBundle = new List<BubbleSettings>();
        
        private BubbleSettings _previousBubbleSettings;
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
                randomizedBubbleSettings = bubbleSettings.FindAll(x => x.HasAnswer);
            }
        }
        
        private void Update()
        {
            if (!_isPlaying || !_nextBubble) return;
            _currentInterval -= Time.deltaTime;
            if (_currentInterval > 0) return;
            if (randomizeOrder)
            {
                if (currentRandomizedBubbleIndex == 0 && randomizedBubbleSettings.Count > 0)
                {
                    int randomIndex = Random.Range(0, randomizedBubbleSettings.Count);
                    BubbleSettings randomBubbleSettings = randomizedBubbleSettings[randomIndex];
                    currentRandomizedBundle.Add(randomBubbleSettings);
                    int index = bubbleSettings.IndexOf(randomBubbleSettings);
                    SelectRandomizedBubble(index);
                }
                if (currentRandomizedBubbleIndex >= currentRandomizedBundle.Count)
                {
                    currentRandomizedBubbleIndex = 0;
                    currentRandomizedBundle = new List<BubbleSettings>();
                }
                else
                {
                    Debug.Log("Index of: " +currentRandomizedBundle[currentRandomizedBubbleIndex].DialogueString);
                    Debug.Log("Index: " + bubbleSettings.IndexOf(currentRandomizedBundle[currentRandomizedBubbleIndex]));
                    SpawnBubble(bubbleSettings.IndexOf(currentRandomizedBundle[currentRandomizedBubbleIndex]));
                    _previousBubbleSettings = currentRandomizedBundle[currentRandomizedBubbleIndex];
                    randomizedBubbleSettings.Remove(currentRandomizedBundle[^1]);
                    currentRandomizedBubbleIndex++;
                    _nextBubble = false;
                }
                
                if (randomizedBubbleSettings.Count <= 0 &&
                    currentRandomizedBubbleIndex >= currentRandomizedBundle.Count)
                {
                    if (loop)
                    {
                        randomizedBubbleSettings = bubbleSettings.FindAll(x => x.HasAnswer);
                        currentRandomizedBubbleIndex = 0;
                        currentRandomizedBundle = new List<BubbleSettings>();
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
                    currentRandomizedBundle.Add(bubbleSettings[i]);
                }
                else break;
            }
            currentRandomizedBundle.Reverse();
            Debug.Log($"Current Randomized Bundle: {currentRandomizedBundle.Count}");
            foreach (BubbleSettings bubbleSettings in currentRandomizedBundle)
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
            BubbleManager.currentBubble = bubble;
            bubble.CurrentSpawnPoint = spawnPoint;
            bubble.Init(this, bubbleSettings[index]);
        }
    }
}
