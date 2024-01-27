using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Manager;
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

public class Meat : MonoBehaviour, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
{
    public List<CookedState> cookedStates;
    public FoodState FoodState => _currentFoodState[_currentSide];
    public bool IsBurnt => _currentFoodState.Any(foodState => foodState == FoodState.Burnt);
    public bool IsOnGrill => transform.parent.CompareTag("Stove");
    public bool IsOnFlip {get; set;}
    [SerializeField] ParticleSystem burntParticle;
    
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
    }

    private void FixedUpdate()
    {
        _animator.SetBool("OnGrill", IsOnGrill);
        _animator.SetBool("IsBurnt", IsBurnt);
        _animator.SetBool("IsOnFlip", IsOnFlip);
        _animator.SetBool("IsCurrentSideCooked", _currentSide == 1? _currentFoodState[0] == FoodState.Cooked : _currentFoodState[1] == FoodState.Cooked);
        _animator.SetBool("IsOppositeSideCooked", _currentSide == 1? _currentFoodState[1] == FoodState.Cooked : _currentFoodState[0] == FoodState.Cooked);
        burntParticle.gameObject.SetActive(_currentFoodState[_currentSide] == FoodState.Cooked && IsOnGrill);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (IsOnGrill && !_holdClickTween.IsActive())
            _holdClickTween = DOVirtual.DelayedCall(0.25f, () =>
            {
                FlipController.Instance.OpenBar(this, GetComponent<RectTransform>().position);
                FlipController.Instance.OnPerfect += FlipSide;
            }).OnComplete(() => IsOnFlip = true);
        GetComponent<Image>().raycastTarget = false;
        transform.DOScale(Vector3.one * 1.5f, 0.25f);
        transform.SetParent(GameObject.Find("GamePlayCanvas").transform);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (IsOnFlip) return;
        if (eventData.button != PointerEventData.InputButton.Left) return;
        ChopsticksController.Instance.Animator.SetBool("IsPick", true);
        transform.position = (Vector2)Camera.main.ScreenToWorldPoint(eventData.position);
        _holdClickTween.Kill();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        ChopsticksController.Instance.Animator.SetTrigger("IsDrop");
        IsOnFlip = false;
        _holdClickTween.Kill();
        transform.DOScale(Vector3.one, 0.25f);
        GetComponent<Image>().raycastTarget = true;
        
        if (!eventData.pointerEnter || !eventData.pointerEnter.CompareTag("Stove") && !IsOnFlip)
        {
            FoodSpawnerManager.Instance.AddFoodToTheNearestSlot(this);
            return;
        }
        
        Stove stove = eventData.pointerEnter.GetComponent<Stove>();
        if (stove.AvailableSlot <= 0)
        {
            FoodSpawnerManager.Instance.AddFoodToTheNearestSlot(this);
            return;
        }
        stove.DropFood(this);
    }

    public void ChangeFoodStateHandler()
    {
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
    }
    
    public void FlipSide()
    {
        if (_animator.GetCurrentAnimatorStateInfo(0).IsName("Flip")) return;
        if (IsBurnt) return;
        _lastedOppositeSideCookedTime = _cookedTimeOnGrill[_currentSide];
        _currentSide = _currentSide == 0 ? 1 : 0;
        _animator.SetTrigger("IsFlip");
        ChopsticksController.Instance.Animator.SetTrigger("IsFlip");
    }

    public void OnGrill()
    {
        if (FoodState == FoodState.Burnt) return;
        _cookedTimeOnGrill[_currentSide] += Time.deltaTime;
        ChangeFoodStateHandler();
        if (_cookedTimeOnGrill[_currentSide == 0? 1 : 0] <= 0 || _cookedTimeOnGrill[_currentSide == 0? 1 : 0] <= _lastedOppositeSideCookedTime-1.5f) return;
        _cookedTimeOnGrill[_currentSide == 0? 1 : 0] -= Time.deltaTime;
    }
}