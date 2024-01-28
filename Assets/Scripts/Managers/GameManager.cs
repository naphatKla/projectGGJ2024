using System.Collections.Generic;
using Plugins.Singleton;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Managers
{
    public class GameManager : MonoSingleton<GameManager>
    {
        private BubbleManager BubbleManager => BubbleManager.Instance;
        [Button("Check Ending")]
        private void CheckEnding()
        {
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
        }
        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
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
