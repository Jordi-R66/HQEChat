using System.Net;
using System.Net.Sockets;
using System.Text;

namespace STIChat {
	public class Server {
		private Int16 RemoteClients = -1;
		internal struct RemoteClient {
			public string Username;
			public IPAddress RemoteIP;

			public Socket SocketObj;
			public readonly Int16 ID;

			public RemoteClient(string username, Socket handler, Int16 id) {
				this.Username = username;
				this.SocketObj = handler;

				this.ID = id;

				if (handler.RemoteEndPoint is IPEndPoint endPoint) {
					this.RemoteIP = endPoint.Address;
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
			if (listener != null) {

				byte[] buffer = new byte[1024];

				Int32 received = handler.Receive(buffer, SocketFlags.None);
				string response = Encoding.Unicode.GetString(buffer, 0, received);

				if (response.Contains(Constantes.eom_sequence)) {
					handler.Send(Constantes.ackBytes);
					if (response.Contains(Constantes.eoc_sequence)) {
						handler.Shutdown(SocketShutdown.Both);
						handler.Close();
					}
					return response.Replace(Constantes.eom_sequence, "");
				}
			}
			return null;
		}

		private void AcceptNewConnections() {
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
						} else {
							handler.Close();
						}
					}
				}
			}
		}

		private void CommsManager() {
			string? message;
			while (true) {
				foreach (KeyValuePair<Int16, RemoteClient> kvp in DictConnectedClients) {
					Int16 remoteId = kvp.Key;
					RemoteClient remoteClient = kvp.Value;
					Socket handler = remoteClient.SocketObj;
					message = ReceiveMessage(handler);
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
		}
	}
}