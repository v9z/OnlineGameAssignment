using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ChatSystem : NetworkBehaviour
{
    [SerializeField] TMP_InputField chatInput;
    [SerializeField] TMP_Text chatDisplay;

    private static ChatSystem instance;

    private void Start()
    {
        instance = this;

        chatInput.onEndEdit.AddListener(OnEndEdit);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        chatInput.onEndEdit.RemoveListener(OnEndEdit);
    }

    private void OnEndEdit(string text)
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            SendMessageToServer();
            chatInput.ActivateInputField();
        }
    }

    public void SendMessageToServer()
    {
        if (string.IsNullOrWhiteSpace(chatInput.text)) return;

        string message = $"[{NetworkManagerUI.getUsername()}]: {chatInput.text}";
        SendChatMessageServerRpc(message);
        chatInput.text = "";
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendChatMessageServerRpc(string message)
    {
        BroadcastMessageClientRpc(message);
    }

    [ClientRpc]
    private void BroadcastMessageClientRpc(string message)
    {
        instance.chatDisplay.text += message + "\n";
    }
}
