using System;
using System.Data;
using System.Data.SQLite;

namespace FamAlbum
{

    public class ConnectionManager
    {
        private string _connectionString;

        public ConnectionManager(string connectionString)
        {
            _connectionString = connectionString;
        }
        public string GetConnString()
        {
            return _connectionString;
        }
        //public SQLiteConnection GetConnection()
        //{
        //    var connection = new SQLiteConnection(_connectionString);
        //    connection.Open();
        //    return connection;
        //}

        public SQLiteConnection GetConnection()
        {
            int attempts = 0;
            while (attempts < 3)
            {
                try
                {
                    var connection = new SQLiteConnection(_connectionString);
                    connection.Open();
                    return connection;
                }
                catch (Exception ex)
                {
                    attempts++;
                    System.Threading.Thread.Sleep(200); // brief pause
                    if (attempts == 3)
                        throw new Exception($"Failed to open SQLite connection after 3 attempts: {ex.Message}", ex);
                }
            }
            return null; // unreachable, but required
        }
    }
    }