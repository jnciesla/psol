﻿#pragma warning disable CS0436 // Type conflicts with imported type
using System;
using Bindings;
using System.Collections.Generic;
using System.Xml;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using PSol.Data.Models;
using static Bindings.ServerPackets;

namespace PSol.Client
{
    internal class ClientData
    {
        private ClientTCP ctcp;
        private delegate void Packet_(byte[] data);
        private static Dictionary<int, Packet_> packets;


        public void InitializeMessages()
        {
            ctcp = new ClientTCP();
            packets = new Dictionary<int, Packet_>
            {
                {(int) SMessage, HandleMessage},
                {(int) SPlayerData, DownloadData},
                {(int) SAckRegister, GoodRegister},
                {(int) SPulse, HandleServerPulse},
                {(int) SFullData, GetStaticPulse},
                {(int) SGalaxy, HandleGalaxy },
                {(int) SItems, HandleItems },
                {(int) SInventory, HandleInventory }
            };
        }

        public void HandleNetworkMessages(byte[] data)
        {
            PacketBuffer buffer = new PacketBuffer();

            buffer.AddBytes(data);
            var packetNum = buffer.GetInteger();
            buffer.Dispose();

            if (packets.TryGetValue(packetNum, out Packet_ Packet))
                Packet.Invoke(data);
        }

        private void HandleMessage(byte[] data)
        {
            Color color;
            PacketBuffer buffer = new PacketBuffer();
            buffer.AddBytes(data);
            buffer.GetInteger();
            var colorCode = buffer.GetInteger();
            switch (colorCode)
            {
                case (int)MessageColors.Warning:
                    color = Color.DarkRed;
                    break;
                case (int)MessageColors.Notification:
                    color = Color.DarkGoldenrod;
                    break;
                case (int)MessageColors.Minor:
                    color = Color.DarkOliveGreen;
                    break;
                default:
                    color = Color.DarkGray;
                    break;
            }

            InterfaceGUI.AddChats(buffer.GetString(), color);
        }

        private void GoodRegister(byte[] data)
        {
            PacketBuffer buffer = new PacketBuffer();
            buffer.AddBytes(data);
            buffer.GetInteger();
            GameLogic.PlayerIndex = buffer.GetInteger(); // Index on server side
            buffer.Dispose();
            ctcp.SendLogin();
            MenuManager.Clear(2);
            InterfaceGUI.AddChats("Registration successful.", Color.DarkOliveGreen);
        }

        private void DownloadData(byte[] data)
        {
            PacketBuffer buffer = new PacketBuffer();
            buffer.AddBytes(data);
            buffer.GetInteger();
            GameLogic.PlayerIndex = buffer.GetInteger(); // Index on server side
            var i = GameLogic.PlayerIndex;
            Types.Player[i] = new User
            {
                Id = buffer.GetString(),
                Name = buffer.GetString(),
                X = buffer.GetFloat(),
                Y = buffer.GetFloat(),
                Rotation = buffer.GetFloat(),
                Health = buffer.GetInteger(),
                MaxHealth = buffer.GetInteger(),
                Shield = buffer.GetInteger(),
                MaxShield = buffer.GetInteger(),
                Inventory = buffer.GetList<Inventory>()
            };
            buffer.Dispose();
            MenuManager.Clear(1);
            InterfaceGUI.AddChats("User data downloaded.", Color.DarkOliveGreen);

        }

        private void GetStaticPulse(byte[] data)
        {
            InterfaceGUI.AddChats("Existing connections downloaded.", Color.DarkOliveGreen);
            // Someone new connected so this is all the data we don't need updating every 100ms
            PacketBuffer buffer = new PacketBuffer();
            buffer.AddBytes(data);
            buffer.GetInteger();
            for (var i = 1; i != Constants.MAX_PLAYERS; i++)
            {
                Types.Player[i].Name = buffer.GetString();
            }
            buffer.Dispose();
        }

