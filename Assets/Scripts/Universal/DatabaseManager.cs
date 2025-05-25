using UnityEngine;
using MySql.Data.MySqlClient;
using System;
using System.Net;
using Renci.SshNet;
using Renci.SshNet.Common;
using System.IO;
using System.Xml.Linq;
using System.Security.Cryptography;
using System.Buffers.Text;
using System.Collections.Generic;
using Org.BouncyCastle.Crypto.Parameters;


public class DatabaseManager : MonoBehaviour
{
    public class User
    {
        public int uid;
        public string username;
    }

    public class Level
    {
        public int id;
        public string name, author;
        public double succesRatio, rating;
        public DateTime created, updated;
    }

    public User CurrentUser;
    public static DatabaseManager Instance;
    private SshClient ssh;
    private MySqlConnection connection;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
            return;
        }
        Debug.Log("Trying to set up SSH tunnel...");
        if (SshTunel())
        {
            Debug.Log("Trying to connect to MySQL...");
            if (ConnectToDatabase())
            {
                Debug.Log("Successfully set up DB connection");
            }
        }
    }

    private bool SshTunel()
    {
        var config = LoadConnectionData();
        if (!config.Tunnel) return true;
        var key = Resources.Load<TextAsset>("ssh_key");
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(key.text));
        var connectionInfo = new PrivateKeyConnectionInfo(config.TunnelServer, config.TunnelUser, new PrivateKeyFile(stream));
        connectionInfo.Timeout = TimeSpan.FromSeconds(30);

        ssh = new SshClient(connectionInfo);
        try
        {
            Debug.Log("Trying SSH connection...");
            ssh.Connect();
            if (ssh.IsConnected)
            {
                Debug.Log("SSH connection is active: " + ssh.ConnectionInfo.ToString());
            }
            else
            {
                Debug.Log("SSH connection has failed: " + ssh.ConnectionInfo.ToString());
            }

            Debug.Log("\r\nTrying port forwarding...");
            var portFwld = new ForwardedPortLocal(IPAddress.Loopback.ToString(), config.TunnelPort, config.Server, config.Port);
            ssh.AddForwardedPort(portFwld);
            portFwld.Start();
            if (portFwld.IsStarted)
            {
                Debug.Log("Port forwarded: " + portFwld.ToString());
                Debug.Log("\r\nTrying database connection...");
                return true;
            }
            else
            {
                Debug.Log("Port forwarding has failed.");
            }
        }
        catch (SshException ex)
        {
            Debug.Log("SSH client connection error: " + ex.Message);
        }
        catch (System.Net.Sockets.SocketException ex1)
        {
            Debug.Log("Socket connection error: " + ex1.Message);
        }
        return false;
    }

    private bool ConnectToDatabase()
    {
        try
        {
            var sqlConfig = LoadConnectionData();
            var port = sqlConfig.Tunnel ? sqlConfig.TunnelPort : sqlConfig.Port;
            var server = sqlConfig.Tunnel ? IPAddress.Loopback.ToString() : sqlConfig.Server;
            var connectionString = $"Server={server};Port={port};Database={sqlConfig.Database};Uid={sqlConfig.Uid};Pwd={sqlConfig.Password};";
            Debug.Log(connectionString);
            connection = new MySqlConnection(connectionString);
            connection.Open();

            if (connection.State == System.Data.ConnectionState.Open)
            {
                return true;
            }
            else
            {
                Debug.LogError("Failed to connect to MySQL database. Connection state: " + connection.State);
            }
        }
        catch (MySqlException ex)
        {
            Debug.LogError($"MySQL Connection Error ({ex.Number}): {ex.Message}");
        }
        catch (Exception ex)
        {
            Debug.LogError("General Connection Error: " + ex.Message);
        }
        return false;
    }

    private MySqlConfig LoadConnectionData()
    {
        var configFile = Resources.Load<TextAsset>("mysql_config");
        MySqlConfig sqlConfig = new MySqlConfig();
        if (configFile != null)
        {
            sqlConfig = JsonUtility.FromJson<MySqlConfig>(configFile.text);

            Debug.Log("Connection data loaded from file.");
        }
        else
        {
            Debug.LogError("MySQL configuration file (mysql_config.json) not found in Resources folder! Please create it.");
        }
        return sqlConfig;
    }

    public void Login(string username, string password)
    {
        string query = "select id, username, created from platf_users where username=@username and hash=SHA2(CONCAT(@password, salt), 0);";
        MySqlCommand cmd = new MySqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@password", password);
        cmd.Parameters.AddWithValue("@username", username);
        try
        {
            using (var reader = cmd.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    reader.Read();
                    CurrentUser = new User() { uid = reader.GetInt32("id"), username = reader.GetString("username") };
                    return;
                }
                CurrentUser = null;
            }
        }
        catch (Exception e)
        {
            throw new ApplicationException("Invalid login", e);
        }
        throw new ApplicationException("Invalid login");
    }

    public void RegisterUser(string username, string password)
    {
        RandomNumberGenerator rng = RandomNumberGenerator.Create();
        byte[] random = new byte[16];
        rng.GetNonZeroBytes(random);
        var salt = System.Convert.ToBase64String(random);
        string query = "insert into platf_users (username, hash, salt) values (@username, SHA2(CONCAT(@password, @salt), 0), @salt);";
        MySqlCommand cmd = new MySqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@username", username);
        cmd.Parameters.AddWithValue("@password", password);
        cmd.Parameters.AddWithValue("@salt", salt);
        try
        {
            var rows = cmd.ExecuteNonQuery();
            if (rows == 1)
            {
                CurrentUser = new User() { uid = (int)cmd.LastInsertedId, username = username };
                return;
            }
            CurrentUser = null;
        }
        catch (Exception e)
        {
            throw new ApplicationException("Username already exists", e);
        }
        throw new ApplicationException("Username already exists");
    }

    public IList<Level> SearchLevels(string author, string levelName, bool myLevels, bool ascending, string sortColumn, double minSuccesRatio, double maxSuccesRatio, double minRating, double maxRating)
    {
        switch (sortColumn)
        {
            case "successRatio": sortColumn = "(l.successful / l.attempts)"; break;
            case "update": sortColumn = "l.updated"; break;
            default: sortColumn = "l." + sortColumn; break;
        }
        string query = @"select l.id, l.name, u.username, l.created, l.updated, (l.successful / l.attempts) as succesRatio, l.rating
          from platf_levels l join platf_users u on l.owner_id = u.id
          where u.username like '%@auhtor%'
            and l.name like '%@levelName%'
            and l.rating >= @minRating and l.rating <= @maxRating
            and round((l.successful / l.attempts), 2) >= @minSuccesRatio and round((l.successful / l.attempts), 2) <= @maxSuccesRatio"
            + (myLevels ? " and u.id = @userId" : "")
          + (sortColumn != "" ? " order by " + sortColumn + (ascending ? " asc" : " desc") : "");
        Debug.Log("Level search query: " + query);
        MySqlCommand cmd = new MySqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@author", author);
        cmd.Parameters.AddWithValue("@levelName", levelName);
        cmd.Parameters.AddWithValue("@userId", CurrentUser.uid);
        cmd.Parameters.AddWithValue("@minSuccesRatio", minSuccesRatio);
        cmd.Parameters.AddWithValue("@maxSuccesRatio", maxSuccesRatio);
        cmd.Parameters.AddWithValue("@minRating", minRating);
        cmd.Parameters.AddWithValue("@maxRating", maxRating);
        var searchResult = new List<Level>();
        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                searchResult.Add(new Level()
                {
                    id = reader.GetInt32("id"),
                    author = reader.GetString("username"),
                    name = reader.GetString("name"),
                    created = reader.GetDateTime("created"),
                    rating = reader.GetDouble("rating"),
                    succesRatio = reader.GetDouble("succesRatio"),
                    updated = reader.GetDateTime("updated")
                });
            }
        }
        return searchResult;
    }

}