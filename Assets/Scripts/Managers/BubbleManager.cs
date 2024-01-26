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

    }
}
