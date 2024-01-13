﻿// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE.txt in the project root for license information.

namespace Tvl.VisualStudio.MouseFastScroll.UnitTests
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Windows;
    using System.Windows.Input;
    using Microsoft.VisualStudio.Text.Editor;
    using WindowsInput;
    using WindowsInput.Native;
    using Xunit;

    public class FastScrollProcessorTests
    {
        private InputSimulator inputSimulator = new InputSimulator();

        [StaFact]
        public void ScrollWithoutAltPressed()
        {
            string content = string.Join("\r\n", Enumerable.Range(0, 200).Select(i => Guid.NewGuid().ToString()));
            var processor = CompositionHelper.GetProcessor(content, out var exportProvider, out var wpfTextView);
            var args = new MouseWheelEventArgs(Mouse.PrimaryDevice, Environment.TickCount, 120);

            Assert.Equal(ModifierKeys.None, Keyboard.Modifiers & ModifierKeys.Alt);
            Assert.False(args.Handled);
            var firstVisibleLine = wpfTextView.TextViewLines.FirstVisibleLine.Start.GetContainingLine().LineNumber;
            processor.PreprocessMouseWheel(args);
            Assert.False(args.Handled);

            // The line number stays the same because the editor's default mouse handler is not part of these tests
            Assert.Equal(firstVisibleLine, wpfTextView.TextViewLines.FirstVisibleLine.Start.GetContainingLine().LineNumber);
        }

        [StaFact]
        public void ScrollDownWithAltPressed()
        {
            string content = string.Join("\r\n", Enumerable.Range(0, 200).Select(i => Guid.NewGuid().ToString()));
            var processor = CompositionHelper.GetProcessor(content, out var exportProvider, out var wpfTextView);
            var args = new MouseWheelEventArgs(Mouse.PrimaryDevice, Environment.TickCount, -120) { RoutedEvent = UIElement.MouseWheelEvent };

            inputSimulator.Keyboard.KeyDown(VirtualKeyCode.RMENU);
            Thread.Sleep(8);
            try
            {
                Assert.Equal(ModifierKeys.Alt, Keyboard.Modifiers & ModifierKeys.Alt);
                Assert.False(args.Handled);
                var lastVisibleLine = wpfTextView.TextViewLines.LastVisibleLine.Start.GetContainingLine().LineNumber;
                processor.PreprocessMouseWheel(args);
                Assert.True(args.Handled);

                // For a Scroll Page Down operation, the previous LastVisibleLine becomes the new FirstVisibleLine
                Assert.Equal(lastVisibleLine, wpfTextView.TextViewLines.FirstVisibleLine.Start.GetContainingLine().LineNumber);
            }
            finally
            {
                inputSimulator.Keyboard.KeyUp(VirtualKeyCode.RMENU);
            }
        }

        [StaFact]
        public void ScrollUpPartialPageWithAltPressed()
        {
            string content = string.Join("\r\n", Enumerable.Range(0, 200).Select(i => Guid.NewGuid().ToString()));
            var processor = CompositionHelper.GetProcessor(content, out var exportProvider, out var wpfTextView);
            var args = new MouseWheelEventArgs(Mouse.PrimaryDevice, Environment.TickCount, 120) { RoutedEvent = UIElement.MouseWheelEvent };

            inputSimulator.Keyboard.KeyDown(VirtualKeyCode.RMENU);
            Thread.Sleep(8);
            try
            {
                Assert.Equal(ModifierKeys.Alt, Keyboard.Modifiers & ModifierKeys.Alt);
                Assert.False(args.Handled);
                var lastVisibleLine = wpfTextView.TextViewLines.LastVisibleLine.Start.GetContainingLine().LineNumber;
                wpfTextView.TextViewLines.Scroll(ScrollDirection.Down, lastVisibleLine / 2);
                processor.PreprocessMouseWheel(args);
                Assert.True(args.Handled);

                // For a Scroll Page Up operation, the movement stops if the top line is reached
                Assert.Equal(0, wpfTextView.TextViewLines.FirstVisibleLine.Start.GetContainingLine().LineNumber);
            }
            finally
            {
                inputSimulator.Keyboard.KeyUp(VirtualKeyCode.RMENU);
            }
        }

        [StaFact]
        public void ScrollUpFullPageWithAltPressed()
        {
            string content = string.Join("\r\n", Enumerable.Range(0, 200).Select(i => Guid.NewGuid().ToString()));
            var processor = CompositionHelper.GetProcessor(content, out var exportProvider, out var wpfTextView);
            var args = new MouseWheelEventArgs(Mouse.PrimaryDevice, Environment.TickCount, 120) { RoutedEvent = UIElement.MouseWheelEvent };

            inputSimulator.Keyboard.KeyDown(VirtualKeyCode.RMENU);
            Thread.Sleep(8);
            try
            {
                Assert.Equal(ModifierKeys.Alt, Keyboard.Modifiers & ModifierKeys.Alt);
                Assert.False(args.Handled);
                var lastVisibleLine = wpfTextView.TextViewLines.LastVisibleLine.Start.GetContainingLine().LineNumber;
                wpfTextView.TextViewLines.Scroll(ScrollDirection.Down, lastVisibleLine + 4);
                processor.PreprocessMouseWheel(args);
                Assert.True(args.Handled);

                Assert.Equal(4, wpfTextView.TextViewLines.FirstVisibleLine.Start.GetContainingLine().LineNumber);
            }
            finally
            {
                inputSimulator.Keyboard.KeyUp(VirtualKeyCode.RMENU);
            }
        }
    }
}
