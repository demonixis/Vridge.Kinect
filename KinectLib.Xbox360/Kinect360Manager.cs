using Microsoft.Kinect;
using System;

namespace KinectLib.Xbox360
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

    public class Kinect360Manager
    {
        private KinectSensor m_KinectSensor;
        private Skeleton[] m_Skeletons = null;
        private TrackingData m_TrackingData;

        public ref TrackingData TrackingData => ref m_TrackingData;

        public event Action<TrackingData> NewTrackingData;

        public Kinect360Manager()
        {
            m_TrackingData.Initialize();
        }

        public bool Start()
        {
            if (KinectSensor.KinectSensors.Count > 0)
            {
                m_KinectSensor = KinectSensor.KinectSensors[0];
                m_KinectSensor.SkeletonStream.Enable(new TransformSmoothParameters());
                m_KinectSensor.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(OnSkeletonFrameReady);
                m_KinectSensor.Start();

                return true;
            }

            return true;
        }

        public bool Stop()
        {
            if (m_KinectSensor != null)
            {
                m_KinectSensor.SkeletonFrameReady -= new EventHandler<SkeletonFrameReadyEventArgs>(OnSkeletonFrameReady);
                m_KinectSensor.Stop();
                m_KinectSensor.Dispose();
                m_KinectSensor = null;
                return true;
            }

            return false;
        }

        private void OnSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                // No player
                if (skeletonFrame == null)
                    return;

                var skeletonCount = skeletonFrame.SkeletonArrayLength;
                if (skeletonCount == 0)
                    return;

                m_Skeletons = new Skeleton[skeletonCount];
                skeletonFrame.CopySkeletonDataTo(m_Skeletons);

                for (int i = 0; i < skeletonCount; i++)
                {
                    if (m_Skeletons[i].TrackingState == SkeletonTrackingState.Tracked)
                    {
                        var trackedBones = 0;

                        foreach (Joint joint in m_Skeletons[i].Joints)
                        {
                            if (joint.JointType == JointType.Head)
                            {
                                m_TrackingData.HeadPosition[0] = joint.Position.X;
                                m_TrackingData.HeadPosition[1] = joint.Position.Y;
                                m_TrackingData.HeadPosition[2] = joint.Position.Z;
                                trackedBones++;
                            }
                            else if (joint.JointType == JointType.HandLeft)
                            {
                                UpdateJoint(ref m_TrackingData.LeftTransform, joint, i);
                                trackedBones++;
                            }
                            else if (joint.JointType == JointType.HandLeft)
                            {
                                UpdateJoint(ref m_TrackingData.RightTransform, joint, i);
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

        private void UpdateJoint(ref float[] transform, Joint joint, int index)
        {
            transform[0] = joint.Position.X;
            transform[1] = joint.Position.Y;
            transform[2] = joint.Position.Z;

            var quat = m_Skeletons[index].BoneOrientations[joint.JointType].AbsoluteRotation.Quaternion;
            transform[3] = quat.X;
            transform[4] = quat.Y;
            transform[5] = quat.Z;
            transform[6] = quat.W;
        }
    }
}
