using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class PlayerScript : NetworkBehaviour
{
    public float speed = 1;
    public float jumpForce = 10;

    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckDistance = 0.1f;

    private bool isGrounded;

    public NetworkVariable<int> playerNum = new NetworkVariable<int>();

    [SerializeField] GameObject bulletPrefab;

    float timer = 1;

    public Vector2 startPos;
    private bool shouldMoveToStart = false;

    private NetworkVariable<bool> rbSimulated = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    Rigidbody2D rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (IsServer)
        {
            AssignPlayerNumberServerRpc();
            startPos = new Vector2(-7.5f, 3.3f);
            transform.position = startPos;
        }
        else
        {
            startPos = new Vector2(7.5f, -4.35f);
            transform.position = startPos;
        }

        rbSimulated.OnValueChanged += (oldValue, newValue) =>
        {
            rb.simulated = newValue;
        };
    }

    // Update is called once per frame
    void Update()
    {
        if (shouldMoveToStart)
        {
            transform.position = Vector2.MoveTowards(transform.position, startPos, 5f * Time.deltaTime);

            if (Vector2.Distance(transform.position, startPos) < 0.01f)
            {
                shouldMoveToStart = false;
                if (IsServer) rbSimulated.Value = true;
            }
        }

        if (!IsOwner) return;

        isGrounded = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, groundLayer);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            Jump();
        }

        if (Input.GetMouseButtonDown(0) && timer >= 1) //Shooting logic
        {
            Vector2 spawnPosition = GetBulletSpawnPosition();
            Vector2 direction = (spawnPosition - (Vector2)transform.position).normalized;

            SpawnBulletServerRpc(spawnPosition, direction);
            timer = 0;
        }

        if(timer < 1) //Shooting cooldown timer
        {
            timer += Time.deltaTime;
        }

        if(IsClient)
        {
            if (!rb.simulated && Vector2.Distance(transform.position, startPos) < 0.01f) //The client players rigidbody doesnt get simulated again so this was the simplest fix for that i could come up with
            {
                RequestEnableSimulationServerRpc();
                shouldMoveToStart = false;
            }
        }
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        float hori = Input.GetAxisRaw("Horizontal");

        rb.linearVelocity = new Vector2(hori * speed * Time.deltaTime, rb.linearVelocityY);
    }

    void Jump()
    {
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    [ServerRpc(RequireOwnership = false)]
    void RequestEnableSimulationServerRpc()
    {
        rbSimulated.Value = true;
        rb.simulated = true;
    }


    private Vector2 GetBulletSpawnPosition()
    {
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = ((Vector2)mouseWorldPosition - (Vector2)transform.position).normalized;

        return (Vector2)transform.position + direction * 0.4f;
    }

    [ServerRpc]
    private void SpawnBulletServerRpc(Vector2 position, Vector2 direction)
    {
        GameObject spawnedBullet = Instantiate(bulletPrefab, position, Quaternion.identity);
        NetworkObject networkObject = spawnedBullet.GetComponent<NetworkObject>();

        networkObject.Spawn(true);

        spawnedBullet.GetComponent<BulletScript>().SetDirectionServerRpc(direction);
    }

    [ServerRpc(RequireOwnership = false)]
    private void AssignPlayerNumberServerRpc() //Assings the players a number for scoring
    {
        if (NetworkManager.Singleton.ConnectedClientsList.Count == 1)
        {
            playerNum.Value = 1;
        }
        else if (NetworkManager.Singleton.ConnectedClientsList.Count == 2)
        {
            playerNum.Value = 2;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void TriggerMoveToStartServerRpc()
    {
        rbSimulated.Value = false;

        MovePlayersToStartClientRpc(false);
    }

    [ClientRpc]
    private void MovePlayersToStartClientRpc(bool simulateRb)
    {
        shouldMoveToStart = true;
        rb.simulated = simulateRb;
    }

}
