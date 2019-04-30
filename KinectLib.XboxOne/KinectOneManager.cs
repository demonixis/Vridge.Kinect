using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectLib.XboxOne
{
    public struct TrackingData
    {
        public float[] HeadPosition;
        public float[] LeftHandPosition;
        public float[] LeftHandRotation;
        public float[] RightHandRotation;
    }

    public class KinectOneManager
    {
        public event Action<TrackingData> NewTrackingData;

        public bool Start()
        {
            return false;
        }

        public bool Stop()
        {
            return false;
        }
    }
}
