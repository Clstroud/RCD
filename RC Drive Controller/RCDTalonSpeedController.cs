using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace RCDriveController
{
    class RCDTalonSpeedController
    {
        public float targetValue;
        public float maximumAbsoluteAccelerationPerMillisecond = 0.0002F;
        private bool _rampingEnabled = true;
        public bool rampingEnabled
        {
            get
            {
                return this._rampingEnabled;
            }
            set
            {
                this._rampingEnabled = value;

                if (this.loggingEnabled)
                {
                    this.PrintRampingState();
                }
            }
        }
        public bool loggingEnabled = true;
        public string loggingLabel = "Speed controller";
        private float lastLoggedValue = -1.0F;

        private float lastValue = 0.0F;
        private int lastTimestamp = -1;

        public RCDTalonSpeedController(string loggingLabel)
        {
            this.loggingLabel = loggingLabel;
            this.PrintRampingState();
        }

        private void PrintRampingState()
        {
            Debug.Print(this.loggingLabel + " Ramping: " + (this.rampingEnabled ? "ON" : "OFF"));
        }

        private void PrintOutputValue(float value)
        {
            if (this.loggingEnabled && value != this.lastLoggedValue)
            {
                Debug.Print(this.loggingLabel + " speed controller value: " + value);
                this.lastLoggedValue = value;
            }
        }

        public float ComputeCurrentValue()
        {
            return this.ComputeCurrentValue(this.targetValue);
        }

        public float ComputeCurrentValue(float target)
        {
            this.targetValue = target;
            int currentTimestamp = Utility.GetMachineTime().Milliseconds;
            if (rampingEnabled == false)
            {
                this.lastValue = targetValue;
                this.lastTimestamp = currentTimestamp;
                this.PrintOutputValue(targetValue);
            }

            if (lastTimestamp == -1) {
                this.lastTimestamp = currentTimestamp;
                this.PrintOutputValue(this.lastValue);
                return this.lastValue;
            }

            float deltaTarget = targetValue - lastValue;
            int deltaTime = currentTimestamp - lastTimestamp;
            if (deltaTime == 0)
            {
                this.PrintOutputValue(this.lastValue);
                return this.lastValue;
            }

            float proposedAcceleration = deltaTarget / deltaTime;
            float proposedAbsoluteAcceleration = (float)System.Math.Abs(proposedAcceleration);
            if (proposedAbsoluteAcceleration == 0.0F)
            {
                this.lastTimestamp = currentTimestamp;
                this.PrintOutputValue(this.lastValue);
                return this.lastValue;
            }
            float proposedSign = proposedAcceleration / proposedAbsoluteAcceleration;

            float absoluteAcceleration = (float)System.Math.Min(proposedAbsoluteAcceleration, maximumAbsoluteAccelerationPerMillisecond);
            float computedValue = absoluteAcceleration * deltaTime * proposedSign + lastValue;
            this.lastValue = computedValue;
            this.lastTimestamp = currentTimestamp;
            this.PrintOutputValue(computedValue);
            return computedValue;
        }

    }
}
