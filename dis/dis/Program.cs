using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace DIS {
    public class Program {
        private static int port = 8001;
        private static IPEndPoint remoteEndpoint;
        private static UdpClient client;

        static void Main(string[] args) {
            Console.WriteLine("Verbinden mit 127.0.0.1");
            string IP = "127.0.0.1";

            remoteEndpoint = new IPEndPoint(IPAddress.Parse(IP), port);
            client = new UdpClient();
            string msg;
            string orderId;
            long weight;
            int x, y, stationNo;
            
            while(true) {
                Console.WriteLine("\n  Ende             0");
                Console.WriteLine("  Order            1 oo ggg      (o=orderId(num), g=gewicht(to)");
                Console.WriteLine("  Conv.Available   2 n e         (n=1|2|3 e=1|0");
                int selection = In.ReadInt();
                if (selection==0) break;
                switch(selection) {
                    case 1:
                        int orderNo = In.ReadInt();
                        orderId = "H0" + orderNo;
                        weight = In.ReadInt()*1000;
                        msg = String.Format("{0:d3} {1:s4} {2:d6}", 1, orderId, weight);
                        sendMsg(msg);
                        break;
                    case 2:
                        stationNo = In.ReadInt();
                        int on = In.ReadInt();
                        msg = String.Format("{0:d3} {1:d1} {2:d1}", 2, stationNo, on);
                        sendMsg(msg);
                        break;
                    default:
                        Console.WriteLine("ungueltig");
                        break;
                }
            }
        }

         private static void sendMsg(string msg) {
            byte[] data = Encoding.UTF8.GetBytes(msg);
            try {
                client.Send(data, data.Length, remoteEndpoint);
            }
            catch (Exception e) { Console.WriteLine(e.Message); }

        }
   }
}
