namespace STIChat {
	static internal class Program {
		public static void Main() {
			string? choice_entry;
			char? choice = null;

			while (choice == null) {
				Console.Clear();
				Console.WriteLine("S: Créer un serveur\nC: Se connecter à un serveur\n\nEntrez la lettre correspondant à votre choix : ");
				choice_entry = Console.ReadLine();
				if (choice_entry != null) {
					choice = choice_entry[0];
				}
			}

			string? ip;
			Console.Clear();

			if (( choice == 'S' ) || ( choice == 's' )) {
				ip = Fonctions_Utiles.GetLocalIPAddress();

				Console.WriteLine($"Création d'un serveur sur l'ip {ip}...");
				Server NewServer = new(ip, Constantes.PORT);
				Console.WriteLine($"Serveur démarré sur {ip}:{Constantes.PORT}");
				NewServer.Run();

			} else if (( choice == 'C' ) || ( choice == 'c' )) {
				Console.WriteLine("IP du serveur : ");
				string? ip_entry = Console.ReadLine();
				if (!string.IsNullOrEmpty(ip_entry)) {
					ip = ip_entry;
				} else {
					return;
				}

				Console.WriteLine("Port du serveur : ");
				string? port_entry = Console.ReadLine();
				if (string.IsNullOrEmpty(port_entry)) {
					port_entry = "";
				}

				ushort port = UInt16.Parse(port_entry);

				Client NewClient = new Client(ip, port);
				NewClient.Run();

			} else {
				Console.WriteLine($"Erreur '{choice}' n'est pas reconnu");
			}
		}
	}
}