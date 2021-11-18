using System;
using System.Collections.Generic;
using System.IO;

namespace PDPC_BuscaProdutos.Generic {

    /**
     * The Product Info.
     * Keeps Information of a Specific Product.
     */
    public class ProductInfo {

        // Properties.
        public string Name { get; protected set; }
        public string Vendor { get; protected set; }
        public double Price { get; protected set; }

        // Constructs with a CSV String.
        public ProductInfo(string CSV, string Vendor) {
            // Split CSV.
            string[] Split = CSV.Split(',');

            // Check Length.
            if (Split.Length != 2)
                throw new ArgumentOutOfRangeException(nameof(CSV));

            // Set Data.
            Name = Split[0];
            this.Vendor = Vendor;
            Price = double.Parse(Split[1]);
        }

        // Serializes to a Packet.
        public void Serialize(BinaryWriter Writer) {
            // Write Data.
            Writer.Write(Name);
            Writer.Write(Vendor);
            Writer.Write(Price);
        }

        // Serializes Multiple.
        public static void SerializeMultiple(List<ProductInfo> Products, BinaryWriter Writer) {
            // Write Length.
            Writer.Write(Products.Count);

            // For Each Product, write it.
            foreach (ProductInfo Product in Products)
                Product.Serialize(Writer);
        }

        // Constructs from a Packet.
        public ProductInfo(BinaryReader Reader) {
            // Read Data.
            Name = Reader.ReadString();
            Vendor = Reader.ReadString();
            Price = Reader.ReadDouble();
        }

        // Constructs Multiple.
        public static List<ProductInfo> CreateMultiple(BinaryReader Reader) {
            // Read Length.
            int Length = Reader.ReadInt32();

            // Create List.
            List<ProductInfo> Results = new List<ProductInfo>();

            // For loop... read.
            for (int i = 0; i < Length; i++)
                Results.Add(new ProductInfo(Reader));

            // Return Results.
            return Results;
        }

    }
}
