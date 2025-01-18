using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BulletScript : NetworkBehaviour
{
    [SerializeField] private float speed = 10f;

    private readonly NetworkVariable<Vector2> moveDirection = new NetworkVariable<Vector2>();

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            moveDirection.Value = moveDirection.Value.normalized;
        }
    }

    private void Update()
    {
        if (IsServer)
        {
            transform.position += (Vector3)(moveDirection.Value * speed * Time.deltaTime);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetDirectionServerRpc(Vector2 direction)
    {
        moveDirection.Value = direction.normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (!IsServer) return;

        if (col.gameObject.CompareTag("Player"))
        {
            int playerHitNum = col.gameObject.GetComponent<PlayerScript>().playerNum.Value;

            UpdateScoreServerRpc(playerHitNum);

            MovePlayersToStartServerRpc();
        }

        GetComponent<NetworkObject>().Despawn();
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateScoreServerRpc(int playerHitNum)
    {
        if (playerHitNum == 1)
        {
            NetworkManagerUI.Singleton.p2Points.Value++;
        }
        else
        {
            NetworkManagerUI.Singleton.p1Points.Value++;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void MovePlayersToStartServerRpc()
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject != null)
            {
                client.PlayerObject.GetComponent<PlayerScript>().TriggerMoveToStartServerRpc();
            }
        }
    }

}
