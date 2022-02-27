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
using System.Collections.Generic;
using System.Linq;
using DevExpress.Mvvm;
using Newtonsoft.Json;
using WindowsDisplayAPI.Native.DeviceContext;

namespace DisplayRotate.Model
{
    internal class Display : BindableBase
    {
        #region Properties

        [JsonProperty]
        public string DisplayName
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        [JsonProperty]
        public string SerialPortName
        {
            get => GetValue<string>();
            set => SetValue(value);
        }
        [JsonIgnore]
        public short X
        {
            get => GetValue<short>();
            set => SetValue(value);
        }
        [JsonIgnore]
        public short Y
        {
            get => GetValue<short>();
            set => SetValue(value);
        }

        [JsonIgnore]
        public DisplayOrientation Orientation
        {
            get => GetValue<DisplayOrientation>();
            set => SetValue(value);
        }

        [JsonProperty]
        public MpuAddress MpuAddress
        {
            get => GetValue<MpuAddress>();
            set => SetValue(value);
        }

        [JsonIgnore]
        public bool ConnectionStatus
        {
            get => GetValue<bool>();
            set => SetValue(value);
        }

        [JsonIgnore]
        public bool IsStarted
        {
            get => GetValue<bool>();
            set => SetValue(value);
        }

        public static List<Display> GetDisplays()
        {
            return WindowsDisplayAPI.Display.GetDisplays().Select(display => new Display(display.DeviceName)).ToList();
        }

        #endregion

        #region Constructors

        public Display()
        { }

        [JsonConstructor]
        public Display(string displayName, string serialPortName, MpuAddress mpuAddress)
        {
            DisplayName    = displayName;
            SerialPortName = serialPortName;
            MpuAddress     = mpuAddress;
            X              = 0;
            Y              = 0;
            if (!string.IsNullOrWhiteSpace(displayName))
            {
                Orientation = WindowsDisplayAPI.Display.GetDisplays().Any(x => x.DeviceName     == displayName)
                              ? WindowsDisplayAPI.Display.GetDisplays().First(x => x.DeviceName == displayName).CurrentSetting.Orientation
                              : DisplayOrientation.Identity;
            }
            ConnectionStatus = false;
            IsStarted        = false;
        }

        public Display(string displayName)
        {
            DisplayName    = displayName;
            SerialPortName = "None";
            MpuAddress     = MpuAddress.None;
            X              = 0;
            Y              = 0;
            Orientation = WindowsDisplayAPI.Display.GetDisplays().Any(x => x.DeviceName     == displayName)
                          ? WindowsDisplayAPI.Display.GetDisplays().First(x => x.DeviceName == displayName).CurrentSetting.Orientation
                          : DisplayOrientation.Identity;
            Orientation      = DisplayOrientation.Identity;
            ConnectionStatus = false;
            IsStarted        = false;
        }

        #endregion
    }
}