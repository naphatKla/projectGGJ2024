using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Stove : MonoBehaviour
{
    [SerializeField] private Transform[] stoveSlots;
    private List<Meat> meatsInStove = new List<Meat>();
    private void Awake()
    {
        GetComponent<Image>().alphaHitTestMinimumThreshold = 0.1f;
    }

    private void Update()
    {
        if (meatsInStove.Count <= 0) return;
        meatsInStove.ForEach(meat => Grill(meat));
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        foreach(Transform slot in stoveSlots) 
            Gizmos.DrawSphere(slot.position, 5f);
    }

    public void DropFood(Meat meat)
    {
        if (!meat.CompareTag("Grillable")) return;
        foreach (Transform slot in stoveSlots)
        {
            if (slot.childCount > 0) continue;
            meatsInStove.Add(meat);
            meat.transform.SetParent(slot);
            meat.transform.localPosition = Vector3.zero;
            break;
        }
    }

    public void Grill(Meat meat)
    {
        Debug.Log(meat.FoodState);
        if (meat.IsBurnt) return;
        meat.OnGrill();
    }
}
