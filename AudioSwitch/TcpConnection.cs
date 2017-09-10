using System;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace AudioSwitch {

    internal class TcpConnection {

        private TcpClient client;

        private const byte CmdSet = 0xA0;
        private const byte CmdRead = 0xA1;
        public const byte P1 = 0xB1, P2 = 0xB2, P3 = 0xB3;
        public const byte Low = 0xF0;
        public const byte High = 0xFF;

        public static readonly IPAddress LeftSwitch = new IPAddress(new byte[] {192, 168, 178, 151}),
            RightSwitch = new IPAddress(new byte[] {192, 168, 178, 152});

        public event EventHandler ConnectionEstablished;
        public event EventHandler ConnectFailed;
        public event EventHandler ConnectionLost;

        private NetworkStream stream;
        private readonly IPAddress ip;

        public bool IsConnected => client.Connected;

        public TcpConnection(IPAddress ip) {
            this.ip = ip;
        }

        /// <summary>
        /// (re)establish the connection to the specified ip
        /// </summary>
        /// <returns></returns>
        public void Connect() {
            Task.Run(() => {
                Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-us");
                Debug.WriteLine($"Trying to connect to {ip}");
                client = new TcpClient();
                try {
                    client.Connect(ip, 8888);
                    Debug.WriteLine("Connection successfull");
                    stream = client.GetStream();
                    ConnectionEstablished?.Invoke(this, EventArgs.Empty);
                } catch (SocketException e) {
                    ConnectFailed?.Invoke(this, EventArgs.Empty);
                    Debug.WriteLine($"Couldn't connect because of {e}");
                }
            });
        }

        /// <summary>
        /// Returns the state of the given pin
        /// </summary>
        /// <param name="pinNumber">The pin to check</param>
        /// <returns>true if the pin is set to HIGH, false for LOW</returns>
        public bool GetPinState(byte pinNumber) {
            if (stream == null) ConnectionLost?.Invoke(this, EventArgs.Empty);

            try {
                var data = new byte[] {CmdRead, pinNumber, 0xFF};
                stream?.Write(data, 0, 3);

                var buffer = new byte[]{0};
                stream?.Read(buffer, 0, 1);

                return buffer[0] == High;
            } catch (Exception e) {
                ConnectionLost?.Invoke(this, EventArgs.Empty);
                return false;
            }
        }

        /// <summary>
        /// Send a set command to the connected switch
        /// </summary>
        /// <param name="pinNumber">The pin to set</param>
        /// <param name="state">The state to set the pin to, either high or low</param>
        public void SetPinState(byte pinNumber, byte state) {
            if (stream == null) ConnectionLost?.Invoke(this, EventArgs.Empty);

            try {
                var data = new[] {CmdSet, pinNumber, state};
                stream?.Write(data, 0, 3);
            } catch (Exception e) {
                ConnectionLost?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Close() {
            client.Close();
        }

    }

}