using System;

[Serializable]
public class MySqlConfig
{
    public string Server;
    public string Database;
    public string Uid;
    public string Password;
    public uint Port;
    public bool Tunnel;
    public uint TunnelPort;
    public string TunnelServer;
    public string TunnelUser;
}