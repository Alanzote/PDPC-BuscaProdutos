using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PDPC_BuscaProdutos.Generic {

    /**
     * The Database.
     * CSV madness!
     */
    public class Database {
        
        // The Vendor Identifier.
        public string Vendor { get; set; }

        // Our Database.
        public List<ProductInfo> Data { get; protected set; } = new();

        // Constructor.
        public Database(string FileName, string Vendor) {
            // Check for File.
            if (!File.Exists(FileName))
                throw new FileNotFoundException("Database file not found.", FileName);

            // Read it, skip first line...
            IEnumerable<string> Lines = File.ReadAllLines(FileName).Skip(1);

            // For Each Line, create product and append to list...
            foreach (string Line in Lines)
                Data.Add(new ProductInfo(Line, Vendor));
        }

        // Returns the Products from a Query.
        public List<ProductInfo> ExecuteQuery(Query Query) {
            // Post-process the Query.
            string[] PostProcess = Query.Value.Replace("  ", " ").Replace("-", " ").ToLower().Split(" ");

            // Get Results.
            IEnumerable<ProductInfo> Results = Data;

            // For Each Processed...
            foreach (string Processed in PostProcess)
                Results = Results.Where(x => x.Name.Contains(Processed, StringComparison.InvariantCultureIgnoreCase) || x.Vendor.Contains(Processed, StringComparison.InvariantCultureIgnoreCase));

            // Return Results.
            return Results.Take(Queries.TotalQueries).ToList();
        }
        
    }
}
