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
        [Title("Prefab")]
        [SerializeField] private GameObject bubblePrefab;

        [Title("Dialogue")]
        [SerializeField][MinValue(0)] private int dialogueLineIndex;
        [SerializeField][MinValue(0)] private float fadeInDuration;
        [SerializeField][MinValue(0)] private float fadeOutDuration;
        [SerializeField][MinValue(0)] private float shrinkDuration;
        [SerializeField][MinValue(0.25f)] private float interval;

        [Title("Answer")]
        [SerializeField] private bool hasAnswer;
        [SerializeField][ShowIf(nameof(hasAnswer))][MinValue(0)] private int answerLineIndex;
        [SerializeField][ShowIf(nameof(hasAnswer))] private AudioClip[] answerSounds;
        [SerializeField][ShowIf(nameof(hasAnswer))][MinValue(0)] private int correctAnswerIndex;
        [SerializeField][ShowIf(nameof(hasAnswer))][MinValue(0)] private float scoreGain;
        [SerializeField][ShowIf(nameof(hasAnswer))][MinValue(0)] private float scoreLoss;
        
        public GameObject BubblePrefab => bubblePrefab;
        public float FadeInDuration => fadeInDuration;
        public float FadeOutDuration => fadeOutDuration;
        public float ShrinkDuration => shrinkDuration;
        public float Interval => interval;
        public int DialogueLineIndex => dialogueLineIndex;
        public bool HasAnswer => hasAnswer;
        public int AnswerLineIndex => answerLineIndex;
        public AudioClip[] AnswerSounds => answerSounds;
        public int CorrectAnswerIndex => correctAnswerIndex;
        public float ScoreGain => scoreGain;
        public float ScoreLoss => scoreLoss;
    }
    public class Bubble : MonoBehaviour, IPointerEnterHandler
    {
        [SerializeField] private TMP_Text dialogueText;
        [SerializeField] private BubbleAnswer answerPrefab;
        [SerializeField] private RectTransform[] answerSpawnPoints;

        private bool _hasPointerEntered;
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

        public void Init(BubbleWave wave, BubbleSettings settings)
        {
            _bubbleWave = wave;
            _image = GetComponent<Image>();
            _textTyper = GetComponentInChildren<TextTyper>();
            _settings = settings;
            string dialogue = BubbleManager.GetBubbleDialogue(_settings.DialogueLineIndex);
            _textTyper.TypeText(dialogue, 0.025f);
            dialogueText.text = dialogue;
            if (_settings.HasAnswer)
            {
                SpawnAnswers();
            }
            _stayDuration = dialogue.Length * 0.1f;
            StartFadeIn(_settings.FadeInDuration);
        }
        
        private void Update()
        {
            _stayDuration -= Time.deltaTime;
            if (_stayDuration <= 0f && !_fadeOutTween.IsActive())
            {
                StartFadeOut(_settings.FadeOutDuration);
            }
        }

        private void SpawnAnswers()
        {
            int answerCount = BubbleManager.GetBubbleAnswers(_settings.AnswerLineIndex, out string[] answers);
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
                answer.Init(this);
                answer.AnswerText.text = answers[i];
                answer.IsCorrectAnswer = i == _settings.CorrectAnswerIndex;
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
            _fadeOutTween = _image.DOFade(0f, duration).OnComplete(() => DestroyBubble());
            dialogueText.DOFade(0f, duration);
        }
        
        private void DestroyBubble(bool ignored = true)
        {
            if (ignored)
            {
                //Do something here
            }
            BubbleManager.FreeSpawnPoint(CurrentSpawnPoint);
            _bubbleWave.SetNextBubble();
            Destroy(gameObject);
        }
        
        public void ShrinkBubble()
        {
            transform.DOScale(new Vector3(0, 0, 0), _settings.ShrinkDuration).OnComplete(() => DestroyBubble(false));
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
