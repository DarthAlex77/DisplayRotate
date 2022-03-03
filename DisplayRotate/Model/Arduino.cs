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
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using DevExpress.Mvvm;

namespace DisplayRotate.Model
{
    internal class Arduino : BindableBase
    {
        #region Constructor

        public Arduino(string serialPortName)
        {
            SerialPort              =  new SerialPort(serialPortName, 115200);
            SerialPort.WriteTimeout =  100;
            SerialPort.ReadTimeout  =  100;
            Displays                =  new BindingList<Display>();
            Displays.ListChanged    += Displays_ListChanged;
            SerialPort.DataReceived += Model.Worker.DataReceived;
        }

        #endregion

        #region Properties

        public event ProgressChangedEventHandler DisplayProgressChanged;
        public BackgroundWorker                  Worker     { get; set; }
        public SerialPort                        SerialPort { get; set; }
        public BindingList<Display>              Displays   { get; set; }

        #endregion

        #region Methods

        private void Displays_ListChanged(object sender, ListChangedEventArgs e)
        {
            switch (e.ListChangedType)
            {
                case ListChangedType.ItemAdded:
                    Displays[e.NewIndex].SerialPortName = SerialPort.PortName;
                    break;
                case ListChangedType.ItemChanged:
                    switch (e.PropertyDescriptor.Name)
                    {
                        case "X" or "Y":
                        {
                            if (!DisplayRotateWorker.Worker.IsBusy)
                            {
                                DisplayRotateWorker.Worker.RunWorkerAsync(Displays[e.NewIndex]);
                            }
                            break;
                        }
                        case "IsStarted" when Displays.Any(x => x.IsStarted):
                        {
                            if (Worker == null)
                            {
                                Worker                            =  new BackgroundWorker();
                                Worker.DoWork                     += Model.Worker.GetDataWorker;
                                Worker.WorkerSupportsCancellation =  true;
                                Worker.RunWorkerAsync(this);
                            }
                            break;
                        }
                        case "IsStarted" when Displays.All(x => x.IsStarted ==false):
                        {
                            if (Worker != null)
                            {
                                Worker.RunWorkerCompleted += (_, _) => { Worker = null; };
                                Worker.CancelAsync();
                            }
                            break;
                        }
                    }
                    break;
            }
        }

        public void MpuWorker(Display display, bool? reset)
        {
            Worker                       =  new BackgroundWorker();
            Worker.WorkerReportsProgress =  true;
            Worker.DoWork                += Model.Worker.MpuWorker;
            Worker.ProgressChanged       += (_, args) => DisplayProgressChanged?.Invoke(reset, args);
            Worker.RunWorkerCompleted    += (_, _) => { Worker = null; };
            Worker.RunWorkerAsync(new List<object> {this, display, reset});
        }

        #endregion
    }
}