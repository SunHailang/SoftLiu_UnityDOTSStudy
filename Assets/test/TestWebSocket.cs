using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using UnityEngine;

public class TestWebSocket : MonoBehaviour
{
    private async void CreateWebSocket()
    {
        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        ClientWebSocket clientWeb = new ClientWebSocket();
        //System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12 | System.Net.SecurityProtocolType.Tls11 | System.Net.SecurityProtocolType.Tls;
        await clientWeb.ConnectAsync(new System.Uri("wss://sha-hda1.ubisoft.org/hswpvp/ws"), new System.Threading.CancellationToken());
        if (clientWeb.State == WebSocketState.Open)
        {
            Recv(clientWeb);

            Dictionary<string, object> login = new Dictionary<string, object>();
            login.Add("action", "login");
            login.Add("uuid", "f07f47cc-59a2-46ce-a37e-69540c60aadc");
            string sendData = JsonUtility.ToJson(login);

            byte[] bsend = Encoding.UTF8.GetBytes(sendData);

            //bsend = GetSendData(bsend);
            await clientWeb.SendAsync(new System.ArraySegment<byte>(bsend), WebSocketMessageType.Text, true, new System.Threading.CancellationToken());
            //WebSocketSendAysnc(bsend, WebSocketMessageType.Text);
        }
        else
        {
            Debug.Log($"clientWeb State: {clientWeb.State}");
        }
    }

    private async void Recv(ClientWebSocket webClient)
    {
        try
        {
            while (true)
            {
                byte[] buff = new byte[1024];
                await webClient.ReceiveAsync(new System.ArraySegment<byte>(buff), new System.Threading.CancellationToken());
                string str = Encoding.UTF8.GetString(buff);
                Debug.Log($"Recv: {str}");
            }
        }
        catch (System.Exception error)
        {

            throw;
        }
    }

    public void ButtonClick()
    {
        CreateWebSocket();
    }
}
