using Assets.Src;
using Assets.Src.Units;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.UI.CanvasScaler;

public class TickSystem : NetworkBehaviour
{
    private List<OnlinePlayerManager> managers = new List<OnlinePlayerManager>();
    private MessageHandler messageHandler;
    private int numberOfPlayers;
    private int maxNumberOfPlayers = 2;
    private List<string> leftoverMessages = new List<string>(); //when pipes are full, we need to put messages somewhere
    private Queue<int> pipeQueue = new Queue<int>();

    private List<MessagePipe> pipes = new List<MessagePipe>();

    private float timeSinceCreation;

    private int solidSynchronizationTimer;

    //private StringBuilder sb = new StringBuilder();

    void Start()
    {
        messageHandler = GetComponent<MessageHandler>();
        GameEvents.current.OnManagerCreation += OnManagerCreation;
        GameEvents.current.OnOnlineGetMessage += GetMessage;
        pipes.Add(new MessagePipe());
        pipes.Add(new MessagePipe());
        pipes.Add(new MessagePipe());
        timeSinceCreation = Time.fixedTime;
        solidSynchronizationTimer = 15;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        NetworkManager.NetworkTickSystem.Tick += OnNetworkTick;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        NetworkManager.NetworkTickSystem.Tick -= OnNetworkTick;
    }

    void OnNetworkTick()
    {
        if (numberOfPlayers < maxNumberOfPlayers)
        {
            return;
        }

        foreach (var pipe in pipes)
        {
            int index = pipes.IndexOf(pipe);
            if (pipe.GetPhase() == MessagePipe.Phase.ready)
            {
                if (leftoverMessages.Count > 0)
                {
                    pipe.AddMessagesRange(leftoverMessages);
                    leftoverMessages.Clear();
                }
                pipeQueue.Enqueue(index);
                ChangeToOngoing(pipe, index);
                break;
            }

            if (index == pipeQueue.Peek() && pipe.GetPhase() == MessagePipe.Phase.proceed && !pipe.GetOngoing())
            {
                pipe.SetOngoing(true);
                if (IsServer)
                {                 
                    StartCoroutine(ExecuteMessagesWithDelay(index));
                }
                else
                {
                    ExecuteMessages(index);                   
                }
            }
            else if (index == pipeQueue.Peek() && pipe.GetPhase() == MessagePipe.Phase.clear)
            {
                ClearPipe(index);
            }
        }

        if (IsServer)
        {
            if (solidSynchronizationTimer > 0)
            {
                solidSynchronizationTimer--;
            }
            else if (solidSynchronizationTimer == 0)
            {
                //Debug.Log("synchronize");
                //WriteIntoFile();
                string s = GetSynchronization(true);
                messageHandler.SendUnnamedMessage(s);
                solidSynchronizationTimer--;
            }
        }
    }

    /*private void WriteIntoFile()
    {
        if (File.Exists("testLog.txt"))
        {
            var sr = File.AppendText("testLog.txt");
            sr.Write(sb.ToString());
            sb.Clear();
            sr.Close();
            return;
        }
        else
        {
            var sr = File.CreateText("testLog.txt");
            sr.Write(sb.ToString());
            sb.Clear();
            sr.Close();
        }
    }*/

    private void ChangeToOngoing(MessagePipe pipe, int index)
    {
        pipe.ChangePhase(MessagePipe.Phase.ongoing);
        if (pipe.GetMessages().Count == 0)
        {
            SendMessageToTickSystem(null, index);
            messageHandler.SendUnnamedMessage($"e{index}", OwnerClientId);
            //sb.Append("no msg to send\n");
        }
        else
        {
            string convertedMessage = ConvertListToString(pipe.GetMessages());
            SendMessageToTickSystem(convertedMessage, index);
            //Debug.Log(convertedMessage);
            messageHandler.SendUnnamedMessage($"{index}/" + convertedMessage, OwnerClientId);
            //sb.Append($"sending msg {convertedMessage}\n");
        }
    }

