﻿/*
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
using System.Windows;
using Syncfusion.SfSkinManager;

namespace DisplayRotate
{
    public partial class App : Application
    {
        public App()
        {
            SfSkinManager.ApplyStylesOnApplication = true;
        }
    }
}