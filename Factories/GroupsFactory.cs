using System.Collections.Generic;
using System;
using System.Linq;
using Dapper;
using System.Data;
using MySql.Data.MySqlClient;
using group.Models;
using CryptoHelper;

namespace GroupApp.Factory
{
    public class GroupRepository : IFactory<Group>
    {
        private string connectionString;
        public GroupRepository()
        {
            connectionString = "server=localhost;userid=root;password=root;port=8889;database=groups;SslMode=None";
        }

        internal IDbConnection Connection
        {
            get {
                return new MySqlConnection(connectionString);
            }
        }
         public void AddGroup(Group group_item)
        {
             using (IDbConnection dbConnection = Connection)
            {
                dbConnection.Open();
                dbConnection.Execute("INSERT IGNORE INTO groups (group_name, description, members, created_at, updated_at, user_id) VALUES (@group_name, @description,  @members, NOW(), NOW(), @user_id)", group_item);
            }
        }
        public Group Group_Last_ID()
        {
            using (IDbConnection dbConnection = Connection)
            {
                dbConnection.Open();
                return dbConnection.Query<Group>("SELECT * FROM groups ORDER BY id DESC LIMIT 1").FirstOrDefault();
            }
        }
        public void Add_Joiner(int num1, int num2)
        {
            using (IDbConnection dbConnection = Connection)
            {
                dbConnection.Open();
                dbConnection.Execute($"INSERT INTO joiners (group_id, user_id) VALUES ('{num1}', '{num2}')");
            }
        }
        public IEnumerable<Group> AllGroups()
        {
            using (IDbConnection dbConnection = Connection) 
            {
                dbConnection.Open();
                return dbConnection.Query<Group>($"SELECT users.first_name, groups.user_id, groups.id, members, description, group_name from groups,users WHERE groups.user_id = users.id ");
            }
        }
        public void DeleteGroup(string num)
        {
            using (IDbConnection dbConnection = Connection)
            {
                dbConnection.Open();
                dbConnection.Execute($"DELETE FROM groups WHERE id = {num}");
            }
        }
        public Group Show_Info(string id)
        {
            using (IDbConnection dbConnection = Connection)
            {
                dbConnection.Open();
                var test = dbConnection.Query<Group>($"SELECT groups.description, groups.id, groups.user_id, groups.group_name, groups.members, users.first_name, users.last_name FROM groups, users WHERE groups.user_id = users.id AND groups.id ='{id}';").FirstOrDefault();
                return test;
            }
        }
        public void Join_Group(string group_id, int user_id)
        {
            using (IDbConnection dbConnection = Connection)
            {
                dbConnection.Open();
                dbConnection.Execute($"INSERT INTO joiners (group_id, user_id) VALUES ('{group_id}', '{user_id}')");
                dbConnection.Execute($"UPDATE groups SET members = members + 1 WHERE id ='{group_id}';");
            }
        }
         public IEnumerable<Group>  others(string id)
        {
            using (IDbConnection dbConnection = Connection)
            {
                dbConnection.Open();
                return dbConnection.Query<Group>($"SELECT users.first_name, users.id from groups JOIN joiners ON groups.id = joiners.group_id JOIN users ON users.id = joiners.user_id WHERE groups.id = '{id}' AND users.id NOT IN (SELECT users.id FROM groups LEFT JOIN users ON users.id = groups.user_id where groups.id = '{id}');");
            }
        }
        public void Leave_Group(string group_id, int user_id)
        {
            using (IDbConnection dbConnection = Connection
            {
                dbConnection.Open();
                dbConnection.Execute($"DELETE FROM joiners WHERE joiners.user_id = '{user_id}' AND joiners.group_id = '{group_id}'");
                Console.WriteLine("worked");
                dbConnection.Execute($"UPDATE groups SET members = members - 1 WHERE id ='{group_id}';");
            }
        }
        public int  Switch(string id, int num)
        {
            using (IDbConnection dbConnection = Connection)
            {
                dbConnection.Open();
                var temp = dbConnection.Query<Joiners>($"SELECT COUNT(distinct joiners.user_id) as count FROM joiners WHERE group_id = '{id}' and user_id = '{num}'").SingleOrDefault();
                return temp.count;
            }
        }

    }
}