    private string ConvertListToString(List<string> messages)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < messages.Count; i++)
        {
            sb.Append(Simplify(messages[i]));
            if (i + 1 != messages.Count)
            {
                sb.Append(";");
            }
        }
        return sb.ToString();
    }

    public void SendMessageToTickSystem(string msg, int index)
    {
        if (pipes[index].GetOngoingCalled() >= numberOfPlayers)
        {
            Debug.Log("too much sending to tick or incorrect state ");
            return;
        }
        if(pipes[index].GetPhase() == MessagePipe.Phase.confirm)
        {
            Debug.Log("something went wrong with sending the message");
        }
        if (pipes[index].GetPhase() != MessagePipe.Phase.ongoing) //might cause longer reaction time
        {
            StartCoroutine(WaitTickSystem(msg, index));
            return;
        }
        //sb.Append($"got msg {msg}\n");
        pipes[index].IncrementOngoingCalled();
        List<string> pipe = pipes[index].GetOngoingMessages();

        if (msg != null)
        {
            string[] splitMessage = msg.Split(';');
            foreach (string m in splitMessage.Take(splitMessage.Length))
            {
                pipe.Add(m);
            }
        }

        if (pipes[index].GetOngoingCalled() == numberOfPlayers)
        {
            pipes[index].SetOngoingCalled(0);
            if (pipe.Count == 0)
            {
                pipes[index].ChangePhase(MessagePipe.Phase.clear);
            }
            else
            {
                pipes[index].ChangePhase(MessagePipe.Phase.confirm);

                if (IsServer)
                {
                    //Debug.Log("confirm " + pipe.Count + " " + pipes[index].GetOngoingMessages().Count);
                    ConfirmMessages(pipe.Count, index);
                }
                else
                {
                    //Debug.Log("confirm shit number of msgs " + pipe.Count + $"mess c{pipe.Count}{index}");
                    messageHandler.SendUnnamedMessage($"c{pipe.Count}{index}", OwnerClientId);
                }
            }
        }
    }

    private IEnumerator WaitTickSystem(string msg, int index)
    {
        yield return new WaitForSeconds(0.05f);
        SendMessageToTickSystem(msg, index);
    }

    public void ConfirmMessages(int number, int index, int tries = 0) //sometimes the msg count is 0, that is wrong
    {
        if (pipes[index].GetConfirmCalled() >= numberOfPlayers)
        {
            Debug.Log("too much sending to confirm");
            return;
        }

        if (pipes[index].GetPhase() == MessagePipe.Phase.ongoing)
        {
            Debug.Log("its ongoing " + pipes[index].GetOngoingMessages().Count);
            //sb.Append($"its ongoing, should be confirm \n");
            StartCoroutine(WaitConfirmMessages(number, index));
            return;
        }

        if (number != pipes[index].GetOngoingMessages().Count)
        {
            Debug.Log($"incorrect amount of messages {pipes[index].GetOngoingMessages().Count} while number is {number}, phase is {pipes[index].GetPhase()}");
            //sb.Append($"incorrect amount of messages, sending fix\n");
            pipes[index].SetConfirmCalled(0);
            FixConfirm(index);
            return;
        }
        //sb.Append($"got confirm \n");
        if (pipes[index].GetConfirmCalled() == 0)
        {
            pipes[index].SetDelay(Time.fixedTime - timeSinceCreation);
        }

        pipes[index].IncrementConfirmCalled();
        if (pipes[index].GetConfirmCalled() == numberOfPlayers)
        {
            pipes[index].SetConfirmCalled(0);
            pipes[index].SetDelay((Time.fixedTime - timeSinceCreation) - pipes[index].GetDelay());
            Proceed(index);
            messageHandler.SendUnnamedMessage($"p{index}", OwnerClientId);
        }
    }

    private void FixConfirm(int index)
    {
        pipes[index].ClearOngoingMessages();
        pipes[index].SetConfirmCalled(0);
        pipes[index].SetOngoingCalled(0);
        messageHandler.SendUnnamedMessage($"f{index}", OwnerClientId);
        ChangeToOngoing(pipes[index], index);       
        //FixMessages(pipes[index], index);
    }

    public void FixConfirmClient(int index)
    {
        pipes[index].ClearOngoingMessages();
        pipes[index].SetOngoingCalled(0);
        ChangeToOngoing(pipes[index], index);
        //FixMessages(pipes[index], index);
    }

    /*private void FixMessages(MessagePipe pipe, int index)
    {
        pipe.ChangePhase(MessagePipe.Phase.ongoing);
        Debug.Log("sending new messages to fix");
        if (pipe.GetMessages().Count == 0)
        {
            SendMessageToTickSystem(null, index);
            messageHandler.SendUnnamedMessage($"e{index}", OwnerClientId);
        }
        else
        {
            string convertedMessage = ConvertListToString(pipe.GetMessages());
            SendMessageToTickSystem(convertedMessage, index);
            messageHandler.SendUnnamedMessage($"{index}/" + convertedMessage, OwnerClientId);
        }
    }*/

    public void Proceed(int index)
    {
        pipes[index].ChangePhase(MessagePipe.Phase.proceed);
        //sb.Append($"proceeding \n");
    }

    private IEnumerator WaitConfirmMessages(int number, int index)
    {
        yield return new WaitForSeconds(0.05f);
        ConfirmMessages(number, index);

    }

    private void OnManagerCreation(OnlinePlayerManager manager)
    {
        managers.Add(manager);
        manager.SetTickSystem(this);
    }

    public void GetMessage(string msg)
    {
        AddMessageToPipe(msg);
    }

    public void CreateGroupAIMessage(List<OnlineUnit> units, Vector2 point, Vector2 avgVector)
    {
        if (units == null) return;
        int managerIndex = GetIdUsingUnit(units[0]);

        StringBuilder builder = new StringBuilder();
        builder.Append($"w/{point.ToString()}/{avgVector.ToString()}/");

        foreach(OnlineUnit unit in units)
        {
            if (units.IndexOf(unit) != 0)
            {
                builder.Append("|");
            }
            builder.Append(unit.GetListId().ToString());
        }

        builder.Append($"/{units[0].GetId()}");
        AddMessageToPipe(builder.ToString());
    }

    /*public void MoveMessage(OnlineUnit unit, Vector2 point)
    {
        if (!unit) return;
        int managerIndex = GetIdUsingUnit(unit);
        string message = "m/" + unit.GetListId().ToString() + "/" + point.ToString() + "/" + unit.GetId();
        AddMessageToPipe(message);
    }*/

    public void MoveMessage(string ids, OnlineUnit unit, Vector2 point)
    {
        if (!unit) return;
        int managerIndex = GetIdUsingUnit(unit);
        string message = "m/" + ids + "/" + point.ToString() + "/" + unit.GetId();
        AddMessageToPipe(message);
    }

    public void AttackMessage(List<OnlineUnit> units, OnlineTarget enemy)
    {
        if (units.Count == 0) return;
        int managerIndex = GetIdUsingUnit(units[0]);

        string typeOfEnemy = null;

        if (enemy.tag == "PlayerUnit")
        {
            typeOfEnemy = "u";
        }
        else
        {
            typeOfEnemy = "b";
        }

        foreach (OnlineUnit unit in units)
        {
            string message = "a/" + typeOfEnemy + "/" + unit.GetListId().ToString() + "/" + enemy.GetListId() + "/" + enemy.GetId() + "/" + unit.GetId();
            AddMessageToPipe(message);
        }
    }

    public void RepairMessage(List<OnlineUnit> units, OnlineBuilding building)
    {
        if (units.Count == 0) return;
        int managerIndex = GetIdUsingUnit(units[0]);

        /*foreach (OnlineUnit unit in units)
        {
            string message = "o/" + unit.GetListId().ToString() + "/" + building.GetListId() + "/" + unit.GetId();
            //ownerMessages.Add(message);
            AddMessageToPipe(message);
        }*/
        StringBuilder sb = new StringBuilder();
        foreach (OnlineUnit unit in units)
        {
            sb.Append(unit.GetListId());
            if (unit != units.Last())
            {
                sb.Append("-");
            }
        }

        string message = "o/" + sb.ToString() + "/" + building.GetListId() + "/" + units[0].GetId();
        AddMessageToPipe(message);
    }

    public void AttackMessageGroup(List<OnlineUnit> units, OnlineTarget enemy)
    {
        if (units.Count == 0) return;
        int managerIndex = GetIdUsingUnit(units[0]);

        string typeOfEnemy = null;

        if (enemy.tag == "PlayerUnit")
        {
            typeOfEnemy = "u";
        }
        else
        {
            typeOfEnemy = "b";
        }
        StringBuilder sb = new StringBuilder();
        foreach (OnlineUnit unit in units)
        {
            sb.Append(unit.GetListId());
            if (unit != units.Last())
            {
                sb.Append("-");
            }
        }

        string message = "a/" + typeOfEnemy + "/" + sb.ToString() + "/" + enemy.GetListId() + "/" + enemy.GetId() + "/" + units[0].GetId();
        AddMessageToPipe(message);
    }

    private int GetIdUsingUnit(OnlineUnit unit)
    {
        foreach (OnlinePlayerManager manager in managers)
        {
            if (manager.GetId() == unit.GetId())
            {
                return managers.IndexOf(manager);
            }
        }
        return default;
    }

    public string Simplify(string s)
    {
        return string.Concat(s.Split(' ', '(', ')'));
    }

    public void ExecuteMessages(int i)
    {
        pipeQueue.Dequeue();
        List<string> pipe = pipes[i].GetOngoingMessages();

        if (pipe.Count == 0)
        {
            Debug.Log("its empty ");
            //sb.Append("THE PIPE IS EMPTY");
        }

        foreach (string msg in pipe)
        {
            //Debug.Log("executing message " + msg);
            foreach (OnlinePlayerManager pm in managers)
            {
                pm.WorkOnMessage(msg);
            }
        }
        pipes[i].ClearOngoingMessages();
        pipes[i].ClearMessages();
        pipes[i].ChangePhase(MessagePipe.Phase.ready);
        pipes[i].SetOngoing(false);
    }

    private IEnumerator ExecuteMessagesWithDelay(int i)
    {
        yield return new WaitForSeconds(pipes[i].GetDelay());
        ExecuteMessages(i);

    }

    public void RegisterMessage(string str, int id)
    {
        IncrementPlayers();
        messageHandler.SendUnnamedMessage(str, (ulong)id);
    }

    public void FixMessages(int index)
    {
        messageHandler.SendUnnamedMessage($"c{pipes[index].GetOngoingMessages().Distinct().ToList().Count()}{index}", OwnerClientId);
    }

    /*public OnlineUnit GetUnitFromManager(int playerId, int unitListId)
    {
        if (managers[playerId].GetPlayerUnits().Count > unitListId)
        {
            return managers[playerId].GetPlayerUnits()[unitListId];
        }
        return null;
    }*/

    public OnlineUnit GetUnitFromManagerUsingListId(int playerId, int unitListId)
    {
        return managers[playerId].FindListId(unitListId, false) as OnlineUnit;
    }

    public OnlineBuilding GetBuildingFromManagerUsingListId(int playerId, int unitListId)
    {
        return managers[playerId].FindListId(unitListId, true) as OnlineBuilding;
    }


    /*public OnlineBuilding GetBuildingFromManager(int playerId, int unitListId)
    {
        return managers[playerId].GetPlayerBuildings()[unitListId];
    }*/

    /*public int GetManagerUnitsCount(int id)
    {
        return managers[id].GetPlayerUnits().Count;
    }*/

    public void AddMessageToPipe(string msg)
    {
        foreach (var pipe in pipes)
        {
            if (pipe.GetPhase() == MessagePipe.Phase.ready)
            {
                pipe.AddMessages(msg);
                DeleteOldMessages(pipe.GetMessages(), msg);
                return;
            }
        }
        leftoverMessages.Add(msg);
        DeleteOldMessages(leftoverMessages, msg);
    }

    public void IncrementPlayers()
    {
        numberOfPlayers++;
    }

    private void ClearPipe(int index)
    {
        pipeQueue.Dequeue();
        pipes[index].ClearMessages();
        pipes[index].ChangePhase(MessagePipe.Phase.ready);
    }

    public MessagePipe GetPipe(int index)
    {
        return pipes[index];
    }

    public void DeleteOldMessages(List<string> stringList, string msg) //some messages are not needed (in case we have more move messages on a same unit, we only need the most recent one)
    {
        if (stringList.Count > 1)
        {
            string[] mainMessagePart = msg.Split('/');
            if (mainMessagePart[0] == "m" || mainMessagePart[0] == "e")
            {
                DeleteSpecificMessage(stringList, mainMessagePart, 1);
            }
            else if (mainMessagePart[0] == "a")
            {
                DeleteSpecificMessage(stringList, mainMessagePart, 2);
            }
        }
    }

    public void DeleteSpecificMessage(List<string> stringList, string[] mainMessagePart, int index)
    {
        for (int i = stringList.Count - 2; i >= 0; i--)
        {
            string[] splitmessage = stringList[i].Split("/");
            if (splitmessage[0] == mainMessagePart[0] && splitmessage[index] == mainMessagePart[index])
            {
                Debug.Log("found unneeded message " + stringList[i]);
                stringList.Remove(stringList[i]);
            }
        }
    }

    public string GetSynchronization(bool sendToOthers)
    {
        foreach (OnlinePlayerManager man in managers)
        {
            if (man.IsOwner)
            {
                return (man.GetSynchronization(sendToOthers));
            }
        }
        return null;
    }

    public void Synchronize(string s)
    {
        int id = Int32.Parse(s[1].ToString());
        string[] splitMessage = s[2..].Split("|");
        OnlinePlayerManager man = FindManagerUsingId(id);
        if (s.Length < 3)
        {
            solidSynchronizationTimer = 15;
            return;
        }
        //Debug.Log(s);
        foreach (string msg in splitMessage)
        {
            string[] msgPart = msg.Split("/");            
            if (man.GetPlayerUnits().Count > Int32.Parse(msgPart[0]) && man.GetPlayerUnits()[Int32.Parse(msgPart[0])])
            {
                OnlineUnit playerUnit = man.GetPlayerUnits()[Int32.Parse(msgPart[0])];
                if (msgPart[1] != "n")
                {
                    OnlineTarget enemy;
                    string[] msgAttackPart = msgPart[1].Split("-");
                    OnlinePlayerManager enemyMan = FindManagerUsingId(Int32.Parse(msgAttackPart[0]));
                    if (msgAttackPart[1] == "u" && enemyMan.FindListId(Int32.Parse(msgAttackPart[2]), false))
                    {
                        enemy = enemyMan.FindListId(Int32.Parse(msgAttackPart[2]), false);
                        playerUnit.SetAttackQueued(enemy);
                    }
                    else if (msgAttackPart[1] == "b" && enemyMan.FindListId(Int32.Parse(msgAttackPart[2]), true))
                    {
                        enemy = enemyMan.FindListId(Int32.Parse(msgAttackPart[2]), true);
                        playerUnit.SetAttackQueued(enemy);
                    }
                    else
                    {
                        playerUnit.SetAttackQueued(null);
                    }
                }
                playerUnit.transform.position = new Vector3(man.StringToVector(msgPart[2]).x, man.StringToVector(msgPart[2]).y, -1f);                
                playerUnit.VisibilityControl();
            }
        }
        solidSynchronizationTimer = 15;
    }

    public OnlinePlayerManager FindManagerUsingId(int id)
    {
        foreach (OnlinePlayerManager man in managers)
        {
            if (man.GetId() == id)
            {
                return man;
            }
        }
        return null;
    }

    public void SetMax(int i)
    {
        maxNumberOfPlayers = i;
    }
}

