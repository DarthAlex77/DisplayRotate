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
using System.ComponentModel;
using System.Windows;
using System.Windows.Forms;

namespace DisplayRotate
{
    public partial class MainWindow
    {
        private NotifyIcon  _mNotifyIcon;
        private WindowState _mStoredWindowState = WindowState.Normal;

        public MainWindow()
        {
            InitializeComponent();
            _mNotifyIcon       =  new NotifyIcon();
            _mNotifyIcon.Text  =  @"Display Rotate App";
            _mNotifyIcon.Icon  =  Properties.Resources.Icon;
            _mNotifyIcon.Click += NotifyIcon_Click;
        }

        private void OnStateChanged(object sender, EventArgs args)
        {
            if (WindowState == WindowState.Minimized)
            {
                Hide();
            }
            else
            {
                _mStoredWindowState = WindowState;
            }
        }

        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            CheckTrayIcon();
        }

        private void OnClose(object sender, CancelEventArgs args)
        {
            _mNotifyIcon.Dispose();
            _mNotifyIcon = null;
        }

        private void NotifyIcon_Click(object sender, EventArgs e)
        {
            Show();
            WindowState = _mStoredWindowState;
        }

        private void CheckTrayIcon()
        {
            ShowTrayIcon(!IsVisible);
        }

        private void ShowTrayIcon(bool show)
        {
            if (_mNotifyIcon != null)
            {
                _mNotifyIcon.Visible = show;
            }
        }
    }
}