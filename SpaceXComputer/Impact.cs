using Google.Protobuf;
using System;
using System.Collections.Generic;

#if NET35
using systemAlias = global::KRPC.Client.Compatibility;
using genericCollectionsAlias = global::KRPC.Client.Compatibility;
#else
using systemAlias = global::System;
using genericCollectionsAlias = global::System.Collections.Generic;
#endif

namespace KRPC.Client.Services.Impact
{
    /// <summary>
    /// Extension methods for Impact service.
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Create an instance of the Impact service.
        /// </summary>
        public static global::KRPC.Client.Services.Impact.Service Impact (this global::KRPC.Client.IConnection connection)
        {
            return new global::KRPC.Client.Services.Impact.Service (connection);
        }
    }
    
    public class Service
    {
        global::KRPC.Client.IConnection connection;

        internal Service (global::KRPC.Client.IConnection serverConnection)
        {
            connection = serverConnection;
        }
        
        [global::KRPC.Client.Attributes.RPCAttribute ("Impact", "GetImpactPos")]
        public systemAlias::Tuple<double,double> GetImpactPos (global::KRPC.Client.Services.SpaceCenter.Vessel vessel)
        {
                        var _args = new ByteString[] {
                global::KRPC.Client.Encoder.Encode (vessel, typeof(global::KRPC.Client.Services.SpaceCenter.Vessel))
            };
                        ByteString _data = connection.Invoke ("Impact", "GetImpactPos", _args);
                        return (systemAlias::Tuple<double,double>)global::KRPC.Client.Encoder.Decode (_data, typeof(systemAlias::Tuple<double,double>), connection);
        }
    }
}
