using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System;
using System.Windows;
using Touchless.Vision.Camera;
using System.Windows.Forms;

namespace VLC
{
    public partial class Webcam
    {
        public Webcam()
        {
            InitializeComponent();

            // Refresh the list of available cameras
            VideoDevicesComboBox.Items.Clear();
            foreach (Camera cam in CameraService.AvailableCameras)
            {
                VideoDevicesComboBox.Items.Add(cam);
            }

            if (VideoDevicesComboBox.Items.Count > 0)
            {
                VideoDevicesComboBox.SelectedIndex = 0;
            }
        }

        private CameraFrameSource _frameSource;
        private static Bitmap _latestFrame;

        #region "ImageDirectory dependency property"
        /// <summary>
        /// Gets or Sets the folder where video snapshot will be saved.
        /// </summary>    
        public string ImageDirectory
        {
            get
            {
                return (string)GetValue(ImageDirectoryProperty);
            }
            set
            {
                SetValue(ImageDirectoryProperty, value);
            }
        }


        public DependencyProperty ImageDirectoryProperty =
            DependencyProperty.Register("ImageDirectory", typeof(string), typeof(Webcam), new PropertyMetadata(null));
        #endregion

        public void StartCapturing()
        {
            // Early return if we've selected the current camera
            if (_frameSource != null && _frameSource.Camera == VideoDevicesComboBox.SelectedItem)
            {
                return;
            }

            StopCapturing();
            try
            {
                Camera c = (Camera)VideoDevicesComboBox.SelectedItem;
                setFrameSource(new CameraFrameSource(c));
                _frameSource.Camera.CaptureWidth = 700;
                _frameSource.Camera.CaptureHeight = 500;
                _frameSource.Camera.Fps = 50;
                _frameSource.NewFrame += OnImageCaptured;

                pictureBoxDisplay.Paint += new PaintEventHandler(drawLatestImage);
                _frameSource.StartFrameCapture();
            }
            catch (Exception ex)
            {
                VideoDevicesComboBox.Text = "Select A Camera";
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        private void drawLatestImage(object sender, PaintEventArgs e)
        {
            if (_latestFrame != null)
            {
                // Draw the latest image from the active camera
                e.Graphics.DrawImage(_latestFrame, 0, 0, _latestFrame.Width, _latestFrame.Height);
            }
        }

        public void OnImageCaptured(Touchless.Vision.Contracts.IFrameSource frameSource, Touchless.Vision.Contracts.Frame frame, double fps)
        {
            _latestFrame = frame.Image;
            pictureBoxDisplay.Invalidate();
        }

        private void setFrameSource(CameraFrameSource cameraFrameSource)
        {
            if (_frameSource == cameraFrameSource)
                return;

            _frameSource = cameraFrameSource;
        }

        /// <summary>
        /// Stops the display of webcam video.
        /// </summary>
        public void StopCapturing()
        {
            // Trash the old camera
            if (_frameSource != null)
            {
                _frameSource.NewFrame -= OnImageCaptured;
                _frameSource.Camera.Dispose();
                setFrameSource(null);
                pictureBoxDisplay.Paint -= new PaintEventHandler(drawLatestImage);
            }
        }

        public string TimeStamp()
        {
            string ts = DateTime.Now.ToString();
            ts = ts.Replace("/", "-");
            ts = ts.Replace(":", "-");
            return ts;
        }

        /// <summary>
        /// Takes a snapshot of an webcam image.
        /// The size of the image will be equal to the size of the control.
        /// </summary>
        public string TakeSnapshot()
        {
            string imgDir = (string)GetValue(ImageDirectoryProperty);

            if (imgDir == string.Empty)
            {
                throw new DirectoryNotFoundException("Image directory has not been specified");
            }
            else if (!Directory.Exists(imgDir))
            {
                throw new DirectoryNotFoundException("The specified image directory does not exist");

            }


            if (_frameSource == null)
                return string.Empty;

            Bitmap current = (Bitmap)_latestFrame.Clone();
            string filePath = Path.Combine(imgDir, "Snapshot " + TimeStamp() + ".jpg");

            current.Save(filePath, ImageFormat.Jpeg);
            return filePath;
        }

        #region "IDisposable Support"
        private bool disposedValue; // To detect redundant calls

        // IDisposable
        protected void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {

                    WinFormsHost.Dispose();
                    WinFormsHost = null;

                    pictureBoxDisplay.Dispose();
                    pictureBoxDisplay = null;

                    _frameSource.Camera.Dispose();
                }
            }
            this.disposedValue = true;
        }

        //This code added by Visual Basic to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

    }
}