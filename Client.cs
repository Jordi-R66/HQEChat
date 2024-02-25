using System.Net;
using System.Net.Sockets;
using System.Text;

/*
 * Fonctionnalités du Client :
 *	- Envoi de message : implémenté
 *  - Envoi de messages privés : implémenté
 *  - Réception de messages : implémenté
 *	- Connexion au serveur : implémenté
 *	- Renseignement de l'username : implémenté
 *	- Support des commandes : à faire
 *	- Threading : à faire
 */

namespace HQEChat {
	class ClientCommands {
		public static readonly string quit = "quit";

		public static readonly string[] commands = [quit];
	}

	class Client {
		readonly IPEndPoint ClientEndPoint;
		private bool ClientStopped = false;

		readonly string Username;
		readonly IPAddress TargetIpObj;
		Socket? client;

		public Client(string ip, ushort port, string username) {
			this.TargetIpObj = IPAddress.Parse(ip);

			this.ClientEndPoint = new(this.TargetIpObj, port);
			this.Username = username;
		}

		internal bool SendMessage(string Message) {
			bool HasBeenSentAndBeenReceived = false;

			if (client != null) {
				Message = $"{Constantes.som_sequence}{Message}{Constantes.eom_sequence}";
				byte[] MessageBytes = Encoding.Unicode.GetBytes(Message);
				_ = client.Send(MessageBytes, SocketFlags.None);

				byte[] buffer = new byte[16];
				Int32 received = client.Receive(buffer, SocketFlags.None);
				string response = Encoding.Unicode.GetString(buffer, 0, received);

				HasBeenSentAndBeenReceived = response.Contains(Constantes.ack_sequence);

			}
			return HasBeenSentAndBeenReceived;
		}

		internal string ReceiveMessage() {
			string message = "";

			if (client != null) {
				byte[] buffer = new byte[1024];
				Int32 received = client.Receive(buffer, SocketFlags.None);
				message = Encoding.Unicode.GetString(buffer, 0, received);
			}

			return message;
		}

		internal bool StopClient(bool warn = true) {
			if (client != null) {

				bool canLeave = (warn) ? SendMessage(Constantes.eoc_sequence) : false;

				if (canLeave) {
					client.Disconnect(false);
					client.Dispose();
					client.Close();
					ClientStopped = true;
				}
			}

			return false;
		}

		internal void InterpretCommands(string message) {
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
						StopClient(true);
					}
				}
			}
		}

		internal void MessageReceiver() {
			string lastMessage = "";
			while (true) {
				lastMessage = ReceiveMessage();
				Console.WriteLine(lastMessage);
			}
		}

		internal void MessageSender() {
			while (true) {
				Console.Write("Votre message : ");
				string? message = Console.ReadLine();

				if (!(String.IsNullOrEmpty(message) || String.IsNullOrWhiteSpace(message))) {
					SendMessage(message);
				}
			}
		}

		public bool Run() {
			client = new(ClientEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			client.Connect(ClientEndPoint);

			bool UsernameSent = SendMessage(this.Username);

			if (!UsernameSent) {
				StopClient(false);
				return false;
			}

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
					}
				} else {
					Console.WriteLine("Saisie invalide");
				}
			}
		}
	}
}
