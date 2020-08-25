using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace ProcessControlService.ResourceLibrary.Machines.Drivers
{
    public class DistinguishDriver
    {
        private static readonly log4net.ILog LOG = log4net.LogManager.GetLogger(typeof(DistinguishDriver));
        private ClientSocketKEYENCE clientSocketInstance;
        private const int RECV_DATA_MAX = 10240;
        //public bool connected = false;
        public bool isconnected()
        {
            if (clientSocketInstance != null && clientSocketInstance.commandSocket != null && clientSocketInstance.commandSocket.Connected && clientSocketInstance.dataSocket != null && clientSocketInstance.dataSocket.Connected)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool Connect2(int CommandPortInput, int DataPortInput, byte[] ip)
        {
            if (Connect(CommandPortInput, DataPortInput, ip))
            {
                return true;
            }
            else
            {
                if (Connect(CommandPortInput, DataPortInput, ip))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        bool Connect(int CommandPortInput, int DataPortInput, byte[] ip)
        {
            //
            // First reader to connect.
            //
            //byte[] ip1 = { 192, 168, 100, 100 };
            clientSocketInstance = new ClientSocketKEYENCE(ip, CommandPortInput, DataPortInput);  // 9003 for command, 9004 for data

            //
            // Second reader to connect.
            //
            //byte[] ip2 = { 192, 168, 100, 101 };
            //clientSocketInstance[readerIndex++] = new ClientSocket(ip2, CommandPort, DataPort);  // 9003 for command, 9004 for data
            //
            // Connect to the command port.
            //
            try
            {
                clientSocketInstance.readerCommandEndPoint.Port = CommandPortInput;
                clientSocketInstance.readerDataEndPoint.Port = DataPortInput;
                //
                // Close the socket if opened.
                //
                if (clientSocketInstance.commandSocket != null)
                {
                    clientSocketInstance.commandSocket.Close();
                }

                //
                // Create a new socket.
                //
                clientSocketInstance.commandSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                LOG.Info(string.Format("KEYENCE驱动命令套接字" + clientSocketInstance.readerCommandEndPoint.ToString() + " Connecting.."));
                clientSocketInstance.commandSocket.Connect(clientSocketInstance.readerCommandEndPoint);
                LOG.Info(string.Format("KEYENCE驱动命令套接字" + clientSocketInstance.readerCommandEndPoint.ToString() + " Connected."));
            }
            catch (ArgumentOutOfRangeException ex)
            {
                //
                // Catch exceptions and show the message.
                //
                LOG.Error(string.Format("KEYENCE驱动命令套接字{0}出错：{1}", clientSocketInstance.readerCommandEndPoint.ToString() + " Failed to connect.", ex.Message));
                clientSocketInstance.commandSocket = null;
                //connected = false;
                return false;
            }
            catch (SocketException ex)
            {
                //
                // Catch exceptions and show the message.
                //
                LOG.Error(string.Format("KEYENCE驱动命令套接字{0}出错：{1}", clientSocketInstance.readerCommandEndPoint.ToString() + " Failed to connect.", ex.Message));
                clientSocketInstance.commandSocket = null;
                //connected = false;
                return false;
            }

            //
            // Connect to the data port.
            //
            try
            {
                //
                // Close the socket if opend.
                //
                if (clientSocketInstance.dataSocket != null)
                {
                    clientSocketInstance.dataSocket.Close();
                }

                //
                // If the same port number is used for command port and data port, unify the sockets and skip a new connection. 
                //
                if (clientSocketInstance.readerCommandEndPoint.Port == clientSocketInstance.readerDataEndPoint.Port)
                {
                    clientSocketInstance.dataSocket = clientSocketInstance.commandSocket;
                }
                else
                {
                    //
                    // Create a new socket.
                    //
                    clientSocketInstance.dataSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    LOG.Info(string.Format("KEYENCE驱动数据套接字" + clientSocketInstance.readerDataEndPoint.ToString() + " Connecting.."));
                    clientSocketInstance.dataSocket.Connect(clientSocketInstance.readerDataEndPoint);
                    LOG.Info(string.Format("KEYENCE驱动数据套接字" + clientSocketInstance.readerDataEndPoint.ToString() + " Connected."));
                }
                //
                // Set 100 milliseconds to receive timeout.
                //
                clientSocketInstance.dataSocket.ReceiveTimeout = 4000;
            }

            catch (SocketException ex)
            {
                LOG.Error(string.Format("KEYENCE驱动数据套接字{0}出错：{1}", clientSocketInstance.readerDataEndPoint.ToString() + " Failed to connect.", ex.Message));
                clientSocketInstance.dataSocket = null;
                //connected = false;
                return false;
            }
            //connected = true;
            return true;
        }

        public bool disconnect()
        {

            //connected = false;
            //
            // Close the command socket.
            //
            if (clientSocketInstance.commandSocket != null)
            {
                clientSocketInstance.commandSocket.Close();
                clientSocketInstance.commandSocket = null;
                LOG.Info(string.Format("KEYENCE驱动命令套接字" + clientSocketInstance.readerCommandEndPoint.ToString() + " Disconnected."));
            }

            //
            // Close the data socket.
            //
            if (clientSocketInstance.dataSocket != null)
            {
                clientSocketInstance.dataSocket.Close();
                clientSocketInstance.dataSocket = null;
                LOG.Info(string.Format("KEYENCE驱动数据套接字" + clientSocketInstance.readerDataEndPoint.ToString() + " Disconnected."));
            }
            return true;
        }


        public void On()
        {
            string lon = "LON\r";   // CR is terminator
            Byte[] command = ASCIIEncoding.ASCII.GetBytes(lon);


            if (clientSocketInstance.commandSocket != null && clientSocketInstance.commandSocket.Connected && clientSocketInstance.dataSocket != null && clientSocketInstance.dataSocket.Connected)
            {
                try
                {
                    if (clientSocketInstance.dataSocket.Available != 0)
                    {
                        Byte[] recvBytes = new Byte[RECV_DATA_MAX];
                        int recvSize = clientSocketInstance.dataSocket.Receive(recvBytes);
                    }//如果已经有数据可供读取，则读一下起到清空的作用。
                    clientSocketInstance.commandSocket.Send(command);
                }
                catch (Exception ex)
                {
                    LOG.Error(string.Format("KEYENCE驱动 ON时出错{0}", ex.Message));
                    disconnect();
                }
            }
            else
            {
                disconnect();
                LOG.Error(string.Format("KEYENCE驱动命令或数据套接字出错" + clientSocketInstance.readerCommandEndPoint.ToString() + " is disconnected."));
            }
        }

        public void Off()
        {
            string loff = "LOFF\r"; // CR is terminator
            Byte[] command = ASCIIEncoding.ASCII.GetBytes(loff);

            if (clientSocketInstance.commandSocket != null && clientSocketInstance.commandSocket.Connected)
            {
                try
                {
                    clientSocketInstance.commandSocket.Send(command);
                }
                catch (Exception ex)
                {
                    LOG.Error(string.Format("KEYENCE驱动 OFF时出错{0}", ex.Message));
                    disconnect();
                }
            }
            else
            {
                disconnect();
                LOG.Error(string.Format("KEYENCE驱动命令套接字出错" + clientSocketInstance.readerCommandEndPoint.ToString() + " is disconnected."));
            }
        }

        public string receive()
        {
            Byte[] recvBytes = new Byte[RECV_DATA_MAX];
            int recvSize = 0;
            string data = "0";
            if (clientSocketInstance.dataSocket != null && clientSocketInstance.dataSocket.Connected)
            {
                try
                {
                    recvSize = clientSocketInstance.dataSocket.Receive(recvBytes);
                    LOG.Info(string.Format("KEYENCE驱动收到字节数" + recvSize));
                }
                 catch (SocketException ex)
                {
                    //
                    // Catch the exception, if cannot receive any data.
                    //
                    recvSize = 0;
                    LOG.Error(string.Format("KEYENCE驱动出错" + ex.Message));
                }
            }
            else
            {
                disconnect();
                LOG.Error(string.Format("KEYENCE驱动数据套接字出错" + clientSocketInstance.readerDataEndPoint.ToString() + " is disconnected."));
            }

            if (recvSize == 0)
            {
                LOG.Info(string.Format("KEYENCE驱动" + clientSocketInstance.readerDataEndPoint.ToString() + " has no data."));
                //disconnect();
            }
            else
            {
                //
                // Show the receive data after converting the receive data to Shift-JIS.
                // Terminating null to handle as string.
                //
                
                //recvBytes[recvSize] = 0;
                //data = Encoding.GetEncoding("Shift_JIS").GetString(recvBytes);  //原来处理代码
                data = Encoding.UTF8.GetString(recvBytes, 0, recvSize);//盟威的字符串处理代码
                //LOG.Info(string.Format("KEYENCE驱动" + clientSocketInstance.readerDataEndPoint.ToString() + "\r\n" + data));
            }
            return data;
        }

    }

    class ClientSocketKEYENCE
    {
        public Socket commandSocket;   // socket for command
        public Socket dataSocket;      // socket for data
        public IPEndPoint readerCommandEndPoint;
        public IPEndPoint readerDataEndPoint;

        public ClientSocketKEYENCE(byte[] ipAddress, int readerCommandPort, int readerDataPort)
        {
            IPAddress readerIpAddress = new IPAddress(ipAddress);
            readerCommandEndPoint = new IPEndPoint(readerIpAddress, readerCommandPort);
            readerDataEndPoint = new IPEndPoint(readerIpAddress, readerDataPort);
            commandSocket = null;
            dataSocket = null;
        }
    }
}
