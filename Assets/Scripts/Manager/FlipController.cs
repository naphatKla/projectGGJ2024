using System;
using Plugins.Singleton;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class FlipController : MonoSingleton<FlipController>
{
    [SerializeField] private Image flipBar;
    [SerializeField] private Image perfectBar;
    [SerializeField] private Image Indicator;
    [Header("Bar Settings")]
    [SerializeField] [Range(0,1)] private float perfectBarHighPercent;
    [SerializeField] private float indicatorSpeed;
    [SerializeField] private Vector2 barOffset;
    public event Action OnPerfect, OnFail, OnOpenBar;

    private float _barHigh;
    private float _perfectBarHigh;
    private Vector2 _perfectBarAvailableRange;
    private Vector2 _perfectBarRange;
    private Vector2 _perfectBarRangePercent;
    private int _indicatorDirection = 1;
    private Meat _currentMeat;

    protected override void Awake()
    {
        flipBar.gameObject.SetActive(true);
        base.Awake();
        flipBar.gameObject.SetActive(false);
    }
    void Start()
    {
        _barHigh = flipBar.rectTransform.rect.height;
        _perfectBarHigh = _barHigh * perfectBarHighPercent;
        _perfectBarAvailableRange = new Vector2(-(_barHigh/2) + (_perfectBarHigh/2), (_barHigh/2) - (_perfectBarHigh/2)); // min, max
        perfectBar.rectTransform.sizeDelta = new Vector2(perfectBar.rectTransform.rect.width, _perfectBarHigh);
        
        RandomPosition();
    }

    public void Update()
    {
        if (Input.GetMouseButtonUp(0))
            OperateBar();
        
        if (Indicator.rectTransform.localPosition.y >= _barHigh/2 && _indicatorDirection != -1)
            _indicatorDirection = -1;
        else if (Indicator.rectTransform.localPosition.y <= -_barHigh/2 && _indicatorDirection != 1)
            _indicatorDirection = 1;
        
        Indicator.rectTransform.localPosition = _indicatorDirection == 1? Vector3.MoveTowards(Indicator.rectTransform.localPosition, new Vector3(0,_barHigh/2,0), indicatorSpeed * Time.deltaTime)
        : Vector3.MoveTowards(Indicator.rectTransform.localPosition, new Vector3(0,-_barHigh/2,0), indicatorSpeed * Time.deltaTime);
        OnOpenBar?.Invoke();
        
        if(_currentMeat && _currentMeat.IsBurnt)
            CloseBar();
    }

    public void OpenBar(Meat meat, Vector2 position = default)
    {
        flipBar.gameObject.SetActive(true);
        flipBar.rectTransform.position = position + barOffset;
        _currentMeat = meat;
    }
    
    public void CloseBar()
    {
        flipBar.gameObject.SetActive(false);
        OnPerfect = null;
        OnFail = null;
        OnOpenBar = null;
    }
    
    public void RandomPosition()
    {
        float randomY = Random.Range(_perfectBarAvailableRange.x, _perfectBarAvailableRange.y);
        perfectBar.rectTransform.localPosition = new Vector3(0, randomY, 0);
    }
    
    public void OperateBar()
    {
        float yPosIndicator = Indicator.rectTransform.localPosition.y;
        bool isSucceed = yPosIndicator >= perfectBar.rectTransform.localPosition.y - _perfectBarHigh/2 && yPosIndicator <= perfectBar.rectTransform.localPosition.y + _perfectBarHigh/2; 
        if (isSucceed)
        {
            OnPerfect?.Invoke();
            gameObject.SetActive(false);
            RandomPosition();
        }
        else
        {
            OnFail?.Invoke();
            gameObject.SetActive(false);
        }
        CloseBar();
    }
    

}
