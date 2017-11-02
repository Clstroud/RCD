//using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using CTRE;
using System;

namespace RCDriveController
{
    public class Program
    {
        public static void Main()
        {
            RCDGamepad gamepad = new RCDGamepad(new CTRE.UsbHostDevice());

            TalonSrx driveTalon = new TalonSrx(1);  // Drive Talon
            driveTalon.SetInverted(true);
            driveTalon.SetCurrentLimit(1);
            driveTalon.EnableCurrentLimit(true);

            TalonSrx flywheelTalon = new TalonSrx(2);  // Flywheel Talon
            flywheelTalon.SetCurrentLimit(40);
            flywheelTalon.EnableCurrentLimit(true);

            PigeonImu centerOfMassIMU = new PigeonImu(1);
            PigeonImu headIMU = new PigeonImu(2);

            uint pulsePeriod = 20000;
            uint pulseDuration = 1500;
            PWM pwm_Pin7 = new PWM(CTRE.HERO.IO.Port3.PWM_Pin7, pulsePeriod, pulseDuration, PWM.ScaleFactor.Microseconds, false); // Head forward/backward
            PWM pwm_Pin9 = new PWM(CTRE.HERO.IO.Port3.PWM_Pin9, pulsePeriod, pulseDuration, PWM.ScaleFactor.Microseconds, false); // Head side/side
            PWM pwm_tilt = new PWM(CTRE.HERO.IO.Port3.PWM_Pin6, pulsePeriod, pulseDuration, PWM.ScaleFactor.Microseconds, false); // Tilt steer servo
            PWM pwm_Pin8 = new PWM(CTRE.HERO.IO.Port3.PWM_Pin8, pulsePeriod, pulseDuration, PWM.ScaleFactor.Microseconds, false); // Head spin

            pwm_Pin7.Start(); 
            pwm_Pin9.Start(); 
            pwm_tilt.Start(); 
            pwm_Pin8.Start();

            RCDTalonSpeedController driveSpeedController = new RCDTalonSpeedController("Primary Drive");
            driveSpeedController.loggingEnabled = true;
            driveSpeedController.rampingEnabled = false;

            /*
            RCDTalonSpeedController flywheelSpeedController = new RCDTalonSpeedController("Flywheel");
            flywheelSpeedController.rampingEnabled = false;
            flywheelSpeedController.loggingEnabled = false;

            RCDTalonSpeedController tiltSpeedController = new RCDTalonSpeedController("Tilt Servo");
            tiltSpeedController.rampingEnabled = false;
            tiltSpeedController.loggingEnabled = false;
            */
            //bool previousButtonB = false;
            bool previousButtonX = false;
            bool previousButtonY = false;

            ConfigureBodyLightColors();
            int orientationCompensation = -1;

            while (true)
            {
                if (gamepad.GetConnectionStatus() != UsbDeviceConnection.Connected)
                {
                    continue;
                }

                CTRE.Watchdog.Feed();
                /*
                if (gamepad.buttonB && !previousButtonB)
                {
                    flywheelSpeedController.rampingEnabled = !flywheelSpeedController.rampingEnabled;
                    previousButtonB = true;
                }

                if (!gamepad.buttonB && previousButtonB)
                {
                    previousButtonB = false;
                }
                */
                bool currentButtonX = gamepad.buttonX;
                if (currentButtonX && !previousButtonX)
                {
                    driveSpeedController.rampingEnabled = !driveSpeedController.rampingEnabled;
                    previousButtonX = true;
                }

                if (!currentButtonX && previousButtonX)
                {
                    previousButtonX = false;
                }
                
                bool currentButtonY = gamepad.buttonY;
                if (currentButtonY && !previousButtonY)
                {
                    orientationCompensation *= -1;
                }
                if (!currentButtonY && previousButtonY)
                {
                    previousButtonY = false;
                }

                /* Compute a value for the drive talon */
                Vector leftVec = gamepad.leftVector;
                //float driveSign = (Math.Abs(leftVec.y) != 0.0) ? (leftVec.y / (float)Math.Abs(leftVec.y)) : -1.0F;
                //float radius = (float)Math.Sqrt(leftVec.x * leftVec.x + leftVec.y * leftVec.y);
                //float value = (float)Math.Min(1.0, radius);
                float driveValue = leftVec.y * orientationCompensation; //value* driveSign *
                driveValue = driveSpeedController.ComputeCurrentValue(driveValue);
                driveTalon.Set(driveValue);

                /*
                float position = driveTalon.GetPosition();
                if (position != 0.0F)
                {
                    Debug.Print("Encoder position: " + position);
                }
                */

                /* Throttle buttons are additive so they cancel if pressed simultaneously */
                float buttonThrottle = 0;
                float maxFlywheelVelocity = 1.0F;// 0.85F; // 1.0 Previously
                buttonThrottle += gamepad.leftBumper  ? -maxFlywheelVelocity * orientationCompensation : 0;
                buttonThrottle += gamepad.rightBumper ?  maxFlywheelVelocity * orientationCompensation : 0;

                //float computedFlywheelValue = flywheelSpeedController.ComputeCurrentValue(buttonThrottle);
                flywheelTalon.Set(buttonThrottle);//computedFlywheelValue);

                /* Head spin speed modifiers are additive, so they cancel if pressed simultaneously */

                uint servoZero = 1500;
                uint headSpinScalar = 500; // 500 Previously
                uint headSpinPosition = servoZero;
                headSpinPosition += (uint)(gamepad.leftTrigger  ? -headSpinScalar * orientationCompensation: 0); // Left trigger turns the head slightly faster
                headSpinPosition += (uint)(gamepad.rightTrigger ?  headSpinScalar * orientationCompensation: 0); // Right trigger turns the head slightly slower
                pwm_Pin8.Duration = headSpinPosition;

                pwm_Pin7.Duration = (uint)((gamepad.rightVector.y * 600 * orientationCompensation) + servoZero); // 625 Previously 360 debug
                pwm_Pin9.Duration = (uint)((gamepad.rightVector.x * 600 * orientationCompensation) + servoZero); // 625 Previously 360 debug

                //float tiltSign = (Math.Abs(leftVec.x) != 0.0) ? (leftVec.x / (float)Math.Abs(leftVec.x)) : 1.0F;
                //float tiltClamped = (float)Math.Min(1.0, Math.Abs(leftVec.x) / 0.75F);
                //float tiltValue = tiltSpeedController.ComputeCurrentValue(leftVec.x);//tiltClamped * tiltSign);
                pwm_tilt.Duration = (uint)((leftVec.x * -600 * orientationCompensation) + servoZero);  // 1000 Previously

                /*
                if (gamepad.leftVector.x != 0.0F)
                {
                    float current = driveTalon.Get();
                    float newValue = current + gamepad.leftVector.x * 0.01F;
                    driveTalon.Set(newValue);
                }
                */

                /*
                if (centerOfMassIMU.GetState() == PigeonImu.PigeonState.Ready)
                {
                    float[] pryC = { 0.0F, 0.0F, 0.0F };
                    float[] pryH = { 0.0F, 0.0F, 0.0F };

                    centerOfMassIMU.GetYawPitchRoll(pryC);
                    centerOfMassIMU.GetYawPitchRoll(pryH);

                    Debug.Print("Center of Mass IMU -> Yaw:" + pryC[0] + "   " + "Pitch:" + pryC[1] + "   " + "Roll:" + pryC[2]);
                    Debug.Print("Head IMU -> Yaw:" + pryH[0] + "   " + "Pitch:" + pryH[1] + "   " + "Roll:" + pryH[2]);
                }
                */
            }
        }

        private static void ConfigureBodyLightColors()
        {
            const uint kNumOfNeoPixels = 30;
            HeroPixel pixelStrip = new HeroPixel(HeroPixel.OFF, kNumOfNeoPixels);

            pixelStrip.setStripColor(HeroPixel.WHITE);  // Set default color for entire strip

            uint[] blueIndices = { 0, 5, 3 };          // Pixel indices that should be blue
            uint[] redIndices  = { 1, 6, 4 };          // Pixel indices that should be red
            uint[] yellowIndices = { 2, 7, 5 };        // Pixel indices that should be yellow

            SetColorForIndices(pixelStrip, HeroPixel.BLUE, blueIndices);
            SetColorForIndices(pixelStrip, HeroPixel.RED, redIndices);
            SetColorForIndices(pixelStrip, HeroPixel.YELLOW, yellowIndices);

            pixelStrip.writeOutput();

            // TODO: Add ability to schedule lights to blink on an interval, etc.
        }

        static void SetColorForIndices(HeroPixel pixelStrip, uint aColor, uint[] indices)
        {
            foreach (uint index in indices)
            {
                pixelStrip.setColor(aColor, index, 1);
            }
        }
    }
}