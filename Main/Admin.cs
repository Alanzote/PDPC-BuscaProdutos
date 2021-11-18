using PDPC_BuscaProdutos.Generic;
using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;

namespace PDPC_BuscaProdutos.Main {

    /**
     * The Admin.
     * I can see your logs...
     */
    public static class Admin {

        // The Tcp Client.
        private static TcpClient Tcp;

        // The Main Method for the Admin.
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

            // Loop until...
            while (true) {
                // Clear Screen.
                Console.Clear();

                // Create Packet for Refresh.
                BinaryWriter RefreshPacket = Messaging.CreatePacket();

                // Write that this is a refresh packet.
                RefreshPacket.Write(false);

                // Send a Packet.
                Messaging.SendPacket(Tcp, RefreshPacket);

                // Wait for Response.
                while (Tcp.Available <= 0) { }

                // Receive Packet.
                BinaryReader Packet = Messaging.ReceivePacket(Tcp);

                // Make sure packet is from Server.
                if (!Packet.ReadBoolean()) {
                    // Close Packet.
                    Packet.Close();

                    // Continue...
                    continue;
                }

                // Get Wait Time.
                double WaitTime = Packet.ReadDouble();

                // Read Queries.
                var CurrentQueries = Query.CreateMultiple(Packet).OrderByDescending(x => x.Time).ToList();

                // Close Packet.
                Packet.Close();

                // Write Wait Time.
                Console.WriteLine($"Current Wait Time (Tmax): {WaitTime} ms.");

                // Write Header.
                Console.WriteLine("|------------------------------|-----------------------|");
                Console.WriteLine("| Query                        | Horário               |");
                Console.WriteLine("|------------------------------|-----------------------|");

                // Loop all Queries.
                foreach (Query Current in CurrentQueries) {
                    // Write Query Info.
                    Console.WriteLine($"| {Current.Value.PadRight(28)} | {Current.Time} |");
                    Console.WriteLine("|------------------------------|-----------------------|");
                }

                // Write Availiable Commands.
                Console.WriteLine("Available Commands: set wait time (value).");
                Console.WriteLine("Any other Commands will just result in refresh.");

                // Write Request.
                Console.Write("Command: ");

                // Receive Input.
                string Input = Console.ReadLine();

                // Check for Command.
                if (Input.StartsWith("set wait time")) {
                    // Split the Set Wait Time command and get the last value.
                    string Splt = Input.Split(' ').Last();

                    // Try to cast as double.
                    if (double.TryParse(Splt, out double Time)) {
                        // Create Packet.
                        BinaryWriter WaitTimePacket = Messaging.CreatePacket();

                        // Write that this is a Wait Time Change.
                        WaitTimePacket.Write(true);

                        // Write new Time.
                        WaitTimePacket.Write(Time);

                        // Send it.
                        Messaging.SendPacket(Tcp, WaitTimePacket);
                    }
                }
            }
        }
    }
}
