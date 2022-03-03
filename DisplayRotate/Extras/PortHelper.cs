/*using System;
using System.Diagnostics;
using System.IO;
using System.Management;

namespace DisplayRotate.Extras
{
    public static class PortHelper
    {
        public static bool RestartSerialPort(string serialPort)
        {
            string[] hwids = { };
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Caption like '%(COM%'"))
            {
                foreach (ManagementBaseObject obj in searcher.Get())
                {
                    if (obj.Properties["Name"].Value.ToString().Contains(serialPort))
                    {
                        hwids = (string[]) obj.Properties["HardwareID"].Value;
                    }
                }
            }
            if (hwids != null && hwids.Length != 0)
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "devconx64.exe");
                Process process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = path, Arguments = $"restart \"{hwids[1]}\"", UseShellExecute = false, RedirectStandardOutput = true, CreateNoWindow = true
                    }
                };
                process.Start();
                while (!process.StandardOutput.EndOfStream)
                {
                    string line = process.StandardOutput.ReadLine();
                    // do something with line
                    if (line != null && line.Contains("Restarted"))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}*/