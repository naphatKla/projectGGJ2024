using System;
using System.Collections.Generic;
using DG.Tweening;
using Managers;
using RedBlueGames.Tools.TextTyper;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Bubbles
{
    [Serializable]
    public struct BubbleSettings
    {
        [Title("Dialogue")]
        [SerializeField] private string dialogueString;
        [SerializeField][MinValue(0.25f)] private float interval;

        [Title("Answer")]
        [SerializeField][OnValueChanged(nameof(ValidateAnswerScores))][OnInspectorInit(nameof(ValidateAnswerScores))] private ParameterType ignoreParameterType;
        [SerializeField][ShowIf(nameof(_showIgnoreScore))] private float[] ignoreParameterScores;
        [SerializeField] private bool hasAnswer;
        //[SerializeField][ShowIf(nameof(hasAnswer))] private string answerString;
        [SerializeField][ShowIf(nameof(hasAnswer))] private List<BubbleAnswerSettings> answerSettings;

        private bool _showIgnoreScore;
        public float Interval => interval;
        public string DialogueString => dialogueString;
        public ParameterType IgnoreParameterType => ignoreParameterType;
        public float[] IgnoreParameterScores => ignoreParameterScores;
        public bool HasAnswer => hasAnswer;
        //public string AnswerString => answerString;
        public List<BubbleAnswerSettings> AnswerSettings => answerSettings;
        
        private void ValidateAnswerScores()
        {
            _showIgnoreScore = ignoreParameterType != ParameterType.Generic;
        }
        
        public BubbleSettings(string dialogueString, float interval, ParameterType ignoreParameterType = ParameterType.Generic,
            float[] ignoreParameterScores = null, bool hasAnswer = false,
            List<BubbleAnswerSettings> answerSettings = null)
        {
            this.dialogueString = dialogueString;
            this.interval = interval;
            this.ignoreParameterType = ignoreParameterType;
            this.ignoreParameterScores = ignoreParameterScores;
            this.hasAnswer = hasAnswer;
            //this.answerString = answerString;
            this.answerSettings = answerSettings;
            _showIgnoreScore = ignoreParameterType != ParameterType.Generic;
        }
    }
    public class Bubble : MonoBehaviour, IPointerEnterHandler
    {
        [SerializeField] private TMP_Text dialogueText;
        [SerializeField] private BubbleAnswer answerPrefab;
        [SerializeField] private RectTransform[] answerSpawnPoints;
        [SerializeField][MinValue(0)] private float fadeInDuration;
        [SerializeField][MinValue(0)] private float fadeOutDuration;
        [SerializeField][MinValue(0)] private float shrinkDuration;

        private bool _hasPointerEntered;
        private bool _answered;
        private float _stayDuration;
        
        private BubbleManager BubbleManager => BubbleManager.Instance;
        private BubbleWave _bubbleWave;
        private List<BubbleAnswer> _bubbleAnswers = new List<BubbleAnswer>();
        private BubbleSettings _settings;
        private TextTyper _textTyper;
        private Image _image;
        private Tween _fadeInTween;
        private Tween _fadeOutTween;
        
        public Transform CurrentSpawnPoint { get; set; }
        public float FadeInDuration => fadeInDuration;
        public float FadeOutDuration => fadeOutDuration;
        public float ShrinkDuration => shrinkDuration;

        public void Init(BubbleWave wave, BubbleSettings settings)
        {
            _bubbleWave = wave;
            _image = GetComponent<Image>();
            _textTyper = GetComponentInChildren<TextTyper>();
            _settings = settings;
            string dialogue = settings.DialogueString;
            dialogueText.text = dialogue;
            _textTyper.TypeText(dialogue, 0.025f);
            if (_settings.HasAnswer)
            {
                SpawnAnswers();
            }
            _stayDuration = dialogue.Length * 0.1f;
            StartFadeIn(fadeInDuration);
        }
        
        private void Update()
        {
            _stayDuration -= Time.deltaTime;
            if (_stayDuration <= 0f && !_fadeOutTween.IsActive())
            {
                StartFadeOut(fadeOutDuration);
            }
        }

        private void SpawnAnswers()
        {
            int answerCount = _settings.AnswerSettings.Count;
            for (int i = answerCount - 1; i >= 0; i--)
            {
                Vector3 spawnPoint = answerSpawnPoints[i].anchoredPosition;
                bool flipX = transform.localPosition.x > 0;
                if (flipX)
                {
                    spawnPoint.x *= -1;
                }
                Transform thisTransform = transform;
                spawnPoint = thisTransform.TransformPoint(spawnPoint);
                BubbleAnswer answer = Instantiate(answerPrefab, spawnPoint, Quaternion.identity, thisTransform);
                var settingsAnswerSettings = _settings.AnswerSettings[i];
                //settingsAnswerSettings.AnswerString = answers[i];
                answer.Init(this, settingsAnswerSettings);
                _bubbleAnswers.Add(answer);
                answer.gameObject.SetActive(false);
            }
        }
        
        private void StartFadeIn(float duration)
        {
            dialogueText.color = new Color(dialogueText.color.r, dialogueText.color.g, dialogueText.color.b, 0f);
            dialogueText.DOFade(1f, duration);
            _image.color = new Color(1f, 1f, 1f, 0f);
            _image.DOFade(1f, duration);
        }
        
        private void StartFadeOut(float duration)
        {
            foreach (BubbleAnswer answer in _bubbleAnswers)
            {
                answer.Image.DOFade(0f, duration);
                answer.AnswerText.DOFade(0f, duration);
            }
            _fadeOutTween = _image.DOFade(0f, duration).OnComplete(() =>
            {
                if (!_answered)
                    DestroyBubble();
            });
            dialogueText.DOFade(0f, duration);
        }
        
        private void DestroyBubble(bool ignored = true)
        {
            if (ignored)
            {
                List<ParameterType> separatedParameterTypes = BubbleManager.SeparateParameterTypes(_settings.IgnoreParameterType);
                for (int i = 0; i < separatedParameterTypes.Count; i++)
                {
                    Debug.Log(separatedParameterTypes[i]);
                    BubbleManager.ModifyParameterScore(separatedParameterTypes[i], _settings.IgnoreParameterScores[i]);
                }
            }
            BubbleManager.LastBubbleIgnored = ignored;
            BubbleManager.FreeSpawnPoint(CurrentSpawnPoint);
            _bubbleWave.SetNextBubble();
            Destroy(gameObject);
        }
        
        public void ShrinkBubble()
        {
            _answered = true;
            transform.DOScale(new Vector3(0, 0, 0), shrinkDuration).OnComplete(() => DestroyBubble(false));
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_hasPointerEntered)
            {
                return;
            }
            if (_stayDuration > 0f)
            {
                Debug.Log("Plus Time");
                _stayDuration += 2f;
            }
            _bubbleAnswers.ForEach(answer => answer.gameObject.SetActive(true));
            _hasPointerEntered = true;
        }
    }
}
