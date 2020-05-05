using System;
using System.Net;
using Mirror;

public class MyServerResponse : MessageBase
{
    // The server that sent this
    // this is a property so that it is not serialized,  but the
    // client fills this up after we receive it
    public IPEndPoint EndPoint { get; set; }
    public string deviceUniqueIdentifier;
    public string name;
    public long serverId;
    public Uri uri;
    public float lifeTimer;
    public int playerCount;
    public int maxPlayerCount;
}