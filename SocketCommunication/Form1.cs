using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Linq.Expressions;
using System.Data.Common;
using SocketCommunication;
using System.Net.Security;
using System.Timers;

namespace SocketCommunication
{
    public partial class Dashboard : Form
    {
        #region Variables
        public const string disconnectStr = "****";

        // ToDo: Rename variable correctly, using "_" 
        private bool _connection;
        private string _currentState;
        private string _previousState;
        public static string? _Receive;
        public static string? _TextToSend;

        private System.Timers.Timer heartBeatTimer;
        #endregion


        #region Constructor
        public Dashboard()
        {
            InitializeComponent();

            _currentState = "Server";
            _previousState = "Client";

            // Establishes an IPAddress for the application
            IPAddress[] ipAddresses = Dns.GetHostAddresses(Dns.GetHostName());

            foreach (IPAddress ipAddress in ipAddresses)
            {
                if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                {
                    tbAddress.Text = ipAddress.ToString();
                }
            }
        }
        #endregion

        #region Properties
        public static string? SendMessageBuffer
        {
            get
            {
                return _TextToSend;
            }
            set
            {
                _TextToSend = value;
            }
        }

        public static string? ReceiveMessageBuffer
        {
            get
            {
                return _Receive;
            }
            set
            {
                _Receive = value;
            }
        }
        #endregion


        private void btnClear_Click(object sender, EventArgs e)
        {
            tbChatArea.Text = string.Empty;
        }

        private void tglButton_CheckedChanged(object sender, EventArgs e)
        {
            if(lblSectionheader.Text == "Server")
            {
                _currentState = "Client";
                _previousState = "Server";
                lblSectionheader.Text = _currentState;
                lblSectionheader.Location = new Point(113, 42);
                btnConnectStart.Text = "Connect";
                btnConnectStart.FillColor = Color.FromArgb(143, 188, 143);
                btnConnectStart.BorderColor = Color.FromArgb(0, 128, 0);
            } 
            else
            {
                _currentState = "Server";
                _previousState = "Client";
                lblSectionheader.Text = _currentState;
                lblSectionheader.Location = new Point(109, 42);
                btnConnectStart.Text = "Start Server";
                btnConnectStart.FillColor = Color.FromArgb(128, 191, 255);
                btnConnectStart.BorderColor = Color.FromArgb(0, 115, 230);
            }
        }

        private void btnConnectStart_Click(object sender, EventArgs e)
        {
            string address = tbAddress.Text;
            string port = tbPort.Text;

            // UI/UX: Check if the ipAddress and port text fields are empty
            if (address != string.Empty && port != string.Empty) 
            {
                // Server-Mode or Client-Mode
                if (_currentState == "Server")    // Server --> Listen
                {
                    try
                    {
                        panelConnection.FillColor = Color.Orange;
                        tbChatArea.AppendText($"{DateTime.Now:hh:mm:ss.fff tt}: Server has begun listening on port #{port}.\r\n");
                        SocketProtocol.StartServer(address, port);

                        if (SocketProtocol.ConnectionStatus())
                        {
                            backgroundWorker1.WorkerSupportsCancellation = true;    // Receiving Message
                            backgroundWorker2.WorkerSupportsCancellation = true;    // Sending Message
                            backgroundWorker1.RunWorkerAsync();
                        }
                        else
                        {
                            panelConnection.FillColor = Color.Red;
                            tbChatArea.AppendText($"{DateTime.Now:hh:mm:ss.fff tt}: [Error] - Server is no longer listening.\r\n");
                        }
                    }
                    catch (Exception ex)
                    {
                        tbChatArea.AppendText($"{DateTime.Now:hh:mm:ss.fff tt}: [Fatal Error] - Connection Failed.\r\n");
                    }
                }
                else                        // Client --> Connect
                {
                    try
                    {
                        panelConnection.FillColor = Color.Orange;
                        tbChatArea.AppendText($"{DateTime.Now:hh:mm:ss.fff tt}: Client attempting to connect to a server on port #{port}.\r\n");
                        SocketProtocol.StartClient(address, port);

                        if (SocketProtocol.ConnectionStatus())
                        {
                            backgroundWorker1.WorkerSupportsCancellation = true;    // Receiving Message
                            backgroundWorker2.WorkerSupportsCancellation = true;    // Sending Message
                            //backgroundWorker3.WorkerSupportsCancellation = true;    // Heartbeat Message
                            backgroundWorker1.RunWorkerAsync();     
                            //backgroundWorker3.RunWorkerAsync();     
                        }
                        else
                        {
                            panelConnection.FillColor = Color.Red;
                            tbChatArea.AppendText($"{DateTime.Now:hh:mm:ss.fff tt}: [Error] - Client is no longer connecting.\r\n");
                        }
                    }
                    catch (Exception ex)
                    {
                        tbChatArea.AppendText($"{DateTime.Now:hh:mm:ss.fff tt}: [Fatal Error] - Connection Failed.\r\n");
                    }
                }

                bool connection = SocketProtocol.ConnectionStatus();

                if (connection == true)
                {
                    btnConnectStart.Enabled = false;
                    _connection = true;

                    // Handles UI/UX Features
                    if (_currentState == "Server")
                    {
                        btnSend.Enabled = true;
                        panelConnection.FillColor = Color.Green;
                        tbChatArea.AppendText($"{DateTime.Now:hh:mm:ss.fff tt}: Client has connected to the Server at port #{port}.\r\n");
                    }
                    else
                    {
                        btnSend.Enabled = true;
                        panelConnection.FillColor = Color.Green;
                        tbChatArea.AppendText($"{DateTime.Now:hh:mm:ss.fff tt}: Client has connected to a Server at port #{port}.\r\n");
                    }
                }
                else
                {
                    _connection = false;
                    btnConnectStart.Enabled = true;
                    MessageBox.Show($"There was an error while connecting the server and client.\r\n[Connection/State] = [{connection}/{_currentState}]", "Connection Error");
                }

                // Responsive UI/UX feature
                tbAddress.BorderColor = Color.Black;
                tbPort.BorderColor = Color.Black;
            }
            else
            {
                // Responsive UI/UX feature
                if (address == string.Empty)
                {
                    tbAddress.BorderColor = Color.Red;
                }
                else
                {
                    tbAddress.BorderColor = Color.Black;
                }

                // Responsive UI/UX feature
                if (port == string.Empty)
                {
                    tbPort.BorderColor = Color.Red;
                }
                else
                {
                    tbPort.BorderColor = Color.Black;
                }
            }
        }

