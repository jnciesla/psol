﻿using System;
using Bindings;
using System.Collections.Generic;
using Ninject;
using PSol.Data.Models;
using PSol.Data.Services.Interfaces;

namespace PSol.Server
{
    internal class HandleData
    {
        private delegate void Packet_(int index, byte[] data);
        private static Dictionary<int, Packet_> packets;
        private readonly IUserService _userService;

        public HandleData(IKernel kernel)
        {
            _userService = kernel.Get<IUserService>();
        }

        public void InitializeMessages()
        {
            Console.WriteLine("Initializing Network Packets...");
            packets = new Dictionary<int, Packet_>
            {
                {(int) ClientPackets.CLogin, HandleLogin},
                {(int) ClientPackets.CRegister, HandleRegister},
                {(int) ClientPackets.CPlayerData, RecvPlayer},
                {(int) ClientPackets.CChat, ParseChat}
            };
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            Console.WriteLine("Initializing Network Packets... PASS");
        }

        public void HandleNetworkMessages(int index, byte[] data)
        {
            var buffer = new PacketBuffer();

            buffer.AddBytes(data);
            var packetNum = buffer.GetInteger();
            buffer.Dispose();

            if (packets.TryGetValue(packetNum, out Packet_ Packet))
                Packet.Invoke(index, data);

        }

        private void HandleLogin(int index, byte[] data)
        {
            Console.WriteLine("Received login packet");
            var buffer = new PacketBuffer();
            buffer.AddBytes(data);
            buffer.GetInteger();
            string username = buffer.GetString();
            string password = buffer.GetString();

            if (!_userService.AccountExists(username))
            {
                SendMessage(index, "Username does not exist!", MessageColors.Warning);
                return;
            }

            if (!_userService.PasswordOK(username, password))
            {
                SendMessage(index, "Password incorrect!", MessageColors.Warning);
                return;
            }

            Types.Player[index] = _userService.LoadPlayer(username);
            ServerTCP.tempPlayer[index].inGame = true;
            XFerLoad(index);
            Console.WriteLine(username + @" logged in successfully.");
        }

        private void HandleRegister(int index, byte[] data)
        {
            Console.WriteLine(@"Received register packet");
            PacketBuffer buffer = new PacketBuffer();
            buffer.AddBytes(data);
            buffer.GetInteger();
            string username = buffer.GetString();
            string password = buffer.GetString();
            bool exists = _userService.AccountExists(username);
            if (!exists)
            {
                Types.Player[index] = new User();
                Types.Player[index] = _userService.RegisterUser(username, password);
                AcknowledgeRegister(index);
            }
            else
            {
                SendMessage(index, "That username already exists!", MessageColors.Warning);
            }
        }

        private static void RecvPlayer(int index, byte[] data)
        {
            var buffer = new PacketBuffer();
            buffer.AddBytes(data);
            buffer.GetInteger();
            float posX = buffer.GetFloat();
            float posY = buffer.GetFloat();
            float rot = buffer.GetFloat();
            Types.Player[index].Rotation = rot;
            Types.Player[index].X = posX;
            Types.Player[index].Y = posY;
        }

        public void SendData(int index, byte[] data)
        {
            var buffer = new PacketBuffer();
            buffer.AddBytes(data);
            try
            {
                ServerTCP.Clients[index].Stream
                    .BeginWrite(buffer.ToArray(), 0, buffer.ToArray().Length, null, null);
            }
            catch
            {
                Console.WriteLine(@"Unable to send packet- client disconnected");
            }

            buffer.Dispose();
        }

        public void BroadcastData(byte[] data)
        {
            for (var i = 1; i < Constants.MAX_PLAYERS; i++)
            {
                if (ServerTCP.Clients[i].Socket != null && ServerTCP.tempPlayer[i].inGame)
                {
                    SendData(i, data);
                }
            }
        }

