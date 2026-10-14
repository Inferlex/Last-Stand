using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class ConfirmMessageHandler : CustomUnnamedMessageHandler<string>
{
    private int numberOfPlayers = 0;
    private List<int> messageCountForEachPlayer = new List<int>();
    private List<bool> gotMessage = new List<bool>();
    private float delay = 0;

    protected override byte MessageType() //type of message, 1 is string
    {
        return 2;
    }

    protected override void OnReceivedUnnamedMessage(ulong clientId, FastBufferReader reader)
    {
        var stringMessage = string.Empty;
        reader.ReadValueSafe(out stringMessage);
        Debug.Log("confirm " + stringMessage);
        if (IsServer)
        {
            char msg = stringMessage.Last();
            if ((int)clientId > numberOfPlayers - 1) //is player registered?
            {
                return;
            }

            if (gotMessage[(int)clientId] == false)
            {
                messageCountForEachPlayer[(int)clientId] = Int32.Parse(stringMessage.Remove(stringMessage.Length - 1).ToString());
            }

            if((int)clientId == 0)
            {
                delay = Time.fixedTime;
            }

            gotMessage[(int)clientId] = true;

            if(messageCountForEachPlayer[(int)clientId] < Int32.Parse(stringMessage.Remove(stringMessage.Length - 1).ToString())) //is the number in stringmessage bigget than the messagecount?
            {
                messageCountForEachPlayer[(int)clientId] = Int32.Parse(stringMessage.Remove(stringMessage.Length - 1).ToString());
            }

            if (!gotMessage.Contains(false) && CheckMessageCount(stringMessage.Last())) //do all players have the same message count and got confirmation?
            {
                //Debug.Log("proceed");
                GetComponent<MessageHandler>().SendUnnamedMessage($"p{stringMessage.Last()}/" + Math.Round((Time.fixedTime - delay), 3)); //proceed
                messageCountForEachPlayer.ForEach(p => { p = 0; });
                delay = Time.fixedTime;
                VoidGotMessage();
            }
        }
        else
        {
            //Debug.Log(clientId + " " + stringMessage);
        }
    }

    public override void SendUnnamedMessage(string dataToSend)
    {
        var writer = new FastBufferWriter(1100, Allocator.Temp);
        var customMessagingManager = NetworkManager.CustomMessagingManager;
        // Tip: Placing the writer within a using scope assures it will
        // be disposed upon leaving the using scope
        using (writer)
        {
            // Write our message type
            writer.WriteValueSafe(MessageType());

            // Write our string message
            writer.WriteValueSafe(dataToSend);
            if (IsServer)
            {
                // This is a server-only method that will broadcast the unnamed message.
                // Caution: Invoking this method on a client will throw an exception!
                customMessagingManager.SendUnnamedMessageToAll(writer);
            }
            else
            {
                // This method can be used by a client or server (client to server or server to client)
                customMessagingManager.SendUnnamedMessage(NetworkManager.ServerClientId, writer);
            }
        }
    }

    public void IncrementPlayers()
    {
        numberOfPlayers++;
        messageCountForEachPlayer.Add(0);
        gotMessage.Add(false);
        //Debug.Log(numberOfPlayers);
    }

    private bool CheckMessageCount(char c)
    {
        if(numberOfPlayers == 1) { return true; } //for testing purposes

        for(int i = 1; i < messageCountForEachPlayer.Count; i++)
        {
            //Debug.Log("message count index " + (i-1) + " " + messageCountForEachPlayer[i-1]);
            //Debug.Log("message count index " + i + " " + messageCountForEachPlayer[i]);
            if (messageCountForEachPlayer[i] != messageCountForEachPlayer[i - 1])
            {
                //Debug.Log("someone fucked up");
                VoidGotMessage();
                GetComponent<MessageHandler>().SendUnnamedMessage($"f{c}");
                return false;
            }
        }     
        return true;
    }

    public void SetDelay(float t)
    {
        delay = t;
    }

    private void VoidGotMessage()
    {
        for(int i = 0; i < gotMessage.Count; i++)
        {
            gotMessage[i] = false;
        }
    }
}