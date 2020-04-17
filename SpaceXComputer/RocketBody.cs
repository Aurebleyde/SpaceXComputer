using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceXComputer
{
    public enum RocketBody
    {
        //RTLS
        LZ1,
        //ASDS
        OCISLY,
        NRCQUEST,

        //SPACEX
        //F9RDev
        F9RDev,

        //SATTARGET
        SATTARGET,

        //FALCON 1
        F1_FIRST_STAGE,
        F1_SECOND_STAGE,
        //FALCON 9
        F9_FIRST_STAGE,
        F9_SECOND_STAGE,
        F9_FAIRING_A,
        F9_FAIRING_B,
        //FALCONHEAVY
        FH_CENTERCORE,
        FH_SIDEBOOSTER_A,
        FH_SIDEBOOSTER_B,
        //STARSHIP
        STARSHIP,
        SUPERHEAVY,
        //DRAGON
        DRAGON,
        DRAGONV2,

        //CASIP
        //CARBON II
        C2_FIRST_STAGE,
        C2_SECOND_STAGE,
        C2_THIRD_STAGE,
        //CARBON II HEAVY
        C2H_CENTERCORE,
        C2H_SIDEBOOSTER_A,
        C2H_SIDEBOOSTER_B,
        C2H_SECOND_STAGE,
        C2H_THIRD_STAGE,
    }
}
