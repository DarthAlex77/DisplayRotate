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
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using WindowsDisplayAPI;
using WindowsDisplayAPI.Native.DeviceContext;

namespace DisplayRotate.Model
{
    internal static class DisplayRotateWorker
    {
        static DisplayRotateWorker()
        {
            Worker        =  new BackgroundWorker();
            Worker.DoWork += DisplayRotate;
        }
        public static BackgroundWorker Worker          { get; }

        internal static void DisplayRotate(object sender, DoWorkEventArgs e)
        {
            Display display = (Display) e.Argument;
            DisplayOrientation orientation;
            if (display.X is > 24576 or < -24576)
            {
                orientation = DisplayOrientation.Rotate180Degree;
                Rotate();
            }
            else
            {
                orientation = display.Y switch
                {
                    >= -8192 and <= 8192 => DisplayOrientation.Identity,
                    >= 8192              => DisplayOrientation.Rotate90Degree,
                    < -8912              => DisplayOrientation.Rotate270Degree,
                    _                    => DisplayOrientation.Identity

                }; 
                Rotate();
            }

            void Rotate()
            {
                if (display.Orientation != orientation)
                {
                    WindowsDisplayAPI.Display d          = WindowsDisplayAPI.Display.GetDisplays().First(x => x.DeviceName == display.DisplayName);
                    Size                      resolution = d.CurrentSetting.Resolution;
                    if (((uint) d.CurrentSetting.Orientation + (uint) orientation) % 2 == 1)
                    {
                        (resolution.Height, resolution.Width) = (resolution.Width, resolution.Height);
                    }
                    d.SetSettings(new DisplaySetting(resolution, d.CurrentSetting.Position, d.CurrentSetting.ColorDepth, d.CurrentSetting.Frequency, d.CurrentSetting.IsInterlaced, orientation, d.CurrentSetting.OutputScalingMode),
                                  true);
                    display.Orientation = orientation;
                }
            }
        }
    }
}