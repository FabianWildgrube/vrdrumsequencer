using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WebSocketSharp;
using Newtonsoft.Json;

public struct SampleRequest
{
    public int id;
    public float[] Items;
}

public class SampleServer : MonoBehaviour
{
    public static SampleServer instance;

    [SerializeField]
    string host;

    [SerializeField]
    string port;

    WebSocket ws;

    Dictionary<int, Action<float[]>> receiveHandlers = new Dictionary<int, Action<float[]>>();
    int receiveHandlerCtr = 0;

    Queue<Action> mainThreadDispatchQueue = new Queue<Action>();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }

        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);

            ws = new WebSocket("ws://" + host + ":" + port);

            ws.OnOpen += (sender, e) =>
            {
                Debug.Log("Connection established");
            };
            ws.OnMessage += (sender, e) =>
            {
                handleIncomingMessage(e);
            };
            ws.OnClose += (sender, e) =>
            {
                Debug.Log("Server connection closed");
            };

            ws.Connect();
        }
    }

    private void Update()
    {
        while(mainThreadDispatchQueue.Count > 0)
        {
            Action fn = mainThreadDispatchQueue.Dequeue();
            fn();
        }
    }

    private void handleIncomingMessage(MessageEventArgs e)
    {
        //extract requestID from first 4 bytes;
        int requestId = BitConverter.ToInt32(e.RawData, 0);

        Debug.Log("Received response for Request " + requestId);

        float[] samples = new float[e.RawData.Length / 4];

        const int sampleDataStartOffset = 4; //skip the first 4 bytes (requestID integer)
        for (int byteIdx = sampleDataStartOffset; byteIdx < e.RawData.Length; byteIdx += sizeof(float))
        {
            int floatIdx = byteIdx / sizeof(float);
            samples[floatIdx] = BitConverter.ToSingle(e.RawData, byteIdx);
        }

        if (receiveHandlers.ContainsKey(requestId))
        {
            Action<float[]> handler = receiveHandlers[requestId];
            mainThreadDispatchQueue.Enqueue(() =>
            {
                Debug.Log("Calling Handler for request " + requestId);
                handler(samples);
                receiveHandlers.Remove(requestId);
            });
        }
        else
        {
            Debug.LogWarning("No requestHandler for request " + requestId + " found. Ignoring message!");
        }
    }

    public void SendSampleRequest(SampleDefinition definition, Action<float[]> onReceived)
    {
        int requestId = receiveHandlerCtr++;

        if (ws.IsAlive)
        {
            receiveHandlers.Add(requestId, onReceived);
            string request = generateRequestJSON(definition, requestId);
            ws.Send(request);
            Debug.Log("Sent sampleRequest " + requestId);
        } else
        {
            Debug.LogError("Couldn't send request " + requestId + "! SampleServer seems to be Down!");
        }
    }

    private string generateRequestJSON(SampleDefinition definition, int requestId)
    {
        var request = new SampleRequest();
        request.id = requestId;
        request.Items = definition.vectorValues;

        return JsonConvert.SerializeObject(request);
    }
}
