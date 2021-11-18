using System;
using System.Collections.Generic;
using System.IO;

namespace PDPC_BuscaProdutos.Generic {

    /**
     * The Query.
     * Requests a query.
     */
    public class Query {

        // Properties.
        public string Value { get; protected set; }

        // Query Time.
        public DateTime Time { get; protected set; }

        // Creates with a String.
        public Query(string Query) {
            // Set Data.
            Value = Query;
            Time = DateTime.Now;
        }

        // Constructs from a Packet.
        public Query(BinaryReader Reader) {
            // Read Data.
            Value = Reader.ReadString();
            Time = DateTime.FromBinary(Reader.ReadInt64());
        }

        // Constructs Multiple from a Packet.
        public static List<Query> CreateMultiple(BinaryReader Reader) {
            // Read Length.
            int Length = Reader.ReadInt32();

            // Create Results.
            List<Query> Results = new List<Query>();

            // Loop...
            for (int i = 0; i < Length; i++)
                Results.Add(new Query(Reader));

            // Return Results.
            return Results;
        }

        // Serializes the Query.
        public void Serialize(BinaryWriter Writer) {
            // Write Data.
            Writer.Write(Value);
            Writer.Write(Time.ToBinary());
        }

        // Serializes Multiple.
        public static void SerializeMultiple(List<Query> Queries, BinaryWriter Writer) {
            // Write Length.
            Writer.Write(Queries.Count);

            // For Each Query.
            foreach (Query Q in Queries)
                Q.Serialize(Writer);
        }
    }
}
