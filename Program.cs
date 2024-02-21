namespace HQEChat {
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
				string? ip_entry = null, port_entry = null, username = null;
				while (string.IsNullOrEmpty(ip_entry) || string.IsNullOrWhiteSpace(ip_entry)) {
					Console.Clear();
					Console.WriteLine("IP du serveur : ");
					ip_entry = Console.ReadLine();
				}

				while ((string.IsNullOrEmpty(ip_entry)) || (string.IsNullOrWhiteSpace(ip_entry)) || (port_entry == null)) {
					Console.Clear();
					Console.WriteLine("Port du serveur : ");
					port_entry = Console.ReadLine();
					if ((ip_entry != "" && ip_entry != null) && !(string.IsNullOrWhiteSpace(ip_entry))) {
						if (port_entry == "default") {
							port_entry = "";
							break;
						}
						if (( port_entry != null ) && ( port_entry.Length > 0 )) {
							foreach (char c in port_entry) {
								if (( c <= '0' ) || ( c >= '9' )) {
									port_entry = null;
									break;
								}
							}
						}
					}
				}

				while (string.IsNullOrEmpty(username) || string.IsNullOrWhiteSpace(username)) {
					Console.Clear();
					Console.WriteLine("Nom d'utilisateur : ");
					username = Console.ReadLine();
				}

				ip = ip_entry;
				ushort port = UInt16.Parse(port_entry);

				Client NewClient = new Client(ip, port, username);
				NewClient.Run();

			} else {
				Console.WriteLine($"Erreur '{choice}' n'est pas reconnu");
			}
		}
	}
}