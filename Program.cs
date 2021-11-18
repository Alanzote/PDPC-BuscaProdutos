using PDPC_BuscaProdutos.Main;

namespace PDPC_BuscaProdutos {

    /**
     * The Program.
     * Full control.
     */
    public class Program {

        // Main.
        static void Main(string[] args) {
#if SERVER
            // Initialize Server.
            Server.Main(args);
#elif CLIENT
            // Initialize Client.
            Client.Main(args);
#elif ADMIN
            // Initialize Admin.
            Admin.Main(args);
#else
            // Initialize Shop.
            Shop.Main(args);
#endif
        }
    }
}
