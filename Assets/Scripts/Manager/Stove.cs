using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Stove : MonoBehaviour
{
    [SerializeField] private Transform[] stoveSlots;
    private void Awake()
    {
        GetComponent<Image>().alphaHitTestMinimumThreshold = 0.1f;
    }

    private void Update()
    {
        if (stoveSlots.All(slot => slot.childCount > 0)) return;
        Meat[] meats = stoveSlots.Where(slot => slot.childCount > 0)
            .Select(slot => slot.GetChild(0).GetComponent<Meat>()).ToArray();
        foreach (Meat meat in meats)
        {
            if (meat.IsBurnt) continue;
            meat.OnGrill();
        }
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
            meat.transform.SetParent(slot);
            meat.transform.localPosition = Vector3.zero;
            break;
        }
    }

    public void Grill(Meat meat)
    {
        if (meat.IsBurnt) return;
        meat.OnGrill();
    }
}
