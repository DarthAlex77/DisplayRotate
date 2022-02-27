/* 
 * This file is part of the AutoDisplayRotateApp distribution (https://github.com/DarthAlex77/DisplayRotate).
 * Copyright (c) 2022 Aleksey Surgaev.
 * 
 * This program is free software: you can redistribute it and/or modify  
 * it under the terms of the GNU General Public License as published by  
 * the Free Software Foundation, version 3.
 *
 * This program is distributed in the hope that it will be useful, but 
 * WITHOUT ANY WARRANTY; without even the implied warranty of 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU 
 * General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License 
 * along with this program. If not, see <http://www.gnu.org/licenses/>.
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using DisplayRotate.Extras;

namespace DisplayRotate.Model
{
    internal static class Worker
    {
        private static readonly AutoResetEvent ResetEvent = new AutoResetEvent(false);
        private static          string         _serialPortResponse;
        private static readonly Stopwatch      Timer = new Stopwatch();
        private static readonly int            PollingFrequency;

        static Worker()
        {
            PollingFrequency = Convert.ToInt32(ConfigurationManager.AppSettings["PollingFrequency"]);
        }

        internal static void MpuWorker(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker  = (BackgroundWorker) sender;
            List<object>     args    = (List<object>) e.Argument;
            Arduino          arduino = (Arduino) args[0];
            Display          display = (Display) args[1];
            bool?            reset   = (bool?) args[2]; //True to ResetMpu, False to CalibrateMpu,Null to InitializeMpu
            _serialPortResponse = string.Empty;
            try
            {
                worker.ReportProgress(1, arduino.SerialPort.PortName);
                arduino.SerialPort.Open();
                Task.Delay(arduino.SerialPort.WriteTimeout).Wait();
                switch (reset)
                {
                    case null:
                        arduino.SerialPort.Write($"1{(byte) display.MpuAddress}");
                        break;
                    case true:
                        arduino.SerialPort.Write($"5{(byte) display.MpuAddress}");
                        break;
                    default:
                        arduino.SerialPort.Write($"4{(byte) display.MpuAddress}");
                        break;
                }
                worker.ReportProgress(2, display.DisplayName);
                Timer.Start();
                while (true)
                {
                    ResetEvent.WaitOne();
                    if (_serialPortResponse.Contains("OK"))
                    {
                        break;
                    }
                    if (Timer.Elapsed.TotalSeconds > 15)
                    {
                        e.Cancel = true;
                        Task.Delay(arduino.SerialPort.WriteTimeout).Wait();
                        arduino.SerialPort.Write("6");
                        break;
                    }
                }
                Timer.Reset();
                arduino.SerialPort.Close();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
            _serialPortResponse = string.Empty;
            worker.ReportProgress(e.Cancel ? 4 : 3, display.DisplayName);
        }

        internal static void GetDataWorker(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker  = (BackgroundWorker) sender;
            Arduino          arduino = (Arduino) e.Argument;
            try
            {
                arduino.SerialPort.Open();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
                foreach (Display arduinoDisplay in arduino.Displays)
                {
                    arduinoDisplay.IsStarted = false;
                }
                return;
            }
            while (!worker.CancellationPending)
            {
                _serialPortResponse = string.Empty;
                IEnumerable<Display> displays = arduino.Displays.Where(x => x.IsStarted).ToList();
                foreach (Display arduinoDisplay in displays)
                {
                    //Get Connection status
                    Task.Delay(arduino.SerialPort.WriteTimeout).Wait();
                    arduino.SerialPort.WriteLine($"2{(byte) arduinoDisplay.MpuAddress}");
                    ResetEvent.WaitOne();
                    bool? result = _serialPortResponse.ToBoolean();
                    if (result != null)
                    {
                        arduinoDisplay.ConnectionStatus = (bool) result;
                        _serialPortResponse             = string.Empty;
                        Timer.Reset();
                    }
                    else
                    {
                        arduinoDisplay.ConnectionStatus = false;
                    }
                    //Get Sensor Data
                    Task.Delay(arduino.SerialPort.WriteTimeout).Wait();
                    arduino.SerialPort.WriteLine($"3{(byte) arduinoDisplay.MpuAddress}");
                    ResetEvent.WaitOne();
                    string[] data = _serialPortResponse.TrimEnd('\r', '\n').Split(';');
                    if (short.TryParse(data[0].Trim(';'), out short x) && short.TryParse(data[1].Trim(';'), out short y))
                    {
                        arduinoDisplay.X    = x;
                        arduinoDisplay.Y    = y;
                        _serialPortResponse = string.Empty;
                    }
                }
                Task.Delay(PollingFrequency - arduino.SerialPort.WriteTimeout * displays.Count()).Wait();
            }
            _serialPortResponse = string.Empty;
            arduino.SerialPort.Close();
        }

        public static void DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort serialPort = (SerialPort) sender;
            _serialPortResponse += serialPort.ReadExisting();
            ResetEvent.Set();
        }
    }
}