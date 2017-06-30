﻿//
//  WinCompose — a compose key for Windows — http://wincompose.info/
//
//  Copyright © 2013—2017 Sam Hocevar <sam@hocevar.net>
//
//  This program is free software. It comes without any warranty, to
//  the extent permitted by applicable law. You can redistribute it
//  and/or modify it under the terms of the Do What the Fuck You Want
//  to Public License, Version 2, as published by the WTFPL Task Force.
//  See http://www.wtfpl.net/ for more details.
//

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;

namespace WinCompose
{
    /// <summary>
    /// Interaction logic for Popup.xaml
    /// </summary>
    public partial class Popup : Window
    {
        public Popup()
        {
            ShowInTaskbar = false;
            InitializeComponent();

            // FIXME: remove Event Handler on destruction!
            Composer.KeyEvent += new EventHandler(OnKey);
        }

        public void OnKey(object sender, EventArgs e)
        {
            if (!Composer.IsComposing())
            {
                Hide();
                return;
            }

            List<uint> tid_list = new List<uint>();

#if false
            // This code tries to list all possible threads in case one of
            // them has an hwndCaret, but it doesn’t really improve things
            // with Visual Studio or Qt applications.
            IntPtr win = NativeMethods.GetForegroundWindow();
            uint pid;
            NativeMethods.GetWindowThreadProcessId(win, out pid);
            IntPtr th32s = NativeMethods.CreateToolhelp32Snapshot(TH32CS.SNAPTHREAD, pid);
            if (th32s != IntPtr.Zero)
            {
                THREADENTRY32 te = new THREADENTRY32();
                te.dwSize = (uint)Marshal.SizeOf(te);
                if (NativeMethods.Thread32First(th32s, out te))
                {
                    do
                    {
                        if (te.th32OwnerProcessID == pid)
                        {
                            tid_list.Add(te.th32ThreadID);
                        }
                        te.dwSize = (uint)Marshal.SizeOf(te);
                    }
                    while (NativeMethods.Thread32Next(th32s, out te));
                }
                NativeMethods.CloseHandle(th32s);
            }
#else
            tid_list.Add(0);
#endif

            GUITHREADINFO guiti = new GUITHREADINFO();
            guiti.cbSize = (uint)Marshal.SizeOf(guiti);

            foreach (var tid in tid_list)
            {
                NativeMethods.GetGUIThreadInfo(tid, ref guiti);
                if (guiti.hwndCaret != IntPtr.Zero)
                    break;
            }

            if (guiti.hwndCaret != IntPtr.Zero)
            {
                POINT point = new POINT();
                NativeMethods.ClientToScreen(guiti.hwndCaret, out point);
                int x = guiti.rcCaret.left + point.x;
                int y = guiti.rcCaret.top + point.y;
                int w = guiti.rcCaret.right - guiti.rcCaret.left;
                int h = guiti.rcCaret.bottom - guiti.rcCaret.top;

                PopupText.Text = string.Format("({0}, {1}) {2}x{3}", x, y, w, h);
                Left = x - 5;
                Top = y + h + 5;
                Show();
            }
            else
            {
                Hide();
            }
        }
    }
}