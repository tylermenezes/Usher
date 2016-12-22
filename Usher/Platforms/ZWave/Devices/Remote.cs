using ZWaveLib;
using System;
using System.Collections.Generic;
using Usher.Platforms.Generic.Devices;
namespace Usher.Platforms.ZWave.Devices
{
    public class Remote : ZWaveEndDevice<Remote>, IRemote
    {
        protected static int[] RequiredClasses = new int[] { 43 };
        public Remote(Gateway gateway, ZWaveNode node)
        {
            this.Gateway = gateway;
            this.ZWaveNode = node;
            node.NodeUpdated += NodeDataRecieved;
        }

        public Remote(){}

        protected Dictionary<int, List<ButtonPressedHandler>> Handlers = new Dictionary<int, List<ButtonPressedHandler>>();
        public event ButtonPressedHandler OnButtonPress;
        public void RegisterButtonPressHandler(int button, ButtonPressedHandler onPress)
        {
            if (!Handlers.ContainsKey(button)) Handlers.Add(button, new List<ButtonPressedHandler>());
            Handlers[button].Add(onPress);
        }

        protected void NodeDataRecieved(object sender, NodeEvent eventData)
        {
            var val = int.Parse(eventData.Value.ToString()); // Seems to be the only way to get this?
            OnButtonPress?.Invoke(val);
            if (Handlers.ContainsKey(val)) {
                foreach (var handler in Handlers[val]) {
                    handler(val);
                }
            }
        }
    }
}