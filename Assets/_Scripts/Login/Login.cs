using System;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Login : MonoBehaviour
{
    [SerializeField] TMP_InputField userLogin;
        [SerializeField] TMP_InputField passLogin;
        [SerializeField] Button btnLogin, btnDangKy;
        private string wsServer = "ws://localhost:8080";
        private ClientWebSocket _client;
        private Socket clientSocket;
        private string ip = "127.0.0.1"; // hoặc 192.168.x.x nếu test mobile
        private int port = 8080;
    
        private byte[] buffer = new byte[1024];
    
        void Start()
        {
            Connect();
        }
    
        public void Connect()
        {
            try
            {
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    
                IPAddress ipAddress = IPAddress.Parse(ip);
                IPEndPoint endPoint = new IPEndPoint(ipAddress, port);
    
                Debug.Log("Connecting...");
                clientSocket.BeginConnect(endPoint, OnConnect, null);
            }
            catch (Exception e)
            {
                Debug.LogError("Connect error: " + e);
            }
        }
    
        private void OnConnect(IAsyncResult ar)
        {
            try
            {
                clientSocket.EndConnect(ar);
                Debug.Log("Connected TCP!");
    
                // bắt đầu nhận data
            }
            catch (Exception e)
            {
                Debug.LogError("Connect fail: " + e);
            }
        }
}
