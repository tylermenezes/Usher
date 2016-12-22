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
        public override string Provider => "zwave";
        public override string Instance => Gateway.InstanceId;
        public override string Id
        {
            get
            {
                return ZWaveNode.Id.ToString();
            }
            protected set {}
        }
        public Gateway Gateway;
        public ZWaveNode ZWaveNode;

        public static bool IsNodeInstance(ZWaveNode node)
        {
            // ReSharper disable once PossibleNullReferenceException (if you don't specify requiredClasses you deserve all the
            // exceptions you get!)
            var requiredClasses = (int[])typeof(T).GetField("RequiredClasses", BindingFlags.NonPublic | BindingFlags.Static)
                                    .GetValue(null);
            foreach (var cl in requiredClasses)
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
            var resp = ZWaveNode.SendDataRequest(message.ToArray());
            
            // Create the waiter thread.
            (new Thread((object threadData) => {
                var o = (object[])threadData;
                var s = (SemaphoreSlim)o[0];
                var m = (ZWaveMessage)o[1];

                var debugPacket = m.RawData.ToList()
                                .Select(p => $"{p:X2}")
                                .Aggregate("", (acc, x) => $"{acc} {x}")
                                .Substring(1);
                Utilities.Logger.Debug($"zwave write: (node {ZWaveNode.Id}) <- {debugPacket}");

                m.Wait();
                s.Release();
            })).Start(new object[] {semaphor, resp});

            // Return the task.
            return semaphor.WaitAsync();
        }
    }
}