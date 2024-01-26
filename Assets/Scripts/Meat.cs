using System;
using System.Collections.Generic;
using System.Linq;
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
    public FoodState FoodState => _currentFoodState[_currentSide];
    public bool IsBurnt => _currentFoodState.Any(foodState => foodState == FoodState.Burnt);
    public bool IsOnGrill => transform.parent.CompareTag("Stove");
    
    private Vector3 _spawnPosition;
    private Image _image;
    private FoodState[] _currentFoodState = new FoodState[2];
    private CookedState[] _currentCookedState = new CookedState[2];
    private float[] _currentCookedTime = new float[2];
    private float[] _cookedTimeOnGrill = new float[2];
    private int _currentSide;
    private Animator _animator;
    
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
    }

    private void FixedUpdate()
    {
        _animator.SetBool("OnGrill", IsOnGrill);
        _animator.SetBool("IsBurnt", IsBurnt);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log(IsOnGrill);
        if (IsOnGrill && eventData.button == PointerEventData.InputButton.Right)
        {
            FlipSide();
            Debug.Log("Flip");
            return;
        }
        if (eventData.button != PointerEventData.InputButton.Left) return;
        GetComponent<Image>().raycastTarget = false;
        transform.position = eventData.position;
        transform.DOScale(Vector3.one * 1.5f, 0.25f);
        transform.SetParent(GameObject.Find("Canvas").transform);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
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
        if (_cookedTimeOnGrill[_currentSide] < _currentCookedTime[_currentSide]) return;
        _currentFoodState[_currentSide] = _currentCookedState[_currentSide].NextFoodState;
        _currentCookedState[_currentSide] = cookedStates.Find(cookedState => cookedState.FoodState == _currentFoodState[_currentSide]);
        _currentCookedTime[_currentSide] = _currentCookedState[_currentSide].CookedTime;
        _cookedTimeOnGrill[_currentSide] = 0f;
        
        if (FoodState != FoodState.Burnt) return;
        this.DOKill();
        transform.localScale = Vector3.one;
        _image.raycastTarget = false;
        _image.DOColor(Color.black, 0.15f);
    }
    
    public void FlipSide()
    {
        if (_animator.GetCurrentAnimatorStateInfo(0).IsName("Flip")) return;
        if (IsBurnt) return;
        _image.DOColor(_currentCookedState[_currentSide].meatColor, 0.5f);
        _currentSide = _currentSide == 0 ? 1 : 0;
        _animator.SetTrigger("IsFlip");
    }

    public void OnGrill()
    {
        if (FoodState == FoodState.Burnt) return;
        _cookedTimeOnGrill[_currentSide] += Time.deltaTime;
        ChangeFoodStateHandler();
    }
}