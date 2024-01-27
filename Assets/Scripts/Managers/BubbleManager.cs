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
        public List<ParameterArchetype> ParameterArchetypes => parameterArchetypes;
        public List<Transform> AvailableSpawnPoints => _availableSpawnPoints;
        private bool _lastBubbleIgnored = true;
        public bool LastBubbleIgnored {get => _lastBubbleIgnored; set => _lastBubbleIgnored = value;}

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

        [Title("IO")]
        [SerializeField] private BubbleWave waveToImport;
        [SerializeField] private string fileName;
        private List<BubbleSettings> _overrideBubbleSettings = new List<BubbleSettings>();
        [Button("Import CSV")]
        private void ImportCSV()
        {
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
                for (int j = 1; j < 3; j++)
                {
                    if (splitLines[j] != "")
                        leadingsWithInterval.Add(splitLines[j]);
                }
                string questionWithInterval = splitLines[3];
                List<string> answers = new List<string>();
                for (int j = 4; j < 7; j++)
                {
                    if (splitLines[j] != string.Empty)
                        answers.Add(splitLines[j]);
                }
                string combinedResult = splitLines[7];
                AddLeading(leadingsWithInterval.ToArray());
                AddQuestion(questionWithInterval, answers.ToArray(), combinedResult);
            }
            waveToImport.BubbleSettings = _overrideBubbleSettings;
        }

        private void AddLeading(string[] leadingsWithInterval)
        {
            foreach (string s in leadingsWithInterval)
            {
                Debug.Log(s);
                int startIndex = s.IndexOf('<');
                int endIndex = s.IndexOf('>');
                string leading = s.Substring(0, startIndex);
                string interval = s.Substring(startIndex + 1, endIndex - startIndex - 1);
                _overrideBubbleSettings.Add(new BubbleSettings(leading, Convert.ToSingle(interval)));
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
            for (var i = 0; i < types.Count - 1; i++)
            {
                answerSettings.Add(new BubbleAnswerSettings(answers[i], new AnswerArchetype(types[i], new[] {scores[i]})));
            }
            _overrideBubbleSettings.Add(new BubbleSettings(question, Convert.ToSingle(interval), types[^1],
                new[] { scores[^1] }, true, answerSettings));
        }
    }
}
