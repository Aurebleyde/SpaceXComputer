using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceXComputer
{
    public class FlightInfo
    {
        //Orbital Info

        //GTO = 2863333m
        //ISS = 48.36f

        protected float remeningHour = 00;
        protected float remeningMinute = 01;
        protected float remeningSecond = 30;
        protected String Rocket = "F9"; //FH = Falcon Heavy / F9 = Falcon 9 / A5 = Ariane 5 / H2A
        protected Boolean StaticFire = false;
        protected Boolean Grap = false;
        protected Boolean Starlink = false;
        protected Boolean Dragon = false;
        protected Boolean DragonV2 = false;
        protected Boolean JustLandingBurn = false;
        protected Boolean Orion = false;
        protected Boolean multiPhaseDragon = false;
        protected Boolean Hyperloop = false;
        protected Double maxAltitude = 45000;
        protected Double apoapsisTarget = 35786000;
        protected Double periapsisTarget = 600000;
        protected String landing = "RTLS"; //RTLS for LZ and ASDS for drone ship
        protected String LZ = "LZ-1"; //LZ-1/LZ-2/LZ-4/OCISLY/FHLZ/FHOCISLY

        //protected Double semiMajorAxis = ((Startup.GetInstance().GetFlightInfo().getApoapsisTarget() + 600000) + (Startup.GetInstance().GetFlightInfo().getPeriapsisTarget() + 600000)) / 2;
        protected Double dragonCargoApoapsisTarget = 900000;
        protected Double dragonCargoPreiapsisTarget = 900000;
        protected float inclination = 0;
        protected float head = 90/* + 43*/;
        protected float headDesorbitation = 268;

        protected string time = "11h48:00";


        public float getRemeningHour()
        {
            return remeningHour;
        }
        public float getRemeningMinute()
        {
            return remeningMinute;
        }
        public float getRemeningSecond()
        {
            return remeningSecond;
        }

        public String getRocket()
        {
            return Rocket;
        }

        public Boolean getStaticFire()
        {
            return StaticFire;
        }

        public Boolean getGrap()
        {
            return Grap;
        }

        public Boolean getStarlink()
        {
            return Starlink;
        }

        public Boolean getDragon()
        {
            return Dragon;
        }

        public Boolean getDragonV2()
        {
            return DragonV2;
        }

        public Boolean getJustLandingBurn()
        {
            return JustLandingBurn;
        }

        public Boolean getOrion()
        {
            return Orion;
        }

        public Boolean getMultiPhaseDragon()
        {
            return multiPhaseDragon;
        }

        public Boolean getHyperloop()
        {
            return Hyperloop;
        }

        public Double getMaxAltitude()
        {
            return maxAltitude;
        }

        public Double getApoapsisTarget()
        {
            return apoapsisTarget;
        }

        public Double getPeriapsisTarget()
        {
            return periapsisTarget;
        }

        public String getLanding()
        {
            return landing;
        }

        public String getLZ()
        {
            return LZ;
        }

        /*public Double getSemiMajorAxis()
        {
            return semiMajorAxis;
        }*/

        public Double getDragonApoapsisTarget()
        {
            return dragonCargoApoapsisTarget;
        }

        public Double getDragonPeriapsisTarget()
        {
            return dragonCargoPreiapsisTarget;
        }

        public float getInclination()
        {
            return inclination;
        }

        public float getHead()
        {
            return head;
        }

        public float getHeadDesorbitation()
        {
            return headDesorbitation;
        }

        public string getTime()
        {
            return time;
        }
    }
}
