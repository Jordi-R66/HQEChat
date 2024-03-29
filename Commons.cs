﻿using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace HQEChat {
	static public class Constantes {
		static public readonly ushort PORT = 11_107;
		static public readonly string som_sequence = "<|SOM|>"; // START OF MESSAGE
		static public readonly string eom_sequence = "<|EOM|>"; // END OF MESSAGE
		static public readonly string ack_sequence = "<|ACK|>"; // ACKNOWLEDGEMENT
		static public readonly string cmd_sequence = "<|CMD|>"; // COMMAND
		static public readonly string prv_sequence = "<|PRV|>"; // PRIVATE MESSAGE
		static public readonly string eoc_sequence = "<|EOC|>"; // END OF CONNECTION

		static public readonly byte[] ackBytes = Encoding.Unicode.GetBytes(ack_sequence);

		static public readonly string[] sequences = [som_sequence, eom_sequence, ack_sequence, cmd_sequence, prv_sequence, eoc_sequence];
	}

	static public class Fonctions_Utiles {
		static public List<string> GetAvailableIPs() {
			List<string> AvailableIPs = new List<string>();

			NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

			foreach (NetworkInterface networkInterface in networkInterfaces) {
				// Ignore les interfaces de bouclage et les interfaces non-connectées
				if (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Loopback || 
					networkInterface.OperationalStatus == OperationalStatus.Down) {
					continue;
				}

				IPInterfaceProperties ipProps = networkInterface.GetIPProperties();

				foreach (UnicastIPAddressInformation IP in ipProps.UnicastAddresses) {
					if (IP.Address.AddressFamily == AddressFamily.InterNetwork) {
						AvailableIPs.Add(IP.Address.ToString());
					}
				}
			}

			return AvailableIPs;
		}

		static public string GetLocalIPAddress() {
			string hostname = Dns.GetHostName();

			string OutputIP = "";

			IPAddress[] Adresses = Dns.GetHostAddresses(hostname);
			int compteur = 0;
			foreach (IPAddress IP in Adresses) {
				if (IP.AddressFamily == AddressFamily.InterNetwork) {
					OutputIP = IP.ToString();
					compteur++;
				}
			}
			return compteur == 1 ? OutputIP : "";
		}

		static public string RemoveSequencesFromMessage(string input_string) {
			foreach (string seq in Constantes.sequences) {
				if (input_string.Contains(seq)) {
					input_string = input_string.Replace(seq, "");
				}
			}

			return input_string;
		}
	}
}