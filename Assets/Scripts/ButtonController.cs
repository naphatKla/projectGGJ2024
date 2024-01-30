using DG.Tweening;
using Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Animator _animator;
    [Header("OnMouseHover")]
    [SerializeField] private float _hoverScale = 1.1f;
    [SerializeField] AudioClip _hoverSound;
    [Header("OnMouseClick")]
    [SerializeField] private AudioClip clickSound;
    private float _lastHoverTime;
    private Tween _hoverTween;
    void Start()
    {
        _animator = GetComponent<Animator>();
        GetComponent<Button>().onClick.AddListener(() =>
        {
            SoundManager.Instance.PlayFx(clickSound, out AudioSource clickSource);
            clickSource.pitch = Random.Range(0.9f, 1.1f);
            clickSource.volume = 0.3f;
        });
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_animator)
            _animator.enabled = false;
        
        transform.localScale = Vector3.one * _hoverScale;
        SoundManager.Instance.PlayFx(_hoverSound, out AudioSource hoverSource);
        hoverSource.pitch = Random.Range(0.9f, 1.1f);
        hoverSource.volume = 0.3f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_animator)
            _animator.enabled = true;
        transform.localScale = Vector3.one;
    }
}
