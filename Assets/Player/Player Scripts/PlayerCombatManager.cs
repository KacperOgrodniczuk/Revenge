using UnityEngine;

public class PlayerCombatManager : MonoBehaviour
{
    public PlayerManager PlayerManager { get; private set; }

    private void Awake()
    {
        PlayerManager = GetComponent<PlayerManager>();

    }
}
