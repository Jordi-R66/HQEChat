using System.Linq;

namespace HQEChat {
	static internal class Program {
		internal static string SelectIP() {
			string return_val = Fonctions_Utiles.GetLocalIPAddress();

			bool useLoop = (return_val == "") ? true : false;

			while (useLoop) {
				List<string> IPList = Fonctions_Utiles.GetAvailableIPs();
				Console.Clear();
				for (int i = 0; i < IPList.Count; i++) {
					string ip = IPList[i];
					Console.WriteLine($"[{i}] : {ip}");
				}

				Console.Write("\nIP à utiliser (numéro uniquement): ");
				string? entry = Console.ReadLine();
				if (entry != null && entry.Length != 0) {
					foreach (char c in entry) {
						if ((c < 48) || (c > 57)) {
							entry = entry.Replace(c.ToString(), "");
						}
					}
					Int32 n = Int32.Parse(entry);
					if (n < IPList.Count) {
						return_val = IPList[n];
						break;
					} else {
						return_val = "";
					}
				}
			}
			return return_val;
		}

		public static void Main() {
			string? choice_entry;
			char choice;

			char[] validChoices = ['S', 's', 'C', 's'];

			while (true) {
				Console.Clear();
				Console.WriteLine("S: Créer un serveur\nC: Se connecter à un serveur\n\nEntrez la lettre correspondant à votre choix : ");
				choice_entry = Console.ReadLine();
				if (choice_entry != null) {
					choice = choice_entry[0];
					if (validChoices.Contains(choice)) {
						break;
					}
				}
			}

			string ip;
			Console.Clear();

			ip = SelectIP();

			if (( choice == 'S' ) || ( choice == 's' )) {
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