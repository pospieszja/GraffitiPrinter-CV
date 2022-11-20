using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace GraffitiPrinter.UI.Infrastructure
{
    public class Zebra
    {
        private string _zplString;

        public void Print(string ipAddress, Dictionary<string, string> dict, int type)
        {
            if (type == 1)
            {
                _zplString = File.ReadAllText("config/etykieta10x8.prn");
            }

            if (type == 2)
            {
                _zplString = File.ReadAllText("config/etykieta10x8-zbiorcza.prn");
            }

            foreach (var item in dict)
            {
                _zplString = _zplString.Replace($"[{item.Key}]", item.Value);
            }

            try
            {
                // Open connection
                System.Net.Sockets.TcpClient client = new System.Net.Sockets.TcpClient();
                client.Connect(ipAddress, 9100);

                // Write ZPL String to connection
                System.IO.StreamWriter writer = new System.IO.StreamWriter(client.GetStream());
                writer.Write(_zplString);
                writer.Flush();

                // Close Connection
                writer.Close();
                client.Close();
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}