        private void HandleServerPulse(byte[] data)
        {
            var buffer = new PacketBuffer();
            buffer.AddBytes(data);
            buffer.GetInteger(); // Packet Type
            Globals.serverTime = BitConverter.ToInt64(buffer.GetBytes(8), 0);
            for (var i = 1; i != Constants.MAX_PLAYERS; i++)
            {
                var Id = buffer.GetString();
                var X = buffer.GetFloat();
                var Y = buffer.GetFloat();
                var Rotation = buffer.GetFloat();
                var Health = buffer.GetInteger();
                var MaxHealth = buffer.GetInteger();
                var Shield = buffer.GetInteger();
                var MaxShield = buffer.GetInteger();
                var inGame = BitConverter.ToBoolean(buffer.GetBytes(1), 0);
                // If the buffer is not ourselves, skip the update - need to do not in game characters to remove logged out users
                if (i == GameLogic.PlayerIndex) continue;
                Types.Player[i].Id = Id;
                Types.Player[i].X = X;
                Types.Player[i].Y = Y;
                Types.Player[i].Rotation = Rotation;
                Types.Player[i].Health = Health;
                Types.Player[i].MaxHealth = MaxHealth;
                Types.Player[i].Shield = Shield;
                Types.Player[i].MaxShield = MaxShield;
            }

            GameLogic.LocalMobs = buffer.GetList<Mob>();
            GameLogic.LocalCombat = buffer.GetList<Combat>();
            GameLogic.LocalLoot = buffer.GetList<Inventory>();
            GameLogic.WatchCombat();
            buffer.Dispose();
        }

        private void HandleInventory(byte[] data)
        {
            var buffer = new PacketBuffer();
            buffer.AddBytes(data);
            buffer.GetInteger();
            Types.Player[GameLogic.PlayerIndex].Inventory = buffer.GetList<Inventory>();
            Globals.newInventory = true;
            buffer.Dispose();
        }

        private void HandleGalaxy(byte[] data)
        {
            InterfaceGUI.AddChats("Galaxy downloaded.", Color.DarkOliveGreen);
            PacketBuffer buffer = new PacketBuffer();
            buffer.AddBytes(data);
            buffer.GetInteger();
            GameLogic.Galaxy = buffer.GetList<Star>();
            GameLogic.collectPlanets();
            InterfaceGUI.PopulateMap();
            buffer.Dispose();
        }

        private void HandleItems(byte[] data)
        {
            InterfaceGUI.AddChats("Item dictionary downloaded.", Color.DarkOliveGreen);
            PacketBuffer buffer = new PacketBuffer();
            buffer.AddBytes(data);
            buffer.GetInteger();
            GameLogic.Items = buffer.GetList<Item>();
            buffer.Dispose();
        }

        //
        // XML DATA
        //
        private static XmlDocument XML = new XmlDocument();
        private const string Root = "PSOL";
        private const string Filename = "settings.xml";

        public static void NewXMLDoc()
        {
            XmlTextWriter xmlTextWriter = new XmlTextWriter(Filename, Encoding.UTF8);
            //Write a blank XML doc

            var xml = xmlTextWriter;
            {
                xml.WriteStartDocument(true);
                xml.WriteStartElement(Root);
                xml.WriteEndElement();
                xml.WriteEndDocument();
                xml.Flush();
                xml.Close();
            }
        }

        public static void WriteToXml(string selection, string name, string value)
        {
            var check = XML.SelectSingleNode(Root + "/" + selection);
            if (check == null)
            {
                XML.DocumentElement?.AppendChild(XML.CreateElement(selection));
            }

            var xmlNode = XML.SelectSingleNode(Root + "/" + selection + "/Element[@Name='" + name + "']");
            if (xmlNode == null)
            {
                var element = XML.CreateElement("Element");
                element.SetAttribute("Value", value);
                element.SetAttribute("Name", name);
                XML.DocumentElement?[selection]?.AppendChild(element);
            }
            else
            {
                //Update node
                if (xmlNode.Attributes == null) return;
                xmlNode.Attributes["Value"].Value = value;
                xmlNode.Attributes["Name"].Value = name;
            }
        }

        public static void LoadXml()
        {
            if (!File.Exists(Filename))
            {
                NewXMLDoc();
            }
            XML.Load(Filename);
        }

        public static string ReadFromXml(string selection, string name, string defaultValue = "")
        {
            var xmlNode = XML.SelectSingleNode(Root + "/" + selection + "/Element[@Name='" + name + "']");
            if (xmlNode?.Attributes != null)
                return xmlNode.Attributes["Value"].Value;
            WriteToXml(selection, name, defaultValue);
            return defaultValue;
        }

        public static void CloseXml(bool save)
        {
            if (save)
                XML.Save(Filename);
            XML = null;
        }
    }
}