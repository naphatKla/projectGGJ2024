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
    public struct AnswerArchetype
    {
        [SerializeField][OnValueChanged(nameof(ValidateAnswerScores))] private ParameterType parameterType;
        [SerializeField][ShowIf(nameof(_showAnswerScores))] private float[] answerScores;
        public ParameterType ParameterType => parameterType;
        public float[] AnswerScores => answerScores;

        private bool _showAnswerScores;

        private void ValidateAnswerScores()
        {
            _showAnswerScores = parameterType != ParameterType.Generic;
        }
    }
    [Serializable]
    public struct BubbleAnswerSettings
    {
        [SerializeField] private AudioClip answerSound;
        [SerializeField] private AnswerArchetype answerArchetype;
        public AudioClip AnswerSounds => answerSound;
        public AnswerArchetype AnswerArchetype => answerArchetype;
        public string AnswerString {get; set;}
    }
    public class BubbleAnswer : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
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
