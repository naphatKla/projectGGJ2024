using System.Collections.Generic;
using DG.Tweening;
using MoreMountains.Feedbacks;
using Plugins.Singleton;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Managers
{
    public class GameManager : MonoSingleton<GameManager>
    {
        [SerializeField] private Stove stove;
        public float gamePlayTime;
        public int meatGoal;
        [HideInInspector] public int meatCooked;
        [HideInInspector]  public int meatBurned;
        public bool IsAllMeatBurned => meatBurned >= stove.slotCount;
        [SerializeField] private float _timeCount;
        public bool IsWin => meatCooked >= meatGoal;
        public bool IsLose;
        [SerializeField] private MMF_Player loseFeedback;
        [SerializeField] private Image timerImage;
        [SerializeField] private TextMeshProUGUI timerText;
        private BubbleManager BubbleManager => BubbleManager.Instance;
    
        
        void Start()
        {
        
        }
        
        void Update()
        {
            if (IsLose) return;
            _timeCount += Time.deltaTime;
            _timeCount = Mathf.Clamp(_timeCount, 0, gamePlayTime);
            timerImage.fillAmount = 1 - (_timeCount / gamePlayTime);
            timerText.text = $"{Mathf.FloorToInt(gamePlayTime - _timeCount)}";
            if (_timeCount >= gamePlayTime || IsAllMeatBurned || IsAllMeatBurned)
            {
                IsLose = true;
                loseFeedback.PlayFeedbacks();
            }
        }
        
        public void AddMeatCooked()
        {
            meatCooked++;
            if (meatCooked < meatGoal) return;
            CheckEnding();
        }
        
        [Button("Check Ending")]
        private void CheckEnding()
        {
            if (!IsWin) return;
            List<ParameterArchetype> sortedParameters = BubbleManager.ParameterArchetypes;
            sortedParameters.Sort(new ParameterComparer());
            ParameterType endingType = ParameterType.Good;
            Debug.Log($"Sorted Parameters: {sortedParameters[0].ParameterType}");
            if (sortedParameters[0].ParameterType != ParameterType.Good)
            {
                if (BubbleManager.LastBubbleIgnored)
                {
                    endingType = ParameterType.Ignorant;
                }
                else
                {
                    switch (sortedParameters[0].ParameterType)
                    {
                        case ParameterType.FalseHope:
                            endingType = ParameterType.FalseHope;
                            break;
                        case ParameterType.Despair:
                            endingType = ParameterType.Despair;
                            break;
                        case ParameterType.Ignorant:
                            endingType = ParameterType.Ignorant;
                            break;
                    }
                }
            }
            else
            {
                endingType = ParameterType.Good;
            }
            BubbleManager.CurrentBubbleManagerSettings
                .Find(x => x.BubbleWave.IsEnding && x.BubbleWave.EndingType == endingType)
                .BubbleWave
                .PlayWave();
            BubbleManager.CurrentBubbleManagerSettings
                .FindAll(x => !x.BubbleWave.IsEnding).ForEach(x => x.BubbleWave.StopWave());
            EndingManager.endingType = endingType;
            SceneManager.LoadScene("Ending");
        }
    }
    public class ParameterComparer : IComparer<ParameterArchetype>
    {
        public int Compare(ParameterArchetype x, ParameterArchetype y)
        {
            // Compare scores in descending order
            int scoreComparison = y.ParameterScore.CompareTo(x.ParameterScore);

            // If scores are equal, use custom order for Ignorant, Despair, and FalseHope
            if (scoreComparison == 0)
            {
                return GetCustomOrder(x.ParameterType).CompareTo(GetCustomOrder(y.ParameterType));
            }

            return scoreComparison;
        }

        private int GetCustomOrder(ParameterType type)
        {
            switch (type)
            {
                case ParameterType.Good:
                    return 0;
                case ParameterType.FalseHope:
                    return 1;
                case ParameterType.Despair:
                    return 2;
                case ParameterType.Ignorant:
                    return 3;
                default:
                    return 4; // Handle other types (if any) with the same value
            }
        }
    }
}
