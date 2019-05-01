using Microsoft.Kinect;
using System;

namespace KinectLib.XboxOne
{
    public struct TrackingData
    {
        public float[] HeadPosition;
        public float[] LeftTransform;
        public float[] RightTransform;

        public void Initialize()
        {
            HeadPosition = new float[3];
            LeftTransform = new float[7];
            RightTransform = new float[7];
        }
    }

    public class KinectOneManager
    {
        private static KinectSensor m_KinectSensor;
        private static Body[] m_Bodies;
        private static MultiSourceFrameReader m_MultiSourceFrameReader;

        private TrackingData m_TrackingData;

        public ref TrackingData TrackingData => ref m_TrackingData;

        public event Action<TrackingData> NewTrackingData;

        public KinectOneManager()
        {
            m_TrackingData.Initialize();
        }

        public bool IsAvailable()
        {
            var kinect = KinectSensor.GetDefault();
            return kinect.IsAvailable;
        }

        public bool Start()
        {
            Stop();

            m_KinectSensor = KinectSensor.GetDefault();

            if (m_KinectSensor != null)
            {
                m_KinectSensor.Open();
                m_MultiSourceFrameReader = m_KinectSensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth | FrameSourceTypes.Infrared | FrameSourceTypes.Body);
                m_MultiSourceFrameReader.MultiSourceFrameArrived += OnSkeletonFrameReady;
            }

            return m_KinectSensor != null;
        }

        public void Stop()
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

                if (m_Bodies == null)
                    m_Bodies = new Body[bodyCount];

                if (m_Bodies.Length != bodyCount)
                    Array.Resize(ref m_Bodies, bodyCount);

                frame.GetAndRefreshBodyData(m_Bodies);

                for (var i = 0; i < bodyCount; i++)
                {
                    var body = m_Bodies[i];

                    if (body?.IsTracked ?? false)
                    {
                        var trackedBones = 0;

                        foreach (var joint in body.Joints)
                        {
                            var position = joint.Value.Position;

                            if (joint.Key == JointType.Head)
                            {
                                m_TrackingData.HeadPosition[0] = position.X;
                                m_TrackingData.HeadPosition[1] = position.Y;
                                m_TrackingData.HeadPosition[2] = position.Z;
                                trackedBones++;
                            }

                            if (joint.Key == JointType.HandLeft)
                            {
                                UpdateJoint(ref m_TrackingData.LeftTransform, body, joint.Value);
                                trackedBones++;
                            }

                            if (joint.Key == JointType.HandRight)
                            {
                                UpdateJoint(ref m_TrackingData.RightTransform, body, joint.Value);
                                trackedBones++;
                            }

                            if (trackedBones == 3)
                            {
                                NewTrackingData.Invoke(m_TrackingData);
                                break;
                            }
                        }

                        break;
                    }
                }
            }
        }

        private void UpdateJoint(ref float[] transform, Body body, Joint joint)
        {
            transform[0] = joint.Position.X;
            transform[1] = joint.Position.Y;
            transform[2] = joint.Position.Z;

            var quat = body.JointOrientations[joint.JointType].Orientation;
            transform[3] = quat.X;
            transform[4] = quat.Y;
            transform[5] = quat.Z;
            transform[6] = quat.W;
        }
    }
}
