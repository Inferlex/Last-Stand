using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class MessageHandler : CustomUnnamedMessageHandler<string>
{
    protected override byte MessageType() //type of message, 1 is string
    {
        return 1;
    }

    protected override void OnReceivedUnnamedMessage(ulong clientId, FastBufferReader reader)
    {
        var stringMessage = string.Empty;
        reader.ReadValueSafe(out stringMessage);
        if (IsServer)
        {
            if (stringMessage.ToCharArray()[0] == 'c')
            {
                GetComponent<TickSystem>().ConfirmMessages(Int32.Parse(stringMessage.Remove(stringMessage.Length - 1)[1..]), Int32.Parse(stringMessage.Last().ToString()));
                return;
            }
            else if(stringMessage.ToCharArray()[0] != 's' && stringMessage.ToCharArray()[0] != 'x')
            {
                SendUnnamedMessage(stringMessage, clientId);
            }

            if (stringMessage.ToCharArray()[0] == 'r') //if we get more messages at once, we can assure the first message the user will ever make is to register
            {
                GetComponent<ConfirmMessageHandler>().IncrementPlayers();
                GetComponent<TickSystem>().IncrementPlayers();
            }
            else if (stringMessage.ToCharArray()[0] == 'f')
            {
                //GetComponent<TickSystem>().FixMessages(Int32.Parse(stringMessage[1].ToString()));
                
            }
            else if (stringMessage.ToCharArray()[0] == 'e')
            {
                GetComponent<TickSystem>().SendMessageToTickSystem(null, Int32.Parse(stringMessage[1].ToString()));
            }
            else if (stringMessage.ToCharArray()[0] == 'p')
            {
                //<TickSystem>().ConfirmMessages(Int32.Parse(stringMessage.Remove(stringMessage.Length - 1)[1..]), Int32.Parse(stringMessage.Last().ToString()));
                Debug.Log("got proceed");
            }
            else if (stringMessage.ToCharArray()[0] == 's')
            {
                if(clientId != 0)
                {
                    GetComponent<TickSystem>().Synchronize(stringMessage);
                    SendUnnamedMessage(stringMessage, clientId);
                }
            }
            else if(stringMessage.ToCharArray()[0] != 'x')
            {
                //Debug.Log(GetComponent<TickSystem>().GetPipe(Int32.Parse(stringMessage[0].ToString())).GetOngoingCalled());
                GetComponent<TickSystem>().SendMessageToTickSystem(stringMessage[2..], Int32.Parse(stringMessage[0].ToString()));
            }
        }
        else
        {
            if (stringMessage.ToCharArray()[0] == 'f')
            {
                //GetComponent<TickSystem>().FixMessages(Int32.Parse(stringMessage[1].ToString()));
                GetComponent<TickSystem>().FixConfirmClient(Int32.Parse(stringMessage[1].ToString()));
            }
            else if (stringMessage.ToCharArray()[0] == 'e')
            {
                //GetComponent<TickSystem>().ConfirmMessages(Int32.Parse(stringMessage[1].ToString()));
                GetComponent<TickSystem>().SendMessageToTickSystem(null, Int32.Parse(stringMessage[1].ToString()));
            }
            else if (stringMessage.ToCharArray()[0] == 'r')
            {
                GetComponent<TickSystem>().IncrementPlayers();
            }
            else if (stringMessage.ToCharArray()[0] == 'c')
            {
                //GetComponent<TickSystem>().ConfirmMessages(Int32.Parse(stringMessage.Remove(stringMessage.Length - 1)[1..]), Int32.Parse(stringMessage.Last().ToString()));
                GetComponent<TickSystem>().FixMessages(Int32.Parse(stringMessage.Last().ToString()));
            }
            else if (stringMessage.ToCharArray()[0] == 'p')
            {
                GetComponent<TickSystem>().Proceed(Int32.Parse(stringMessage.ToCharArray()[1].ToString()));
            }
            else if (stringMessage.ToCharArray()[0] == 's')
            {
                string s = GetComponent<TickSystem>().GetSynchronization(true);
                GetComponent<TickSystem>().Synchronize(stringMessage);
                SendUnnamedMessage(s, OwnerClientId);
            }
            else if(stringMessage.ToCharArray()[0] == 'x')
            {
                Debug.Log("get synchronize");
                string s = GetComponent<TickSystem>().GetSynchronization(true);
                GetComponent<TickSystem>().Synchronize(stringMessage);
            }
            else
            {
                //Debug.Log(stringMessage);
                //Debug.Log(GetComponent<TickSystem>().GetPipe(Int32.Parse(stringMessage[0].ToString())).GetPipeCalled());
                GetComponent<TickSystem>().SendMessageToTickSystem(stringMessage[2..], Int32.Parse(stringMessage[0].ToString()));
            }
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

    public void SendUnnamedMessage(string dataToSend, ulong senderId)
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
                foreach(var cId in NetworkManager.ConnectedClientsIds)
                {
                    if(cId != NetworkManager.ServerClientId && cId != senderId)
                    {
                        customMessagingManager.SendUnnamedMessage(cId, writer);
                    }
                }
            }
            else
            {
                // This method can be used by a client or server (client to server or server to client)
                customMessagingManager.SendUnnamedMessage(NetworkManager.ServerClientId, writer);
            }
        }
    }
}