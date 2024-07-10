using System;
using UnityEngine;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

public class MqttClientHandlerOEH : MonoBehaviour {
    [Header("Broker Configuration")]
    [SerializeField] string brokerAddress = "localhost";
    [SerializeField] int port = 8883;
    [SerializeField] bool tlsEnabled = true;
    [SerializeField] MqttSslProtocols protocol = MqttSslProtocols.TLSv1_2;

    [Header("Credentials")]
    [SerializeField] string username = "";
    [SerializeField] string password = "";
    [SerializeField][Tooltip("Leave empty for randomly generated id")] string clientId = "";
    [SerializeField] bool automaticallyReconnect = true;

    [Header("Connection Settings")]
    [SerializeField] int delayBetweenReconnects = 500;
    [SerializeField] int timeoutTime = 10000;

    [Header("Messaging Settings")]
    [SerializeField] string topic = "escaperoom/kamerdeur";
    [SerializeField] string message = "Deur open";
    [SerializeField] int qosLevel = 0;

    float timer = 0;
    bool timerEnabled;
    MqttClient mqttClient;
    

    // Start is called before the first frame update
    void Start() {
        if (clientId == "") clientId = Guid.NewGuid().ToString();
    }



    // Update is called once per frame
    void Update() {
        if (mqttClient == null && automaticallyReconnect) {
            timerEnabled = true;
        }else if (!mqttClient.IsConnected && automaticallyReconnect) {
            timerEnabled = true;
        }

        if (timerEnabled) {
            timer += Time.deltaTime;
        }


        if (timerEnabled && timer > delayBetweenReconnects / 1000) {
            MqttConnect();
            timer = 0;
            Debug.Log("Reconnection attempted");
        }
    }
    void OnApplicationQuit() {
        // Disconnect the client when the application quits
        if (mqttClient != null && mqttClient.IsConnected) {
            mqttClient.Disconnect();
        }
    }

    public void SendMqttMessage() {
        SendMqttMessage(topic, message , qosLevel);
    }

    public void SendMqttMessage(string topic, string msg, int qosLevel) {
        if (qosLevel == 0) mqttClient.Publish(topic, System.Text.Encoding.UTF8.GetBytes(msg), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
        if (qosLevel == 1) mqttClient.Publish(topic, System.Text.Encoding.UTF8.GetBytes(msg), MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, false);
        if (qosLevel == 2) mqttClient.Publish(topic, System.Text.Encoding.UTF8.GetBytes(msg), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
    } 


    void MqttConnect() {
        if (mqttClient == null) {
            try {
                mqttClient = new MqttClient(brokerAddress, port, tlsEnabled, protocol, null, null);
            } catch (Exception e) {
                mqttClient = null;

                Debug.LogErrorFormat("Mqtt client failed to connect: {0}", e.ToString());
            }
            mqttClient.Settings.TimeoutOnConnection = timeoutTime;
            try {
                mqttClient.Connect(clientId, username, password);
            } catch (Exception e) {
                mqttClient = null;
                Debug.LogErrorFormat("Mqtt client failed to connect to {0}:{1}\n (check parameters: tls/protocol, address/port, username/password):\n{2}", brokerAddress, port, e.ToString());
            }
            if (mqttClient != null && mqttClient.IsConnected) {
                timerEnabled = false;
            } else {
                Debug.Log("Mqtt client failed to connect.");
            }
        }
    }
}
