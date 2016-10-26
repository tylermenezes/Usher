using ZWaveLib;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Usher.Platforms.Generic.Devices;
namespace Usher.Platforms.ZWave.Devices
{
    public abstract class ZWaveEndDevice<T> : GenericDevice
        where T : ZWaveEndDevice<T>, new()
    {
        public override string Provider { get { return "zwave"; }}
        public override string Instance { get { return gateway.InstanceId; }}
        public override string Id
        {
            get
            {
                return node.Id.ToString();
            }
            protected set {}
        }
        public Gateway gateway;
        public ZWaveNode node;

        public static bool IsNodeInstance(ZWaveNode node)
        {
            var requiredClasses = (int[])typeof(T).GetField("requiredClasses", BindingFlags.NonPublic | BindingFlags.Static)
                                    .GetValue(null);
            foreach (int cl in requiredClasses)
            {
                if (!node.CommandClasses.Select(c => (int)c.Id).Contains(cl))
                    return false;
            }

            return true;
        }

        public Task SendRequest(ZWave.Proto.CommandClass cls, byte command, byte[] argv)
        {
            var message = new List<byte>();
            message.AddRange(new byte[]{(byte)cls, command});
            message.AddRange(argv);

            var semaphor = new SemaphoreSlim(0, 1);
            var resp = node.SendDataRequest(message.ToArray());
            
            // Create the waiter thread.
            (new Thread((object threadData) => {
                var o = (object[])threadData;
                var s = (SemaphoreSlim)o[0];
                var m = (ZWaveMessage)o[1];

                string debugPacket = m.RawData.ToList()
                                .Select(p => String.Format("{0:X2}", p))
                                .Aggregate("", (acc, x) => String.Format("{0} {1}", acc, x))
                                .Substring(1);
                Utilities.Logger.Debug(String.Format("zwave write: (node {0}) <- {1}", node.Id, debugPacket));

                m.Wait();
                s.Release();
            })).Start(new object[] {semaphor, resp});

            // Return the task.
            return semaphor.WaitAsync();
        }
    }
}