using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace PDPC_BuscaProdutos.Generic {

    /**
     * The Messaging Helper Class.
     * Helps Receiving and Sending Data.
     */
    public static class Messaging {

        // The Port to Use.
        public static readonly int TcpPort = 7769;

        // The Ip Endpoint.
        public static readonly IPAddress UdpGroup = IPAddress.Parse("234.5.6.7");

        // The Udp Start Port.
        public static readonly int UdpStartPort = TcpPort + 1;

        // Creates a Packet.
        public static BinaryWriter CreatePacket() {
            // Create Result.
            var Result = new BinaryWriter(new MemoryStream());

#if CLIENT || SHOP
            // Write that we are the Client.
            Result.Write(false);
#elif ADMIN || SERVER
            // Write that we are the Admin.
            Result.Write(true);
#endif

            // Return Result.
            return Result;
        }

        // Converts a Packet from the Binary Writer into a Byte Array.
        public static byte[] ConvertPacket(BinaryWriter Writer) {
            // Reset Stream Position.
            Writer.BaseStream.Seek(0, SeekOrigin.Begin);

            // Get the Buffer.
            byte[] Buffer = new byte[Writer.BaseStream.Length];

            // Read to the Buffer.
            Writer.BaseStream.Read(Buffer, 0, Buffer.Length);

            // Return it.
            return Buffer;
        }

        // Converts a Packet from the Byte Array into a Binary Reader.
        public static BinaryReader ConvertPacket(byte[] Buffer) {
            // Simple, just create it!
            return new BinaryReader(new MemoryStream(Buffer));
        }

        // Sends a Tcp Packet.
        public static void SendPacket(TcpClient Client, BinaryWriter Writer) {
            // Get the Buffer.
            byte[] Buffer = ConvertPacket(Writer);

            // Send.
            Client.GetStream().Write(Buffer, 0, Buffer.Length);

            // Close Writer.
            Writer.Close();
        }

        // Sends a Udp Packet.
        public static void SendPacket(UdpClient Client, byte[] Buffer, int Port) {
            // Send.
            Client.Send(Buffer, Buffer.Length, new IPEndPoint(UdpGroup, Port));
        }

        // Sends a Udp Packet.
        public static void SendPacket(UdpClient Client, BinaryWriter Writer) {
            // Convert to Buffer.
            byte[] Buffer = ConvertPacket(Writer);

            // Call Original Send Packet.
            SendPacket(Client, Buffer, UdpStartPort);

            // Close Writer.
            Writer.Close();
        }

        // Receives a Tcp Packet.
        public static BinaryReader ReceivePacket(TcpClient Client) {
            // Check for Data.
            if (Client.Available <= 0)
                return null;

            // Get Buffer.
            byte[] Buffer = new byte[Client.Available];

            // Read to the Buffer.
            Client.GetStream().Read(Buffer, 0, Buffer.Length);

            // Return a Reader.
            return ConvertPacket(Buffer);
        }

        // Receives a Udp Packet.
        public static BinaryReader ReceivePacket(UdpClient Client) {
            // Check for Data.
            if (Client.Available <= 0)
                return null;

            // Create New Endpoint.
            IPEndPoint ipEndPoint = new IPEndPoint(UdpGroup, UdpStartPort);

            // Get Buffer.
            byte[] Buffer = Client.Receive(ref ipEndPoint);

            // Check for Buffer.
            if (Buffer == null || Buffer.Length == 0)
                return null;

            // Convert the Packet.
            return ConvertPacket(Buffer);
        }
    }
}
