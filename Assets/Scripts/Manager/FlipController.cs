
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class FlipController : MonoBehaviour
{
    [SerializeField] private Image flipBar;
    [SerializeField] private Image perfectBar;
    [SerializeField] private Image Indicator;
    [Header("Bar Settings")]
    [SerializeField] [Range(0,1)] private float perfectBarHighPercent;
    
    private float _barHigh;
    private float _perfectBarHigh;
    private Vector2 _perfectBarAvailableRange;
    private Vector2 _perfectBarRange;
    [SerializeField] private Vector2 _perfectBarRangePercent;
    private int _indicatorDirection = 1;
    
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
        if (Input.GetKeyDown(KeyCode.Space))
        {
            RandomPosition();
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log(IsPerfect());
        }
        
        if (Indicator.rectTransform.localPosition.y >= _barHigh/2 && _indicatorDirection != -1)
            _indicatorDirection = -1;
        else if (Indicator.rectTransform.localPosition.y <= -_barHigh/2 && _indicatorDirection != 1)
            _indicatorDirection = 1;
        
        Indicator.rectTransform.localPosition = _indicatorDirection == 1? Vector3.MoveTowards(Indicator.rectTransform.localPosition, new Vector3(0,_barHigh/2,0), 1f)
        : Vector3.MoveTowards(Indicator.rectTransform.localPosition, new Vector3(0,-_barHigh/2,0), 1f);
    }

    public void RandomPosition()
    {
        float randomY = Random.Range(_perfectBarAvailableRange.x, _perfectBarAvailableRange.y);
        perfectBar.rectTransform.localPosition = new Vector3(0, randomY, 0);
    }
    
    public bool IsPerfect()
    {
        float yPosIndicator = Indicator.rectTransform.localPosition.y;
        return yPosIndicator >= perfectBar.rectTransform.localPosition.y - _perfectBarHigh/2 && yPosIndicator <= perfectBar.rectTransform.localPosition.y + _perfectBarHigh/2;
    }
    

}
