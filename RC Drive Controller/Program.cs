using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using CTRE;

namespace RCDriveController
{
    public class Program
    {
        public static void Main()
        {
            /* create a gamepad object */
            UsbHostDevice usbDevice = new CTRE.UsbHostDevice();
            RCDGamepad myGamepad = new RCDGamepad(usbDevice);

            /* create a talon, the Talon Device ID in HERO LifeBoat is zero */
            CTRE.TalonSrx myTalon = new CTRE.TalonSrx(1);
            CTRE.TalonSrx myTalon1 = new CTRE.TalonSrx(2);
            uint period = 20000; //period between pulses
            uint duration = 1500; //duration of pulse
            PWM pwm_Pin7 = new PWM(CTRE.HERO.IO.Port3.PWM_Pin7, period, duration, PWM.ScaleFactor.Microseconds, false);
            PWM pwm_Pin9 = new PWM(CTRE.HERO.IO.Port3.PWM_Pin9, period, duration, PWM.ScaleFactor.Microseconds, false);
            PWM pwm_Pin6 = new PWM(CTRE.HERO.IO.Port3.PWM_Pin6, period, duration, PWM.ScaleFactor.Microseconds, false);
            PWM pwm_Pin8 = new PWM(CTRE.HERO.IO.Port3.PWM_Pin8, period, duration, PWM.ScaleFactor.Microseconds, false);

            pwm_Pin7.Start(); //starts the signal
            pwm_Pin9.Start(); //starts the signal
            pwm_Pin6.Start(); //starts the signal
            pwm_Pin8.Start(); //starts the signal

            float buttonThrottle = 0;
            uint pin8duration; // head spin duration

            string lastButtons = "";

            /* loop forever */
            while (true)
            {

                /* added inside the while loop */
                if (myGamepad.GetConnectionStatus() == CTRE.UsbDeviceConnection.Connected)
                {

                    /* print the axis value */
                    //Debug.Print("axis:" + myGamepad.GetAxis(1));

                    string buttons = myGamepad.buttonConfigurationDebugString();
                    if (lastButtons != buttons)
                    {
                        Debug.Print("Buttons: " + buttons);
                        lastButtons = buttons;
                    }

                    /* pass axis value to talon */
                    myTalon.Set(myGamepad.GetAxis(1));
                    if (myGamepad.GetButton(5))   //This is the Left Bumper button.
                    {
                        buttonThrottle = -1;
                    }
                    else if (myGamepad.GetButton(6))   //This is the Right Bumper button
                    {
                        buttonThrottle = 1;
                    }
                    else
                    {
                        buttonThrottle = 0;
                    }
                    myTalon1.Set(buttonThrottle);
                    pwm_Pin7.Duration = (uint)((myGamepad.GetAxis(2) * 625) + 1500);
                    pwm_Pin9.Duration = (uint)((myGamepad.GetAxis(5) * 625) + 1500);
                    pwm_Pin6.Duration = (uint)((myGamepad.GetAxis(0) * 1000) + 1500);
                    pin8duration = (uint)(myGamepad.GetButton(7) ? 1000 : 1500);
                    pin8duration = (uint)(myGamepad.GetButton(8) ? 2000 : pin8duration);
                    pwm_Pin8.Duration = pin8duration;

                    CTRE.Watchdog.Feed();

                }
                /* increment counter */
                /* wait a bit */
               // System.Threading.Thread.Sleep(10);
            }
        }
    }
}