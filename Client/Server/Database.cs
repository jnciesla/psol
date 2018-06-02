﻿using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Bindings;

namespace Server
{
    class Database
    {
        public bool FileExists(string file_path)
        {
            return File.Exists(file_path);
        }

        public bool AccountExists(string username)
        {
            string filename = "Data/Accounts/" + username + ".save";
            if (FileExists(filename))
            {
                return true;
            } else
            {
                return false;
            }
        }

        public bool PasswordOK(int index, string username, string password)
        {
            string filename = "Data/Accounts/" + username + ".save";
            BinaryFormatter bf = new BinaryFormatter();
            FileStream fs = new FileStream(filename, FileMode.Open);
            Types.Player[index] = (Types.PlayerStruct)bf.Deserialize(fs);
            fs.Close();

            if(Types.Player[index].Password == password)
            {
                return true;
            } else
            {
                return false;
            }
        }

        public void AddAccount(int index, string username, string password)
        {
            ClearPlayer(index);
            Types.Player[index].Login = username;
            Types.Player[index].Password = password;

            SavePlayer(index);
        }

        public void ClearPlayer(int index)
        {
            Types.Player[index].Login = "";
            Types.Player[index].Password = "";
            Types.Player[index].Name = "";
        }

        public void SavePlayer(int index)
        {
            string filename = "Data/Accounts/" + Types.Player[index].Login + ".save";
            BinaryFormatter bf = new BinaryFormatter();
            FileStream fs = new FileStream(filename, FileMode.OpenOrCreate);
            bf.Serialize(fs, Types.Player[index]);
            fs.Close();
        }

        public void LoadPlayer(int index, string name)
        {
            string filename = "Data/Accounts/" + name + ".save";
            BinaryFormatter bf = new BinaryFormatter();
            FileStream fs = new FileStream(filename, FileMode.Open);
            Types.Player[index] = (Types.PlayerStruct)bf.Deserialize(fs);
            fs.Close();
        }
    }
}
