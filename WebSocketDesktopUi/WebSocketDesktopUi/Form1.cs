using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace WebSocketDesktopUi
{
    public partial class Form1 : Form
    {
        private HttpListener httpListener;
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private WebSocket connectedClient;

        public Form1()
        {
            InitializeComponent();
            StartWebSocketServer();
        }

        private async void StartWebSocketServer()
        {
            httpListener = new HttpListener();
            httpListener.Prefixes.Add("http://localhost:8080/");
            httpListener.Start();

            while (!cancellationTokenSource.Token.IsCancellationRequested)
            {
                var context = await httpListener.GetContextAsync();
                if (context.Request.IsWebSocketRequest)
                {
                    var webSocketContext = await context.AcceptWebSocketAsync(null);
                    var webSocket = webSocketContext.WebSocket;
                    HandleWebSocket(webSocket);
                }
                else
                {
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                }
            }
        }

        private async void HandleWebSocket(WebSocket webSocket)
        {
            connectedClient = webSocket;
            var buffer = new byte[1024 * 4];

            try
            {
                while (webSocket.State == WebSocketState.Open)
                {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                        // Call the new method to process and respond to the message
                        txtLog.AppendText("Received: " + message + Environment.NewLine);

                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Invoke((MethodInvoker)delegate
                {
                    txtLog.AppendText("Error: " + ex.Message + Environment.NewLine);
                });
            }
            finally
            {
                if (webSocket != null)
                {
                    webSocket.Dispose();
                }
                connectedClient = null;
            }
        }

        




        private async void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                if (connectedClient != null && connectedClient.State == WebSocketState.Open)
                {
                    string message = txtMessage.Text;
                    if (string.IsNullOrWhiteSpace(message))
                    {
                        MessageBox.Show("Please enter a message to send.");
                        return;
                    }

                    byte[] buffer = Encoding.UTF8.GetBytes(message);

                    await connectedClient.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
                    txtLog.AppendText("Sent: " + message + Environment.NewLine);
                    txtMessage.Clear();
                }
                else
                {
                    MessageBox.Show("No client is connected or the connection is not open.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }



        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            cancellationTokenSource.Cancel();
            httpListener?.Stop();
        }
    }

}
