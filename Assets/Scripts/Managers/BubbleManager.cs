using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bubbles;
using JetBrains.Annotations;
using Plugins.Singleton;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Managers
{
    [Flags]
    public enum ParameterType
    {
        Generic = 0,
        Good = 1 << 0,
        Ignorant = 1 << 1,
        Despair = 1 << 2,
        FalseHope = 1 << 3
    }
    [Serializable]
    public class ParameterArchetype
    {
        [SerializeField][ReadOnly] private ParameterType parameterType;
        [SerializeField][ReadOnly] private float parameterScore;
        
        public ParameterType ParameterType => parameterType;
        public float ParameterScore => parameterScore;
        
        public ParameterArchetype(ParameterType parameterType)
        {
            this.parameterType = parameterType;
            parameterScore = 0;
        }
        
        public void ModifyParameterScore(float score)
        {
            
            parameterScore += score;
            if (parameterScore < 0) parameterScore = 0;
            Debug.Log($"Current {parameterType} score: {parameterScore}");
        }
    }
    [Serializable]
    public struct BubbleManagerSetting
    {
        [SerializeField] private BubbleWave waveSetting;
        
        public BubbleWave BubbleWave => waveSetting;
    }
    public class BubbleManager : MonoSingleton<BubbleManager>
    {
        [SerializeField] private Transform bubbleCanvas;
        [SerializeField] private List<BubbleManagerSetting> bubbleManagerSettings;
        [SerializeField] private Transform spawnPointPool;
        [SerializeField] private Transform usedSpawnPointPool;
        
        [Title("Debug")]
        [SerializeField] private List<ParameterArchetype> parameterArchetypes = new List<ParameterArchetype>()
        {
            new ParameterArchetype(ParameterType.Good),
            new ParameterArchetype(ParameterType.Ignorant),
            new ParameterArchetype(ParameterType.Despair),
            new ParameterArchetype(ParameterType.FalseHope)
        };
        
        private List<Transform> _availableSpawnPoints = new List<Transform>();

        
        
        public Transform BubbleCanvas => bubbleCanvas;
        public List<Transform> AvailableSpawnPoints => _availableSpawnPoints;

        protected override void Awake()
        {
            base.Awake();
            List<BubbleWave> playOnLevelStartWaveSettings =
                bubbleManagerSettings.FindAll(x => x.BubbleWave.PlayOnLevelStart).Select(x => x.BubbleWave).ToList();
            foreach (Transform child in spawnPointPool)
            {
                _availableSpawnPoints.Add(child);
            }
            playOnLevelStartWaveSettings.ForEach(x => x.PlayWave());
        }
        
        public void ReserveSpawnPoint(Transform spawnPoint)
        {
            _availableSpawnPoints.Remove(spawnPoint);
            spawnPoint.SetParent(usedSpawnPointPool);
        }
        
        public void FreeSpawnPoint(Transform spawnPoint)
        {
            _availableSpawnPoints.Add(spawnPoint);
            spawnPoint.SetParent(spawnPointPool);
        }

        public string GetBubbleDialogue(int dialogueLineIndex)
        {
            string path = Path.Combine(Application.streamingAssetsPath, "Dialogue", "BubbleDialogue.txt");
            path = path.Replace(@"\", "/");
            if (File.Exists(path))
            {
                string[] lines = File.ReadAllLines(path);
                return lines[dialogueLineIndex];
            }
            Debug.LogError("BubbleDialogue.txt not found");
            return null;
        }

        public int GetBubbleAnswers(int answerLineIndex, out string[] answerStrings)
        {
            answerStrings = null;
            string path = Path.Combine(Application.streamingAssetsPath, "Answer", "BubbleAnswer.txt");
            path = path.Replace(@"\", "/");
            if (File.Exists(path))
            {
                string[] lines = File.ReadAllLines(path);
                string answerLine = lines[answerLineIndex];
                string[] answers = answerLine.Split(';');
                for (int i = 0; i < answers.Length; i++)
                {
                    answers[i] = answers[i].Trim();
                }
                answerStrings = answers;
                return answers.Length;
            }
            Debug.LogError("BubbleAnswer.txt not found");
            return 0;
        }
        
        public void ModifyParameterScore(ParameterType parameterType, float score)
        {
            if (parameterType == ParameterType.Generic) return;
            Debug.Log($"Modify {parameterType} score by {score}");
            ParameterArchetype archetype = parameterArchetypes.Find(x => x.ParameterType == parameterType);
            archetype.ModifyParameterScore(score);
        }
        
        public List<ParameterType> SeparateParameterTypes(ParameterType parameterType)
        {
            List<ParameterType> parameterTypes = new List<ParameterType>();
            foreach (ParameterType type in Enum.GetValues(typeof(ParameterType)))
            {
                if (parameterType.HasFlag(type) && type != ParameterType.Generic)
                {
                    parameterTypes.Add(type);
                }
            }
            return parameterTypes;
        }
    }
}
