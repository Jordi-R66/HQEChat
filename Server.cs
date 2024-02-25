using System.Net;
using System.Net.Sockets;
using System.Text;

/*
 * Capacités du Serveur :
 *	- Accepter des connexions entrantes : implémenté
 *	- Relais des messages : implémenté
 *	- Messages privés : implémenté
 *	- Interprétation des commandes : à faire
 */

namespace HQEChat {
	public class Server {
		private Int16 RemoteClients = -1;
		public class RemoteClient {
			public string Username;
			public IPAddress? RemoteIP;

			public Socket SocketObj;
			public readonly Int16 ID;

			internal List<Int16> BlockedClients = new List<Int16>(); // IDs des clients bloqués

			public RemoteClient(string username, Socket handler, Int16 id) {
				this.Username = username;
				this.SocketObj = handler;

				this.ID = id;

				if (handler.RemoteEndPoint is IPEndPoint endPoint) {
					this.RemoteIP = endPoint.Address;
				} else {
					this.RemoteIP = null;
				}
			}

			public Int16 GetId() {
				return this.ID;
			}
		}

		readonly IPEndPoint ServerEndPoint;

		readonly IPAddress ServerIpObj;
		Socket? listener;

		//private static List<RemoteClient> ConnectedClients = new List<RemoteClient>();
		private static Dictionary<Int16, RemoteClient> DictConnectedClients = new Dictionary<Int16, RemoteClient>();

		private static readonly object verrou = new object();

		public Server(string ip, ushort port) {
			ServerIpObj = IPAddress.Parse(ip);
			ServerEndPoint = new(ServerIpObj, port);
		}

		public string ReceiveMessage(Socket handler) {
			/* Recevoir un message sans objet client */
			string response = "";
			if (listener != null) {

				byte[] buffer = new byte[1024];

				Int32 received = handler.Receive(buffer, SocketFlags.None);
				response = Encoding.Unicode.GetString(buffer, 0, received);

				if (response.Contains(Constantes.eom_sequence)) {
					handler.Send(Constantes.ackBytes);
					response = response.Replace(Constantes.eom_sequence, "");
					if (response.Contains(Constantes.eoc_sequence)) {
						handler.Shutdown(SocketShutdown.Both);
						handler.Close();
					} else if (response.Contains(Constantes.cmd_sequence)) {
						// Structure d'une commande :
						//  <CMD> cmdId arg
						InterpretCommands(response.Replace(Constantes.cmd_sequence, ""));
						response = "";
					} else if (response.Contains(Constantes.prv_sequence)) {
						// Structure d'un message privé :
						//	<PRV> senderId destId message
						response = response.Replace($"{Constantes.prv_sequence} ", "");
						string[] SplitResponse = response.Split(" ");

						if (SplitResponse.Length > 2) {
							Int16 senderId, destId;

							senderId = Int16.Parse(SplitResponse[0]);
							destId = Int16.Parse(SplitResponse[1]);

							string message = "";

							for (uint i=2; i < SplitResponse.Length; i++) {
								message += $" {SplitResponse[i]}";
							}

							SendPrivate(message, senderId, destId);
							response = "";
						}
					}

					return response;
				}
			}
			return response;
		}

		internal string ReceiveMessage(RemoteClient? remoteClient=null) {
			/* Recevoir un message avec un objet client */
			string message = "";
			if (remoteClient != null) {

				Socket handler = remoteClient.SocketObj;
				message = ReceiveMessage(handler);
			}
			return message;
		}

		private void AcceptNewConnections() {
			// Accepte les nouvelles connexions
			listener = new(ServerEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			listener.Bind(ServerEndPoint);

			listener.Listen(100);

			while (true) {
				Socket handler = listener.Accept();
				if (handler.Connected) {
					lock (verrou) {
						string? username = ReceiveMessage(handler);
						if (!String.IsNullOrEmpty(username)) {
							RemoteClient remoteClient = new RemoteClient(username, handler, ++RemoteClients);
							DictConnectedClients.Add(RemoteClients, remoteClient);
							// L'objet client est ajouté au dictionnaire des clients connectés afin d'intéragir avec dans les autres fonctions
						} else {
							handler.Close();
						}
					}
				}
			}
		}

		void InterpretCommands(string message) {
			// WIP
			return;
		}

		int SendMessage(string message, Socket destHandler) {
			int bytesSent = 0;

			byte[] msg_bytes = Encoding.Unicode.GetBytes(message);
			bytesSent = destHandler.Send(msg_bytes, SocketFlags.None);

			return bytesSent;
		}

		int SendPrivate(string message, Int16 senderId, Int16 destId) {
			int bytesSent = 0;

			if (DictConnectedClients.ContainsKey(senderId) && DictConnectedClients.ContainsKey(destId)) {
				RemoteClient Sender = DictConnectedClients[senderId], Dest = DictConnectedClients[destId];

				string senderUsername = Sender.Username;
				Socket destHandler = Dest.SocketObj;

				message = $"DM de {senderUsername} : {message}";
				bytesSent = SendMessage(message, destHandler);
			}
			return bytesSent;
		}

		private void CommsManager() {
			string? message;

			while (true) {

				foreach (KeyValuePair<Int16, RemoteClient> kvp in DictConnectedClients) {
					Int16 remoteId = kvp.Key; // Clé unique associée à un client connecté
					RemoteClient remoteClient = kvp.Value; // Objet de gestion du client distant

					message = ReceiveMessage(remoteClient);

					if (message != null && message.Length > 0) {

						message = Fonctions_Utiles.RemoveSequencesFromMessage(message);

						if (message.Length > 0) {
							Console.WriteLine($"{remoteClient.Username} : {message}");
							foreach (RemoteClient destClient in DictConnectedClients.Values) {
								if (destClient.ID != remoteId) {
									SendMessage($"{remoteClient.Username} : {message}", destClient.SocketObj);
								}
							}
						}
					}
				}
			}
		}

		public void Run() {
			Thread ListenerThread = new Thread(AcceptNewConnections);
			Thread MessagesManager = new Thread(CommsManager);

			ListenerThread.Start();
			MessagesManager.Start();
		}
	}
}