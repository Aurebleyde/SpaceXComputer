using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using KRPC.Client;
using KRPC.Client.Services.KRPC;
using KRPC.Client.Services.SpaceCenter;

namespace SpaceXComputer
{
    class Ariane5Event : Event
    {
        protected Connection connection;

        protected Ariane5 ariane5;

        public Ariane5Event(Vessel vessel, Connection connectionLink)
        {
            connection = connectionLink;
            ariane5 = new Ariane5(vessel, RocketBody.F9_FIRST_STAGE);

            foreach (Vessel vesselTarget in connection.SpaceCenter().Vessels)
            {
                if (vesselTarget.Name.Equals("[ArianeSpace] Ariane V ECA") && vesselTarget.Type.Equals(VesselType.Probe))
                {
                    ariane5.ariane5 = vesselTarget;
                    ariane5.ariane5.Name = "Ariane 5 ECA";
                    Console.WriteLine("ARIANE V : Accisition signal.");
                }
            }

            ariane5.Ariane5Startup(connection);
            Thread.Sleep(5000);
            Thread GT = new Thread(ariane5.GravityTurn);
            GT.Start();
            ariane5.EAPSep();
            ariane5.ariane5.AutoPilot.TargetPitch = 28;
            Thread FairingSep = new Thread(ariane5.CoiffeSep);
            FairingSep.Start();
            ariane5.EPCSep();
            ariane5.ESCstartup();
            Thread.Sleep(999999);
        }
    }
}
