using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using DG.Tweening;
using System.Linq;
using Newtonsoft.Json;
using Best.SocketIO;
using Best.SocketIO.Events;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization;

public class SocketIOManager : MonoBehaviour
{
    [Header("scripts")]
    [SerializeField] private SlotManager slotManager;
    [SerializeField] private UIManager uIManager;

    internal GameData initialData = null;
    internal UIData initUIData = null;
    internal GameData resultData = null;
    internal PlayerData playerdata = null;

    internal SocketModel socketModel = new SocketModel();

    private Helper helper = new Helper();


    [SerializeField]
    internal List<string> bonusdata = null;
    //WebSocket currentSocket = null;
    internal bool isResultdone = false;

    private SocketManager manager;

    [SerializeField]
    internal JSHandler _jsManager;

    //[SerializeField]
    //private string SocketURI;

    protected string SocketURI = null;
    protected string TestSocketURI = "http://localhost:5000";
    //protected string SocketURI = "http://localhost:5000";

    [SerializeField]
    private string TestToken;

    // TODO: WF to be added
    protected string gameID = "";
    // protected string gameID = "SL-WF";

    internal bool isLoading;
    internal bool SetInit = false;
    private const int maxReconnectionAttempts = 6;
    private readonly TimeSpan reconnectionDelay = TimeSpan.FromSeconds(10);

    private void Awake()
    {
        isLoading = true;
        SetInit = false;
        // Debug.unityLogger.logEnabled = false;
    }

    private void Start()
    {
        //OpenWebsocket();
        OpenSocket();
    }




    void ReceiveAuthToken(string jsonData)
    {
        Debug.Log("Received data: " + jsonData);

        // Parse the JSON data
        var data = JsonUtility.FromJson<AuthTokenData>(jsonData);
        SocketURI = data.socketURL;
        myAuth = data.cookie;
        // Proceed with connecting to the server using myAuth and socketURL
    }

    string myAuth = null;

