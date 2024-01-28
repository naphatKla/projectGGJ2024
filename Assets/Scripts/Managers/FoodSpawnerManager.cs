using System.Collections;
using System.Linq;
using Plugins.Singleton;
using Sirenix.Utilities;
using UnityEngine;

public class FoodSpawnerManager : MonoSingleton<FoodSpawnerManager>
{
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private Meat[] foods;
    public int maxSpawn => spawnPoints.Length;
    public int AvailableSlot => spawnPoints.Count(point => point.childCount <= 0);
    
    void Start()
    {
        StartCoroutine(LoopSpawnWithDelay(spawnPoints.Length, 0.25f));
    }
    
    public void SpawnFood()
    {
        Transform point = spawnPoints.FirstOrDefault(point => point.childCount <= 0);
        Instantiate(foods[Random.Range(0, foods.Length)], point.position, Quaternion.identity, point);
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

    private IEnumerator LoopSpawnWithDelay(int loops, float delay)
    {
        for (int i = 0; i < loops; i++)
        {
            SpawnFood();
            yield return new WaitForSeconds(delay);
        }
    }
}