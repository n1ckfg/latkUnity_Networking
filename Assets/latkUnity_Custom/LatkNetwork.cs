using System;
using UnityEngine;
using BestHTTP.SocketIO;

public class LatkNetwork : MonoBehaviour {

    public string serverAddress = "vr.fox-gieg.com";
    public int serverPort = 8080;
    public bool doDebug = true;

    private SocketManager socketManager;
    private string socketAddress;
    private bool connected = false;

    public bool getConnectionStatus() {
        return connected;
    }

	private void Start() {
        socketAddress = "http://" + serverAddress + ":" + serverPort + "/socket.io/:8443";
        initSocketManager(socketAddress);
    }

    private void initSocketManager(string uri) {
        socketManager = new SocketManager(new Uri(uri));
        socketManager.Socket.AutoDecodePayload = false;
        socketManager.Socket.On("error", socketError);
        socketManager.Socket.On("connect", socketConnected);
        socketManager.Socket.On("reconnect", socketConnected);

        socketManager.Socket.On("newFrameFromServer", receivedLocalSocketMessage);
    }

    void socketConnected(Socket socket, Packet packet, params object[] args) {
        connected = true;

        if (doDebug) {
            Debug.Log(DateTime.Now + " Connected to server.");
        }
    }

    void socketError(Socket socket, Packet packet, params object[] args) {
        connected = false;
        if (doDebug) {
            Debug.LogError(DateTime.Now + " Failed to connect to server.");

            if (args.Length > 0) {
                Error error = args[0] as Error;
                if (error != null) {
                    switch (error.Code) {
                        case SocketIOErrors.User:
                            Debug.LogError("Socket Error Type: Exception in an event handler.");
                            break;
                        case SocketIOErrors.Internal:
                            Debug.LogError("Socket Error Type: Internal error.");
                            break;
                        default:
                            Debug.LogError("Socket Error Type: Server error.");
                            break;
                    }
                    Debug.LogError(error.ToString());
                    return;
                }
            }
            Debug.LogError("Could not parse error.");
        }
    }

    void receivedLocalSocketMessage(Socket socket, Packet packet, params object[] args) {
        string eventName = "data";
        string jsonString;
        if (packet.SocketIOEvent == SocketIOEventTypes.Event) {
            eventName = packet.DecodeEventName();
            jsonString = packet.RemoveEventName(true);
        } else if (packet.SocketIOEvent == SocketIOEventTypes.Ack) {
            jsonString = packet.ToString();
            jsonString = jsonString.Substring(1, jsonString.Length-2);
        } else {
            jsonString = packet.ToString();
        }

        if (doDebug) {
            Debug.Log(DateTime.Now + " - " + "Local Socket Event Name: " + eventName + " - Message: " + jsonString);
        }

        switch (eventName) {
            case "newFrameFromServer":
                // TODO
                break;
        }
    }

    public void sendStrokeData(string data) {
        socketManager.Socket.Emit("clientStrokeToServer", data);
    }

    private void OnApplicationQuit() {
        socketManager.Close();
        if (doDebug) {
            Debug.Log("Closed connection");
        }
    }

}
