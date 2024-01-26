using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Bubbles
{
    public class BubbleAnswer : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private Bubble _bubble;
        private Image _image;
        private TMP_Text _answerText;
        private bool _isCorrectAnswer;
        
        public Image Image => _image;
        public TMP_Text AnswerText => _answerText;
        public bool IsCorrectAnswer {get => _isCorrectAnswer; set => _isCorrectAnswer = value;}

        public void Init(Bubble bubble)
        {
            _bubble = bubble;
        }
        private void Awake()
        {
            _image = GetComponent<Image>();
            _answerText = GetComponentInChildren<TMP_Text>();
        }
        

        public void OnPointerClick(PointerEventData eventData)
        {
            _bubble.ShrinkBubble();
            Debug.Log("Correct answer: " + _isCorrectAnswer);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
           
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            
        }
    }
}
