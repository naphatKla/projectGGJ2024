using System.Collections.Generic;
using Plugins.Singleton;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Managers
{
    public class GameManager : MonoSingleton<GameManager>
    {
        [Button("Check Ending")]
        private void CheckEnding()
        {
            List<ParameterArchetype> sortedParameters = BubbleManager.Instance.ParameterArchetypes;
            sortedParameters.Sort(new ParameterComparer());
            if (sortedParameters[0].ParameterType != ParameterType.Good)
            {
                if (BubbleManager.Instance.LastBubbleIgnored)
                {
                    Debug.Log("Bad Ending 1");
                    return;
                }
                switch (sortedParameters[0].ParameterType)
                {
                    case ParameterType.FalseHope:
                        Debug.Log("Bad Ending 3");
                        break;
                    case ParameterType.Despair:
                        Debug.Log("Bad Ending 2");
                        break;
                    case ParameterType.Ignorant:
                        Debug.Log("Bad Ending 1");
                        break;
                }
            }
            else
            {
                Debug.Log("Good Ending");
            }
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
                case ParameterType.FalseHope:
                    return 0;
                case ParameterType.Despair:
                    return 1;
                case ParameterType.Ignorant:
                    return 2;
                default:
                    return 3; // Handle other types (if any) with the same value
            }
        }
    }
}
