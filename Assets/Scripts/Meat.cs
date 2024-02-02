using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Managers;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum FoodState
{
    Raw,
    Cooked,
    Burnt
}
[Serializable] public struct CookedState
{
    public FoodState FoodState;
    public float CookedTime;
    public FoodState NextFoodState;
}

public class 
    Meat : MonoBehaviour, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
{
    public List<CookedState> cookedStates;
    public FoodState FoodState => _currentFoodState[_currentSide];
    public bool IsBurnt => _currentFoodState.Any(foodState => foodState == FoodState.Burnt);
    public bool IsOnGrill => transform.parent.CompareTag("Stove");
    public bool IsOnFlip {get; set;}
    public bool IsFlipping{get; set;}
    [SerializeField] Vector2 randomRotationRange;
    [SerializeField] ParticleSystem[] burntParticle;
    private bool _isMouseDown;
    private Stove _lastedStove;
    
    [SerializeField] private AudioClip dropSound;
    [SerializeField] private AudioClip flipSound;
    [SerializeField] private AudioClip burntSound;
    
    private Vector3 _spawnPosition;
    private Image _image;
    private FoodState[] _currentFoodState = new FoodState[2];
    private CookedState[] _currentCookedState = new CookedState[2];
    private float[] _currentCookedTime = new float[2];
    private float[] _cookedTimeOnGrill = new float[2];
    private int _currentSide;
    private Animator _animator;
    private Tween _holdClickTween;
    private float _lastedOppositeSideCookedTime;
    
    
    private void Start()
    {
        _spawnPosition = transform.position;
        for (int i = 0; i < 2; i++)
        {
            _currentFoodState[i] = cookedStates[0].FoodState;
            _currentCookedTime[i] = cookedStates[0].CookedTime;
            _currentCookedState[i] = cookedStates[0];
            _cookedTimeOnGrill[i] = 0f;
        }
        _currentSide = 0;
        _image = GetComponent<Image>();
        _animator = GetComponent<Animator>();
        _image.alphaHitTestMinimumThreshold = 0.1f;
        DOVirtual.DelayedCall(0.5f, ()=> _animator.SetBool("IsSpawning", false));
        transform.rotation = Quaternion.Euler(0, 0, UnityEngine.Random.Range(randomRotationRange.x, randomRotationRange.y));
    }
    
    private void FixedUpdate()
    {
        _animator.SetBool("OnGrill", IsOnGrill);
        _animator.SetBool("IsBurnt", IsBurnt);
        _animator.SetBool("IsOnFlip", IsOnFlip);
        _animator.SetBool("IsCurrentSideCooked", _currentSide == 1? _currentFoodState[0] == FoodState.Cooked : _currentFoodState[1] == FoodState.Cooked);
        _animator.SetBool("IsOppositeSideCooked", _currentSide == 1? _currentFoodState[1] == FoodState.Cooked : _currentFoodState[0] == FoodState.Cooked);
        foreach (ParticleSystem particle in burntParticle)
            particle.gameObject.SetActive(_currentFoodState[_currentSide] == FoodState.Cooked && IsOnGrill);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (GameManager.Instance.IsLose || GameManager.Instance.IsWin) return;
        if (IsFlipping) return;
        if (eventData.button != PointerEventData.InputButton.Left) return;
        _isMouseDown = true;
        if (IsOnGrill && !_holdClickTween.IsActive())
            _holdClickTween = DOVirtual.DelayedCall(0.25f, () =>
            {
                IsFlipping = true;
                FlipController.Instance.OpenBar(this, GetComponent<RectTransform>().position);
                FlipController.Instance.OnPerfect += FlipSide;
                FlipController.Instance.OnFail += CancelFlip;
            }).OnComplete(() => IsOnFlip = true);
        GetComponent<Image>().raycastTarget = false;
        transform.DOScale(Vector3.one * 1.5f, 0.25f);
        transform.SetParent(GameObject.Find("GamePlayCanvas").transform);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (GameManager.Instance.IsLose || GameManager.Instance.IsWin) return;
        if (!_isMouseDown) return;
        if (IsFlipping) return;
        if (IsOnFlip) return;
        if (eventData.button != PointerEventData.InputButton.Left) return;
        transform.position = (Vector2)Camera.main.ScreenToWorldPoint(eventData.position);
        if (BubbleManager.Instance.currentBubble)
            BubbleManager.Instance.currentBubble.GetComponent<Image>().raycastTarget = false;
        _holdClickTween.Kill();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!_isMouseDown) return;
        if (eventData.button != PointerEventData.InputButton.Left) return;
        _isMouseDown = false;
        if (BubbleManager.Instance.currentBubble)
            BubbleManager.Instance.currentBubble.GetComponent<Image>().raycastTarget = true;
        IsOnFlip = false;
        _holdClickTween.Kill();
        transform.DOScale(Vector3.one, 0.25f);
        GetComponent<Image>().raycastTarget = true;
        
        SoundManager.Instance.PlayFx(dropSound,out AudioSource dropSource);
        dropSource.pitch = UnityEngine.Random.Range(0.8f, 1.2f);
        dropSource.volume = 0.6f;
        
        if (!eventData.pointerEnter || !eventData.pointerEnter.CompareTag("Stove") && !IsFlipping)
        {
            FoodSpawnerManager.Instance.AddFoodToTheNearestSlot(this);
            return;
        }
        
        Stove stove = eventData.pointerEnter.GetComponent<Stove>();
        if (!stove)
        {
            _lastedStove.DropFood(this);
            return;
        }
            
        if (stove.AvailableSlot <= 0)
        {
            FoodSpawnerManager.Instance.AddFoodToTheNearestSlot(this);
            return;
        }
        stove.DropFood(this);
        _lastedStove = stove;
    }

    public void ChangeFoodStateHandler()
    {
        if (GameManager.Instance.IsLose || GameManager.Instance.IsWin) return;
        if (_cookedTimeOnGrill[_currentSide] < _currentCookedTime[_currentSide]) return;
        _currentFoodState[_currentSide] = _currentCookedState[_currentSide].NextFoodState;
        _currentCookedState[_currentSide] = cookedStates.Find(cookedState => cookedState.FoodState == _currentFoodState[_currentSide]);
        _currentCookedTime[_currentSide] = _currentCookedState[_currentSide].CookedTime;
        _cookedTimeOnGrill[_currentSide] = 0f;
        
        if (FoodState != FoodState.Burnt) return;
        this.DOKill();
        DOVirtual.DelayedCall(0.5f, () => { FoodSpawnerManager.Instance.SpawnFood();});
        transform.localScale = Vector3.one;
        _image.raycastTarget = false;
        GameManager.Instance.meatBurned++;
        SoundManager.Instance.PlayFx(burntSound, out AudioSource flipSource);
        flipSource.pitch = UnityEngine.Random.Range(0.8f, 1.2f);
        GameManager.Instance.meatBurnFeedback.PlayFeedbacks();
    }
    
    public void FlipSide()
    {
        if (_animator.GetCurrentAnimatorStateInfo(0).IsTag("Flip")) return;
        if (IsBurnt) return;
        _lastedOppositeSideCookedTime = _cookedTimeOnGrill[_currentSide];
        _currentSide = _currentSide == 0 ? 1 : 0;
        _animator.SetTrigger("IsFlip");
        DOVirtual.DelayedCall(1, () => IsFlipping = false);
        SoundManager.Instance.PlayFx(flipSound, out _);
    }

    public void CancelFlip()
    {
        if (IsBurnt) return;
        IsFlipping = false;
    }

    public void OnGrill()
    {
        if (FoodState == FoodState.Burnt) return;
        _cookedTimeOnGrill[_currentSide] += Time.deltaTime;
        ChangeFoodStateHandler();
        if (_cookedTimeOnGrill[_currentSide == 0? 1 : 0] <= 0 || _cookedTimeOnGrill[_currentSide == 0? 1 : 0] <= _lastedOppositeSideCookedTime-1.5f) return;
        _cookedTimeOnGrill[_currentSide == 0? 1 : 0] -= Time.deltaTime;
    }
    
    private void OnApplicationFocus(bool hasFocus)
    { 
        _isMouseDown = false;
        if (BubbleManager.Instance.currentBubble)
            BubbleManager.Instance.currentBubble.GetComponent<Image>().raycastTarget = true;
        _holdClickTween.Kill();
        GetComponent<Image>().raycastTarget = true;
        FoodSpawnerManager.Instance.AddFoodToTheNearestSlot(this);
        
    }
}