using System;
using System.Collections.Generic;
using DG.Tweening;
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
    public Color meatColor; // Delete this after done // sprite
    public float CookedTime;
    public FoodState NextFoodState;
}

public class Meat : MonoBehaviour, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
{
    public List<CookedState> cookedStates;
    public FoodState FoodState => _currentFoodState;
    public bool IsBurnt => _currentFoodState == FoodState.Burnt;
    
    private Vector3 _spawnPosition;
    private Image _image;
    private FoodState _currentFoodState;
    private CookedState _currentCookedState;
    private float _currentCookedTime;
    private float _cookedTimeOnGrill;
    
    private void Start()
    {
        _spawnPosition = transform.position;
        _currentFoodState = cookedStates[0].FoodState;
        _currentCookedTime = cookedStates[0].CookedTime;
        _currentCookedState = cookedStates[0];
        _cookedTimeOnGrill = 0f;
        _image = GetComponent<Image>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        GetComponent<Image>().raycastTarget = false;
        transform.position = eventData.position;
        transform.DOScale(Vector3.one * 1.5f, 0.25f);
        transform.SetParent(GameObject.Find("Canvas").transform);
        _grillAnimation.Kill();
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        transform.DOScale(Vector3.one, 0.25f);
        GetComponent<Image>().raycastTarget = true;
        if (!eventData.pointerEnter || !eventData.pointerEnter.CompareTag("Stove"))
        {
            transform.position = _spawnPosition;
            return;
        }
        eventData.pointerEnter.GetComponent<Stove>().DropFood(this);
    }

    public void ChangeFoodStateHandler()
    {
        if (_cookedTimeOnGrill < _currentCookedTime) return;
        _currentFoodState = _currentCookedState.NextFoodState;
        _currentCookedState = cookedStates.Find(cookedState => cookedState.FoodState == _currentFoodState);
        _currentCookedTime = _currentCookedState.CookedTime;
        _cookedTimeOnGrill = 0f;
        _image.DOColor(_currentCookedState.meatColor, 0.75f);
        
        if (FoodState != FoodState.Burnt) return;
        this.DOKill();
        _grillAnimation.Kill(true);
        transform.localScale = Vector3.one;
        _image.raycastTarget = false;
    }

    public void OnGrill()
    {
        if (FoodState == FoodState.Burnt) return;
        _cookedTimeOnGrill += Time.deltaTime;
        ChangeFoodStateHandler();
        PlayGrillAnimation();
    }
    
    private Tween _grillAnimation;
    public void PlayGrillAnimation()
    {
        if (_grillAnimation.IsActive()) return;
        _grillAnimation = transform.DOScale(Vector2.one * 1.1f, 0.75f).SetLoops(2, LoopType.Yoyo);
    }
}