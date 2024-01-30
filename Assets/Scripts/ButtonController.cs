using DG.Tweening;
using Managers;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Animator _animator;
    [Header("OnMouseHover")]
    [SerializeField] private float _hoverScale = 1.1f;
    [SerializeField] AudioClip _hoverSound;
    private Tween _hoverTween;
    void Start()
    {
        _animator = GetComponent<Animator>();
        
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_animator)
            _animator.enabled = false;
        
        transform.localScale = Vector3.one * _hoverScale;
        SoundManager.Instance.PlayFx(_hoverSound, out _);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_animator)
            _animator.enabled = true;
        transform.localScale = Vector3.one;
    }
}
