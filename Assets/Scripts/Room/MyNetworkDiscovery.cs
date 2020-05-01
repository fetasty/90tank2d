using System.Text.RegularExpressions;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using Mirror;
using Mirror.Discovery;
using UnityEngine.Events;
public class MyServerFoundEvent : UnityEvent<MyServerResponse> {}
public class MyNetworkDiscovery : NetworkDiscoveryBase<ServerRequest, MyServerResponse>
{
    public int myServerBroadcastListenPort = 47777;
    public float myActiveDiscoveryInterval = 1f; // 每1秒询问一次
    public MyServerFoundEvent OnServerFound = new MyServerFoundEvent();    // 发现服务器之后的回调函数
    public long ServerId { get; private set; }
    public Transport transport;
    public override void Start()
    {
        ServerId = RandomLong();
        if (transport == null)
            transport = Transport.activeTransport;
        base.Start();
    }
    public static IPAddress GetRealBroadcastIP() {
        NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
        Regex r = new Regex("vmnet|ppoe|tapvpn|ndisip|virtual|sinforvnic");
        foreach (NetworkInterface adapter in adapters) {
            // 过滤回环地址
            if (adapter.NetworkInterfaceType == NetworkInterfaceType.Loopback) { continue; }
            // 过滤虚拟网卡
            string lowerName = adapter.Name.ToLower();
            if (r.Match(lowerName).Success) { continue; }
            string lowerInfo = adapter.Description.ToLower();
            if (r.Match(lowerInfo).Success) { continue; }
            // 支持IPv4
            if (adapter.Supports(NetworkInterfaceComponent.IPv4)) {
                UnicastIPAddressInformationCollection uniCast = adapter.GetIPProperties().UnicastAddresses;
                foreach (UnicastIPAddressInformation uni in uniCast) {
                    // 网络地址为IPv4类型
                    if (uni.Address.AddressFamily == AddressFamily.InterNetwork) {
                        return GetBroadcast(uni.Address, uni.IPv4Mask);
                    }
                }
            }
        }
        return IPAddress.Broadcast;
    }
    public static string GetBroadcast(string ipAddress, string subnetMask) 
    {
        byte[] ip = IPAddress.Parse(ipAddress).GetAddressBytes();
        byte[] sub = IPAddress.Parse(subnetMask).GetAddressBytes();
        // 广播地址=子网按位求反 再 或IP地址 
        for (int i = 0; i < ip.Length; i++)
        {
            ip[i] = (byte)((~sub[i]) | ip[i]);
        }
        return new IPAddress(ip).ToString();
    }
    public static IPAddress GetBroadcast(IPAddress ipAddress, IPAddress subnetMask) {
        byte[] ip = ipAddress.GetAddressBytes();
        byte[] mask = subnetMask.GetAddressBytes();
        // 广播地址=子网按位求反 再 或IP地址
        for (int i = 0; i < ip.Length; i++)
        {
            ip[i] = (byte)((~mask[i]) | ip[i]);
        }
        return new IPAddress(ip);
    }
    public void MyStartDiscovery() {
        if (!SupportedOnThisPlatform)
            throw new PlatformNotSupportedException("Network discovery not supported in this platform");
        StopDiscovery();
        try
        {
            // Setup port
            clientUdpClient = new UdpClient(0)
            {
                EnableBroadcast = true,
                MulticastLoopback = false
            };
        }
        catch (Exception)
        {
            // Free the port if we took it
            MyShutdown();
            throw;
        }
        _ = ClientListenAsync();
        InvokeRepeating(nameof(MyBroadcastDiscoveryRequest), 0, myActiveDiscoveryInterval);
    }
    private void MyShutdown()
    {
        if (serverUdpClient != null)
        {
            try
            {
                serverUdpClient.Close();
            }
            catch (Exception)
            {
                // it is just close, swallow the error
            }

            serverUdpClient = null;
        }

        if (clientUdpClient != null)
        {
            try
            {
                clientUdpClient.Close();
            }
            catch (Exception)
            {
                // it is just close, swallow the error
            }

            clientUdpClient = null;
        }

        CancelInvoke();
    }
    public void MyBroadcastDiscoveryRequest()
    {
        if (clientUdpClient == null)
            return;

        IPEndPoint endPoint = new IPEndPoint(GetRealBroadcastIP(), myServerBroadcastListenPort);

        using (PooledNetworkWriter writer = NetworkWriterPool.GetWriter())
        {
            writer.WriteInt64(secretHandshake);

            try
            {
                ServerRequest request = GetRequest();

                request.Serialize(writer);

                ArraySegment<byte> data = writer.ToArraySegment();

                clientUdpClient.SendAsync(data.Array, data.Count, endPoint);
            }
            catch (Exception)
            {
                // It is ok if we can't broadcast to one of the addresses
            }
        }
    }
    
    /// <summary>
    /// Process the request from a client
    /// </summary>
    protected override MyServerResponse ProcessRequest(ServerRequest request, IPEndPoint endpoint)
    {
        try
        {
            return new MyServerResponse
            {
                serverId = ServerId,
                name = GameData.playerName,
                uri = transport.ServerUri(),
                lifeTimer = myActiveDiscoveryInterval * 2f,
                playerCount = NetworkManager.singleton.numPlayers,
                maxPlayerCount = NetworkManager.singleton.maxConnections
            };
        }
        catch (System.Exception)
        {
            Debug.LogError($"Transport {transport} does not support network discovery");
            throw;
        }
    }
    /// <summary>
    /// Process the answer from a server
    /// </summary>
    protected override void ProcessResponse(MyServerResponse response, IPEndPoint endpoint)
    {
        response.EndPoint = endpoint;
        UriBuilder realUri = new UriBuilder(response.uri)
        {
            Host = response.EndPoint.Address.ToString()
        };
        response.uri = realUri.Uri;
        OnServerFound.Invoke(response);
    }
}
