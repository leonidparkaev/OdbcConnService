using OdbcConnector;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Configuration;
using Microsoft.Extensions.Logging;
using System.ServiceProcess;
using System.Collections.Generic;
using System.Linq;
using System;

namespace OdbcConnService
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }

        protected override async void OnStart(string[] args)
        {
            string DSN;
            int Port;
            DSN = ConfigurationManager.AppSettings.Get("DSN");
            Port = Int32.Parse(ConfigurationManager.AppSettings.Get("Port"));

            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Any, Port);
            Socket tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            OdbcConn OdbcConnector = new OdbcConn();

            try
            {
                tcpSocket.Bind(ipPoint);
                tcpSocket.Listen(1);
                //Console.WriteLine("Сервер запущен\n");
                WinLogger.cWinLogger.Logger.LogInformation("Сервер запущен\n");

                while (true)
                {
                    var tcpClient = await tcpSocket.AcceptAsync();

                    //byte[] response = new byte[0];
                    List<byte> responseL = new List<byte>();
                    byte[] buffer = new byte[1024];
                    int bytes = 0;

                    bytes = tcpClient.Receive(buffer);
                    responseL.AddRange(buffer.Take(bytes));
                    //OdbcConn.Insert(ref response, buffer, response.Length);
                    var responseArray = responseL.ToArray();
                    var responseString = Encoding.UTF8.GetString(responseArray);
                    //Console.WriteLine($"От клиента {tcpClient.RemoteEndPoint} получены данные:\n{responseString}\n");
                    WinLogger.cWinLogger.Logger.LogInformation($"От клиента {tcpClient.RemoteEndPoint} получены данные:\n{responseString}");

                    string Json = OdbcConnector.GetDataInJsonString(DSN, responseString);
                    byte[] data = Encoding.UTF8.GetBytes(Json);
                    tcpClient.Send(data);
                    //Console.WriteLine($"Клиенту {tcpClient.RemoteEndPoint} отправлены данные.\n");
                    WinLogger.cWinLogger.Logger.LogInformation($"Клиенту {tcpClient.RemoteEndPoint} отправлены данные");
                }
            }
            catch (Exception ex)
            {
                byte[] error = Encoding.UTF8.GetBytes(ex.Message);
                var tcpClient = await tcpSocket.AcceptAsync();
                tcpClient.Send(error);
                //Console.WriteLine($"Ошибка отправки данных клиенту {tcpClient.RemoteEndPoint}:\n{ex.Message}\n");
                WinLogger.cWinLogger.Logger.LogError($"Ошибка отправки данных клиенту {tcpClient.RemoteEndPoint}:\n{ex.Message}");
            }
        }

        protected override void OnStop()
        {
            WinLogger.cWinLogger.Logger.LogInformation("Остановка службы\n");
        }
    }
}
