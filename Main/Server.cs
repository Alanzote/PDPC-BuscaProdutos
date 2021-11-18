using PDPC_BuscaProdutos.Generic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace PDPC_BuscaProdutos.Main {

    /**
     * The Server.
     * Receives from Clients, Admin, sends/receives from Shop.
     */
    public static class Server {

        // Server Properties.
        private static TcpListener Tcp;

        // The UDP Broadcaster.
        private static UdpClient Udp;

        // List of Clients.
        private static readonly List<TcpClient> Clients = new();

        // The Queries Database.
        private static readonly Queries Queries = new();
        
        // The Amount of Clients.
        private static int AmountOfClients;

        // Main of the Server.
        public static void Main(string[] args) {
            // Check for Amount of Shops.
            if (args.Length != 1)
                throw new ArgumentException("To open the Server, the amount of Shops needs to be specified.");

            // Set Amount of Clients.
            AmountOfClients = Convert.ToInt32(args[0]);

            // Create Tcp.
            Tcp = TcpListener.Create(Messaging.TcpPort);

            // Create Udp.
            Udp = new UdpClient(Messaging.UdpStartPort);

            // Join Multicast Group.
            Udp.JoinMulticastGroup(Messaging.UdpGroup);

            // Start Tcp.
            Tcp.Start();

            // Loop Forever...
            while (true) {
                // Check for Pending Connections...
                if (Tcp.Pending()) {
                    // Notify.
                    Console.WriteLine("Accepted pending connection...");

                    // Add to Clients.
                    Clients.Add(Tcp.AcceptTcpClient());
                }

                // For Each Client...
                for (int i = Clients.Count - 1; i >= 0; i--) {
                    // Grab Client.
                    var Client = Clients[i];

                    // Check for Client Connection.
                    if (!Client.Connected) {
                        // Remove Client.
                        Clients.RemoveAt(i);

                        // Notify.
                        Console.WriteLine("Client disconnected.");

                        // Continue...
                        continue;
                    }

                    // Try get Packet.
                    var Packet = Messaging.ReceivePacket(Client);

                    // Check for Packet.
                    if (Packet == null) continue;

                    // Create Result Packet.
                    BinaryWriter ResultPacket = Messaging.CreatePacket();

                    // Check for Admin or Client.
                    if (Packet.ReadBoolean()) {
                        // Read if it is a set wait time...
                        if (Packet.ReadBoolean()) {
                            // Set Wait Time.
                            Queries.WaitTime = Packet.ReadDouble();

                            // There is no more Result Packet, close it.
                            ResultPacket.Close();

                            // And Clear it.
                            ResultPacket = null;

                            // Notify Wait Time was set.
                            Console.WriteLine($"Wait Time was Set by Admin. New Value: {Queries.WaitTime} ms.");
                        } else {
                            // Send In the Wait Time.
                            ResultPacket.Write(Queries.WaitTime);

                            // Admin, find results.
                            var Results = Queries.LastQueries();

                            // Write Results.
                            Query.SerializeMultiple(Results, ResultPacket);

                            // Notify.
                            Console.WriteLine("Sent Admin Packet...");
                        }
                    } else {
                        // Client, read single Query.
                        Query ThisQuery = new Query(Packet);

                        // Execute Query.
                        var Results = Queries.ExecuteQuery(Udp, AmountOfClients, ThisQuery);

                        // Write Results.
                        ProductInfo.SerializeMultiple(Results, ResultPacket);

                        // Notify.
                        Console.WriteLine("Sent Client Packet...");
                    }

                    // Close Packet.
                    Packet.Close();

                    // Send it, if valid.
                    if (ResultPacket != null)
                        Messaging.SendPacket(Client, ResultPacket);
                }
            }
        }
    }
}
