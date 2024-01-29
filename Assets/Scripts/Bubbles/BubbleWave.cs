using System.Collections.Generic;
using DG.Tweening;
using Managers;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;
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
        [SerializeField][ReadOnly] private int currentStackIndex;
        [SerializeField] private List<BubbleSettings> randomizedPrompts = new List<BubbleSettings>();
        [SerializeField] private List<BubbleSettings> randomizedStack = new List<BubbleSettings>();
        
        private BubbleSettings _previousBubbleSettings;
        private bool _isPlaying;
        private bool _readyToEnd;
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
            randomizedPrompts.Clear();
            randomizedStack.Clear();
            currentBubbleIndex = 0;
            _currentInterval = 0;
            if (randomizeOrder)
            {
                randomizedPrompts = bubbleSettings.FindAll(x => x.HasAnswer);
            }
        }
        
        private void Update()
        {
            if (!_isPlaying || !_nextBubble || GameManager.Instance.IsLose) return;
            _currentInterval -= Time.deltaTime;
            if (_currentInterval > 0) return;
            if (randomizeOrder)
            {
                SpawnRandomly();
            }
            else
            {
                SpawnOrderly();
            }
            _currentInterval = 0;
        }

        private void SpawnRandomly()
        {
            if (currentStackIndex >= randomizedStack.Count)
            {
                currentStackIndex = 0;
                randomizedStack.Clear();
                RandomStack();
            }
            else
            {
                BubbleSettings randomBubble = randomizedStack[currentStackIndex];
                int index = bubbleSettings.FindIndex(x => x.Id == randomBubble.Id);
                Debug.Log("String: " + randomBubble.DialogueString);
                Debug.Log("Index: " + index);
                SpawnBubble(index);
                _previousBubbleSettings = randomBubble;
                randomizedPrompts.Remove(randomizedStack[^1]);
                currentStackIndex++;
                _nextBubble = false;
            }

            if (randomizedPrompts.Count > 0 ||
                currentStackIndex < randomizedStack.Count) return;
            if (loop)
            {
                randomizedPrompts = bubbleSettings.FindAll(x => x.HasAnswer);
                currentStackIndex = 0;
                randomizedStack.Clear();
                _isPlaying = true;
                return;
            }
            _isPlaying = false;

        }

        private void SpawnOrderly()
        {
            SpawnBubble(currentBubbleIndex);
            _previousBubbleSettings = bubbleSettings[currentBubbleIndex];
            currentBubbleIndex++;
            _nextBubble = false;
            if (currentBubbleIndex < bubbleSettings.Count) return;
            if (isEnding) _readyToEnd = true;
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
            if (randomizeOrder)
            {
                RandomStack();
            }
            DOVirtual.DelayedCall(startDelay, () =>
            {
                _isPlaying = true;
            });
        }

        private void RandomStack()
        {
            int randomIndex = Random.Range(0, randomizedPrompts.Count);
            BubbleSettings randomBubbleSettings = randomizedPrompts[randomIndex];
            randomizedStack.Add(randomBubbleSettings);
            int index = bubbleSettings.FindIndex(x => x.Id == randomBubbleSettings.Id);
            for (int i = index - 1; i >= 0; i--)
            {
                if (!bubbleSettings[i].HasAnswer)
                {
                    randomizedStack.Add(bubbleSettings[i]);
                }
                else break;
            }
            randomizedStack.Reverse();
            if (randomizedStack.Count == 0) Debug.LogWarning("Randomized Stack is empty");
        }
        
        public void StopWave()
        {
            _isPlaying = false;
        }
        
        public void SetNextBubble()
        {
            if (_readyToEnd)
            {
                GameManager.Instance.GoToEnding();
            }
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
