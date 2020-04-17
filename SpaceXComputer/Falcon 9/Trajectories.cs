using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

#if NET35
using systemAlias = global::KRPC.Client.Compatibility;
using genericCollectionsAlias = global::KRPC.Client.Compatibility;
#else
using systemAlias = global::System;
using genericCollectionsAlias = global::System.Collections.Generic;
#endif

namespace KRPC.Client.Services.Trajectories
{
    /// <summary>
    /// Extension methods for Trajectories service.
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Create an instance of the Trajectories service.
        /// </summary>
        public static global::KRPC.Client.Services.Trajectories.Service Trajectories(this global::KRPC.Client.IConnection connection)
        {
            return new global::KRPC.Client.Services.Trajectories.Service(connection);
        }
    }

    /// <summary>
    /// Trajectories service.
    /// </summary>
    public class Service
    {
        global::KRPC.Client.IConnection connection;

        internal Service(global::KRPC.Client.IConnection serverConnection)
        {
            connection = serverConnection;
        }

        [global::KRPC.Client.Attributes.RPCAttribute("Trajectories", "GetImpactTime")]
        public double GetImpactTime()
        {
            ByteString _data = connection.Invoke("Trajectories", "GetImpactTime");
            return (double)global::KRPC.Client.Encoder.Decode(_data, typeof(double), connection);
        }

        [global::KRPC.Client.Attributes.RPCAttribute("Trajectories", "HasImpact")]
        public bool HasImpact()
        {
            ByteString _data = connection.Invoke("Trajectories", "HasImpact");
            return (bool)global::KRPC.Client.Encoder.Decode(_data, typeof(bool), connection);
        }

        [global::KRPC.Client.Attributes.RPCAttribute("Trajectories", "ImpactPos")]
        public systemAlias::Tuple<double, double> ImpactPos()
        {
            ByteString _data = connection.Invoke("Trajectories", "ImpactPos");
            return (systemAlias::Tuple<double, double>)global::KRPC.Client.Encoder.Decode(_data, typeof(systemAlias::Tuple<double, double>), connection);
        }
    }
}