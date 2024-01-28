using DG.Tweening;
using Managers;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class DropOffZone : MonoBehaviour, IDropHandler
{
    [SerializeField] private MMF_Player correctFeedback;
    [SerializeField] private MMF_Player rawFeedback;
    [SerializeField] private MMF_Player mediumRareFeedback;
    
    public void OnDrop(PointerEventData eventData)
    {
        if (!eventData.pointerDrag) return;
        if (!eventData.pointerDrag.CompareTag("Grillable")) return;
        
        Meat meat = eventData.pointerDrag.GetComponent<Meat>();
        CheckMeatHandler(meat);
    }

    public void CheckMeatHandler(Meat meat)
    {
        Animator animator = meat.GetComponent<Animator>();
        if (animator.GetBool("IsCurrentSideCooked") && animator.GetBool("IsOppositeSideCooked"))
        {
            Debug.Log("Welllll done");
            meat.DOKill(true);
            Destroy(meat.gameObject);
            DOVirtual.DelayedCall(0.5f, () => { FoodSpawnerManager.Instance.SpawnFood();});
            correctFeedback.PlayFeedbacks();
            GameManager.Instance.AddMeatCooked();
            return;
        }

        if ((animator.GetBool("IsCurrentSideCooked") && !animator.GetBool("IsOppositeSideCooked")) ||
            (!animator.GetBool("IsCurrentSideCooked") && animator.GetBool("IsOppositeSideCooked")))
        {
            Debug.Log("We can't eat the meat with one side cooked");
            mediumRareFeedback.PlayFeedbacks();
            return;
        }
        
        if (!animator.GetBool("IsCurrentSideCooked") && !animator.GetBool("IsOppositeSideCooked"))
        {
            Debug.Log("What are you doing? The meat is raw");
            rawFeedback.PlayFeedbacks();
            return;
        }
    }
}
