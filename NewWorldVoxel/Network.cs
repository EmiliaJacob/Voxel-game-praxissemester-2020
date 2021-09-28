using System;
using System.Net;
using System.Collections.Generic;
using Lidgren.Network;

namespace NewWorldVoxel
{
    class Network
    {
        private static Network network_;
        private const int CHANNELRANGE = 31;

        private NetPeer peer_;
        private NetPeerConfiguration config_;

        public string HostName;
        public string UserName;

        private readonly List<string>[] channelBuffer_;
        private Dictionary<string, IPAddress> openHosts_;
        private Dictionary<IPAddress, string> clients_;
        
        public bool IsServer;
        public bool ConnectionEstablished;

        public Network()
        {
            channelBuffer_ = new List<string>[CHANNELRANGE];

            for (int channel = 0; channel < CHANNELRANGE; channel++)
                channelBuffer_[channel] = new List<string>();

            openHosts_ = new Dictionary<string, IPAddress>();
            clients_ = new Dictionary<IPAddress, string>();
            IsServer = false;
            ConnectionEstablished = false;
        }

        public static Network GetInstance()
        {
            if (network_ != null)
                return network_;
            else
            {
                network_ = new Network();
                return network_;
            }
        }

        public void CreateLobby(string userName, int playerCount)
        {
            if (peer_ == null)
            {
                if (userName.Length <= 4 || userName == string.Empty)
                    HostName = "default";
                else
                    HostName = userName;

                UserName = HostName;

                if (playerCount < 2)
                    playerCount = 2;

                config_ = new NetPeerConfiguration(AppDomain.CurrentDomain.FriendlyName.Replace(".exe", ""));
                config_.Port = 50001;
                config_.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
                config_.EnableMessageType(NetIncomingMessageType.DiscoveryRequest);
                config_.MaximumConnections = playerCount;

                peer_ = new NetServer(config_);
                peer_.Start();

                string[] ipParts = peer_.Configuration.BroadcastAddress.ToString().Split('.');
                foreach (var item in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
                    if (item.ToString().Contains(ipParts[0]) && item.ToString().Contains(ipParts[1]) &&
                        item.ToString().Contains(ipParts[2]))
                        clients_.Add(item, HostName);

                IsServer = true;
                ConnectionEstablished = true;

                Console.WriteLine("Broadcast address: " + peer_.Configuration.BroadcastAddress);
                Console.WriteLine("Peer started as server");
            }
            else
                Console.WriteLine("Network already started");
        }

        public void JoinLobby(string serverIP, string userName)
        {
            if (peer_ == null)
            {
                UserName = userName;
                config_ = new NetPeerConfiguration(AppDomain.CurrentDomain.FriendlyName.Replace(".exe", ""));

                peer_ = new NetClient(config_);
                peer_.Start();

                ConnectionEstablished = true;

                NetOutgoingMessage hailMessage = peer_.CreateMessage();
                hailMessage.Write(UserName);
                peer_.Connect(host: serverIP, port: 50001, hailMessage: hailMessage);

                Console.WriteLine("Peer started as client");
            }
            else
                Console.WriteLine("Network already running");
        }

        public void SendMessage(string message, int channel)
        {
            if (!ConnectionEstablished) return;

            if (channel >= 0 && channel < CHANNELRANGE)
            {
                NetOutgoingMessage outgoingMessage = peer_.CreateMessage();
                outgoingMessage.Write(message);

                if (peer_.GetType() == typeof(NetServer))
                    ((NetServer) peer_).SendMessage(outgoingMessage, peer_.Connections,
                        NetDeliveryMethod.ReliableOrdered, channel);

                if (peer_.GetType() == typeof(NetClient))
                    ((NetClient) peer_).SendMessage(outgoingMessage, NetDeliveryMethod.ReliableOrdered, channel);

                Console.WriteLine($"Message({message}) was sent on channel: {channel}");
            }
            else
                throw new IndexOutOfRangeException($"Network message channel with id {channel} does not exist.");
        }

        public void ReceiveMessages()
        {
            if (!ConnectionEstablished) return;

            NetIncomingMessage incomingMessage;
            while ((incomingMessage = peer_.ReadMessage()) != null)
            {
                switch (incomingMessage.MessageType)
                {
                    case NetIncomingMessageType.Data:
                        ReceiveData(incomingMessage);
                        break;
                    case NetIncomingMessageType.StatusChanged:
                        ReceiveStatusChanged(incomingMessage);
                        break;
                    case NetIncomingMessageType.ConnectionApproval:
                        ReceiveConnectionApproval(incomingMessage);
                        break;
                    case NetIncomingMessageType.DiscoveryRequest:
                        ReceiveDiscoveryRequest(incomingMessage);
                        break;
                    case NetIncomingMessageType.DiscoveryResponse:
                        ReceiveDiscoveryResponse(incomingMessage);
                        break;


                    case NetIncomingMessageType.DebugMessage:
                    case NetIncomingMessageType.ErrorMessage:
                    case NetIncomingMessageType.WarningMessage:
                    case NetIncomingMessageType.VerboseDebugMessage:
                        Console.WriteLine(incomingMessage.ReadString());
                        break;
                    default:
                        Console.WriteLine($"unhandled message with type: {incomingMessage.MessageType}");
                        break;
                }

                peer_.Recycle(incomingMessage);
            }
        }

        private void ReceiveData(NetIncomingMessage incomingMessage)
        {
            string message = incomingMessage.ReadString();

            switch (incomingMessage.SequenceChannel)
            {
                case (31):

                    if (message == "Clients")
                    {
                        int clientCount = incomingMessage.ReadInt32();
                        clients_.Clear();
                        for (int client = 0; client < clientCount; client++)
                        {
                            string name = incomingMessage.ReadString();
                            string ip = incomingMessage.ReadString();
                            IPAddress ipAddress = IPAddress.Parse(ip);
                            clients_.Add(ipAddress, name);
                        }
                    }

                    break;

                default: // unimplementierte Data - Nachrichten TODO: Nicht einfach weiterleiten, sondern Exception werfen

                    channelBuffer_[incomingMessage.SequenceChannel].Add(message);
                    Console.WriteLine($"message {message} was received on channel {incomingMessage.SequenceChannel}");
                    break;
            }

            if (IsServer) // Host sendet empfangene Nachrichten an Clients weiter
            {
                NetOutgoingMessage outgoingMessage = peer_.CreateMessage();
                outgoingMessage.Write(message);
                List<NetConnection> otherClients = new List<NetConnection>(peer_.Connections);
                otherClients.Remove(incomingMessage.SenderConnection);

                if (otherClients.Count > 0)
                    peer_.SendMessage(outgoingMessage, otherClients, NetDeliveryMethod.ReliableOrdered, incomingMessage.SequenceChannel);
            }
        }

        public long GetOwnId()
        {
            if (!ConnectionEstablished) return -1;

            return peer_.UniqueIdentifier;
        }

        private void ReceiveStatusChanged(NetIncomingMessage incomingMessage)
        {
            switch (incomingMessage.SenderConnection.Status)
            {
                case NetConnectionStatus.Connected:
                    if (!IsServer)
                    {
                        HostName = incomingMessage.SenderConnection.RemoteHailMessage.ReadString();
                        Console.WriteLine($"Connected to Host: {incomingMessage.SenderConnection.RemoteEndPoint.Address} : {HostName}");
                    }
                    else if (peer_.ConnectionsCount > 0)
                        InformClients();

                    break;

                case NetConnectionStatus.Disconnected:
                    if ((NetConnectionStatus) incomingMessage.ReadByte() == NetConnectionStatus.Disconnected)
                    {
                        Console.WriteLine(incomingMessage.ReadString());
                        if (IsServer)
                        {
                            string userName = clients_[incomingMessage.SenderEndPoint.Address];
                            IPAddress address = incomingMessage.SenderEndPoint.Address;

                            clients_.Remove(incomingMessage.SenderEndPoint.Address);

                            Console.WriteLine("Removed: " + userName + " with address: " + address.ToString());


                            if (peer_.ConnectionsCount > 0)
                                InformClients();
                        }
                    }

                    break;
            }
        }

        private void ReceiveConnectionApproval(NetIncomingMessage incomingMessage)
        {
            if (!IsServer) return;

            string userName = incomingMessage.SenderConnection.RemoteHailMessage.ReadString();
            IPAddress ip = incomingMessage.SenderEndPoint.Address;

            if (clients_.ContainsKey(ip) || clients_.ContainsValue(userName))
            {
                incomingMessage.SenderConnection.Deny("This username or IP is already connected");
                return;
            }

            clients_.Add(ip, userName);

            NetOutgoingMessage returnHail = peer_.CreateMessage();
            returnHail.Write(HostName);
            incomingMessage.SenderConnection.Approve(returnHail);
        }

        private void ReceiveDiscoveryRequest(NetIncomingMessage incomingMessage)
        {
            if (!IsServer || Game.GetInstance().GameIsActive || clients_.Count == peer_.Configuration.MaximumConnections) return;

            NetOutgoingMessage outMessage = peer_.CreateMessage();
            outMessage.Write(HostName);
            outMessage.Write(GetClients()[HostName].ToString());

            peer_.SendDiscoveryResponse(outMessage, incomingMessage.SenderEndPoint);
        }

        private void ReceiveDiscoveryResponse(NetIncomingMessage incomingMessage)
        {
            if (IsServer) return;

            string hostName = incomingMessage.ReadString();
            IPAddress hostIP = IPAddress.Parse(incomingMessage.ReadString());

            if (!openHosts_.ContainsKey(hostName))
                openHosts_.Add(hostName, hostIP);
        }

        public void Disconnect()
        {
            if (ConnectionEstablished)
            {
                string reason = peer_.GetType() == typeof(NetServer) ? "Server closed!" : UserName + " disconnected";
                peer_.Shutdown(reason);

                UserName = string.Empty;

                peer_ = null;
                ConnectionEstablished = false;
                IsServer = false;

                for (int channel = 0; channel < CHANNELRANGE; channel++)
                    channelBuffer_[channel].Clear();

                clients_.Clear();
            }
        }

        public Dictionary<string, IPAddress> GetClients()
        {
            Dictionary<string, IPAddress> clientDict = new Dictionary<string, IPAddress>();

            foreach (var client in clients_)
                clientDict.Add(client.Value, client.Key);
            return clientDict;
        }

        private void InformClients()
        {
            NetOutgoingMessage clientUpdate = peer_.CreateMessage();
            clientUpdate.Write("Clients");
            clientUpdate.Write(clients_.Count);
            foreach (var client in clients_)
            {
                clientUpdate.Write(client.Value);
                clientUpdate.Write(client.Key.ToString());
            }

            peer_.SendMessage(clientUpdate, peer_.Connections, NetDeliveryMethod.ReliableOrdered, 31);
        }

        public List<string> GetChannelBuffer(int channel)
        {
            if (channel > CHANNELRANGE - 1 || channel < 0)
                throw new IndexOutOfRangeException("Network message channel with id " + channel +
                                                   " does not exist. Range is from 0-" + (CHANNELRANGE - 1));
            else
                return channelBuffer_[channel];
        }

        public string PopFirstMessage(int channel)
        {
            if (channel > CHANNELRANGE - 1 || channel < 0)
                throw new IndexOutOfRangeException("Network message channel with id " + channel +
                                                   " does not exist. Range is from 0-" + (CHANNELRANGE - 1));
            else
            {
                string message = channelBuffer_[channel][0];
                channelBuffer_[channel].RemoveAt(0);
                return message;
            }
        }

        public void SearchForHosts()
        {
            if (peer_ == null)
            {
                ConnectionEstablished = true;

                config_ = new NetPeerConfiguration(AppDomain.CurrentDomain.FriendlyName.Replace(".exe", ""));
                config_.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);
                peer_ = new NetPeer(config_);
                peer_.Start();

                peer_.DiscoverLocalPeers(50001);

                openHosts_.Clear();
            }
        }

        public void ResetHostSearch()
        {
            if (peer_ != null && peer_.GetType() == typeof(NetPeer))
            {
                ConnectionEstablished = false;
                peer_ = null;
            }
        }

        public void ClearOpenHosts()
        {
            openHosts_.Clear();
        }

        public Dictionary<string, IPAddress> GetOpenHosts() => openHosts_;
    }
}
