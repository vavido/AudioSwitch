using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AudioSwitch {

    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        private bool ignoreChange = false;

        private readonly TcpConnection connectionL;
        private readonly TcpConnection connectionR;

        public MainWindow() {
            InitializeComponent();

            StatusText.Text = "Versuche zu verbinden";
            Mouse.OverrideCursor = Cursors.Wait;
            EnableUi(false);

            connectionL = new TcpConnection(TcpConnection.LeftSwitch);
            connectionR = new TcpConnection(TcpConnection.RightSwitch);

            connectionL.ConnectionEstablished += OnConnectionEstablished;
            connectionL.ConnectFailed += OnConnectFailed;
            connectionL.ConnectionLost += OnConnectionLost;

            connectionR.ConnectionEstablished += OnConnectionEstablished;
            connectionR.ConnectFailed += OnConnectFailed;
            connectionR.ConnectionLost += OnConnectionLost;

            connectionL.Connect();
            connectionR.Connect();
        }

        private void OnConnectionLost(object sender, EventArgs eventArgs) {
            Dispatcher.Invoke(() => {
                UpdateStatusText();
                EnableUi(connectionL.IsConnected && connectionR.IsConnected);
            });
        }

        private void OnConnectFailed(object sender, EventArgs eventArgs) {
            Dispatcher.Invoke(() => {
                StatusText.Text =
                    $"{((sender as TcpConnection) == connectionL ? "links: " : "rechts: ")}Verbindung konnte nicht hergestellt werden";
                Mouse.OverrideCursor = null;
            });
        }

        private void OnConnectionEstablished(object sender, EventArgs eventArgs) {
            Dispatcher.Invoke(() => {
                UpdateStatusText();
                SyncState();
                EnableUi(connectionL.IsConnected && connectionR.IsConnected);
                if (connectionL.IsConnected && connectionR.IsConnected) {
                    Mouse.OverrideCursor = null;
                }
            });
        }

        private void UpdateStatusText() {
            StatusText.Text =
                $"links: {(connectionL.IsConnected ? "" : "nicht ")}verbunden; rechts: {(connectionR.IsConnected ? "" : "nicht ")}verbunden";
        }

        private void SyncState() {
            ignoreChange = true;
            if (connectionL.IsConnected) {
                if (connectionL.GetPinState(TcpConnection.P1)) {
                    R1B.IsChecked = true;
                } else {
                    R1A.IsChecked = true;
                }
                if (connectionL.GetPinState(TcpConnection.P2)) {
                    R2B.IsChecked = true;
                } else {
                    R2A.IsChecked = true;
                }
                if (connectionL.GetPinState(TcpConnection.P3)) {
                    R3B.IsChecked = true;
                } else {
                    R3A.IsChecked = true;
                }
            }
            if (connectionR.IsConnected) {
                connectionR.SetPinState(TcpConnection.P1,
                    R1A.IsChecked == true ? TcpConnection.Low : TcpConnection.High);
                connectionR.SetPinState(TcpConnection.P2,
                    R2A.IsChecked == true ? TcpConnection.Low : TcpConnection.High);
                connectionR.SetPinState(TcpConnection.P3,
                    R3A.IsChecked == true ? TcpConnection.Low : TcpConnection.High);
            }
            ignoreChange = false;
        }

        private void EnableUi(bool enable) {
            R1A.IsEnabled = enable;
            R1B.IsEnabled = enable;
            R2A.IsEnabled = enable;
            R2B.IsEnabled = enable;
            R3A.IsEnabled = enable;
            R3B.IsEnabled = enable;
        }

        private void OnCheck(object sender, RoutedEventArgs e) {
            if (!(sender is RadioButton button) || ignoreChange) return;

            var state = ((string) button.Content).Equals("A") ? TcpConnection.Low : TcpConnection.High;

            if (!CheckBoxCoupling.IsChecked == true) {
                var pin = TcpConnection.P1;
                switch (button.GroupName) {
                    case "Relais1":
                        pin = TcpConnection.P1;
                        break;
                    case "Relais2":
                        pin = TcpConnection.P2;
                        break;
                    case "Relais3":
                        pin = TcpConnection.P3;
                        break;
                }

                connectionL.SetPinState(pin, state);
                connectionR.SetPinState(pin, state);
            } else {
                ignoreChange = true;
                if (state == TcpConnection.Low) {
                    R1A.IsChecked = true;
                    R2A.IsChecked = true;
                    R3A.IsChecked = true;
                } else {
                    R1B.IsChecked = true;
                    R2B.IsChecked = true;
                    R3B.IsChecked = true;
                }

                connectionL.SetPinState(TcpConnection.P1, state);
                connectionR.SetPinState(TcpConnection.P1, state);
                connectionL.SetPinState(TcpConnection.P2, state);
                connectionR.SetPinState(TcpConnection.P2, state);
                connectionL.SetPinState(TcpConnection.P3, state);
                connectionR.SetPinState(TcpConnection.P3, state);
                ignoreChange = false;
            }
        }

        private void ReconnectButton_Click(object sender, RoutedEventArgs e) {
            Mouse.OverrideCursor = Cursors.Wait;
            if (connectionL.IsConnected) {
                connectionL.Close();
            }
            if (connectionR.IsConnected) {
                connectionR.Close();
            }
            connectionR.Connect();
            connectionL.Connect();
        }

    }

}