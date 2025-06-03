using UnityEngine;
using MySql.Data.MySqlClient;
using System;
using System.Net;
using Renci.SshNet;
using Renci.SshNet.Common;
using System.IO;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Data;


public class DatabaseManager : MonoBehaviour
{
    public class User
    {
        public int uid;
        public string username;
    }

    public class BasicLevelData
    {
        public int id;
        public string author, name, content;
    }

    public class Level
    {
        public int id, Uid;
        public string name, author;
        public double succesRatio, rating;
        public DateTime created, updated;
    }

    public User CurrentUser;
    public BasicLevelData CurrentLevel;
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

    public void CloseConnection()
    {
        if (connection != null && connection.State == System.Data.ConnectionState.Open) connection.Close();
        if (ssh != null && ssh.IsConnected) ssh.Disconnect();
        if (ssh != null) ssh.Dispose();
    }

    public void Login(string username, string password)
    {
        string query = "select id, username, created from platf_users where username=@username and hash=SHA2(CONCAT(@password, salt), 0)";
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
        string query = "insert into platf_users (username, hash, salt) values (@username, SHA2(CONCAT(@password, @salt), 0), @salt)";
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
        Debug.Log("order column : " + sortColumn);
        switch (sortColumn)
        {
            case "successRatio": sortColumn = "(l.successful / l.attempts)"; break;
            case "update": sortColumn = "l.updated"; break;
            case "": sortColumn = ""; break;
            default: sortColumn = "l." + sortColumn; break;
        }
        string query = @"select l.id, l.name, u.username, l.created, l.updated, (l.successful / l.attempts) as succesRatio, l.rating, u.id as Uid
          from platf_levels l join platf_users u on l.owner_id = u.id
          where u.username like Concat('%', @author, '%') 
            and l.name like concat('%', @levelName, '%') 
            and ((l.rating >= @minRating and l.rating <= @maxRating) or l.rating is null)
            and ((round((l.successful / l.attempts), 2) >= @minSuccesRatio and round((l.successful / l.attempts), 2) <= @maxSuccesRatio) or l.attempts = 0)"
            + (myLevels ? " and u.id = @userId" : "")
          + (!string.IsNullOrEmpty(sortColumn) ? " order by " + sortColumn + (ascending ? " asc" : " desc") : "");
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
        try
        {
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
                        rating = reader.IsDBNull("rating") ? 0 : reader.GetDouble("rating"),
                        succesRatio = reader.IsDBNull("succesRatio") ? 0 : reader.GetDouble("succesRatio"),
                        updated = reader.GetDateTime("updated"),
                        Uid = reader.GetInt32("Uid")
                    });
                }
            }
            return searchResult;
        }
        catch (Exception e)
        {
            Debug.Log("Error in search");
            throw new ApplicationException("Search Failed", e);
        }
    }

    public void SaveLevel(string name, int id, byte[] content)
    {
        var query = @"insert into platf_levels (" + (id != -1 ? "id, " : "") + @"name, owner_id)
                    values(" + (id != -1 ? "@id, " : "") + @"@name, @owner_id)
                    on duplicate key update
                    name = @name, updated = @updated";
        MySqlCommand cmd = new MySqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@name", name);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.Parameters.AddWithValue("@owner_id", CurrentUser.uid);
        cmd.Parameters.AddWithValue("@updated", DateTime.Now);
        try
        {
            var rows = cmd.ExecuteNonQuery();
            Debug.Log("level created");
            if (rows > 0)
            {
                //CurrentLevel = new BasicLevelData() { id = id == -1 ? (int)cmd.LastInsertedId : id, author = CurrentUser.username, name = name };
                if (id == -1) id = (int)cmd.LastInsertedId;
                var contentQuery = @"insert into platf_level_content (level_id, content)
                                    values(@id, @content)
                                    on duplicate key update
                                    content = @content";
                MySqlCommand contentcmd = new MySqlCommand(contentQuery, connection);
                contentcmd.Parameters.AddWithValue("@id", id);
                contentcmd.Parameters.AddWithValue("@content", content);
                try
                {
                    var contentRows = contentcmd.ExecuteNonQuery();
                    if (contentRows > 0)
                    {
                        Debug.Log("content saved");
                        return;
                    }
                }
                catch (Exception e)
                {
                    throw new ApplicationException("Level could not be saved", e);
                }
                throw new ApplicationException("Level could not be saved");
            }
        }
        catch (Exception e)
        {
            throw new ApplicationException("Level could not be saved", e);
        }
        throw new ApplicationException("Level could not be saved");
    }

    public byte[] LevelContent(int level_id)
    {
        var query = "select c.content from platf_level_content c where c.level_id = @level_id";
        MySqlCommand cmd = new MySqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@level_id", level_id);
        try
        {
            using (var reader = cmd.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    reader.Read();
                    int colOrdinal = reader.GetOrdinal("content");
                    long blobLength = reader.GetBytes(colOrdinal, 0, null, 0, 0);
                    var content = new byte[blobLength];
                    reader.GetBytes(colOrdinal, 0, content, 0, (int)blobLength);
                    return content;
                }
                throw new ApplicationException("Couldn't retrive level data");
            }
        }
        catch (Exception e)
        {
            throw new ApplicationException("Couldn't retrive level data", e);
        }
        throw new ApplicationException("Couldn't retrive level data");
    }

    public void DeleteLevel(int level_id)
    {
        var query = "delete from platf_levels where id = @level_id";
        MySqlCommand cmd = new MySqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@level_id", level_id);
        try
        {
            Debug.Log("test");
            var rows = cmd.ExecuteNonQuery();
            Debug.Log("Query executed, rows affected : " + Convert.ToString(rows));
            if (rows > 0)
            {
                var contentQuery = "delete from platf_level_content where level_id = @level_id";
                MySqlCommand contentCmd = new MySqlCommand(contentQuery, connection);
                contentCmd.Parameters.AddWithValue("@level_id", level_id);
                try
                {
                    var contentRows = contentCmd.ExecuteNonQuery();
                    Debug.Log("Query executed, rows affected : " + Convert.ToString(rows));
                    if (contentRows > 0)
                    {
                        return;
                    }
                    throw new ApplicationException("Failed to delete level");
                }
                catch (Exception e)
                {
                    throw new ApplicationException("Failed to delete level", e);
                }
            }
        }
        catch (Exception e)
        {
            throw new ApplicationException("Failed to delete level", e);
        }
    }

    public void UpdateAttempts(int attempts, int successful)
    {
        var query = @"Update platf_levels
                    set attempts = attempts + @attempts,
                    successful = successful + @successful
                    where id = @level_id";
        var cmd = new MySqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@attempts", attempts);
        cmd.Parameters.AddWithValue("@successful", successful);
        cmd.Parameters.AddWithValue("@level_id", CurrentLevel.id);
        try
        {
            var rows = cmd.ExecuteNonQuery();
            if (rows > 0)
            {
                var ratingsquery = @"insert into platf_ratings (level_id, user_id, attempts, successful)
                                    values (@level_id, @user_id, @attempts, @successful)
                                    on duplicate key update
                                    attempts = attempts + @attempts,
                                    successful = successful + @successful";
                var ratingsCmd = new MySqlCommand(ratingsquery, connection);
                ratingsCmd.Parameters.AddWithValue("@level_id", CurrentLevel.id);
                ratingsCmd.Parameters.AddWithValue("@user_id", CurrentUser.uid);
                ratingsCmd.Parameters.AddWithValue("@attempts", attempts);
                ratingsCmd.Parameters.AddWithValue("@successful", successful);
                try
                {
                    var ratingRows = ratingsCmd.ExecuteNonQuery();
                    if (ratingRows > 0)
                    {
                        return;
                    }
                    throw new ApplicationException("Failed to update level attempts");
                }
                catch (Exception e)
                {
                    throw new ApplicationException("Failed to update level attempts", e);
                }
            }
            throw new ApplicationException("Failed to update level attempts");
        }
        catch (Exception e)
        {
            throw new ApplicationException("Failed to update level attempts", e);
        }
    }

    public void UpdateRatings(double rating)
    {
        var query = @"insert into platf_ratings (level_id, user_id, rating)
                    values (@level_id, @user_id, @rating)
                    on duplicate key update
                    rating = @rating";
        var cmd = new MySqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@level_id", CurrentLevel.id);
        cmd.Parameters.AddWithValue("@user_id", CurrentUser.uid);
        cmd.Parameters.AddWithValue("@rating", rating);
        try
        {
            var rows = cmd.ExecuteNonQuery();
            if (rows > 0)
            {
                var generalQuery = @"update platf_levels
                                    set rating = (select avg(rating) from platf_ratings where level_id = @level_id)
                                    where id = @level_id";
                var generalCmd = new MySqlCommand(generalQuery, connection);
                generalCmd.Parameters.AddWithValue("@level_id", CurrentLevel.id);
                try
                {
                    var genRows = generalCmd.ExecuteNonQuery();
                    if (genRows > 0)
                    {
                        return;
                    }
                    throw new ApplicationException("Failed to update ratings");
                }
                catch (Exception e)
                {
                    throw new ApplicationException("Failed to update ratings", e);
                }
            }
            throw new ApplicationException("Failed to update ratings");
        }
        catch (Exception e)
        {
            throw new ApplicationException("Failed to update ratings", e);
        }
    }


}

