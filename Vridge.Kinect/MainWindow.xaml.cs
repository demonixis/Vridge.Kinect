using System.Windows;
using VRE.Vridge.API.Client.Remotes;
using KinectLib.Xbox360;
using KinectLib.XboxOne;
using VRE.Vridge.API.Client.Messages.BasicTypes;
using VRE.Vridge.API.Client.Messages.v3.Controller;
using System.Numerics;
using System.Windows.Controls;

namespace Vridge.Kinect
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Kinect360Manager m_Kinect360Manager;
        private KinectOneManager m_KinectOneManager;
        private VridgeRemote m_VridgeRemote;
        private bool m_Use360Driver;
        private bool m_SendLeftPosition = true;
        private bool m_SendLeftRotation = false;
        private bool m_SendRightPosition = false;
        private bool m_SendRightRotation = false;

        public MainWindow()
        {
            m_VridgeRemote = new VridgeRemote("localhost", "Vridge.Kinect", Capabilities.Controllers | Capabilities.HeadTracking);

            InitializeComponent();

            Closing += (s, e) => Shutdown();
            Closed += (s, e) => Shutdown();

            KinectTypeCombo.SelectedIndex = m_Use360Driver ? 1 : 0;

            SetToggleValue(SendLeftPosition, m_SendLeftPosition);
            SetToggleValue(SendLeftRotation, m_SendLeftRotation);
            SetToggleValue(SendRightPosition, m_SendRightPosition);
            SetToggleValue(SendRightRotation, m_SendRightRotation);
        }

        private void Start()
        {
            Shutdown();

            var connected = false;

            if (m_Use360Driver)
            {
                m_Kinect360Manager = new Kinect360Manager();

                if (m_Kinect360Manager.Start())
                {
                    m_Kinect360Manager.NewTrackingData += OnKinect360Data;
                    connected = true;
                }
            }
            else
            {
                m_KinectOneManager = new KinectOneManager();

                if (m_KinectOneManager.Start())
                {
                    m_KinectOneManager.NewTrackingData += OnKinectOneData;
                    connected = true;
                }
            }

            ConnectionStatus.Text = connected ? "Connected" : "Not Connected";
        }

        private void Shutdown()
        {
            if (m_Kinect360Manager != null)
            {
                m_Kinect360Manager.Stop();
                m_Kinect360Manager = null;
            }

            if (m_KinectOneManager != null)
            {
                m_KinectOneManager.Stop();
                m_KinectOneManager = null;
            }

            ConnectionStatus.Text = "Not Connected";
        }

        private void OnKinectOneData(KinectLib.XboxOne.TrackingData obj)
        {
            SendHeadData(ref obj.HeadPosition);
            SendHandData(true, ref obj.LeftTransform);
            SendHandData(false, ref obj.RightTransform); ;
        }

        private void OnKinect360Data(KinectLib.Xbox360.TrackingData obj)
        {
            SendHeadData(ref obj.HeadPosition);
            SendHandData(true, ref obj.LeftTransform);
            SendHandData(false, ref obj.RightTransform);
        }

        private void SendHeadData(ref float[] data)
        {
            if (m_VridgeRemote.Head == null)
                return;

            m_VridgeRemote.Head.SetPosition(data[0], data[1], data[2]);
        }

        private void SendHandData(bool left, ref float[] data)
        {
            if (m_VridgeRemote.Controller == null)
                return;

            if (left && !m_SendLeftPosition && !m_SendLeftRotation)
                return;

            if (!left && !m_SendRightPosition && !m_SendRightRotation)
                return;

            Vector3? position = null;
            var quat = new Quaternion(0, 0, 0, 0);

            if (left && m_SendLeftPosition || !left && m_SendRightPosition)
                position = new Vector3(data[0], data[1], data[2]);

            if (left && m_SendLeftRotation || !left && m_SendRightRotation)
                quat = new Quaternion(data[3], data[4], data[5], data[6]);

            m_VridgeRemote.Controller.SetControllerState(
                left ? 0 : 1,
                HeadRelation.IsInHeadSpace,
                left ? HandType.Left : HandType.Right,
                quat,
                position,
                0,
                0,
                0,
                false,
                false,
                false,
                false,
                false,
                false);
        }

        private void OnConnectClicked(object sender, RoutedEventArgs e) => Start();
        private void OnShutdownClick(object sender, RoutedEventArgs e) => Shutdown();

        private void KinectTypeCombo_Selected(object sender, RoutedEventArgs e)
        {
            var combo = sender as ComboBox;

            if (combo == null)
                return;

            m_Use360Driver = combo.SelectedIndex == 1;

            if (m_Kinect360Manager != null || m_KinectOneManager != null)
            {
                Shutdown();
                Start();
            }
        }

        private void SendLeftPosition_Click(object sender, RoutedEventArgs e)
        {
            HandleCheckbox(sender, ref m_SendLeftPosition);
        }

        private void SendLeftRotation_Click(object sender, RoutedEventArgs e)
        {
            HandleCheckbox(sender, ref m_SendLeftRotation);
        }

        private void SendRightPosition_Click(object sender, RoutedEventArgs e)
        {
            HandleCheckbox(sender, ref m_SendRightPosition);
        }

        private void SendRightRotation_Click(object sender, RoutedEventArgs e)
        {
            HandleCheckbox(sender, ref m_SendRightRotation);
        }

        private void SetToggleValue(CheckBox box, bool value) => box.IsChecked = value;

        private void HandleCheckbox(object sender, ref bool target)
        {
            var checkbox = (CheckBox)sender;

            if (checkbox.IsChecked.HasValue)
                target = checkbox.IsChecked.Value;
            else
                target = false;
        }
    }
}