        #region Background Tasks

        // ToDo: We are dealing with old threads here, so when we grab _receive it is an old instance [That is my best thought]
        // Task handler for reading incoming messages
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            bool heartBeatReceived = false;
            bool clientDisconnectToken = false;

            if (SocketProtocol.ConnectionStatus())
            {
                while (SocketProtocol.ConnectionStatus())
                {
                    try
                    {
                        string previousState = _previousState.ToString();
                        var receive = SocketProtocol.STR.ReadLine();

                        // ToDo: Is this needed?
                        if (backgroundWorker1.CancellationPending == true)
                        {
                            MessageBox.Show($"BG1 cancel");
                            e.Cancel= true;
                            break;
                        }

                        // ToDo: Is this needed?
                        if (SocketProtocol.ConnectionStatus() == false)
                        {
                            MessageBox.Show($"{_currentState} BG-Work1 inside connection check error triggered.");
                        }


                        // Check the received message for edge cases.
                        if (receive == null)
                        {
                            //MessageBox.Show($"{_currentState} Empty Received");
                            break;
                        }
                        else if (receive == "HeartBeat")
                        {
                            //MessageBox.Show($"{_currentState} HeartBeat Received");
                            heartBeatReceived = true;
                            break;
                        }
                        else if (receive == disconnectStr)
                        {
                            //MessageBox.Show($"{_currentState} DisconnectStr Received");
                            Invoke(new Action(() =>
                            {
                                tbChatArea.AppendText($"{DateTime.Now:hh:mm:ss.fff tt}: Client has disconnected from the Server.\r\n");
                            }));

                            clientDisconnectToken = true;
                            break;
                        }   // no else needed

                        this.tbChatArea.Invoke(new MethodInvoker(delegate ()
                        {
                            tbChatArea.AppendText($"{DateTime.Now:hh:mm:ss.fff tt}: [{previousState}] {receive}.\r\n");
                        }));

                        // Reset the receive-message variables 
                        receive = "";
                    }
                    catch (Exception ex)
                    {
                        Invoke(new Action(() =>
                        {
                            tbChatArea.AppendText($"{DateTime.Now:hh:mm:ss.fff tt}: BG1-TRY/CATCH {ex}.\r\n");
                        }));
                        break;
                    }
                }
            }
            else
            {
                Invoke(new Action(() =>
                {
                    tbChatArea.AppendText($"{DateTime.Now:hh:mm:ss.fff tt}: BG1-Disconnected.\r\n");
                }));
            }

            if(clientDisconnectToken == true && _currentState == "Server")
            {
                backgroundWorker1.CancelAsync();
                backgroundWorker2.CancelAsync();
                SocketProtocol.STR.Close();
                SocketProtocol.STW.Close();

                Invoke(new Action(() =>
                {
                    SocketProtocol.DisconnectServer(false);  // Server-Soft-Disconnect
                    btnConnectStart.Enabled = true;
                    btnSend.Enabled = false;
                    panelConnection.FillColor = Color.Red;
                    tbChatArea.AppendText($"{DateTime.Now:hh:mm:ss.fff tt}: Server has Soft disconnected.\r\n" +
                        "****************************************************************\r\n");
                }));
            }
        }

