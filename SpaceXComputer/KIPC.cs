using Google.Protobuf;
using System;
using System.Collections.Generic;

namespace KRPC.Client.Services.KIPC
{
    /// <summary>
    /// Extension methods for KIPC service.
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Create an instance of the KIPC service.
        /// </summary>
        public static global::KRPC.Client.Services.KIPC.Service KIPC (this global::KRPC.Client.IConnection connection)
        {
            return new global::KRPC.Client.Services.KIPC.Service (connection);
        }
    }

    /// <summary>
    /// KIPC service.
    /// </summary>
    public class Service
    {
        global::KRPC.Client.IConnection connection;

        internal Service (global::KRPC.Client.IConnection serverConnection)
        {
            connection = serverConnection;
        }

        
        [global::KRPC.Client.Attributes.RPCAttribute ("KIPC", "GetMessages")]
        public global::System.Collections.Generic.IList<String> GetMessages ()
        {
                        ByteString _data = connection.Invoke ("KIPC", "GetMessages");
                        return (global::System.Collections.Generic.IList<String>)global::KRPC.Client.Encoder.Decode (_data, typeof(global::System.Collections.Generic.IList<String>), connection);
        }

        
        [global::KRPC.Client.Attributes.RPCAttribute ("KIPC", "GetPartsTagged")]
        public global::System.Collections.Generic.IList<global::KRPC.Client.Services.SpaceCenter.Part> GetPartsTagged (global::KRPC.Client.Services.SpaceCenter.Vessel vessel, String tag)
        {
                        var _args = new ByteString[] {
                global::KRPC.Client.Encoder.Encode (vessel, typeof(global::KRPC.Client.Services.SpaceCenter.Vessel)),
                global::KRPC.Client.Encoder.Encode (tag, typeof(String))
            };
                        ByteString _data = connection.Invoke ("KIPC", "GetPartsTagged", _args);
                        return (global::System.Collections.Generic.IList<global::KRPC.Client.Services.SpaceCenter.Part>)global::KRPC.Client.Encoder.Decode (_data, typeof(global::System.Collections.Generic.IList<global::KRPC.Client.Services.SpaceCenter.Part>), connection);
        }

        
        [global::KRPC.Client.Attributes.RPCAttribute ("KIPC", "GetProcessor")]
        public global::System.Collections.Generic.IList<global::KRPC.Client.Services.KIPC.Processor> GetProcessor (global::KRPC.Client.Services.SpaceCenter.Part part)
        {
                        var _args = new ByteString[] {
                global::KRPC.Client.Encoder.Encode (part, typeof(global::KRPC.Client.Services.SpaceCenter.Part))
            };
                        ByteString _data = connection.Invoke ("KIPC", "GetProcessor", _args);
                        return (global::System.Collections.Generic.IList<global::KRPC.Client.Services.KIPC.Processor>)global::KRPC.Client.Encoder.Decode (_data, typeof(global::System.Collections.Generic.IList<global::KRPC.Client.Services.KIPC.Processor>), connection);
        }

        
        [global::KRPC.Client.Attributes.RPCAttribute ("KIPC", "GetProcessors")]
        public global::System.Collections.Generic.IList<global::KRPC.Client.Services.KIPC.Processor> GetProcessors (global::KRPC.Client.Services.SpaceCenter.Vessel vessel)
        {
                        var _args = new ByteString[] {
                global::KRPC.Client.Encoder.Encode (vessel, typeof(global::KRPC.Client.Services.SpaceCenter.Vessel))
            };
                        ByteString _data = connection.Invoke ("KIPC", "GetProcessors", _args);
                        return (global::System.Collections.Generic.IList<global::KRPC.Client.Services.KIPC.Processor>)global::KRPC.Client.Encoder.Decode (_data, typeof(global::System.Collections.Generic.IList<global::KRPC.Client.Services.KIPC.Processor>), connection);
        }

        
        [global::KRPC.Client.Attributes.RPCAttribute ("KIPC", "PeekMessage")]
        public String PeekMessage ()
        {
                        ByteString _data = connection.Invoke ("KIPC", "PeekMessage");
                        return (String)global::KRPC.Client.Encoder.Decode (_data, typeof(String), connection);
        }

        
        [global::KRPC.Client.Attributes.RPCAttribute ("KIPC", "PopMessage")]
        public String PopMessage ()
        {
                        ByteString _data = connection.Invoke ("KIPC", "PopMessage");
                        return (String)global::KRPC.Client.Encoder.Decode (_data, typeof(String), connection);
        }

        
        [global::KRPC.Client.Attributes.RPCAttribute ("KIPC", "ResolveBodies")]
        public global::System.Collections.Generic.IList<global::KRPC.Client.Services.SpaceCenter.CelestialBody> ResolveBodies (global::System.Collections.Generic.IList<Int32> bodyIds)
        {
                        var _args = new ByteString[] {
                global::KRPC.Client.Encoder.Encode (bodyIds, typeof(global::System.Collections.Generic.IList<Int32>))
            };
                        ByteString _data = connection.Invoke ("KIPC", "ResolveBodies", _args);
                        return (global::System.Collections.Generic.IList<global::KRPC.Client.Services.SpaceCenter.CelestialBody>)global::KRPC.Client.Encoder.Decode (_data, typeof(global::System.Collections.Generic.IList<global::KRPC.Client.Services.SpaceCenter.CelestialBody>), connection);
        }

        
        [global::KRPC.Client.Attributes.RPCAttribute ("KIPC", "ResolveBody")]
        public global::KRPC.Client.Services.SpaceCenter.CelestialBody ResolveBody (Int32 bodyId)
        {
                        var _args = new ByteString[] {
                global::KRPC.Client.Encoder.Encode (bodyId, typeof(Int32))
            };
                        ByteString _data = connection.Invoke ("KIPC", "ResolveBody", _args);
                        return (global::KRPC.Client.Services.SpaceCenter.CelestialBody)global::KRPC.Client.Encoder.Decode (_data, typeof(global::KRPC.Client.Services.SpaceCenter.CelestialBody), connection);
        }

        
        [global::KRPC.Client.Attributes.RPCAttribute ("KIPC", "ResolveVessel")]
        public global::KRPC.Client.Services.SpaceCenter.Vessel ResolveVessel (String vesselGuid)
        {
                        var _args = new ByteString[] {
                global::KRPC.Client.Encoder.Encode (vesselGuid, typeof(String))
            };
                        ByteString _data = connection.Invoke ("KIPC", "ResolveVessel", _args);
                        return (global::KRPC.Client.Services.SpaceCenter.Vessel)global::KRPC.Client.Encoder.Decode (_data, typeof(global::KRPC.Client.Services.SpaceCenter.Vessel), connection);
        }

        
        [global::KRPC.Client.Attributes.RPCAttribute ("KIPC", "ResolveVessels")]
        public global::System.Collections.Generic.IList<global::KRPC.Client.Services.SpaceCenter.Vessel> ResolveVessels (global::System.Collections.Generic.IList<String> vesselGuids)
        {
                        var _args = new ByteString[] {
                global::KRPC.Client.Encoder.Encode (vesselGuids, typeof(global::System.Collections.Generic.IList<String>))
            };
                        ByteString _data = connection.Invoke ("KIPC", "ResolveVessels", _args);
                        return (global::System.Collections.Generic.IList<global::KRPC.Client.Services.SpaceCenter.Vessel>)global::KRPC.Client.Encoder.Decode (_data, typeof(global::System.Collections.Generic.IList<global::KRPC.Client.Services.SpaceCenter.Vessel>), connection);
        }


        
        [global::KRPC.Client.Attributes.RPCAttribute ("KIPC", "get_CountMessages")]
        public Int32 CountMessages {
            get {
                                ByteString _data = connection.Invoke ("KIPC", "get_CountMessages");
                                return (Int32)global::KRPC.Client.Encoder.Decode (_data, typeof(Int32), connection);
            }
        }


    }


    
    public class Processor : global::KRPC.Client.RemoteObject
    {
        /// <summary>
        /// Construct an instance of this remote object. Should not be called directly. This interface is intended for internal decoding.
        /// </summary>
        public Processor (global::KRPC.Client.IConnection connection, UInt64 id) : base (connection, id)
        {
        }

        
        [global::KRPC.Client.Attributes.RPCAttribute ("KIPC", "Processor_SendMessage")]
        public Boolean SendMessage (String json)
        {
                        var _args = new ByteString[] {
                global::KRPC.Client.Encoder.Encode (this, typeof(KIPC.Processor)),
                global::KRPC.Client.Encoder.Encode (json, typeof(String))
            };
                        ByteString _data = connection.Invoke ("KIPC", "Processor_SendMessage", _args);
                        return (Boolean)global::KRPC.Client.Encoder.Decode (_data, typeof(Boolean), connection);
        }



        
        [global::KRPC.Client.Attributes.RPCAttribute ("KIPC", "Processor_get_DiskSpace")]
        public Int32 DiskSpace {
            get {
                                var _args = new ByteString[] {
                    global::KRPC.Client.Encoder.Encode (this, typeof(KIPC.Processor))
                };
                                ByteString _data = connection.Invoke ("KIPC", "Processor_get_DiskSpace", _args);
                                return (Int32)global::KRPC.Client.Encoder.Decode (_data, typeof(Int32), connection);
            }
        }

        
        [global::KRPC.Client.Attributes.RPCAttribute ("KIPC", "Processor_get_Part")]
        public global::KRPC.Client.Services.SpaceCenter.Part Part {
            get {
                                var _args = new ByteString[] {
                    global::KRPC.Client.Encoder.Encode (this, typeof(KIPC.Processor))
                };
                                ByteString _data = connection.Invoke ("KIPC", "Processor_get_Part", _args);
                                return (global::KRPC.Client.Services.SpaceCenter.Part)global::KRPC.Client.Encoder.Decode (_data, typeof(global::KRPC.Client.Services.SpaceCenter.Part), connection);
            }
        }

        
        [global::KRPC.Client.Attributes.RPCAttribute ("KIPC", "Processor_get_Powered")]
        public Boolean Powered {
            get {
                                var _args = new ByteString[] {
                    global::KRPC.Client.Encoder.Encode (this, typeof(KIPC.Processor))
                };
                                ByteString _data = connection.Invoke ("KIPC", "Processor_get_Powered", _args);
                                return (Boolean)global::KRPC.Client.Encoder.Decode (_data, typeof(Boolean), connection);
            }
            set {
                                var _args = new ByteString[] {
                    global::KRPC.Client.Encoder.Encode (this, typeof(KIPC.Processor)),
                    global::KRPC.Client.Encoder.Encode (value, typeof(Boolean))
                };
                                connection.Invoke ("KIPC", "Processor_set_Powered", _args);
            }
        }

        
        [global::KRPC.Client.Attributes.RPCAttribute ("KIPC", "Processor_get_TerminalVisible")]
        public Boolean TerminalVisible {
            get {
                                var _args = new ByteString[] {
                    global::KRPC.Client.Encoder.Encode (this, typeof(KIPC.Processor))
                };
                                ByteString _data = connection.Invoke ("KIPC", "Processor_get_TerminalVisible", _args);
                                return (Boolean)global::KRPC.Client.Encoder.Decode (_data, typeof(Boolean), connection);
            }
            set {
                                var _args = new ByteString[] {
                    global::KRPC.Client.Encoder.Encode (this, typeof(KIPC.Processor)),
                    global::KRPC.Client.Encoder.Encode (value, typeof(Boolean))
                };
                                connection.Invoke ("KIPC", "Processor_set_TerminalVisible", _args);
            }
        }

    }


}

