using System.Collections.Generic;
using Managers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Bubbles
{
    public class BubbleWave : MonoBehaviour
    {
        [SerializeField] private bool playOnLevelStart;
        [SerializeField] private List<BubbleSettings> bubbleSettings;
        [SerializeField] private bool randomizeOrder;

        [Title("Debug")]
        [SerializeField][ReadOnly] private int currentBubbleIndex;
            
        private BubbleSettings CurrentBubbleSettings => bubbleSettings[currentBubbleIndex];
        private BubbleSettings _previousBubbleSettings;
        private List<BubbleSettings> _randomizedBubbleSettings = new List<BubbleSettings>();
        private bool _isPlaying;
        private bool _nextBubble = true;
        private float _currentInterval;

        public BubbleManager BubbleManager => BubbleManager.Instance;

        public bool PlayOnLevelStart => playOnLevelStart;
        public List<BubbleSettings> BubbleSettings => bubbleSettings;
        public bool RandomizeOrder => randomizeOrder;
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
            if (randomizeOrder)
            {
                if (_randomizedBubbleSettings.Count == 0) _isPlaying = false;
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
            if (currentBubbleIndex >= bubbleSettings.Count)
            {
                _isPlaying = false;
            }
        }

        public void PlayWave()
        {
            _isPlaying = true;
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
            Bubble bubble = Instantiate(CurrentBubbleSettings.BubblePrefab, spawnPoint.position, Quaternion.identity, BubbleManager.BubbleCanvas).GetComponent<Bubble>();
            bubble.CurrentSpawnPoint = spawnPoint;
            bubble.Init(this, bubbleSettings[index]);
        }
    }
}
