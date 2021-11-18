using PDPC_BuscaProdutos.Generic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;

namespace PDPC_BuscaProdutos.Main {

    /**
     * The Client.
     * Query this, Query that, Query everything you want.
     */
    public static class Client {

        // The Tcp Client.
        private static TcpClient Tcp;

        // The Main Method for the Client.
        public static void Main(string[] args) {
            // Try...
            try {
                // Create Client.
                Tcp = new TcpClient("localhost", Messaging.TcpPort);
            } catch (Exception Ex) {
                // Connection failure... notify...
                Console.WriteLine($"Connection failed: {Ex.Message}.");

                // Return.
                return;
            }

            // Current Results Being Shown.
            List<ProductInfo> CurrentResults = null;

            // The Previous Query.
            Query PreviousQuery = new Query(string.Empty);

            // Send Initial Query.
            BinaryWriter Writer = Messaging.CreatePacket();

            // Write the Query.
            PreviousQuery.Serialize(Writer);

            // Send it.
            Messaging.SendPacket(Tcp, Writer);

            // While true...
            while (true) {
                // Clear Console.
                Console.Clear();

                // Check for Previous Query, write it...
                if (PreviousQuery != null) {
                    // Write it.
                    Console.WriteLine($"Query: '{PreviousQuery.Value}' at {PreviousQuery.Time}, waiting for data...");

                    // Wait for response.
                    while (Tcp.Available <= 0) { }

                    // Response Received.
                    BinaryReader Packet = Messaging.ReceivePacket(Tcp);

                    // Make sure packet came from server.
                    if (!Packet.ReadBoolean()) {
                        // Close Packet.
                        Packet.Close();

                        // Continue...
                        continue;
                    }

                    // Read Results.
                    CurrentResults = ProductInfo.CreateMultiple(Packet);

                    // Close Packet.
                    Packet.Close();
                }

                // Check for Results.
                if (CurrentResults != null && CurrentResults.Count > 0) {

                    // Write Header.
                    Console.WriteLine("|---------------------------------|------------------|-----------|");
                    Console.WriteLine("| Nome                            | Loja             |  Preço    |");
                    Console.WriteLine("|---------------------------------|------------------|-----------|");

                    // For Each Result.
                    foreach (ProductInfo Result in CurrentResults) {
                        // Write Data.
                        Console.WriteLine($"| {Result.Name.PadRight(31)} | {Result.Vendor.PadRight(16)} | {Result.Price:C} |");

                        // Write End of Product.
                        Console.WriteLine("|---------------------------------|------------------|-----------|");
                    }

                } else {
                    // Nullify Results.
                    CurrentResults = null;

                    // Notify.
                    Console.WriteLine("Query returned no results.");
                }

                // Request Query.
                Console.Write("New Query: ");

                // Read.
                PreviousQuery = new Query(Console.ReadLine());

                // Create Packet.
                Writer = Messaging.CreatePacket();

                // Write the Query.
                PreviousQuery.Serialize(Writer);

                // Send it.
                Messaging.SendPacket(Tcp, Writer);
            }
        }
    }
}
