using System;
using System.Linq;
using DG.Tweening;
using Managers;
using UnityEngine;
using UnityEngine.UI;

public class Stove : MonoBehaviour
{
    [SerializeField] private Transform[] stoveSlots;
    public int slotCount => stoveSlots.Length;
    public int AvailableSlot => stoveSlots.Count(point => point.childCount <= 0);
    [SerializeField] private AudioClip[] cookingSound;
    [SerializeField] private AudioClip fireSound;
    private AudioSource _cookingSource;
    
    private void Awake()
    {
        GetComponent<Image>().alphaHitTestMinimumThreshold = 0.1f;
    }

    private void Start()
    {
        SoundManager.Instance.PlayFx(fireSound,out AudioSource fireSource,true);
        fireSource.volume = 0.25f;
    }

    private void Update()
    {
        if (GameManager.Instance.IsWin || GameManager.Instance.IsLose) return;
        Meat[] meats = stoveSlots.Where(slot => slot.childCount > 0)
            .Select(slot => slot.GetChild(0).GetComponent<Meat>()).ToArray();
        foreach (Meat meat in meats)
        {
            if (meat.IsBurnt) continue;
            meat.OnGrill();
        }

        if (stoveSlots.All(slot => slot.childCount <= 0))
        {
            if (_cookingSource)
            {
                _cookingSource.DOFade(0f, 0.5f).OnComplete(() => { _cookingSource.Stop(); });
            }
            return;
        }

        if (_cookingSource && _cookingSource.isPlaying) return;
        SoundManager.Instance.PlayFx(cookingSound[UnityEngine.Random.Range(0, cookingSound.Length)], out _cookingSource);
        _cookingSource.volume = 0.5f;
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
        float distance(Transform slot) => Vector3.Distance(meat.transform.position, slot.position);
        stoveSlots = stoveSlots.OrderBy(distance).ToArray();
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
