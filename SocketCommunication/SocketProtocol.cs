using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using SocketCommunication;

namespace SocketCommunication
{
    internal class SocketProtocol
    {
        //Hello :)
        #region Variables
        // ToDo: Rename variable correctly, using "_" 
        //public static bool _connection;
        public static TcpClient _client = null;
        public static TcpListener _listener = null;
        public static StreamReader STR;
        public static StreamWriter STW;
        //public static TextBox tbTempMessage= new TextBox();
        //public static string _currentState;
        //public static string _previousState;

        //public static string _receive;
        //public static string _TextToSend;

        //public const string disconnectStr = "****";

        //Delete ???
        public string ipAddressStr;
        public int portNum;

        #endregion

        #region Constructor
        public SocketProtocol() 
        {
            // ADD CODE
            // The Global variables should be established by here, NULL if needed
            //_client = null;
            //_listener = null;
            STW = null;
        }
        #endregion

        #region Properties

        public TcpClient sp_client
        {
            get 
            { 
                return _client; 
            }
            set 
            { 
                _client = value;
            }
        }

        public TcpListener sp_listener
        {
            get
            {
                return _listener;
            }
            set
            {
                _listener = value;
            }
        }

        public string IpaddressFunc
        {
            get
            {
                return ipAddressStr;
            }
            set
            {
                ipAddressStr = value;
            }
        }
        #endregion

        #region Start Listener and Client
        public static void StartServer(string ipAddress, string port)
        {
            try
            {
                _listener = new TcpListener(IPAddress.Any, Convert.ToInt32(port));  //_listener = new TcpListener(ipAddress, Convert.ToInt32(port));
                _listener.Start();

                //------
                _client = _listener.AcceptTcpClient();
                //_client = _listener.AcceptTcpClientAsync();
                //_client = _listener.Pending();
                //_listener.AcceptTcpClient();  TEST THIS WHEN POSSIBLE 
                //------

                STR = new StreamReader(_client.GetStream());
                STW = new StreamWriter(_client.GetStream());
                STW.AutoFlush = true;
            }
            catch (Exception ex)
            {
                //_listener.Stop();   //Use dispose or remove
                MessageBox.Show("There was an error when attempting to start the Server", "Connection Error");
            }
        }

        public static void StartClient(string ipAddress, string port)
        {
            try
            {
                _client = new TcpClient();
                IPEndPoint IpEnd = new IPEndPoint(IPAddress.Parse(ipAddress), Convert.ToInt32(port));
                _client.Connect(IpEnd);
                STW = new StreamWriter(_client.GetStream());
                STR = new StreamReader(_client.GetStream());
                STW.AutoFlush = true;
            }
            catch (Exception ex)
            {
                _client.Close();    //Use dispose or remove
                MessageBox.Show("There was an error when attempting to start the Client", "Connection Error");
            }
        }
        #endregion

        #region Check TCP-Connection
        // ConnectionStatus between the Listener and Client
        public static bool ConnectionStatus()
        {
            return _client.Connected ? true : false;
        }
        #endregion

        #region Disconnect TCP Server/Client
        public static void DisconnectServer(bool serverHardDisconnect)
        {

            try
            {
                // Are we resetting the Server-Side Application, if not just reset the Background-Workers and Listener
                if(serverHardDisconnect == true)
                {
                    // Restart logic
                    Application.Restart();
                    Application.Exit();
                }

                //_connection = false;
                _client.Close();

            }
            catch(Exception ex) 
            {
                MessageBox.Show("Server - Nope");
            }
        }

        public static void DisconnectClient()
        {
            try
            {
                //_connection = false;
                _client.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Client - Nope");
            }
        }
        #endregion
    }
}
