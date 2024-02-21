using System.Net;
using System.Net.Sockets;
using System.Text;

namespace STIChat {
	class ClientCommands {
		public static readonly string quit = "quit";

		public static readonly string[] commands = [quit];
	}

	class Client {
		readonly IPEndPoint ClientEndPoint;
		private bool ClientStopped = false;

		readonly IPAddress TargetIpObj;
		Socket? client;

		public Client(string ip, ushort port) {
			TargetIpObj = IPAddress.Parse(ip);

			ClientEndPoint = new(TargetIpObj, port);
		}

		bool SendMessage(string Message) {
			bool HasBeenSent = false;

			if (client != null) {
				Message = $"{Constantes.som_sequence}{Message}{Constantes.eom_sequence}";
				byte[] MessageBytes = Encoding.Unicode.GetBytes(Message);
				_ = client.Send(MessageBytes, SocketFlags.None);

				byte[] buffer = new byte[16];
				Int32 received = client.Receive(buffer, SocketFlags.None);
				string response = Encoding.Unicode.GetString(buffer, 0, received);

				HasBeenSent = response.Contains(Constantes.ack_sequence);

			}
			return HasBeenSent;
		}

		bool StopClient() {
			if (client != null) {
				bool canLeave = SendMessage(Constantes.eoc_sequence);
				if (canLeave) {
					client.Disconnect(false);
					client.Dispose();
					client.Close();
					ClientStopped = true;
				}
			}
			return false;
		}

		void InterpretCommands(string message) {
			if (message.StartsWith('/')) {
				message = message.Replace("/", "");

				string command;

				if (message.Contains(' ')) {
					command = message.Split(" ")[0];
				} else {
					command = message;
				}

				if (ClientCommands.commands.Contains(command)) {
					if (command == ClientCommands.quit) {
						StopClient();
					}
				}
			}
		}

		public bool Run() {
			client = new(ClientEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

			client.Connect(ClientEndPoint);
			while (true) {
				Console.Write("Votre message : ");
				string? message = Console.ReadLine();

				if (!String.IsNullOrEmpty(message)) {
					message = Fonctions_Utiles.RemoveSequencesFromMessage(message);

					InterpretCommands(message);

					if (ClientStopped) {
						return false;
					}

					bool beenSent = SendMessage(message);
					if (beenSent) {
						Console.WriteLine(message);
						Console.WriteLine("Message reçu par le serveur distant");
					} else {
						continue;
					}
				} else {
					Console.WriteLine("Saisie invalide");
				}
			}
		}
	}
}
