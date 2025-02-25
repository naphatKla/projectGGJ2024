using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
        [SerializeField] private List<BubbleManagerSetting> bubbleManagerSettings_TH;
        [SerializeField] private List<BubbleManagerSetting> bubbleManagerSettings_EN;
        [SerializeField] private Transform spawnPointPool;
        [SerializeField] private Transform usedSpawnPointPool;
        public Bubble currentBubble;
        
        [Title("Debug")]
        [SerializeField][ReadOnly] private List<BubbleManagerSetting> currentBubbleManagerSettings;
        [SerializeField][ReadOnly] private List<ParameterArchetype> parameterArchetypes = new List<ParameterArchetype>()
        {
            new ParameterArchetype(ParameterType.Good),
            new ParameterArchetype(ParameterType.Ignorant),
            new ParameterArchetype(ParameterType.Despair),
            new ParameterArchetype(ParameterType.FalseHope)
        };
        
        private List<Transform> _availableSpawnPoints = new List<Transform>();
        
        
        public List<BubbleManagerSetting> CurrentBubbleManagerSettings => currentBubbleManagerSettings;
        public Transform BubbleCanvas => bubbleCanvas;
        public List<ParameterArchetype> ParameterArchetypes => parameterArchetypes;
        public List<Transform> AvailableSpawnPoints => _availableSpawnPoints;
        private bool _lastBubbleIgnored = true;
        public bool LastBubbleIgnored {get => _lastBubbleIgnored; set => _lastBubbleIgnored = value;}

        protected override void Awake()
        {
            base.Awake();
            if (PlayerPrefs.HasKey("Language"))
            {
                switch (PlayerPrefs.GetString("Language"))
                {
                    case "TH":
                        currentBubbleManagerSettings = bubbleManagerSettings_TH;
                        break;
                    case "EN":
                        currentBubbleManagerSettings = bubbleManagerSettings_EN;
                        break;
                    default:
                        currentBubbleManagerSettings = bubbleManagerSettings_EN;
                        break;
                }
            }
            else
            {
                currentBubbleManagerSettings = bubbleManagerSettings_EN;
            }
            List<BubbleWave> playOnLevelStartWaveSettings =
                currentBubbleManagerSettings.FindAll(x => x.BubbleWave.PlayOnLevelStart).Select(x => x.BubbleWave).ToList();
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

        [Title("IO")]
        [SerializeField] private BubbleWave waveToImport;
        [SerializeField] private string fileName;
        private int _currentId;
        private List<BubbleSettings> _overrideBubbleSettings = new List<BubbleSettings>();
        [Button("Import CSV")]
        private void ImportCSV()
        {
            _currentId = 0;
            _overrideBubbleSettings = new List<BubbleSettings>();
            string path = Path.Combine(Application.streamingAssetsPath, "Import", $"{fileName}.csv");
            path = path.Replace(@"\", "/");
            if (!File.Exists(path)) return;
            string[] allLines = File.ReadAllLines(path);
            Debug.Log(allLines[1]);
            for (int i = 1; i < allLines.Length; i++)
            {
                string[] splitLines = Regex.Split(allLines[i], ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
                for (int j = 0; j < splitLines.Length; j++)
                {
                    Debug.Log(splitLines[j]);
                    if (splitLines[j].Length > 1 && splitLines[j][1] == '"')
                    {
                        splitLines[j] = splitLines[j].Trim('"');
                        int lastQuoteIndex = splitLines[j].LastIndexOf('"');
                        splitLines[j] = splitLines[j].Remove(lastQuoteIndex, 1);
                        splitLines[j] = '"' + splitLines[j];
                        continue;
                    }
                    splitLines[j] = splitLines[j].Trim('"');
                }
                List<string> leadingsWithInterval = new List<string>();
                for (int j = 0; j < 2; j++)
                {
                    if (splitLines[j] != "")
                        leadingsWithInterval.Add(splitLines[j]);
                }
                string questionWithInterval = splitLines[2];
                List<string> answers = new List<string>();
                for (int j = 3; j < 6; j++)
                {
                    if (splitLines[j] != string.Empty)
                        answers.Add(splitLines[j]);
                }
                string combinedResult = splitLines[6];
                AddLeading(leadingsWithInterval.ToArray());
                if (answers.All(x => x == string.Empty)) continue;
                AddQuestion(questionWithInterval, answers.ToArray(), combinedResult);
            }
            waveToImport.BubbleSettings = _overrideBubbleSettings;
        }

        private void AddLeading(string[] leadingsWithInterval)
        {
            foreach (string s in leadingsWithInterval)
            {
                Debug.Log("test: " + s);
                int startIndex = s.IndexOf('<');
                int endIndex = s.IndexOf('>');
                string leading = s.Substring(0, startIndex);
                string interval = s.Substring(startIndex + 1, endIndex - startIndex - 1);
                _overrideBubbleSettings.Add(new BubbleSettings(_currentId, leading, Convert.ToSingle(interval)));
                _currentId++;
            }
        }

        private void AddQuestion(string questionWithInterval, string[] answers, string combinedResult)
        {
            int startIndex = questionWithInterval.IndexOf('<');
            int endIndex = questionWithInterval.IndexOf('>');
            string question = questionWithInterval.Substring(0, startIndex);
            string interval = questionWithInterval.Substring(startIndex + 1, endIndex - startIndex - 1);
            string[] splitResult = combinedResult.Split(',');
            List<ParameterType> types = new List<ParameterType>();
            List<float> scores = new List<float>();
            for (int i = 0; i < splitResult.Length; i++)
            {
                splitResult[i] = splitResult[i].Trim();
                string type = splitResult[i].Substring(0, 1);
                if (type == "-")
                {
                    
                    types.Add(ParameterType.Generic);
                    scores.Add(0);
                    continue;
                }
                float score = Convert.ToSingle(splitResult[i].Substring(1, splitResult[i].Length - 1));
                switch (type)
                {
                    case "I":
                        types.Add(ParameterType.Ignorant);
                        break;
                    case "D":
                        types.Add(ParameterType.Despair);
                        break;
                    case "F":
                        types.Add(ParameterType.FalseHope);
                        break;
                    case "G":
                        types.Add(ParameterType.Good);
                        break;
                }
                scores.Add(score);
            }
            List<BubbleAnswerSettings> answerSettings = new List<BubbleAnswerSettings>();
            Debug.Log(types.Count);
            for (var i = 0; i < answers.Length; i++)
            {
                answerSettings.Add(new BubbleAnswerSettings(answers[i], new AnswerArchetype(types[i], new[] {scores[i]})));
            }
            _overrideBubbleSettings.Add(new BubbleSettings(_currentId, question, Convert.ToSingle(interval), types[^1],
                new[] { scores[^1] }, true, answerSettings));
            _currentId++;
        }

        [Title("Debug")]
        [Button("Set TH")]
        private void SetTH()
        {
            PlayerPrefs.SetString("Language", "TH");
        }
        [Button("Set EN")]
        private void SetEN()
        {
            PlayerPrefs.SetString("Language", "EN");
        }
        
    }
}
