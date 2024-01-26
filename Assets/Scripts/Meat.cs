using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Meat : MonoBehaviour, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
{
  private Vector3 _spawnPosition;

  private void Start()
  {
    _spawnPosition = transform.position;
  }

  public void OnPointerDown(PointerEventData eventData)
  {
    GetComponent<Image>().raycastTarget = false;
    transform.position = eventData.position;
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
    GetComponent<Image>().raycastTarget = true;
    if (!eventData.pointerEnter || !eventData.pointerEnter.CompareTag("Stofe"))
    {
      transform.position = _spawnPosition;
      return;
    }
  }
}
