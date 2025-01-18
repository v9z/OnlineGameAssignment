using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NetworkManagerUI : NetworkBehaviour
{

    [SerializeField] Button hostButton;
    [SerializeField] Button clientButton;

    [SerializeField] GameObject menu;
    [SerializeField] GameObject gameUI;

    [SerializeField] TMP_InputField usernameInput;
    private static string playerUsername;

    [SerializeField] TMP_Text scoreText;
    public NetworkVariable<int> p1Points = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone);
    public NetworkVariable<int> p2Points = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone);

    public static NetworkManagerUI Singleton { get; private set; }

    void Awake()
    {
        if (Singleton == null)
        {
            Singleton = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        menu.SetActive(true); 
        gameUI.SetActive(false);

        playerUsername = $"Player{Random.Range(1,9)}{Random.Range(1,9)}";

        hostButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
            OnStart();
        });

        clientButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
            OnStart();
        });
    }

    // Update is called once per frame
    void Update()
    {
        scoreText.text = $"{p1Points.Value} | {p2Points.Value}";
    }

    void OnStart()
    {
        setUsername();
        menu.SetActive(false);
        gameUI.SetActive(true);
    }

    public void setUsername()
    {
        if(!string.IsNullOrWhiteSpace(usernameInput.text))
        {
            playerUsername = usernameInput.text;
        }
    }

    public static string getUsername()
    {
        return playerUsername;
    }

    public void ToggleFullscreen()
    {
        Screen.fullScreen = !Screen.fullScreen;
    }
}
