using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking;

public class ChatSystem : MonoBehaviour {

    public List<string> chatHistory = new List<string>();

    private string currentMessage = string.Empty;

    private void onGUI()
    {
        //if(!NetworkManager.IsClientConnected())
        GUILayout.BeginHorizontal(GUILayout.Width(250));
        currentMessage = GUILayout.TextField(currentMessage);
        if (GUILayout.Button("Send"))
        {
            if (!string.IsNullOrEmpty(currentMessage.Trim()))
            {
                GetComponent<NetworkView>().RPC("ChatMessage", RPCMode.AllBuffered, new object[] { currentMessage });
                currentMessage = string.Empty;
            }
        }
        GUILayout.EndHorizontal();

        foreach (string chat in chatHistory)
            GUILayout.Label(chat);
    }

    [RPC]
    public void ChatMessage(string message)
    {
        chatHistory.Add(message);
    }
}