        // Handles sending messages on both Server and Client
        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            if (SocketProtocol.ConnectionStatus())
            {
                try
                {
                    string currentState = _currentState.ToString();
                    SocketProtocol.STW.WriteLine(_TextToSend);

                    //ToDo: Is this needed?
                    if (SocketProtocol.ConnectionStatus() == false)
                    {
                        MessageBox.Show($"{_currentState} BG-Work2 inside connection check error triggered.");
                    } 
                    else
                    {
                        //ToDo: Is this needed?
                        if (backgroundWorker2.CancellationPending == true)  // Remove ??
                        {
                            //e.Cancel = true;
                            tbChatArea.AppendText("INSIDE HEREREERE\r\n");
                        }
                        else
                        {
                            this.tbChatArea.Invoke(new MethodInvoker(delegate ()
                            {
                                if(_TextToSend == disconnectStr)
                                {
                                    tbChatArea.AppendText("****************************************************************\r\n");
                                }
                                else
                                {
                                    tbChatArea.AppendText($"{DateTime.Now:hh:mm:ss.fff tt}: [{currentState}] {_TextToSend}\r\n");
                                }
                            }));


                            // Reset timer for heartbeat
                            //if (_currentState == "Client")
                            //{
                            //    heartBeatTimer.Stop();
                            //    heartBeatTimer.Start();
                            //}
                        }
                    }
                }
                catch (Exception ex)
                {
                    Invoke(new Action(() =>
                    {
                        tbChatArea.AppendText($"{DateTime.Now:hh:mm:ss.fff tt}: BG2-TRY/CATCH {ex}\r\n");
                    }));
                }
            }
            else
            {
                Invoke(new Action(() =>
                {
                    tbChatArea.AppendText($"{DateTime.Now:hh:mm:ss.fff tt}: BG2-Disconnected\r\n");
                }));
            }

            backgroundWorker2.CancelAsync();
        }

        #region Heartbeat

        // Handles the Heartbeat 
        private void backgroundWorker3_DoWork(object sender, DoWorkEventArgs e)
        {
            //ToDo: Add timer Start/Stop here, Add timer for client BG2(sending), every message sent restarts heartbeat
            while (SocketProtocol.ConnectionStatus())
            {


                if (_currentState == "Server")
                {
                    // Add code ???
                }
                else
                {
                    string currentState = _currentState.ToString();
                    string heartBeat = "HeartBeat";
                    Thread.Sleep(30000);

                    //heartBeatTimer = new System.Timers.Timer();
                    //heartBeatTimer.AutoReset = true;
                    //heartBeatTimer.Enabled = true;
                    //heartBeatTimer.Start();

                    _TextToSend = heartBeat;
                    backgroundWorker2.RunWorkerAsync();

                    // Start timer
                    // Reset timer when client sends a message
                    // When the client disconnects we stop sending heartbeats
                }
            }
        }
        


        private void startHeartbeatTimer()
        {

        }

        private void endHeartbeatTimer()
        {

        }
        #endregion

        #endregion

        private void btnSend_Click(object sender, EventArgs e)
        {
            string message = tbOutboundMsg.Text;

            if (message != "")
            {
                if (SocketProtocol.ConnectionStatus())
                {
                    _TextToSend = message;
                    backgroundWorker2.RunWorkerAsync();
                }
                else
                {
                    MessageBox.Show("Sending Message Failed.", "Sending Failure");
                }
            }
            else
            {
                tbOutboundMsg.BorderColor = Color.Red;
            }

            tbOutboundMsg.Text = "";
        }

        private void tbChatArea_TextChanged(object sender, EventArgs e)
        {
            if(tbChatArea.Text == "")
            {
                btnClear.Visible = false;
            } 
            else
            {
                btnClear.Visible = true;
            }
            this.Refresh();
        }
        
        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            btnDisconnectFunc();
        }

        // ToDo: Can this be moved into btnDisconnect_Click ???
        private void btnDisconnectFunc()
        {
            if (_currentState == "Server")
            {
                if (MessageBox.Show("Are you sure you want to stop the Server from listening? \tThis will restart the appliction.", "Exiting Server", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                {
                    SocketProtocol.DisconnectServer(true);  // Server-Hard-Disconnect
                    btnConnectStart.Enabled = true;
                    btnSend.Enabled = false;
                    panelConnection.FillColor = Color.Red;
                    tbChatArea.AppendText($"{DateTime.Now:hh:mm:ss.fff tt}: Server has disconnected.\r\n");
                }
            }
            else
            {
                // Send disconnect token to Server
                _TextToSend = disconnectStr;
                backgroundWorker2.RunWorkerAsync();

                Thread.Sleep(1500);
                SocketProtocol.DisconnectClient();

                btnConnectStart.Enabled = true;
                btnSend.Enabled = false;
                panelConnection.FillColor = Color.Red;
                tbChatArea.AppendText($"{DateTime.Now:hh:mm:ss.fff tt}: Client has disconnected.\r\n");
            }
            _connection = false;
        }
    }
}