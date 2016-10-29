using ZWaveLib;
using System;
using System.Collections.Generic;
using Usher.Platforms.Generic.Devices;
namespace Usher.Platforms.ZWave.Devices
{
    public class Remote : ZWaveEndDevice<Remote>, IRemote
    {
        protected static int[] requiredClasses = new int[] { 43 };
        public Remote(Gateway gateway, ZWaveNode node)
        {
            this.gateway = gateway;
            this.node = node;
            node.NodeUpdated += nodeDataRecieved;
        }

        public Remote(){}

        protected Dictionary<int, List<ButtonPressedHandler>> handlers = new Dictionary<int, List<ButtonPressedHandler>>();
        public event ButtonPressedHandler OnButtonPress;
        public void RegisterButtonPressHandler(int button, ButtonPressedHandler onPress)
        {
            if (!handlers.ContainsKey(button)) handlers.Add(button, new List<ButtonPressedHandler>());
            handlers[button].Add(onPress);
        }

        protected void nodeDataRecieved(object sender, NodeEvent eventData)
        {
            var val = int.Parse(eventData.Value.ToString()); // Seems to be the only way to get this?
            if (OnButtonPress != null) OnButtonPress(val);
            if (handlers.ContainsKey(val)) {
                foreach (ButtonPressedHandler handler in handlers[val]) {
                    handler(val);
                }
            }
        }
    }
}