﻿/*
  LittleBigMouse.Screen.Config
  Copyright (c) 2017 Mathieu GRENET.  All right reserved.

  This file is part of LittleBigMouse.Screen.Config.

    LittleBigMouse.Screen.Config is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    LittleBigMouse.Screen.Config is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MouseControl.  If not, see <http://www.gnu.org/licenses/>.

	  mailto:mathieu@mgth.fr
	  http://www.mgth.fr
*/

using System;
using System.Runtime.CompilerServices;
using HLab.Notify;
using HLab.Windows.Monitors;
using Microsoft.Win32;

namespace LittleBigMouse.ScreenConfigs
{
    /// <summary>
    /// Actual real monitor size 
    /// </summary>
    public class ScreenSizeInMm : ScreenSize
    {
        public ScreenSizeInMm(Screen screen)
        {
            Screen = screen;
            this.SubscribeNotifier();
        }

        [TriggedOn(nameof(Screen), "Monitor", "AttachedDisplay")]
        public DisplayDevice AttachedDisplay => this.Get(() => Screen.Monitor.AttachedDisplay);

        [TriggedOn(nameof(Screen), "Orientation")]
        [TriggedOn(nameof(Screen), "Monitor", "AttachedDisplay", "DeviceCaps","Size")]
        public override double Width
        {
            get => this.Get(() => LoadValueMonitor(
                () => Screen.Orientation % 2 == 0
                ? Screen.Monitor.AttachedDisplay?.DeviceCaps.Size.Width??0
                : Screen.Monitor.AttachedDisplay?.DeviceCaps.Size.Height??0
                , "InMm.Width"));

            set => this.Set(value, (oldValue, newValue) =>
            {
                if (Screen.FixedAspectRatio)
                {
                    var ratio = newValue / oldValue;
                    Screen.FixedAspectRatio = false;
                    Height *= ratio;
                    Screen.FixedAspectRatio = true;
                }

                Screen.Config.Saved = false;
            });
        }

        [TriggedOn("Screen","Orientation")]
        [TriggedOn("Screen", "Monitor", "AttachedDisplay", "DeviceCaps", "Size")]
        public override double Height
        {
            get => this.Get(() => LoadValueMonitor(
                ()=>Screen.Orientation % 2 == 0 
                ? Screen.Monitor.AttachedDisplay?.DeviceCaps.Size.Height??0 
                : Screen.Monitor.AttachedDisplay?.DeviceCaps.Size.Width??0
                ,"InMm.Height"));
            set
            {
                this.Set(value, (oldValue, newValue) =>
                {
                    if (Screen.FixedAspectRatio)
                    {
                        var ratio = newValue / oldValue;
                        Screen.FixedAspectRatio = false;
                        Width *= ratio;
                        Screen.FixedAspectRatio = true;
                    }

                    Screen.Config.Saved = false;
                } );
            }
        }

        public override double X
        {
            get => this.Get(() => LoadValueConfig(() => 0, "InMm.X"));
            set
            {
                if (Screen.Primary)
                {
                    foreach (var screen in Screen.Config.AllBut(Screen))
                    {
                        screen.InMm.X -= value;
                    }
                }
                else if (this.Set(value)) Screen.Config.Saved = false;
            }
        }
        public override double Y
        {
            get => this.Get(() => LoadValueConfig(() => 0,"InMm.Y"));
            set
            {
                if (Screen.Primary)
                {
                    foreach (var screen in Screen.Config.AllBut(Screen))
                    {
                        screen.InMm.Y -= value;
                    }
                }
                else if (this.Set(value)) Screen.Config.Saved = false;
            }
        }
        public override double TopBorder
        {
            get => this.Get(() => LoadValueMonitor(() => 20));
            set { if (this.Set(value)) Screen.Config.Saved = false; }
        }
        public override double BottomBorder
        {
            get => this.Get(() => LoadValueMonitor(() => 20));
            set { if (this.Set(value)) Screen.Config.Saved = false; }
        }
        public override double LeftBorder
        {
            get => this.Get(() => LoadValueMonitor(() => 20));
            set { if (this.Set(value)) Screen.Config.Saved = false; }
        }
        public override double RightBorder
        {
            get => this.Get(() => LoadValueMonitor(()=>20));
            set { if (this.Set(value)) Screen.Config.Saved = false; }
        }
        private double LoadValueMonitor(Func<double> def, [CallerMemberName]string name = null)
        {
            if(Screen==null) throw new PropertyNotReady(0.0); 

            using (RegistryKey key = Screen.OpenMonitorRegKey())
            {
                return key.GetKey(name, def);
            }
        }
        private double LoadValueConfig(Func<double> def, [CallerMemberName]string name = null)
        {
            using (RegistryKey key = Screen.OpenConfigRegKey())
            {
                return key.GetKey(name, def);
            }
        }
    }
}