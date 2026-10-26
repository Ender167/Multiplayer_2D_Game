using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

public class EntitySpawner : NetworkBehaviour
{
    [SerializeField] public List<EnemyItem> enemies = new List<EnemyItem>();

    public void SpawnEnemies()
    {
        foreach(EnemyItem item in enemies)
        {
            GameObject clone = Instantiate(item.enemy, item.pos, Quaternion.identity);
            clone.GetComponent<NetworkObject>().Spawn();
        }
    }

}
[System.Serializable]
public class EnemyItem
{
    public GameObject enemy;
    public Vector2 pos;
}