public class MessagePipe
{
    public enum Phase
    {
        ready,
        ongoing,
        confirm,
        clear,
        proceed
    }

    private Phase phase;

    private List<string> messages;
    private List<string> OngoingMessages;
    private int ongoingCalled = 0;
    private int confirmCalled = 0;
    private float delay;
    private bool ongoing;

    public MessagePipe()
    {
        phase = Phase.ready;
        messages = new List<string>();
        OngoingMessages = new List<string>();
        ongoing = false;
    }

    public List<string> GetMessages() => messages;

    public List<string> GetOngoingMessages() => OngoingMessages;

    public void AddMessages(string msg) => messages.Add(msg);

    public void AddMessagesRange(List<string> msgs) => messages.AddRange(msgs);

    public void AddOngoingMessages(string msg) => OngoingMessages.Add(msg);

    public void ClearMessages() => messages.Clear();

    public void ClearOngoingMessages() => OngoingMessages.Clear();

    public void ChangePhase(Phase p) => phase = p;

    public Phase GetPhase() => phase;

    public int GetOngoingCalled() => ongoingCalled;

    public void SetOngoingCalled(int i) => ongoingCalled = i;

    public void IncrementOngoingCalled() => ongoingCalled++;

    public int GetConfirmCalled() => confirmCalled;

    public void SetConfirmCalled(int i) => confirmCalled = i;

    public void IncrementConfirmCalled() => confirmCalled++;

    public float GetDelay() => delay;

    public void SetDelay(float val) => delay = val;

    public bool GetOngoing() => ongoing;

    public void SetOngoing(bool val) => ongoing = val;
}
