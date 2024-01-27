using System.Linq;
using Plugins.Singleton;
using Sirenix.Utilities;
using UnityEngine;

public class FoodSpawnerManager : MonoSingleton<FoodSpawnerManager>
{
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private Meat[] foods;
    public int maxSpawn => spawnPoints.Length;
    
    void Start()
    {
        SpawnFood();
    }
    
    public void SpawnFood()
    {
        spawnPoints.Where(point => point.childCount <= 0).ForEach(point =>
        {
            Instantiate(foods[Random.Range(0, foods.Length)], point.position, Quaternion.identity, point);
        });
    }
    
    public void AddFoodToTheNearestSlot(Meat meat)
    {
        float distance(Transform slot) => Vector3.Distance(meat.transform.position, slot.position);
        spawnPoints = spawnPoints.OrderBy(distance).ToArray();
        foreach (Transform slot in spawnPoints)
        {
            if (slot.childCount > 0) continue;
            meat.transform.SetParent(slot);
            meat.transform.localPosition = Vector3.zero;
            break;
        }
    }
}
