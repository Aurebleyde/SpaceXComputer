using System;

namespace PIDLoop
{
    public class PID
    {
        private double LastInput;
        private double LastSampleTime;
        private double ErrorSum;
        private double Kp;
        private double Ki;
        private double Kd;
        private double Setpoint;
        private double MinOutput;
        private double MaxOutput;
        private double PTerm;
        private double ITerm;
        private double DTerm;
        private double deadband;
        private double Output;

        public PID (double LastInput, double LastSampleTime, double ErrorSum, double Kp, double Ki, double Kd, double Setpoint, double MinOutput, double MaxOutput)
        {
            this.LastInput = LastInput;
            this.LastSampleTime = LastSampleTime;
            this.ErrorSum = ErrorSum;
            this.Kp = Kp;
            this.Ki = Ki;
            this.Kd = Kd;
            this.Setpoint = Setpoint;
            this.MinOutput = MinOutput;
            this.MaxOutput = MaxOutput;
            this.PTerm = 0;
            this.ITerm = 0;
            this.DTerm = 0;
            this.deadband = 0;
        }

        public double Update(double SampleTime, double Input)
        {
            var Error = this.Setpoint - Input;
            PTerm = Kp * Error;
            ITerm = 0;
            DTerm = 0;
            var in_deadband = Math.Abs(Error) < deadband;

            if (LastSampleTime < SampleTime)
            {
                if (in_deadband == false)
                {
                    var dt = SampleTime - this.LastSampleTime;
                    if (Ki > 0 || Ki <0)
                    {
                        ITerm = (ErrorSum + Error * dt) * Ki;
                    }

                    var ChangeRate = (Input - LastInput) / dt;

                    if (Kd > 0 || Kd < 0)
                    {
                        DTerm = -ChangeRate * Kd;
                    }
                }
            }

            Output = PTerm + ITerm + DTerm;

            if (Output > MaxOutput)
            {
                Output = MaxOutput;

                if ((Ki < 0 || Ki > 0) && LastSampleTime < SampleTime)
                {
                    ITerm = Output - Math.Min(PTerm + DTerm, MaxOutput);
                }
            }
            else if (Output < MinOutput)
            {
                Output = MinOutput;

                if ((Ki > 0 || Ki < 0) && LastSampleTime < SampleTime)
                {
                    ITerm = Output - Math.Max(PTerm + DTerm, MinOutput);
                }
            }

            LastSampleTime = SampleTime;
            LastInput = Input;

            if (Ki < 0 || Ki > 0)
            {
                ErrorSum = ITerm / Ki;
            }
            else
            {
                ErrorSum = 0;
            }

            return Output;
        }

        public void Reset()
        {
            this.ErrorSum = 0;
            ITerm = 0;
            LastSampleTime = MaxOutput;
        }
    }
}
