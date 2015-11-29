using System;
using System.Windows;
using System.IO;
using System.Windows.Media;

namespace VLC
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string FileName = string.Empty;

        public string ServerIP = string.Empty;

        public int ServerPort = 0;

        public string DestinationPath = string.Empty;

        public MainWindow()
        {
            InitializeComponent();

            // Add any initialization after the InitializeComponent() call.

            IpBox.Text = "127.0.0.1";
            PortBox.Text = "9000";

            // Create directory for saving image files
            const string imagePath = "C:\\WebcamSnapshots";

            const string resultPath = "C:\\VLC-Results";

            if (!Directory.Exists(imagePath))
            {
                Directory.CreateDirectory(imagePath);
            }

            if (!Directory.Exists(resultPath))
            {
                Directory.CreateDirectory(resultPath);
            }

            DestinationPath = resultPath;
            // Set some properties of the Webcam control
            WebcamCtrl.ImageDirectory = imagePath;
        }

        private void StartCaptureButton_Click(object sender, RoutedEventArgs e)
        {
            // Display webcam video
            try
            {
                WebcamCtrl.StartCapturing();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void StopCaptureButton_Click(Object sender, RoutedEventArgs e)
        {
            WebcamCtrl.StopCapturing();
        }

        private void TakeSnapshotButton_Click(Object sender, RoutedEventArgs e)
        {
            // Take snapshot of webcam video
            FileName = WebcamCtrl.TakeSnapshot();
            WebcamCtrl.StopCapturing();
            CamGrid.Visibility = Visibility.Collapsed;
            TransmitGrid.Visibility = Visibility.Visible;
            TransmitImage.Source = new ImageSourceConverter().ConvertFromString(FileName) as ImageSource;
            TransmitImage.Visibility = Visibility.Visible;
        }

        private void TransmitButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(IpBox.Text))
            {
                MessageBox.Show("Server IP Address must be filled out!");
                return;
            }

            if (string.IsNullOrEmpty(PortBox.Text))
            {
                MessageBox.Show("Server Port must be filled out!");
                return;
            }

            int num;
            if (!int.TryParse(PortBox.Text, out num))
            {
                MessageBox.Show("Server Port must be a valid integer!");
                return;
            }

            ServerIP = IpBox.Text;
            ServerPort = num;
            StartGrid.Visibility = Visibility.Collapsed;
            TransmitGrid.Visibility = Visibility.Visible;
        }

        private void ReceiveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(IpBox.Text))
            {
                MessageBox.Show("Server IP Address must be filled out!");
                return;
            }

            if (string.IsNullOrEmpty(PortBox.Text))
            {
                MessageBox.Show("Server Port must be filled out!");
                return;
            }

            int num;
            if (!int.TryParse(PortBox.Text, out num))
            {
                MessageBox.Show("Server Port must be a valid integer!");
                return;
            }

            ServerIP = IpBox.Text;
            ServerPort = num;
            StartGrid.Visibility = Visibility.Collapsed;
            ReceiveGrid.Visibility = Visibility.Visible;
        }

        private void BtnFileOpen_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new System.Windows.Forms.OpenFileDialog();
            var result = fileDialog.ShowDialog();
            switch (result)
            {
                case System.Windows.Forms.DialogResult.OK:
                    var file = fileDialog.FileName;
                    TxtFile.Text = file;
                    TxtFile.ToolTip = file;
                    FileName = file;
                    TransmitImage.Source = new ImageSourceConverter().ConvertFromString(FileName) as ImageSource;
                    TransmitImage.Visibility = Visibility.Visible;
                    break;
                case System.Windows.Forms.DialogResult.Cancel:
                default:
                    TxtFile.Text = null;
                    TxtFile.ToolTip = null;
                    FileName = string.Empty;
                    break;
            }
        }

        private void ActiveWebcamBtn_Click(object sender, RoutedEventArgs e)
        {
            TransmitGrid.Visibility = Visibility.Collapsed;
            CamGrid.Visibility = Visibility.Visible;
        }

        private void StartVLCBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(FileName))
            {
                TransmitErrorText.Content = "Please select a file before transmitting";
                return;
            }

            TransmitErrorText.Content = string.Empty;
            Sockets.SendData(ServerIP, ServerPort, FileName);
        }

        private void StartRecieveBtn_Click(object sender, RoutedEventArgs e)
        {
            FileName = Sockets.ReceiveData(ServerIP, ServerPort, DestinationPath, WebcamCtrl.TimeStamp());
            if (!string.IsNullOrEmpty(FileName))
            {
                ReceiveImage.Source = new ImageSourceConverter().ConvertFromString(FileName) as ImageSource;
                ReceiveImage.Visibility = Visibility.Visible;
            }
        }

        private void ReceiveBackToHome_Click(object sender, RoutedEventArgs e)
        {
            ReceiveGrid.Visibility = Visibility.Collapsed;
            FileName = string.Empty;
            ReceiveImage.Visibility = Visibility.Hidden;
            ReceiveImage.Source = null;
            StartGrid.Visibility = Visibility.Visible;
        }

        private void TransmitBackToHome_Click(object sender, RoutedEventArgs e)
        {
            TransmitGrid.Visibility = Visibility.Collapsed;
            FileName = string.Empty;
            TransmitImage.Visibility = Visibility.Hidden;
            TransmitImage.Source = null;
            StartGrid.Visibility = Visibility.Visible;
        }
    }
}
