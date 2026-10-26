using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

public class ProjectileDirectionIndicator : NetworkBehaviour
{
    public GameObject indicatorSprite;
    public Vector2 indicatorScale = Vector2.one;
    [SerializeField] public List<ProjectileIndicator> indicators = new List<ProjectileIndicator>();
    public List<ProjectileIndicator> toBeRemoved = new List<ProjectileIndicator>();
    void Start()
    {
        if (!IsOwner) return;

    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;

        CheckForProjectiles();

        for (int i = indicators.Count - 1; i >= 0; i--)
        {
            ProjectileIndicator indicator = indicators[i];
            if (indicator.target == null)
            {
                Destroy(indicator.indicatorInstance);
                toBeRemoved.Add(indicator);
            }
            else
            {
                indicator.FaceTarget();
            }

        }
        foreach (ProjectileIndicator indicator in toBeRemoved)
        {
            indicators.Remove(indicator);
        }
        toBeRemoved.Clear();

    }
    void CheckForProjectiles()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 10f);

        if(colliders.Length > 1)
        {
            ClientData currentClientData = GetComponent<ShipManager>().FetchUserClientData();
            foreach(Collider2D coll in colliders)
            {
                DamageDealer damageDealer = coll.GetComponent<DamageDealer>();
                if(damageDealer != null)
                {
                    ClientData collClientData = damageDealer.GetOriginClientData();
                    if (collClientData != null && (collClientData.clientTeam.Value != currentClientData.clientTeam.Value || !damageDealer.GetOriginShip().IsPlayer()) && 
                        !IsTargetIndicated(coll.transform))
                    {
                        GameObject clone = Instantiate(indicatorSprite, transform.position, transform.rotation, transform);
                        clone.transform.localScale = indicatorScale;
                        ProjectileIndicator indicator = new ProjectileIndicator(this, clone, coll.transform);
                        indicators.Add(indicator);

                    }
                }
            }
        }
    }

    bool IsTargetIndicated(Transform target)
    {
        for(int i = 0; i < indicators.Count; i++)
        {
            if (indicators[i].target == target)
                return true;
        }
        return false;
    }
    private void OnDisable()
    {
        for (int i = indicators.Count - 1; i >= 0; i--)
        {
            ProjectileIndicator indicator = indicators[i];
            Destroy(indicator.indicatorInstance);
            toBeRemoved.Add(indicator);

        }
        foreach (ProjectileIndicator indicator in toBeRemoved)
        {
            indicators.Remove(indicator);
        }
        toBeRemoved.Clear();
    }
}
[System.Serializable]
public class ProjectileIndicator
{
    public ProjectileDirectionIndicator manager;
    public GameObject indicatorInstance;
    public Transform target;

    public ProjectileIndicator(ProjectileDirectionIndicator manager1, GameObject indicatorInstance1, Transform target1)
    {
        manager = manager1;
        indicatorInstance = indicatorInstance1;
        target = target1;
    }
    public void FaceTarget()
    {
        if(indicatorInstance != null || target != null)
        {
            indicatorInstance.transform.up = (target.position - indicatorInstance.transform.position).normalized;
        }
    }
    public bool IsTarget(Transform t)
    {
        if (target == t)
            return true;
        return false;
    }
    public override string ToString()
    {
        return target.name;
    }
}