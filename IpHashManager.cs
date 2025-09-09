using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;
using System.Text;
using Nous.Utils;
namespace Nous
{
    public class IpHashManager
    {
        private readonly string _connectionString;

        public IpHashManager()
        {
            // Mirror api_server.py behavior: require individual env vars, no defaults
            string host = RequireEnv("DB_HOST");
            string user = RequireEnv("DB_USER");
            string password = RequireEnv("DB_PASSWORD");
            string database = RequireEnv("DB_NAME");
            string port = RequireEnv("DB_PORT");

            _connectionString = $"server={host};port={port};user={user};password={password};database={database};";
        }

        private static string RequireEnv(string key)
        {
            var value = Environment.GetEnvironmentVariable(key);
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationException($"Missing required environment variable: {key}");
            return value;
        }

        public string GenerateHash(string ip)
        {
            var currentHour = DateTime.UtcNow.ToString("yyyy-MM-dd-HH");
            var hashInput = $"{ip}-{currentHour}";
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(hashInput);
                var hash = sha256.ComputeHash(bytes);
                var sb = new StringBuilder();
                for (int i = 0; i < 4; i++) 
                    sb.Append(hash[i].ToString("x2"));
                return sb.ToString().ToUpper();
            }
        }

        public void InsertIp(string ip)
        {
            var hashcode = GenerateHash(ip);
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO ip_hash (ip_address, hashcode) VALUES (@ip, @hashcode) ON DUPLICATE KEY UPDATE hashcode=@hashcode";
                    cmd.Parameters.AddWithValue("@ip", ip);
                    cmd.Parameters.AddWithValue("@hashcode", hashcode);
                    cmd.ExecuteNonQuery();
                    Logger.SWrite($"Inserted/Updated IP: {ip} with hashcode: {hashcode}");
                    
                }
            }
        }

        public void InsertIps(IEnumerable<string> ipList)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    foreach (var ip in ipList)
                    {
                        var hashcode = GenerateHash(ip);
                        cmd.CommandText = "INSERT INTO ip_hash (ip_address, hashcode) VALUES (@ip, @hashcode) ON DUPLICATE KEY UPDATE hashcode=@hashcode";
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@ip", ip);
                        cmd.Parameters.AddWithValue("@hashcode", hashcode);
                        cmd.ExecuteNonQuery();
                        Logger.SWrite($"Inserted/Updated IP: {ip} with hashcode: {hashcode}");
                    }
                }
            }
        }

        public void UpdateHashcodes()
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var selectCmd = conn.CreateCommand())
                {
                    selectCmd.CommandText = "SELECT ip_address FROM ip_hash";
                    var ips = new List<string>();
                    using (var reader = selectCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ips.Add(reader.GetString(0));
                        }
                    }
                    using (var updateCmd = conn.CreateCommand())
                    {
                        foreach (var ip in ips)
                        {
                            var newHash = GenerateHash(ip);
                            updateCmd.CommandText = "UPDATE ip_hash SET hashcode=@hashcode WHERE ip_address=@ip";
                            updateCmd.Parameters.Clear();
                            updateCmd.Parameters.AddWithValue("@hashcode", newHash);
                            updateCmd.Parameters.AddWithValue("@ip", ip);
                            updateCmd.ExecuteNonQuery();
                        }
                    }
                }
                Logger.SWrite("All hashcodes updated.");
            }
        }

        public string? GetIpByHashcode(string hashcode)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT ip_address FROM ip_hash WHERE hashcode=@hashcode";
                    cmd.Parameters.AddWithValue("@hashcode", hashcode);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return reader.GetString(0);
                        }
                    }
                }
            }
            return null;
        }
    }
} 