    internal void OpenSocket()
    {
        // Create and setup SocketOptions
        SocketOptions options = new SocketOptions();
        options.ReconnectionAttempts = maxReconnectionAttempts;
        options.ReconnectionDelay = reconnectionDelay;
        options.Reconnection = true;

        Application.ExternalCall("window.parent.postMessage", "authToken", "*");

#if UNITY_WEBGL && !UNITY_EDITOR
        Application.ExternalEval(@"
            window.addEventListener('message', function(event) {
                if (event.data.type === 'authToken') {
                    var combinedData = JSON.stringify({
                        cookie: event.data.cookie,
                        socketURL: event.data.socketURL
                    });
                    // Send the combined data to Unity
                    SendMessage('SocketManager', 'ReceiveAuthToken', combinedData);
                }});");
        StartCoroutine(WaitForAuthToken(options));
#else
        Func<SocketManager, Socket, object> authFunction = (manager, socket) =>
        {
            return new
            {
                token = TestToken,
                gameId = gameID
            };
        };
        options.Auth = authFunction;
        // Proceed with connecting to the server
        SetupSocketManager(options);
#endif
    }

    private IEnumerator WaitForAuthToken(SocketOptions options)
    {
        // Wait until myAuth is not null
        while (myAuth == null)
        {
            yield return null;
        }

        // Once myAuth is set, configure the authFunction
        Func<SocketManager, Socket, object> authFunction = (manager, socket) =>
        {
            return new
            {
                token = myAuth,
                gameId = gameID
            };
        };
        options.Auth = authFunction;

        Debug.Log("Auth function configured with token: " + myAuth);

        // Proceed with connecting to the server
        SetupSocketManager(options);
    }
    private void OnSocketState(bool state)
    {
        if (state)
        {
            Debug.Log("my state is " + state);
            InitRequest("AUTH");
        }
        else
        {

        }
    }
    private void OnSocketError(string data)
    {
        Debug.Log("Received error with data: " + data);
    }
    private void OnSocketAlert(string data)
    {
        Debug.Log("Received alert with data: " + data);
        // AliveRequest("YES I AM ALIVE");
    }

    private void OnSocketOtherDevice(string data)
    {
        Debug.Log("Received Device Error with data: " + data);
        uIManager.ADfunction();
    }

    private void AliveRequest()
    {
        SendData("YES I AM ALIVE");
    }

    void OnConnected(ConnectResponse resp)
    {
        Debug.Log("Connected!");
        SendPing();

        //InitRequest("AUTH");
    }

    private void SendPing()
    {
        InvokeRepeating("AliveRequest", 0f, 3f);
    }

    private void OnDisconnected(string response)
    {
        Debug.Log("Disconnected from the server");
        StopAllCoroutines();
        uIManager.DisconnectionPopup();
    }

    private void OnError(string response)
    {
        Debug.LogError("Error: " + response);
    }

    private void OnListenEvent(string data)
    {
        Debug.Log("Received some_event with data: " + data);
        ParseResponse(data);
    }

    private void SetupSocketManager(SocketOptions options)
    {
        // Create and setup SocketManager
#if UNITY_EDITOR
        this.manager = new SocketManager(new Uri(TestSocketURI), options);
#else
        this.manager = new SocketManager(new Uri(SocketURI), options);
#endif

        // Set subscriptions
        this.manager.Socket.On<ConnectResponse>(SocketIOEventTypes.Connect, OnConnected);
        this.manager.Socket.On<string>(SocketIOEventTypes.Disconnect, OnDisconnected);
        this.manager.Socket.On<string>(SocketIOEventTypes.Error, OnError);
        this.manager.Socket.On<string>("message", OnListenEvent);
        this.manager.Socket.On<bool>("socketState", OnSocketState);
        this.manager.Socket.On<string>("internalError", OnSocketError);
        this.manager.Socket.On<string>("alert", OnSocketAlert);
        this.manager.Socket.On<string>("AnotherDevice", OnSocketOtherDevice);


        // Start connecting to the server
        this.manager.Open();
    }

    // Connected event handler implementation

    private void InitRequest(string eventName)
    {
        InitData message = new InitData();
        message.Data = new AuthData();
        message.Data.GameID = gameID;
        message.id = "Auth";
        // Serialize message data to JSON
        string json = JsonUtility.ToJson(message);
        SendData(eventName, json);
    }

    internal void CloseSocket()
    {
        SendData("EXIT");
        DOVirtual.DelayedCall(0.1f, () =>
        {
            if (this.manager != null)
            {
                Debug.Log("Dispose my Socket");
                this.manager.Close();
            }
        });
    }

    private void ParseResponse(string jsonObject)
    {
        Debug.Log(jsonObject);
        // Root myData = JsonConvert.DeserializeObject<Root>(jsonObject);
        JObject resp = JObject.Parse(jsonObject);
        // string id = myData.id;
        string messageId = resp["id"].ToString();

        Debug.Log("message id " + messageId);



        var message = resp["message"];
        var gameData = message["GameData"];
        socketModel.playerData = message["PlayerData"].ToObject<PlayerData>();

        if (messageId == "InitData")
        {
            socketModel.uIData = message["UIData"].ToObject<UIData>();
            socketModel.initGameData.Bets = gameData["Bets"].ToObject<List<double>>();
            isLoading = false;
            Application.ExternalCall("window.parent.postMessage", "OnEnter", "*");
        }
        else if (messageId == "ResultData")
        {
            socketModel.resultGameData.ResultReel = helper.ConvertStringListsToIntLists(gameData["resultSymbols"].ToObject<List<List<string>>>());
            socketModel.resultGameData.linesToEmit = gameData["linestoemit"].ToObject<List<int>>();
            isResultdone = true;
            // socketModel.resultGameData.symbolsToEmit = gameData["symbolsToEmit"].ToObject<List<List<string>>>();
            // socketModel.resultGameData.WinAmout = gameData["WinAmout"].ToObject<double>();
            // socketModel.resultGameData.freeSpins = gameData["freeSpins"].ToObject<double>();
            // socketModel.resultGameData.jackpot = gameData["jackpot"].ToObject<double>();
            // socketModel.resultGameData.isBonus = gameData["isBonus"].ToObject<bool>();
            // socketModel.resultGameData.BonusStopIndex = gameData["BonusStopIndex"].ToObject<double>();
            print("result data: " + JsonConvert.SerializeObject(socketModel.resultGameData.ResultReel));

        }
        // switch (id)
        // {
        //     case "InitData":
        //         {
        //             initialData = myData.message.GameData;
        //             initUIData = myData.message.UIData;
        //             playerdata = myData.message.PlayerData;
        //             bonusdata = myData.message.BonusData;
        //             if (!SetInit)
        //             {
        //                 Debug.Log(jsonObject);
        //                 List<string> LinesString = ConvertListListIntToListString(initialData.Lines);
        //                 List<string> InitialReels = ConvertListOfListsToStrings(initialData.Reel);
        //                 InitialReels = RemoveQuotes(InitialReels);
        //                 PopulateSlotSocket(InitialReels, LinesString);
        //                 SetInit = true;
        //             }
        //             else
        //             {
        //                 RefreshUI();
        //             }
        //             break;
        //         }
        //     case "ResultData":
        //         {
        //             Debug.Log(jsonObject);
        //             myData.message.GameData.FinalResultReel = ConvertListOfListsToStrings(myData.message.GameData.ResultReel);
        //             myData.message.GameData.FinalsymbolsToEmit = TransformAndRemoveRecurring(myData.message.GameData.symbolsToEmit);
        //             resultData = myData.message.GameData;
        //             playerdata = myData.message.PlayerData;
        //             isResultdone = true;
        //             break;
        //         }
        // }
    }

    private void RefreshUI()
    {
        uIManager.InitialiseUIData(initUIData.AbtLogo.link, initUIData.AbtLogo.logoSprite, initUIData.ToULink, initUIData.PopLink, initUIData.paylines);
    }
    private void PopulateSlotSocket(List<string> slotPop, List<string> LineIds)
    {
        slotManager.shuffleInitialMatrix();
        for (int i = 0; i < LineIds.Count; i++)
        {
            slotManager.FetchLines(LineIds[i], i);
        }

        // slotManager.SetInitialUI();
        isLoading = false;

        Application.ExternalCall("window.parent.postMessage", "OnEnter", "*");
    }

    internal void AccumulateResult(double currBet)
    {
        isResultdone = false;
        MessageData message = new MessageData();
        message.data = new BetData();
        message.data.currentBet = currBet;
        message.data.spins = 1;
        message.data.currentLines = 20;
        message.id = "SPIN";

        // Serialize message data to JSON
        string json = JsonConvert.SerializeObject(message);
        SendData("message", json);
    }

    internal void SendData(string eventName, object message = null)
    {
        isResultdone = false;
        string json = JsonConvert.SerializeObject(message);

        if (this.manager.Socket == null || !this.manager.Socket.IsOpen)
        {
            Debug.LogWarning("Socket is not connected.");
            return;
        }
        if (message == null)
        {
            this.manager.Socket.Emit(eventName);
            return;
        }
        this.manager.Socket.Emit(eventName, json);
        Debug.Log("JSON data sent: " + json);

    }

    private List<string> RemoveQuotes(List<string> stringList)
    {
        for (int i = 0; i < stringList.Count; i++)
        {
            stringList[i] = stringList[i].Replace("\"", ""); // Remove inverted commas
        }
        return stringList;
    }

    private List<string> ConvertListListIntToListString(List<List<int>> listOfLists)
    {
        List<string> resultList = new List<string>();

        foreach (List<int> innerList in listOfLists)
        {
            List<string> stringList = new List<string>();
            foreach (int number in innerList)
            {
                stringList.Add(number.ToString());
            }
            string joinedString = string.Join(",", stringList.ToArray()).Trim();
            resultList.Add(joinedString);
        }

        return resultList;
    }

    private List<string> ConvertListOfListsToStrings(List<List<string>> inputList)
    {
        List<string> outputList = new List<string>();

        foreach (List<string> row in inputList)
        {
            string concatenatedString = string.Join(",", row);
            outputList.Add(concatenatedString);
        }

        return outputList;
    }

    private List<string> TransformAndRemoveRecurring(List<List<string>> originalList)
    {
        // Flattened list
        List<string> flattenedList = new List<string>();
        foreach (List<string> sublist in originalList)
        {
            flattenedList.AddRange(sublist);
        }

        // Remove recurring elements
        HashSet<string> uniqueElements = new HashSet<string>(flattenedList);

        // Transformed list
        List<string> transformedList = new List<string>();
        foreach (string element in uniqueElements)
        {
            transformedList.Add(element.Replace(",", ""));
        }

        return transformedList;
    }
}





