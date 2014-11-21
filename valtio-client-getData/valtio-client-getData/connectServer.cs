using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;


namespace valtio_client_getData
{

    public static class connectServer
    {
        public static Socket m_ClientSocket;
        public static byte[] szData;
        
        
        //private BlkIOTrace t = new BlkIOTrace();
        public static byte[] szDataSum = new byte[9648]; //4096+48=4144
        public static int test = 0;
        public static int count = 0;

        
        static void Main(string[] args)
        {
            //receiveData inst_receiveData = new receiveData();

            m_ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse("192.168.101.130"), 8462); //포트 대기 설정
            //"192.168.101.130" "147.46.240.44" "147.47.111.177"


            while (true)
            {
                try
                {
                    m_ClientSocket.Connect(ipep);
                    Console.WriteLine("Connected!");
                    break;
                }
                catch (SocketException er)
                {
                    Console.WriteLine("Unable to connect to server. Press Enter to try again.");
                    //Console.WriteLine(er.ToString());
                    Console.ReadLine();
                    //return;
                }
            }

            // Give preference to server
            byte[] message = Encoding.ASCII.GetBytes("/dev/sda,8700\n");
            m_ClientSocket.Send(message);



            SocketAsyncEventArgs arg = new SocketAsyncEventArgs();
            szData = new byte[10000]; //5000
            arg.SetBuffer(szData, 0, 9600); //4096
            arg.UserToken = m_ClientSocket;
            arg.Completed
                += new EventHandler<SocketAsyncEventArgs>(receiveData.getData);
            m_ClientSocket.ReceiveAsync(arg);
            //Console.WriteLine("test: "+test);

            Console.ReadLine();
        }
    }
}
