using VRE.Vridge.API.Client.Messages.BasicTypes;
using VRE.Vridge.API.Client.Messages.v3.Controller;
using System.Numerics;
using VRE.Vridge.API.Client.Remotes;

namespace Vridge.Kinect
{
    public class VridgeTrackedDevice
    {
        public int ControllerId = 0;
        public HeadRelation HeadRelation = HeadRelation.SticksToHead;
        public HandType SuggestedHand = HandType.Right;
        public Quaternion Orientation;
        public Vector3? Position;
        public double AnalogX;
        public double AnalogY;
        public double AnalogTrigger;
        public bool IsMenuPressed;
        public bool IsSystemPressed;
        public bool IsTriggerPressed;
        public bool IsGripPressed;
        public bool IsTouchpadPressed;
        public bool IsTouchpadTouched;

        public void SetControllerState(VridgeRemote vridge)
        {
            vridge.Controller.SetControllerState(
                ControllerId,
                HeadRelation,
                SuggestedHand,
                Orientation,
                Position,
                AnalogX,
                AnalogY,
                AnalogTrigger,
                IsMenuPressed,
                IsSystemPressed,
                IsTriggerPressed,
                IsGripPressed,
                IsTouchpadPressed,
                IsTouchpadTouched);
        }
    }
}