        public void SendMessage(int index, string message, MessageColors color)
        {
            var buffer = new PacketBuffer();
            buffer.AddInteger((int)ServerPackets.SMessage);
            buffer.AddInteger((int)color);
            buffer.AddString(message);
            // Use index -1 to broadcast from server to all players
            if (index != -1)
            {
                SendData(index, buffer.ToArray());
            }
            else
            {
                BroadcastData(buffer.ToArray());
            }

            buffer.Dispose();
        }

        public void AcknowledgeRegister(int index)
        {
            var buffer = new PacketBuffer();
            buffer.AddInteger((int)ServerPackets.SAckRegister);
            buffer.AddInteger(index);
            SendData(index, buffer.ToArray());
            buffer.Dispose();
        }

        public void XFerLoad(int index)
        {
            var buffer = new PacketBuffer();
            buffer.AddInteger((int)ServerPackets.SPlayerData);
            buffer.AddInteger(index);
            buffer.AddString(Types.Player[index].Name);
            buffer.AddFloat(Types.Player[index].X);
            buffer.AddFloat(Types.Player[index].Y);
            buffer.AddFloat(Types.Player[index].Rotation);
            buffer.AddInteger(Types.Player[index].Health);
            buffer.AddInteger(Types.Player[index].MaxHealth);
            buffer.AddInteger(Types.Player[index].Shield);
            buffer.AddInteger(Types.Player[index].MaxShield);
            SendData(index, buffer.ToArray());
            buffer.Dispose();
            SendMessage(-1, Types.Player[index].Name + " has connected.", MessageColors.Notification);
            Globals.FullData = true;
        }

        public void PreparePulseBroadcast()
        {
            var buffer = new PacketBuffer();
            buffer.AddInteger((int)ServerPackets.SPulse);
            buffer.AddBytes(BitConverter.GetBytes(DateTime.UtcNow.ToBinary()));
            for (var i = 1; i < Constants.MAX_PLAYERS; i++)
            {
                buffer.AddFloat(Types.Player[i].X);
                buffer.AddFloat(Types.Player[i].Y);
                buffer.AddFloat(Types.Player[i].Rotation);
                buffer.AddInteger(Types.Player[i].Health);
                buffer.AddInteger(Types.Player[i].MaxHealth);
                buffer.AddInteger(Types.Player[i].Shield);
                buffer.AddInteger(Types.Player[i].MaxShield);
                buffer.AddBytes(BitConverter.GetBytes(ServerTCP.tempPlayer[i].inGame));
            }
            BroadcastData(buffer.ToArray());
            buffer.Dispose();
        }

        public void PrepareStaticBroadcast()
        {
            Globals.FullData = false;
            var buffer = new PacketBuffer();
            buffer.AddInteger((int)ServerPackets.SFullData);
            for (var i = 1; i < Constants.MAX_PLAYERS; i++)
            { 
                buffer.AddString(Types.Player[i].Name ?? ""); // Don't send null
            }
            BroadcastData(buffer.ToArray());
            buffer.Dispose();
        }

        public void ParseChat(int index, byte[] data)
        {
            var buffer = new PacketBuffer();
            buffer.AddBytes(data);
            buffer.GetInteger();
            string str = buffer.GetString();
            if (str.ToLower().StartsWith("/c"))
            {
                
                RelayChat(index, str.Substring(3));
            }
        }

        public void RelayChat(int index, string str)
        {
            var buffer = new PacketBuffer();
            var newString = Types.Player[index].Name + ": " + str;
            buffer.AddInteger((int)ServerPackets.SMessage);
            buffer.AddInteger((int)MessageColors.Chat);
            buffer.AddString(newString);
            BroadcastData(buffer.ToArray());
            buffer.Dispose();
        }

        public void SendGalaxy()
        {
            var buffer = new PacketBuffer();
            buffer.AddInteger((int)ServerPackets.SGalaxy);
            buffer.AddArray(Globals.Galaxy);
            BroadcastData(buffer.ToArray());
            buffer.Dispose();
        }

    }
}