using UnityEngine;
using Unity.Netcode;

public class TimedObjectDestructor : NetworkBehaviour
{
    public float duration = 2f;
    public bool needsInitialization = true;
    public NetworkVariable<float> timer = new NetworkVariable<float>();
    public NetworkVariable<bool> initialized = new NetworkVariable<bool>(false);

    public GameObject effect;

    void Start()
    {
        if (!IsOwner) return;

        if (!needsInitialization)
        {
            timer.Value = duration;
        }
    }

    public void Initialize(WeaponStats newStats)
    {
        if (!IsServer) return;
        timer.Value = newStats.timeUntilDestruction;
        initialized.Value = true;
    }
    void Update()
    {
        if (!IsServer) return;

        if ((initialized.Value && needsInitialization) || (!initialized.Value && !needsInitialization))
        {
            if (timer.Value > 0)
            {
                timer.Value -= Time.deltaTime;
            }
            if (timer.Value <= 0)
            {
                RequestProjectileDestructionRpc();
            }
        }
    }
    public void EarlyDestruction()
    {
        if (!IsServer) return;
        RequestProjectileDestructionRpc();
    }
    [Rpc(SendTo.Server)]
    public void RequestProjectileDestructionRpc()
    {
        if (!IsServer) return;

        if(effect != null)
        {
            GameObject clone = Instantiate(effect, transform.position, transform.rotation);
            clone.GetComponent<NetworkObject>().Spawn();
        }
        GetComponent<NetworkObject>().Despawn();

    }
}
