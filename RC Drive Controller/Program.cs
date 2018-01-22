using Microsoft.SPOT.Hardware;
using CTRE;
using Microsoft.SPOT;

namespace RCDriveController
{
    public class Program
    {
        private static byte currentRumbleLevel = 0;
        private static int[] rumbleDecrementTimestamps = new int[10];
        private static byte rumbleIncrementAmount = 75;

        public static void Main()
        {
            RCDGamepad gamepad = new RCDGamepad(new CTRE.UsbHostDevice());

            TalonSrx driveTalon = new TalonSrx(1);
            driveTalon.SetInverted(true);
            driveTalon.SetCurrentLimit(1);
            driveTalon.EnableCurrentLimit(true);
            driveTalon.ConfigNeutralMode(TalonSrx.NeutralMode.Coast);

            TalonSrx flywheelTalon = new TalonSrx(2);
            flywheelTalon.SetCurrentLimit(40);
            flywheelTalon.EnableCurrentLimit(true);
            flywheelTalon.ConfigNeutralMode(TalonSrx.NeutralMode.Coast);

            PigeonImu centerOfMassIMU = new PigeonImu(1);
            PigeonImu headIMU = new PigeonImu(2);

            uint pulsePeriod = 20000;
            uint pulseDuration = 1500;

            PWM pwm_tilt = new PWM(CTRE.HERO.IO.Port3.PWM_Pin9, pulsePeriod, pulseDuration, PWM.ScaleFactor.Microseconds, false);
            PWM pwm_headRoll = new PWM(CTRE.HERO.IO.Port3.PWM_Pin7, pulsePeriod, pulseDuration, PWM.ScaleFactor.Microseconds, false);
            PWM pwm_headPitch = new PWM(CTRE.HERO.IO.Port3.PWM_Pin6, pulsePeriod, pulseDuration, PWM.ScaleFactor.Microseconds, false);
            PWM pwm_headSpin = new PWM(CTRE.HERO.IO.Port3.PWM_Pin8, pulsePeriod, pulseDuration, PWM.ScaleFactor.Microseconds, false);
            PWM pwm_disco = new PWM(CTRE.HERO.IO.Port3.PWM_Pin4, pulsePeriod, pulseDuration, PWM.ScaleFactor.Microseconds, false);

            pwm_tilt.Start();
            pwm_headRoll.Start(); 
            pwm_headPitch.Start(); 
            pwm_headSpin.Start();
            pwm_disco.Start();

            RCDTalonSpeedController driveSpeedController = new RCDTalonSpeedController("Primary Drive");
            driveSpeedController.loggingEnabled = false;
            driveSpeedController.rampingEnabled = false;
            
            bool previousButtonX = false;
            bool previousButtonY = false;
            bool previousButtonB = false;
            bool previousButtonA = false;
            bool previousStickButtonL = false;
            bool previousStickButtonR = false;

            bool staticOperationMode = false;

            ConfigureBodyLightColors();
            int orientationCompensation = -1;

            while (true)
            {
                if (gamepad.GetConnectionStatus() != UsbDeviceConnection.Connected)
                {
                    continue;
                }

                CTRE.Watchdog.Feed();

                // Check for whether we need to flip orientation
                bool currentButtonY = gamepad.buttonY;
                if (!currentButtonY && previousButtonY)
                {
                    orientationCompensation *= -1;

                    // No (working) interface for haptic feedback currently exists
                    // RumbleForDuration(gamepad, 0.25);
                }
                previousButtonY = currentButtonY;

                // Check for whether we need to switch control modes between driving and static modes
                bool currentButtonB = gamepad.buttonB;
                if (!currentButtonB && previousButtonB)
                {
                    staticOperationMode = !staticOperationMode;

                    // No (working) interface for haptic feedback currently exists
                    // RumbleForDuration(gamepad, 0.25);
                }
                previousButtonB = currentButtonB;

                uint discoDuration = 0;

                // Check for whether we need to send happy beeps
                bool currentButtonA = gamepad.buttonA;
                if (currentButtonA)
                {
                    discoDuration = 1500;
                }
                previousButtonA = currentButtonA;

                // Check if we need to send sad beeps
                bool currentButtonX = gamepad.buttonX;
                if (currentButtonX)
                {
                    discoDuration = 2500;
                }
                previousButtonX = currentButtonX;

                // Check if we need to send volume down
                bool currentStickButtonL = gamepad.buttonLeftJoyClick;
                if (currentStickButtonL)
                {
                    discoDuration = 3500;
                }
                previousStickButtonL = currentStickButtonL;

                // Check if we need to send volume up
                bool currentStickButtonR = gamepad.buttonRightJoyClick;
                if (currentStickButtonR)
                {
                    discoDuration = 4500;
                }
                previousStickButtonR = currentStickButtonR;
                pwm_disco.Duration = discoDuration;

                /* Capture button states for gamepad  */
                Vector leftVec = gamepad.leftVector;
                Vector rightVec = gamepad.rightVector;
                bool leftBumper = gamepad.leftBumper;
                bool rightBumper = gamepad.rightBumper;
                bool leftTrigger = gamepad.leftTrigger;
                bool rightTrigger = gamepad.rightTrigger;

                uint servoZero = 1500;
                pwm_headRoll.Duration = (uint)((rightVec.x * -600 * orientationCompensation) + servoZero);
                pwm_headPitch.Duration = (uint)((rightVec.y * 600 * orientationCompensation) + servoZero);

                if (staticOperationMode)
                {
                    // Not driving or tilting while in static mode
                    driveTalon.Set(0);
                    pwm_tilt.Duration = servoZero;

                    pwm_headSpin.Duration = (uint)((leftVec.x * 600 * orientationCompensation) + servoZero);

                } else
                {
                    // Drive forward/backward
                    float driveValue = leftVec.y * orientationCompensation;
                    driveValue = driveSpeedController.ComputeCurrentValue(driveValue);
                    driveTalon.Set(driveValue);

                    uint headSpinScalar = 500;
                    uint headSpinPosition = servoZero;
                    headSpinPosition += (uint)(leftTrigger ? -headSpinScalar * orientationCompensation : 0); 
                    headSpinPosition += (uint)(rightTrigger ? headSpinScalar * orientationCompensation : 0);
                    pwm_headSpin.Duration = headSpinPosition;

                    pwm_tilt.Duration = (uint)((leftVec.x * 600 * orientationCompensation) + servoZero);
                }

                /* Throttle buttons are additive so they cancel if pressed simultaneously */
                float flywheelThrottle = 0;
                float flywheelMagnitude = 1.0F;
                flywheelThrottle += leftBumper ? flywheelMagnitude : 0;
                flywheelThrottle += rightBumper ? -flywheelMagnitude : 0;
                flywheelTalon.Set(flywheelThrottle);

                // No current (working) interface for Logitech remote's rumble feature.
                // Commenting out so we don't waste cycles
                // UpdateRumbleState(gamepad);

                // Wait a bit
                System.Threading.Thread.Sleep(5);
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

        private static void RumbleForDuration(Gamepad gamepad, double durationInSeconds)
        {
            int currentTimeMilliseconds = Utility.GetMachineTime().Milliseconds;
            int futureTimeToEndRumbling = currentTimeMilliseconds + (int)(durationInSeconds * 1000.0);
            for (int i = 0; i < 10; i++)
            {
                int valueAtIndex = rumbleDecrementTimestamps[i];
                if (valueAtIndex == 0)
                {
                    rumbleDecrementTimestamps[i] = futureTimeToEndRumbling;
                    break;
                }
            }
            byte newRumbleValue = (byte)System.Math.Min(System.Math.Max(currentRumbleLevel + rumbleIncrementAmount, 0), 255);
            currentRumbleLevel = newRumbleValue;
            Debug.Print("Incrementing rumble to: " + newRumbleValue);
            gamepad.SetRumble(newRumbleValue, newRumbleValue);
        }

        private static void UpdateRumbleState(Gamepad gamepad)
        {
            int currentTimeMilliseconds = Utility.GetMachineTime().Milliseconds;
            byte newRumbleValue = currentRumbleLevel;
            for (int i = 0; i < 10; i++)
            {
                int valueAtIndex = rumbleDecrementTimestamps[i];
                if ((valueAtIndex == 0) || (valueAtIndex > currentTimeMilliseconds)) {
                    continue;
                }
                rumbleDecrementTimestamps[i] = 0;
                newRumbleValue = (byte)System.Math.Min(System.Math.Max(newRumbleValue - rumbleIncrementAmount, 0), 255);
            }
            if (newRumbleValue != currentRumbleLevel)
            {
                Debug.Print("Decrementing rumble to: " + newRumbleValue);
                currentRumbleLevel = newRumbleValue;
            }
            gamepad.SetRumble(newRumbleValue, newRumbleValue);
        }
    }
}