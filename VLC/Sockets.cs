using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;

namespace VLC
{
    public class Sockets
    {
        public static void SendData(string address, int port, string filePath)
        {
            // Create a TcpClient.
            // Note, for this client to work you need to have a TcpServer
            // connected to the same address as specified by the server, port
            // combination.
            using (var client = new TcpClient(address, port))
            {
                using (var stream = client.GetStream())
                {
                    byte[] dataToSend = File.ReadAllBytes(filePath);
                    byte[] dataSize = BitConverter.GetBytes(Convert.ToUInt32(dataToSend.Length));

                    stream.Write(dataSize, 0, 4);
                    var ACK = stream.ReadByte();

                    stream.Write(dataToSend, 0, dataToSend.Length);
                    stream.Flush();

                    // Close everything.
                    stream.Close();
                }

                client.Close();
            }
        }

        public static string ReceiveData(string address, int port, string destinationPath, string timestamp, string extension)
        {
            var filePath = Path.Combine(destinationPath, "Result " + timestamp + extension);

            // Create a TcpClient.
            // Note, for this client to work you need to have a TcpServer 
            // connected to the same address as specified by the server, port
            // combination.
            using (TcpClient client = new TcpClient(address, port))
            {
                using (NetworkStream stream = client.GetStream())
                {
                    var junk = new byte[1];
                    junk[0] = 1;
                    stream.Write(junk, 0, 1);

                    using (Stream fs = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite))
                    {
                        // Buffer for reading data
                        Byte[] bytes = new Byte[1032];
                        int length;

                        while (true)
                        {
                            length = stream.Read(bytes, 0, bytes.Length);
                            Console.Write("Length:" + length);

                            if ((length == 4 && System.Text.Encoding.UTF8.GetString(bytes, 0, 4) == "DONE") ||length == 0)
                            {
                                break;
                            }
                            fs.Write(bytes, 0, length);
                        }

                    }
                    // Close everything.
                    stream.Close();
                }

                client.Close();
            }

            return filePath;
        }
    }
}
