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
using System.IO.Ports;
using System.Linq;
using System.Windows;
using DevExpress.Mvvm;
using DisplayRotate.Extras;
using DisplayRotate.Model;
using Newtonsoft.Json;

namespace DisplayRotate.ViewModel
{
    internal class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            //ResetSerialPortCommand = new DelegateCommand(ResetSerialPort, CanResetSerialPort);
            InitializeCommand      = new DelegateCommand(Initialize,      CanRun);
            StartCommand           = new DelegateCommand(Start,           CanStart);
            StopCommand            = new DelegateCommand(Stop,            CanStop);
            CalibrateCommand       = new DelegateCommand(Calibrate,       CanRun);
            ResetCommand           = new DelegateCommand(Reset,           CanRun);
            SaveDataCommand        = new DelegateCommand(SaveData);
            InitializeList();
        }

        #region Methods

        private bool CanResetSerialPort()
        {
            return SelectedArduino != null && SelectedArduino.Displays.All(x => !x.IsStarted) && SelectedArduino.Worker == null && SelectedArduino.SerialPort.PortName != "None";
        }

        // private void ResetSerialPort()
        // {
        //     if (!PortHelper.RestartSerialPort(SelectedArduino.SerialPort.PortName))
        //     {
        //         MessageBox.Show($"Restarting {SelectedArduino.SerialPort.PortName} Failed");
        //     }
        // }

        private bool CanRun()
        {
            if (SelectedDisplay != null)
            {
                Arduino arduino = Arduinos.First(x => x.SerialPort.PortName == SelectedDisplay.SerialPortName);
                if (arduino.Worker == null || arduino.Worker.IsBusy == false)
                {
                    return true;
                }
            }
            return false;
        }

        private bool CanStop()
        {
            return SelectedDisplay?.IsStarted == true;
        }

        private bool CanStart()
        {
            if (SelectedDisplay!=null)
            {
                Arduino arduino = Arduinos.First(x => x.SerialPort.PortName == SelectedDisplay.SerialPortName);
                if(arduino.Displays.All(x=>x.IsStarted==false)&&arduino.Worker!=null)
                {
                    return false;
                }
                if (!SelectedDisplay.IsStarted && SelectedDisplay.SerialPortName != "None" && SelectedDisplay.MpuAddress != MpuAddress.None)
                {
                    return true;
                }
            }
            return false;
        }

        private void Reset()
        {
            Arduinos.First(x => x.SerialPort.PortName == SelectedDisplay.SerialPortName).MpuWorker(SelectedDisplay, true);
        }

        private void Calibrate()
        {
            Arduinos.First(x => x.SerialPort.PortName == SelectedDisplay.SerialPortName).MpuWorker(SelectedDisplay, false);
        }

        private void Stop()
        {
            SelectedDisplay.IsStarted = false;
        }

        private void Start()
        {
            SelectedDisplay.IsStarted = true;
        }

        private void Initialize()
        {
            Arduinos.First(x => x.SerialPort.PortName == SelectedDisplay.SerialPortName).MpuWorker(SelectedDisplay, null);
        }

        private void SaveData()
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["Displays"].Value = JsonConvert.SerializeObject(Arduinos.SelectMany(arduino => arduino.Displays).ToList());
            config.Save(ConfigurationSaveMode.Modified);
        }

        private void InitializeList()
        {
            bool          autoStart     = Convert.ToBoolean(ConfigurationManager.AppSettings["AutoStart"]);
            string        displayString = ConfigurationManager.AppSettings["Displays"];
            List<Display> displays      = null;
            if (!string.IsNullOrWhiteSpace(displayString))
            {
                try
                {
                    displays = JsonConvert.DeserializeObject<List<Display>>(displayString);
                }
                catch (Exception)
                {
                    /*ignored*/
                }
                if (displays == null || displays.Count == 0)
                {
                    displays = Display.GetDisplays();
                }
            }
            else
            {
                displays = Display.GetDisplays();
            }
            //
            Arduinos = new BindingList<Arduino>();
            foreach (string portName in SerialPort.GetPortNames())
            {
                Arduinos.Add(new Arduino(portName));
            }
            Arduinos.Add(new Arduino("None"));
            foreach (Display display in displays.ToList())
            {
                Arduino arduino = Arduinos.FirstOrDefault(x => x.SerialPort.PortName == display.SerialPortName);
                if (arduino != null)
                {
                    arduino.Displays.Add(display);
                    displays.Remove(display);
                }
                else
                {
                    Arduinos.First(x => x.SerialPort.PortName == "None").Displays.Add(display);
                }
            }
            foreach (Arduino arduino in Arduinos)
            {
                if (arduino.SerialPort.PortName != "None" && autoStart)
                {
                    foreach (Display arduinoDisplay in arduino.Displays)
                    {
                        arduinoDisplay.IsStarted = true;
                    }
                }
                arduino.DisplayProgressChanged += WorkerReport;
            }
        }

        private void WorkerReport(object sender, ProgressChangedEventArgs e)
        {
            bool? operation = (bool?) sender;
            switch (e.ProgressPercentage)
            {
                case 1:
                    IsBusy      = true;
                    BusyContent = $"Connecting to {e.UserState}";
                    break;
                case 2:
                    switch (operation)
                    {
                        case null:
                            BusyContent = $"Initializing {e.UserState}";
                            break;
                        case true:
                            BusyContent = $"Resetting {e.UserState}";
                            break;
                        default:
                            BusyContent = $"Calibrating {e.UserState}";
                            break;
                    }
                    break;
                case 3:
                    switch (operation)
                    {
                        case null:
                            BusyContent = $"Initializing {e.UserState} Complete";
                            IsBusy      = false;
                            break;
                        case true:
                            BusyContent = $"Resetting {e.UserState} Complete";
                            IsBusy      = false;
                            break;
                        default:
                            BusyContent = $"Calibrating {e.UserState} Complete";
                            IsBusy      = false;
                            break;
                    }
                    break;
                case 4:
                    switch (operation)
                    {
                        case null:
                            MessageBox.Show($"Initializing {e.UserState} Failed");
                            BusyContent = $"Initializing {e.UserState} Failed";
                            IsBusy      = false;
                            break;
                        case true:
                            MessageBox.Show($"Resetting {e.UserState} Failed");
                            BusyContent = $"Resetting {e.UserState} Failed";
                            IsBusy      = false;
                            break;
                        default:
                            MessageBox.Show($"Calibrating {e.UserState} Failed");
                            BusyContent = $"Calibrating {e.UserState} Failed";
                            IsBusy      = false;
                            break;
                    }
                    break;
            }
        }

        #endregion

        #region Properties

        public DelegateCommand      ResetSerialPortCommand { get; set; }
        public DelegateCommand      InitializeCommand      { get; set; }
        public DelegateCommand      StartCommand           { get; set; }
        public DelegateCommand      StopCommand            { get; set; }
        public DelegateCommand      CalibrateCommand       { get; set; }
        public DelegateCommand      ResetCommand           { get; set; }
        public DelegateCommand      SaveDataCommand        { get; set; }
        public BindingList<Arduino> Arduinos               { get; set; }
        public Arduino              SelectedArduino        { get; set; }
        public Display SelectedDisplay
        {
            get => GetValue<Display>();
            set => SetValue(value);
        }
        public bool IsBusy
        {
            get => GetValue<bool>();
            set => SetValue(value);
        }
        public string BusyContent
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        #endregion
    }
}