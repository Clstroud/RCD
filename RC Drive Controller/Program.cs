using Microsoft.SPOT.Hardware;
using CTRE;

namespace RCDriveController
{
    public class Program
    {
        public static void Main()
        {
            RCDGamepad gamepad = new RCDGamepad(new CTRE.UsbHostDevice());

            TalonSrx driveTalon = new TalonSrx(1);
            driveTalon.SetInverted(true);
            driveTalon.SetCurrentLimit(1);
            driveTalon.EnableCurrentLimit(true);

            TalonSrx flywheelTalon = new TalonSrx(2);
            flywheelTalon.SetCurrentLimit(40);
            flywheelTalon.EnableCurrentLimit(true);

            PigeonImu centerOfMassIMU = new PigeonImu(1);
            PigeonImu headIMU = new PigeonImu(2);

            uint pulsePeriod = 20000;
            uint pulseDuration = 1500;

            PWM pwm_tilt = new PWM(CTRE.HERO.IO.Port3.PWM_Pin6, pulsePeriod, pulseDuration, PWM.ScaleFactor.Microseconds, false);
            PWM pwm_headRoll = new PWM(CTRE.HERO.IO.Port3.PWM_Pin7, pulsePeriod, pulseDuration, PWM.ScaleFactor.Microseconds, false);
            PWM pwm_headPitch = new PWM(CTRE.HERO.IO.Port3.PWM_Pin9, pulsePeriod, pulseDuration, PWM.ScaleFactor.Microseconds, false);
            PWM pwm_headSpin = new PWM(CTRE.HERO.IO.Port3.PWM_Pin8, pulsePeriod, pulseDuration, PWM.ScaleFactor.Microseconds, false);

            pwm_tilt.Start();
            pwm_headRoll.Start(); 
            pwm_headPitch.Start(); 
            pwm_headSpin.Start();

            RCDTalonSpeedController driveSpeedController = new RCDTalonSpeedController("Primary Drive");
            driveSpeedController.loggingEnabled = true;
            driveSpeedController.rampingEnabled = false;

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

                // Check if we need to toggle manual ramping on the drive speed controller
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
                
                // Check for whether we need to flip orientation
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
                float driveValue = leftVec.y * orientationCompensation;
                driveValue = driveSpeedController.ComputeCurrentValue(driveValue);
                driveTalon.Set(driveValue);

                /* Throttle buttons are additive so they cancel if pressed simultaneously */
                float buttonThrottle = 0;
                float maxFlywheelVelocity = 1.0F;
                buttonThrottle += gamepad.leftBumper  ? -maxFlywheelVelocity * orientationCompensation : 0;
                buttonThrottle += gamepad.rightBumper ?  maxFlywheelVelocity * orientationCompensation : 0;
                flywheelTalon.Set(buttonThrottle);

                /* Head spin speed modifiers are additive, so they cancel if pressed simultaneously */

                uint servoZero = 1500;
                uint headSpinScalar = 500;
                uint headSpinPosition = servoZero;
                headSpinPosition += (uint)(gamepad.leftTrigger  ? -headSpinScalar * orientationCompensation: 0); // Left trigger turns the head slightly faster
                headSpinPosition += (uint)(gamepad.rightTrigger ?  headSpinScalar * orientationCompensation: 0); // Right trigger turns the head slightly slower
                pwm_headSpin.Duration = headSpinPosition;

                pwm_headRoll.Duration = (uint)((gamepad.rightVector.y * 600 * orientationCompensation) + servoZero);
                pwm_headPitch.Duration = (uint)((gamepad.rightVector.x * 600 * orientationCompensation) + servoZero);
                pwm_tilt.Duration = (uint)((leftVec.x * -600 * orientationCompensation) + servoZero);
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