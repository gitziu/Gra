using UnityEngine;
using MySql.Data.MySqlClient;
using System;
using System.Net;
using Renci.SshNet;
using Renci.SshNet.Common;

public class MySQLTestConnection : MonoBehaviour
{
    // ... (usunięte publiczne pola do haseł)

    private MySqlConnection connection;
    private string connectionString;

    void Start()
    {
        LoadConnectionData(); // Wywołaj funkcję wczytującą dane
        Debug.Log("Trying to connect to MySQL...");
        ConnectToDatabase();
    }

    void sshTunel()
    {
        PasswordConnectionInfo connectionInfo = new PasswordConnectionInfo("example.com", 2222, "username", "password");
        connectionInfo.Timeout = TimeSpan.FromSeconds(30);

        using (var client = new SshClient(connectionInfo))
        {
            try
            {
                Console.WriteLine("Trying SSH connection...");
                client.Connect();
                if (client.IsConnected)
                {
                    Console.WriteLine("SSH connection is active: {0}", client.ConnectionInfo.ToString());
                }
                else
                {
                    Console.WriteLine("SSH connection has failed: {0}", client.ConnectionInfo.ToString());
                }

                Console.WriteLine("\r\nTrying port forwarding...");
                var portFwld = new ForwardedPortLocal(IPAddress.Loopback.ToString(), 2222, "example.com", 3306);
                client.AddForwardedPort(portFwld);
                portFwld.Start();
                if (portFwld.IsStarted)
                {
                    Console.WriteLine("Port forwarded: {0}", portFwld.ToString());
                    Console.WriteLine("\r\nTrying database connection...");


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

        }
    }
    void ConnectToDatabase()
    {
        try
        {
            connection = new MySqlConnection(connectionString);
            connection.Open();

            if (connection.State == System.Data.ConnectionState.Open)
            {
                Debug.Log("Successfully connected to MySQL database!");
                // Tutaj możesz wykonać jakieś zapytania, np. odczytać dane
                // ExampleQuery();
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
        finally
        {
            // Zawsze zamykaj połączenie, gdy skończysz!
            if (connection != null && connection.State == System.Data.ConnectionState.Open)
            {
                connection.Close();
                Debug.Log("MySQL connection closed.");
            }
        }
    }
    void LoadConnectionData()
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

        connectionString = $"Server={sqlConfig.Server};Port={sqlConfig.Port};Database={sqlConfig.Database};Uid={sqlConfig.Uid};Pwd={sqlConfig.Password};";
    }

    // ... reszta kodu ConnectToDatabase() i ExampleQuery() bez zmian
}