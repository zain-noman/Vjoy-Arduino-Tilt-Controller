#define ROBUST

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;
using vJoyInterfaceWrap;

namespace ConsoleVersion
{
    class Program
    {
        static uint id = 0;
        static vJoy joystick;

        static void Main(string[] args)
        {
            SerialPort _port;
            float[] Thetas;
            joystick = new vJoy();
            Thetas = new float[3];
            Thetas[0] = 0;
            Thetas[1] = 0;
            Thetas[2] = 0;
            VJoyActivation();
            
            Console.WriteLine("insert COM port name: ");
            string COMportName = Console.ReadLine();
            Console.WriteLine("\ninsert baud rate: ");
            int baudRate = Int32.Parse(Console.ReadLine());

            _port = new SerialPort();
            _port.PortName = COMportName;
            _port.BaudRate = baudRate;
            _port.Parity = Parity.None;
            _port.DataBits =8;
            _port.StopBits = StopBits.One;
            _port.ReadTimeout = 500;
            _port.WriteTimeout = 500;
            _port.Open();

            long maxval = 0;
            joystick.GetVJDAxisMax(id, HID_USAGES.HID_USAGE_X, ref maxval);


            while (true)
            {
                try
                {
                    string a = _port.ReadLine();
                    string[] substrings = a.Split('|');
                    if (substrings.Length == 3)
                    {
                        Thetas[0] = clamp(float.Parse(substrings[0]),-50,50);
                        Thetas[1] = clamp(float.Parse(substrings[1]), -50, 50);
                        Thetas[2] = float.Parse(substrings[2]);
                        Console.WriteLine("ThetaX={0} ThetaY={1} ThetaZ={2}", Thetas[0], Thetas[1], Thetas[2]);
                    }
                }
                catch { }

                //normalizing xrot and mapping to full range ans letting x maxRot=50deg
                joystick.SetAxis(Convert.ToInt32( ((Thetas[0] / 100 )+0.5)* maxval ), id, HID_USAGES.HID_USAGE_X);
                joystick.SetAxis(Convert.ToInt32( ((Thetas[1] / 100 )+0.5)* maxval ), id, HID_USAGES.HID_USAGE_Y);


                //threshold of btn=20 deg  (btn1=R1 btn2=L1)
                if (Thetas[2] > 20)
                {
                    joystick.SetBtn(true, id, 1);
                    joystick.SetBtn(false, id, 2);
                }
                else if (Thetas[2] < -20)
                {
                    joystick.SetBtn(true, id, 2);
                    joystick.SetBtn(false, id, 1);
                }
                else
                {
                    joystick.SetBtn(false, id, 1);
                    joystick.SetBtn(false, id, 2);
                }

                Thread.Sleep(20);
            }
        }  

        static void VJoyActivation()
        {
            //Check ID
            Console.WriteLine("Joystick Number : ");
            id = UInt32.Parse(Console.ReadLine());
            if (id <= 0 || id > 16)
            {
                Console.WriteLine("Illegal device ID {0}\nExit!", id);
                return;
            }

            //Check Activaton
            if (!joystick.vJoyEnabled())
            {
                Console.WriteLine("vJoy driver not enabled: Failed Getting vJoy attributes.\n");
                return;
            }
            else
                Console.WriteLine("Vendor: {0}\nProduct :{1}\nVersion Number:{2}\n", joystick.GetvJoyManufacturerString(), joystick.GetvJoyProductString(), joystick.GetvJoySerialNumberString());

            // Get the state of the requested device
            VjdStat status = joystick.GetVJDStatus(id);
            switch (status)
            {
                case VjdStat.VJD_STAT_OWN:
                    Console.WriteLine("vJoy Device {0} is already owned by this feeder\n", id);
                    break;
                case VjdStat.VJD_STAT_FREE:
                    Console.WriteLine("vJoy Device {0} is free\n", id);
                    break;
                case VjdStat.VJD_STAT_BUSY:
                    Console.WriteLine("vJoy Device {0} is already owned by another feeder\nCannot continue\n", id);
                    return;
                case VjdStat.VJD_STAT_MISS:
                    Console.WriteLine("vJoy Device {0} is not installed or disabled\nCannot continue\n", id);
                    return;
                default:
                    Console.WriteLine("vJoy Device {0} general error\nCannot continue\n", id);
                    return;
            };


            // Test if DLL matches the driver
            UInt32 DllVer = 0, DrvVer = 0;
            bool match = joystick.DriverMatch(ref DllVer, ref DrvVer);
            if (match)
                Console.WriteLine("Version of Driver Matches DLL Version ({0:X})\n", DllVer);
            else
                Console.WriteLine("Version of Driver ({0:X}) does NOT match DLL Version ({1:X})\n", DrvVer, DllVer);


            // Acquire the target
            if ((status == VjdStat.VJD_STAT_OWN) || ((status == VjdStat.VJD_STAT_FREE) && (!joystick.AcquireVJD(id))))
            {
                Console.WriteLine("Failed to acquire vJoy device number {0}.\n", id);
                return;
            }
            else
                Console.WriteLine("Acquired: vJoy device number {0}.\n", id);

            Console.WriteLine("\npress enter to stat feeding");
            Console.ReadLine();
        }

        static float clamp(float val,float min, float max)
        {
            if (val < min)
                return val;
            else if (val > max)
                return max;
            else
                return val;
        }
    }
}
