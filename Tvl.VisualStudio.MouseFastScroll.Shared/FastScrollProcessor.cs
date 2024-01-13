// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE.txt in the project root for license information.

namespace Tvl.VisualStudio.MouseFastScroll
{
    using System;
    using System.Runtime.InteropServices;
    using ITextView = Microsoft.VisualStudio.Text.Editor.ITextView;
    using MouseProcessorBase = Microsoft.VisualStudio.Text.Editor.MouseProcessorBase;
    using MouseWheelEventArgs = System.Windows.Input.MouseWheelEventArgs;
    using ScrollDirection = Microsoft.VisualStudio.Text.Editor.ScrollDirection;

    internal enum Keys
    {
        ShiftKey = 0x10,
        Menu = 0x12,
        LShiftKey = 0xA0,
        RShiftKey = 0xA1,
        LMenu = 0xA4,
        RMenu = 0xA5,
    }

    internal class FastScrollProcessor : MouseProcessorBase
    {
        public FastScrollProcessor(ITextView textView, int scrollSpeedMultiplier = 4)
        {
            this.TextView = textView;
            this.WheelScrollLines = GetMouseWheelScrollLines();
            this.ScrollSpeedMultiplier = scrollSpeedMultiplier;
        }

        public int WheelScrollLines { get; set; }

        public int ScrollSpeedMultiplier { get; set; }

        private ITextView TextView
        {
            get;
            set;
        }

        [DllImport("user32.dll")]
        public static extern bool SystemParametersInfoA(int uiAction, int uiParam, out int pvParam, int fWinIni);

        public static int GetMouseWheelScrollLines()
        {
            int value;
            if (SystemParametersInfoA(104, 0, out value, 0))
            {
                return value;
            }

            return 3;
        }

        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(int key);

        private static bool IsKeyDown(int key)
        {
            short retVal = GetAsyncKeyState(key);

            // If the high-order bit is 1 then the key is down, otherwise it is up.
            return (retVal & 0x8000) == 0x8000;
        }

        public override void PreprocessMouseWheel(MouseWheelEventArgs e)
        {
            var scroller = TextView.ViewScroller;
            int absDelta = Math.Abs(e.Delta);
            if (scroller != null && absDelta % 120 == 0)
            {
                bool fastByPage = false;
                if (IsKeyDown((int)Keys.LMenu) || (fastByPage = IsKeyDown((int)Keys.RMenu)))
                {
                    // fast scroll
                    if (!fastByPage)
                    {
                        // fast scroll by lines
                        int lines = WheelScrollLines * (absDelta / 120) * ScrollSpeedMultiplier;
                        if (IsKeyDown((int)Keys.ShiftKey))
                        {
                            int lineHeight = (int)TextView.LineHeight;
                            scroller.ScrollViewportHorizontallyByPixels(Math.Sign(e.Delta) * lineHeight * lines);
                        }
                        else
                        {
                            scroller.ScrollViewportVerticallyByLines(e.Delta < 0 ? ScrollDirection.Down : ScrollDirection.Up, lines);
                        }
                    }
                    else
                    {
                        // scroll by pages
                        int pages = absDelta / 120;
                        if (IsKeyDown((int)Keys.LShiftKey) || IsKeyDown((int)Keys.RShiftKey))
                        {
                            int viewWidth = (int)TextView.ViewportWidth;
                            scroller.ScrollViewportHorizontallyByPixels(Math.Sign(e.Delta) * viewWidth * pages);
                        }
                        else
                        {
                            for (int i = 0; i < pages; i++)
                            {
                                scroller.ScrollViewportVerticallyByPage(e.Delta < 0 ? ScrollDirection.Down : ScrollDirection.Up);
                            }
                        }
                    }

                    e.Handled = true;
                }
            }
        }
    }
}
