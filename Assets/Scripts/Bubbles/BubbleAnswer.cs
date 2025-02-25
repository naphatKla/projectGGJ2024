using System;
using System.Collections.Generic;
using Managers;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Bubbles
{
    [Serializable]
    public class AnswerArchetype
    {
        [SerializeField][OnValueChanged(nameof(ValidateAnswerScores))][OnInspectorInit(nameof(ValidateAnswerScores))] private ParameterType parameterType;
        [SerializeField][ShowIf(nameof(_showAnswerScores))] private float[] answerScores;
        public ParameterType ParameterType => parameterType;
        public float[] AnswerScores => answerScores;

        private bool _showAnswerScores;

        private void ValidateAnswerScores()
        {
            _showAnswerScores = parameterType != ParameterType.Generic;
        }
        
        public AnswerArchetype(ParameterType parameterType, float[] answerScores)
        {
            this.parameterType = parameterType;
            this.answerScores = answerScores;
            _showAnswerScores = parameterType != ParameterType.Generic;
        }
    }
    [Serializable]
    public struct BubbleAnswerSettings
    {
        [SerializeField] private string answerString;
        [SerializeField] private AudioClip answerSound;
        [SerializeField] private AnswerArchetype answerArchetype;
        public AudioClip AnswerSounds => answerSound;
        public AnswerArchetype AnswerArchetype => answerArchetype;
        public string AnswerString => answerString;

        public BubbleAnswerSettings(string answerString, AnswerArchetype answerArchetype)
        {
            this.answerArchetype = answerArchetype;
            this.answerString = answerString;
            answerSound = null;
        }
    }
    public class BubbleAnswer : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private AudioClip answerSound;
        private BubbleManager BubbleManager => BubbleManager.Instance;
        private Bubble _bubble;
        private BubbleAnswerSettings _settings;
        private Image _image;
        private TMP_Text _answerText;

        public Image Image => _image;
        public TMP_Text AnswerText => _answerText;

        public void Init(Bubble bubble, BubbleAnswerSettings settings)
        {
            _bubble = bubble;
            _settings = settings;
            AnswerText.text = settings.AnswerString;
        }
        private void Awake()
        {
            _image = GetComponent<Image>();
            _answerText = GetComponentInChildren<TMP_Text>();
        }
        
        private void ModifyParameterScore()
        {
            List<ParameterType> separatedParameterTypes = BubbleManager.SeparateParameterTypes(_settings.AnswerArchetype.ParameterType);
            for (int i = 0; i < separatedParameterTypes.Count; i++)
            {
                Debug.Log(separatedParameterTypes[i]);
                BubbleManager.ModifyParameterScore(separatedParameterTypes[i], _settings.AnswerArchetype.AnswerScores[i]);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            ModifyParameterScore();
            SoundManager.Instance.PlayFx(answerSound, out AudioSource answerSource);
            answerSource.pitch = UnityEngine.Random.Range(0.8f, 1.2f);
            _bubble.ShrinkBubble();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
           
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            
        }
    }
}
