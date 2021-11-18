using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;

namespace PDPC_BuscaProdutos.Generic {

    /**
     * The Queries.
     * Keeps a list of queries and writes them into logs.
     */
    public class Queries {

        // Total Amount of Queries.
        public static readonly int TotalQueries = 25;

        // The Total Wait Time.
        public static double WaitTime = 2000;

        // Query Database.
        private List<Query> Database = new();

        // Stream for the Log File.
        private StreamWriter LogFile;

        // The Log of the Queries.
        private static readonly string LogPath = "./Queries.csv";

        // Constructor.
        public Queries() {
            // If the File Exists, delete it.
            if (File.Exists(LogPath))
                File.Delete(LogPath);

            // Create Stream.
            LogFile = new StreamWriter(LogPath) {
                AutoFlush = true
            };

            // Add header.
            LogFile.WriteLine("DateTime,Query");
        }

        // Gets the Last Queries.
        public List<Query> LastQueries() {
            // Order by Time, take first 25.
            return Database.OrderByDescending(x => x.Time).Take(TotalQueries).ToList();
        }

        // Runs a Query.
        public List<ProductInfo> ExecuteQuery(UdpClient Client, int AmountOfClients, Query Query) {
            // Add to Queries.
            Database.Add(Query);

            // Add Query to Log.
            LogFile.WriteLine($"{Query.Time},{Query.Value}");

            // Filter Queries if bigger than 25. 
            if (Database.Count > TotalQueries)
                Database = Database.OrderByDescending(x => x.Time).Take(TotalQueries).ToList();

            // Create new Packet.
            BinaryWriter SendPacket = Messaging.CreatePacket();

            // Serialize Query.
            Query.Serialize(SendPacket);

            // Get the Buffer to Send.
            byte[] SendBuffer = Messaging.ConvertPacket(SendPacket);

            // Close the Writer.
            SendPacket.Close();

            // For Each Client, send the packet.
            for (int i = 0; i < AmountOfClients; i++)
                Messaging.SendPacket(Client, SendBuffer, Messaging.UdpStartPort + i + 1);

            // The Start Time.
            DateTime TimeStart = DateTime.Now;

            // Create Results.
            List<ProductInfo> Results = new();

            // Loop Forever...
            while (true) {
                // Check for Time.
                TimeSpan Time = DateTime.Now - TimeStart;

                // If we passed the limits, break.
                if (Time.TotalMilliseconds > WaitTime)
                    break;

                // Attempt to Receive a Packet.
                BinaryReader ReceivedPacket = Messaging.ReceivePacket(Client);

                // Ignore if the Received Packet is null or if has incorrect signature.
                if (ReceivedPacket == null || ReceivedPacket.ReadBoolean()) {
                    // Close Packet if Exists.
                    ReceivedPacket?.Close();

                    // Continue.
                    continue;
                }

                // Deserialize Multiple.
                Results.AddRange(ProductInfo.CreateMultiple(ReceivedPacket));

                // Close the Packet.
                ReceivedPacket.Close();
            }

            // Return Results.
            return Results;
        }
    }
}
