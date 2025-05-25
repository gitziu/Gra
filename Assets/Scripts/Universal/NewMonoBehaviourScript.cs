using UnityEngine;
using MySql.Data.MySqlClient;
using System;
using System.Net;
using Renci.SshNet;
using Renci.SshNet.Common;
using System.IO;

//ascd

public class MySQLTestConnection : MonoBehaviour
{
    // ... (usunięte publiczne pola do haseł)

    private SshClient ssh;
    private MySqlConnection connection;

    void Start()
    {
        Debug.Log("Trying to set up SSH tunnel...");
        if (SshTunel())
        {
            Debug.Log("Trying to connect to MySQL...");
            if (ConnectToDatabase())
            {
                Debug.Log("Successfully set up DB connection");
                SelectQuery();
            }
        }
    }

    bool SshTunel()
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
            Console.WriteLine("Trying SSH connection...");
            ssh.Connect();
            if (ssh.IsConnected)
            {
                Console.WriteLine("SSH connection is active: {0}", ssh.ConnectionInfo.ToString());
            }
            else
            {
                Console.WriteLine("SSH connection has failed: {0}", ssh.ConnectionInfo.ToString());
            }

            Console.WriteLine("\r\nTrying port forwarding...");
            var portFwld = new ForwardedPortLocal(IPAddress.Loopback.ToString(), config.TunnelPort, config.Server, config.Port);
            ssh.AddForwardedPort(portFwld);
            portFwld.Start();
            if (portFwld.IsStarted)
            {
                Console.WriteLine("Port forwarded: {0}", portFwld.ToString());
                Console.WriteLine("\r\nTrying database connection...");
                return true;
            }
            else
            {
                Console.WriteLine("Port forwarding has failed.");
            }
        }
        catch (SshException ex)
        {
            Console.WriteLine("SSH client connection error: {0}", ex.Message);
        }
        catch (System.Net.Sockets.SocketException ex1)
        {
            Console.WriteLine("Socket connection error: {0}", ex1.Message);
        }
        return false;
    }
    bool ConnectToDatabase()
    {
        try
        {
            var sqlConfig = LoadConnectionData();
            var port = sqlConfig.Tunnel ? sqlConfig.TunnelPort : sqlConfig.Port;
            var server = sqlConfig.Tunnel ? IPAddress.Loopback.ToString() : sqlConfig.Server;
            var connectionString = $"Server={server};Port={port};Database={sqlConfig.Database};Uid={sqlConfig.Uid};Pwd={sqlConfig.Password};";

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
            // Obsługa błędów specyficznych dla MySQL
            Debug.LogError($"MySQL Connection Error ({ex.Number}): {ex.Message}");
            // Typowe numery błędów:
            // 0: Cannot connect to server.
            // 1045: Invalid user name/password.
            // 1042: Unable to connect to host.
            // 1049: Unknown database.
        }
        catch (Exception ex)
        {
            // Ogólna obsługa innych błędów
            Debug.LogError("General Connection Error: " + ex.Message);
        }
        return false;
    }
    MySqlConfig LoadConnectionData()
    {
        // Ścieżka do pliku konfiguracyjnego w folderze Resources
        // Plik powinien być w Assets/Resources/mysql_config.json
        var configFile = Resources.Load<TextAsset>("mysql_config");
        MySqlConfig sqlConfig = new MySqlConfig();
        if (configFile != null)
        {
            // Możesz użyć biblioteki SimpleJSON (do pobrania z Asset Store lub GitHub)
            // lub napisać prosty parser JSON.
            // Na potrzeby przykładu użyjemy SimpleJSON, bo jest proste.
            // Jeśli jej nie masz, zainstaluj lub znajdź inny parser.

            sqlConfig = JsonUtility.FromJson<MySqlConfig>(configFile.text);

            Debug.Log("Connection data loaded from file.");
        }
        else
        {
            Debug.LogError("MySQL configuration file (mysql_config.json) not found in Resources folder! Please create it.");
            // Możesz tu wstawić jakieś domyślne wartości LUB rzucić wyjątek i zatrzymać aplikację
            // aby zapobiec próbie połączenia bez danych.
        }
        return sqlConfig;
    }

    // ... reszta kodu ConnectToDatabase() i ExampleQuery() bez zmian

    void SelectQuery()
    {
        string query = "select ID, name from test;";
        MySqlCommand cmd = new MySqlCommand(query, connection);
        MySqlDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            Debug.Log("ID : " + reader.GetInt32("ID") + " , name : " + reader.GetString("name"));
        }
    }
}