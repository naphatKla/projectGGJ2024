using UnityEngine;

public class Plate : MonoBehaviour
{
    [SerializeField] private Transform[] plateSlots;
    
    public void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        foreach(Transform slot in plateSlots) 
            Gizmos.DrawSphere(slot.position, 5f);
    }
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
