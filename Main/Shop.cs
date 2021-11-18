using PDPC_BuscaProdutos.Generic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace PDPC_BuscaProdutos.Main {

    /**
     * The Shop.
     * I have this, that, and don't forget this other expensive thing.
     */
    public static class Shop {

        // The Database.
        private static Database Data;

        // The Udp.
        private static UdpClient Udp;

        // The Main.
        public static void Main(string[] args) {
            // Check for Arguments.
            if (args.Length != 3)
                throw new ArgumentException("Shop requires a path for the Database csv file and the Vendor Identifier and finally a shop identifier.");

            // Create Database.
            Data = new Database(args[0], args[1]);

            // Create Udp.
            Udp = new UdpClient(Messaging.UdpStartPort + int.Parse(args[2]) + 1);
            Udp.JoinMulticastGroup(Messaging.UdpGroup);

            // We wait for queries...
            while (true) {
                // Receive Udp Packet.
                BinaryReader ReceivedPacket = Messaging.ReceivePacket(Udp);

                // Ignore if packet is null or if not server.
                if (ReceivedPacket == null || !ReceivedPacket.ReadBoolean())
                    continue;

                // Read Query.
                Query NewQuery = new Query(ReceivedPacket);

                // Close Packet.
                ReceivedPacket.Close();

                // Notify.
                Console.WriteLine($"Received Query with Value: '{NewQuery.Value}'");

                // Get Results.
                List<ProductInfo> Results = Data.ExecuteQuery(NewQuery);

                // Notify.
                Console.WriteLine($"Found {Results.Count} results, sending data...");

                // Create new Packet to Send.
                BinaryWriter PacketToSend = Messaging.CreatePacket();

                // Serialize Multiple Product Infos.
                ProductInfo.SerializeMultiple(Results, PacketToSend);

                // Send it.
                Messaging.SendPacket(Udp, PacketToSend);
            }
        }
    }
}
