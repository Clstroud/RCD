using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using CTRE;

namespace RCDriveController
{
    public class Program
    {
        public static void Main()
        {
            RCDGamepad gamepad = new RCDGamepad(new CTRE.UsbHostDevice());

            TalonSrx talon1 = new TalonSrx(1);  // TODO: Document which Talon this is
            TalonSrx talon2 = new TalonSrx(2);  // TODO: Document which Talon this is

            uint pulsePeriod = 20000;
            uint pulseDuration = 1500;
            PWM pwm_Pin7 = new PWM(CTRE.HERO.IO.Port3.PWM_Pin7, pulsePeriod, pulseDuration, PWM.ScaleFactor.Microseconds, false); // TODO: Document which PWM this is
            PWM pwm_Pin9 = new PWM(CTRE.HERO.IO.Port3.PWM_Pin9, pulsePeriod, pulseDuration, PWM.ScaleFactor.Microseconds, false); // TODO: Document which PWM this is
            PWM pwm_Pin6 = new PWM(CTRE.HERO.IO.Port3.PWM_Pin6, pulsePeriod, pulseDuration, PWM.ScaleFactor.Microseconds, false); // TODO: Document which PWM this is
            PWM pwm_Pin8 = new PWM(CTRE.HERO.IO.Port3.PWM_Pin8, pulsePeriod, pulseDuration, PWM.ScaleFactor.Microseconds, false); // TODO: Document which PWM this is

            pwm_Pin7.Start(); 
            pwm_Pin9.Start(); 
            pwm_Pin6.Start(); 
            pwm_Pin8.Start(); 

            while (true)
            {
                if (gamepad.GetConnectionStatus() != UsbDeviceConnection.Connected)
                {
                    continue;
                }

                /* Pass Y-Axis value directly to the Talon */
                talon1.Set(gamepad.leftVector.y);

                /* Throttle buttons are additive so they cancel if pressed simultaneously */
                float buttonThrottle = 0;
                buttonThrottle += gamepad.leftBumper  ? -1 : 0;
                buttonThrottle += gamepad.rightBumper ?  1 : 0;
                talon2.Set(buttonThrottle);


                /* Head spin speed modifiers are additive, so they cancel if pressed simultaneously */
                uint headSpinDuration = 1500; // Default head spin duration
                headSpinDuration += (uint)(gamepad.leftTrigger  ? -500 : 0); // Left trigger turns the head slightly faster
                headSpinDuration += (uint)(gamepad.rightTrigger ?  500 : 0); // Right trigger turns the head slightly slower
                pwm_Pin8.Duration = headSpinDuration;

                pwm_Pin7.Duration = (uint)((gamepad.rightVector.y * 625) + 1500);
                pwm_Pin9.Duration = (uint)((gamepad.rightVector.x * 625) + 1500);
                pwm_Pin6.Duration = (uint)((gamepad.leftVector.x * 1000) + 1500);

                CTRE.Watchdog.Feed();
            }
        }
    }
}