using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Events/PlayerSpawnedChannel")]
public class EventChannel : ScriptableObject
{
    public UnityAction<GameObject> OnPlayerSpawned;

    public void SpawnedPlayer(GameObject player)
    {
        OnPlayerSpawned?.Invoke(player);
    }
}
