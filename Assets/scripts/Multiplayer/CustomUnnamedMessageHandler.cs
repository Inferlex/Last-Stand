using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CustomUnnamedMessageHandler<T> : NetworkBehaviour
{
    /// <summary>
    /// Since there is no unique way to identify unnamed messages,
    /// adding a message type identifier to the message itself is
    /// one way to handle know:
    /// "what kind of unnamed message was received?"
    /// </summary>
    protected virtual byte MessageType()
    {
        // The default unnamed message type
        return 0;
    }

    /// <summary>
    /// For most cases, you want to register once your NetworkBehaviour's
    /// NetworkObject (typically in-scene placed) is spawned.
    /// </summary>
    public override void OnNetworkSpawn()
    {
        // Both the server-host and client(s) will always subscribe to the
        // the unnamed message received event
        NetworkManager.CustomMessagingManager.OnUnnamedMessage += ReceiveMessage;
    }

    public override void OnNetworkDespawn()
    {
        // Unsubscribe when the associated NetworkObject is despawned.
        NetworkManager.CustomMessagingManager.OnUnnamedMessage -= ReceiveMessage;
    }

    /// <summary>
    /// This method needs to be overridden to handle reading a unique message type
    /// (that is, derived class)
    /// </summary>
    protected virtual void OnReceivedUnnamedMessage(ulong clientId, FastBufferReader reader)
    {
    }

    /// <summary>
    /// For this unnamed message example, we always read the message type
    /// value to determine if it should be handled by this instance in the
    ///  event it's a child of the CustomUnnamedMessageHandler class.
    /// </summary>
    private void ReceiveMessage(ulong clientId, FastBufferReader reader)
    {
        var messageType = (byte)0;
        // Read the message type value that is written first when we send
        // this unnamed message.
        reader.ReadValueSafe(out messageType);
        // Example purposes only, you might handle this in a more optimal way
        if (messageType == MessageType())
        {
            OnReceivedUnnamedMessage(clientId, reader);
        }
    }

    /// <summary>
    /// For simplicity, the default does nothing
    /// </summary>
    /// <param name="dataToSend"></param>
    public virtual void SendUnnamedMessage(T dataToSend)
    {

    }
}