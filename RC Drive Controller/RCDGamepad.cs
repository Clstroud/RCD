using CTRE;
using System;

namespace RCDriveController
{
    public class RCDGamepad : Gamepad
    {
        private static double JoystickMovementMinimumThreshold = 0.05;

        public RCDGamepad(ISingleGamepadValuesProvider provider) : base(provider)
        {

        }

        public bool buttonY
        {
            get
            {
                return this.GetButton(4);
            }
        }

        public bool buttonX
        {
            get
            {
                return this.GetButton(1);
            }
        }

        public bool buttonA
        {
            get
            {
                return this.GetButton(2);
            }
        }

        public bool buttonB
        {
            get
            {
                return this.GetButton(3);
            }
        }

        public bool buttonRB
        {
            get
            {
                return this.GetButton(6);
            }
        }

        public bool buttonRT
        {
            get
            {
                return this.GetButton(8);
            }
        }

        public bool buttonLT
        {
            get
            {
                return this.GetButton(7);
            }
        }

        public bool buttonLB
        {
            get
            {
                return this.GetButton(5);
            }
        }

        public bool buttonBack
        {
            get
            {
                return this.GetButton(9);
            }
        }

        public bool buttonStart
        {
            get
            {
                return this.GetButton(10);
            }
        }

        public bool buttonLeftJoyClick
        {
            get
            {
                return this.GetButton(11);
            }
        }

        public bool buttonRightJoyClick
        {
            get
            {
                return this.GetButton(12);
            }
        }

        public Vector leftVector
        {
            get
            {
                return this.normalizedJoystickVector(this.GetAxis(0), this.GetAxis(1));
            }
        }

        public Vector rightVector
        {
            get
            {
                return this.normalizedJoystickVector(this.GetAxis(5), this.GetAxis(2));
            }
        }

        private Vector normalizedJoystickVector(double x, double y)
        {
            double xNormalized = Math.Abs(x) > JoystickMovementMinimumThreshold ? x : 0.0;
            double yNormalized = Math.Abs(y) > JoystickMovementMinimumThreshold ? y : 0.0;
            return new Vector(xNormalized, yNormalized);
        }

        public string buttonConfigurationDebugString()
        {
            string buttons = "";

            if (this.buttonY)
            {
                buttons += "[Y]";
            }
            if (this.buttonX)
            {
                buttons += "[X]";
            }
            if (this.buttonA)
            {
                buttons += "[A]";
            }
            if (this.buttonB)
            {
                buttons += "[B]";
            }
            if (this.buttonRB)
            {
                buttons += "[RB]";
            }
            if (this.buttonRT)
            {
                buttons += "[RT]";
            }
            if (this.buttonLB)
            {
                buttons += "[LB]";
            }
            if (this.buttonLT)
            {
                buttons += "[LT]";
            }

            if (this.buttonLeftJoyClick)
            {
                buttons += "[LeftJoy Click]";
            }

            if (this.buttonRightJoyClick)
            {
                buttons += "[RightJoy Click]";
            }

            if (this.leftVector != Vector.zero)
            {
                buttons += "[Left Vector: " + this.leftVector + "]";
            }

            if (this.rightVector != Vector.zero)
            {
                buttons += "[Right Vector: " + this.rightVector + "]";
            }

            return buttons;
        }
    }
}

