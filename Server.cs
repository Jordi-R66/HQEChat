using System.Net;
using System.Net.Sockets;
using System.Text;

// Just a little test for branches

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

		public string? ReceiveMessage(Socket handler) {
			/* Recevoir un message sans objet client */
			if (listener != null) {

				byte[] buffer = new byte[1024];

				Int32 received = handler.Receive(buffer, SocketFlags.None);
				string response = Encoding.Unicode.GetString(buffer, 0, received);

				if (response.Contains(Constantes.eom_sequence)) {
					handler.Send(Constantes.ackBytes);
					response = response.Replace(Constantes.eom_sequence, "");
					if (response.Contains(Constantes.eoc_sequence)) {
						handler.Shutdown(SocketShutdown.Both);
						handler.Close();
					} else if (response.Contains(Constantes.cmd_sequence)) {
						InterpretCommands(response.Replace(Constantes.cmd_sequence, ""));
					}

					return response;
				}
			}
			return null;
		}

		internal string? ReceiveMessage(RemoteClient? remoteClient=null) {
			/* Recevoir un message avec un objet client */
			if (remoteClient != null) {

				Socket handler = remoteClient.SocketObj;
				string? message = ReceiveMessage(handler);
			}
			return null;
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
		}

		private void CommsManager() {
			string? message;
			while (true) {
				foreach (KeyValuePair<Int16, RemoteClient> kvp in DictConnectedClients) {
					Int16 remoteId = kvp.Key; // Clé unique associée à un client connecté
					RemoteClient remoteClient = kvp.Value;

					message = ReceiveMessage(remoteClient);

					if (message != null && message.Length > 0) {
						message = Fonctions_Utiles.RemoveSequencesFromMessage(message);
						if (message.Length > 0) {
							Console.WriteLine($" : {message}");
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