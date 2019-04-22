using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VRE.Vridge.API.Client.Messages.BasicTypes;
using VRE.Vridge.API.Client.Messages.v3.Controller;
using VRE.Vridge.API.Client.Remotes;

namespace Vridge.Kinect
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static KinectSensor m_KinectSensor;
        private static Body[] m_Bodies;
        private static MultiSourceFrameReader m_MultiSourceFrameReader;
        private VridgeRemote m_VridgeRemote;

        public MainWindow()
        {
            m_VridgeRemote = new VridgeRemote("localhost", "Vridge.Kinect", Capabilities.Controllers | Capabilities.HeadTracking);

            InitializeComponent();
        }

        private bool InitilizeKinect()
        {
            if (m_KinectSensor != null)
                Shutdown();

            m_KinectSensor = KinectSensor.GetDefault();

            if (m_KinectSensor != null)
            {
                m_KinectSensor.Open();
                m_MultiSourceFrameReader = m_KinectSensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth | FrameSourceTypes.Infrared | FrameSourceTypes.Body);
                m_MultiSourceFrameReader.MultiSourceFrameArrived += OnSkeletonFrameReady;
            }

            return m_KinectSensor != null;
        }

        private void Shutdown()
        {
            if (m_MultiSourceFrameReader != null)
                m_MultiSourceFrameReader.MultiSourceFrameArrived -= OnSkeletonFrameReady;

            if (m_KinectSensor != null)
            {
                m_KinectSensor.Close();
                m_KinectSensor = null;
            }
        }

        private void OnSkeletonFrameReady(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            var reference = e.FrameReference.AcquireFrame();

            using (var frame = reference.BodyFrameReference.AcquireFrame())
            {
                if (frame == null)
                    return;

                var bodyCount = frame.BodyFrameSource.BodyCount;
                if (bodyCount == 0)
                    return;

                if (m_Bodies.Length != bodyCount)
                    Array.Resize(ref m_Bodies, bodyCount);

                frame.GetAndRefreshBodyData(m_Bodies);

                for (var i = 0; i < bodyCount; i++)
                {
                    var body = m_Bodies[i];

                    if (body?.IsTracked ?? false)
                    {
                        foreach (var joint in body.Joints)
                        {
                            var needBreak = false;
                            var position = joint.Value.Position;

                            if (joint.Key == JointType.Head)
                            {
                                m_VridgeRemote.Head.SetPosition(position.X, position.Y, position.Z);
                                needBreak = true;
                            }

                            var left = joint.Key == JointType.HandLeft;
                            var right = joint.Key == JointType.HandRight;

                            if (left || right)
                            {
                                m_VridgeRemote.Controller.SetControllerState(left ? 0 : 1,
                                    HeadRelation.SticksToHead,
                                    left ? HandType.Left : HandType.Right,
                                    GetQuaternion(body, joint),
                                    new Vector3(position.X, position.Y, position.Z),
                                    0, 0, 0,
                                    false, false, false,
                                    false, false, false);

                                needBreak = true;
                            }

                            if (needBreak)
                                break;
                        }

                        break;
                    }
                }
            }
        }

        private Quaternion GetQuaternion(Body body, KeyValuePair<JointType, Joint> joint)
        {
            var orientation = body.JointOrientations[joint.Key].Orientation;
            return new Quaternion(orientation.X, orientation.Y, orientation.Z, orientation.W);
        }

        private void OnActionClicked(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                if (button.Tag.ToString() == "Connect")
                    InitilizeKinect();
                else
                    Shutdown();

                ConnectionStatus.Text = m_KinectSensor?.IsOpen ?? false ? "Connected" : "Not Connected";
            }
        }
    }
}
