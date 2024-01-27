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
        [SerializeField] private bool playOnLevelStart;
        [SerializeField] private float startDelay;
        [SerializeField] private bool randomizeOrder;
        [SerializeField] private bool loop;
        [SerializeField] private bool isEnding;
        [SerializeField][ShowIf(nameof(isEnding))] private ParameterType endingType;
        [SerializeField] private List<BubbleSettings> bubbleSettings;


        [Title("Debug")]
        [SerializeField][ReadOnly] private int currentBubbleIndex;
        
        private BubbleSettings _previousBubbleSettings;
        private List<BubbleSettings> _randomizedBubbleSettings = new List<BubbleSettings>();
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
                _randomizedBubbleSettings = bubbleSettings;
            }
        }
        
        private void Update()
        {
            if (!_isPlaying || !_nextBubble) return;
            _currentInterval -= Time.deltaTime;
            if (_currentInterval > 0) return;
            if (randomizeOrder && _randomizedBubbleSettings.Count > 0)
            {
                if (loop)
                {
                    _randomizedBubbleSettings = bubbleSettings;
                    _isPlaying = true;
                }
                else
                {
                    _isPlaying = false;
                }
                int randomIndex = Random.Range(0, _randomizedBubbleSettings.Count);
                SpawnBubble(randomIndex);
                _previousBubbleSettings = _randomizedBubbleSettings[randomIndex];
                _randomizedBubbleSettings.RemoveAt(randomIndex);
                _nextBubble = false;
            }
            else
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
