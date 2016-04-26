using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Threading;
using Windows.System;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UWPPlatformGame
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        // timers
        private DispatcherTimer timer; // player 1
        private DispatcherTimer timer2; // movement animations
        private DispatcherTimer timer3; // player 2

        // which keys are pressed 
        private bool UpPressed;
        private bool LeftPressed;
        private bool RightPressed;
        int G = 30, Force;

        private bool UpPressed2;
        private bool LeftPressed2;
        private bool RightPressed2;
        int G2 = 30, Force2;

        // offset to show
        private int currentFrame = 0;
        private int direction = 1;
        private int frameWidth = 36;
        private int currentFrame2 = 0;
        private int direction2 = 1;
        private int frameWidth2 = 36;

        private bool haskey = false;

        public MainPage()
        {
            this.InitializeComponent();
            // key event listeners
            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;
            Window.Current.CoreWindow.KeyUp += CoreWindow_KeyUp;

            // player 1 timer
            timer = new DispatcherTimer();
            timer.Tick += Timer_Tick;
            timer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            timer.Start();

            // movement animation timer
            timer2 = new DispatcherTimer();
            timer2.Tick += Timer_Tick2;
            timer2.Interval = new TimeSpan(0, 0, 0, 0, 50);
            timer2.Start();

            // player 2 timer
            timer3 = new DispatcherTimer();
            timer3.Tick += Timer_Tick3;
            timer3.Interval = new TimeSpan(0, 0, 0, 0, 1);
            timer3.Start();

            
        }

        // animations
        private void Timer_Tick2(object sender, object e)
        {
            if (LeftPressed == false && RightPressed == false)
            {
                currentFrame = 0;
            }
            else {
                // frame
                if (direction == 1) currentFrame++;
                else currentFrame--;
                if (currentFrame == 0 || currentFrame == 3) currentFrame = 0;
                // set offset
                SpriteSheetOffset.X = currentFrame * -frameWidth;
            }

            if (LeftPressed2 == false && RightPressed2 == false)
            {
                currentFrame2 = 0;
            }
            else {
                // frame
                if (direction2 == 1) currentFrame2++;
                else currentFrame2--;
                if (currentFrame2 == 0 || currentFrame2 == 3) currentFrame2 = 0;
                // set offset
                SpriteSheetOffset2.X = currentFrame2 * -frameWidth2;
            }
        }

        // player 2 movement & collisions
        private void Timer_Tick3(object sender, object e)
        {

            Thickness margin2 = player2.Margin;
            Thickness block1Margin2 = block1.Margin;
            Thickness start2 = tileWall.Margin;
            Thickness end2 = tileWall2.Margin;

            if (UpPressed2 == true)
            {
                margin2.Top -= Force2;
                player2.Margin = margin2;
                Force2 -= 3;
            }

            if (RightPressed2 == true)
            {
                player2.RenderTransformOrigin = new Point { X = 0.5, Y = 0.5 };
                player2.RenderTransform = new ScaleTransform() { ScaleX = 1 };
                player2.UpdateLayout();
                margin2.Left += 5;
                player2.Margin = margin2;
            }

            if (LeftPressed2 == true)
            {
                player2.RenderTransformOrigin = new Point { X = 0.5, Y = 0.5 };
                player2.RenderTransform = new ScaleTransform() { ScaleX = -1 };
                player2.UpdateLayout();
                margin2.Left -= 5;
                player2.Margin = margin2;
            }
            // left boundary
            if (margin2.Left <= 0)
            {
                margin2.Left = 0;
                player2.Margin = margin2;
                LeftPressed2 = false;
            }
            // right boundary
            if (margin2.Left >= 1108)
            {
                margin2.Left = 1108;
                player2.Margin = margin2;
                RightPressed2 = false;
            }
            // tileWall side collision
            if (margin2.Left >= start2.Left - player2.Width && margin2.Left < start2.Left + tileWall.Width && margin2.Top > start2.Top && LeftPressed2 == true)
            {
                LeftPressed2 = false;
                margin2.Left = start2.Left + tileWall.Width;
                player2.Margin = margin2;
            }
            // tileWall2 side collision
            if (margin2.Left >= end2.Left - player2.Width && margin2.Left < end2.Left + tileWall2.Width && margin2.Top > end2.Top && RightPressed2 == true)
            {
                RightPressed2 = false;
                margin2.Left = end2.Left - player2.Width;
                player2.Margin = margin2;
            }
            // block 1 side collisions
            if (margin2.Left >= block1Margin2.Left - player2.Width && margin2.Left < block1Margin2.Left + block1.Width && margin2.Top > block1Margin2.Top && RightPressed2 == true)
            {
                RightPressed2 = false;
                margin2.Left = block1Margin2.Left - player2.Width;
                player2.Margin = margin2;
            }
            if (margin2.Left >= block1Margin2.Left - player1.Width && margin2.Left < block1Margin2.Left + block1.Width && margin2.Top > block1Margin2.Top && LeftPressed2 == true)
            {
                LeftPressed2 = false;
                margin2.Left = block1Margin2.Left + block1.Width;
                player2.Margin = margin2;
            }

            if (margin2.Top >= 322)
            {
                margin2.Top = 322; // stop falling at bottom
                player2.Margin = margin2;
                UpPressed2 = false;
            }
            else
            {
                margin2.Top += 5;
                player2.Margin = margin2;
            }
            // block 1 top collision
            if (margin2.Left + player2.Width - 5 > block1Margin2.Left
                && margin2.Left + player2.Width + 5 < block1Margin2.Left + block1.Width + player2.Width
                && margin2.Top + player2.Height >= block1Margin2.Top && margin2.Top < block1Margin2.Top)
            {
                Force2 = 0;
                margin2.Top = block1Margin2.Top - player2.Height;

                UpPressed2 = false;
                player2.Margin = margin2;
            }
            // tileWall top collision
            if (margin2.Left + player2.Width - 5 > start2.Left
                && margin2.Left + player2.Width + 5 < start2.Left + tileWall.Width + player2.Width
                && margin2.Top + player2.Height >= start2.Top && margin2.Top < start2.Top)
            {
                Force2 = 0;
                margin2.Top = start2.Top - player2.Height;
                UpPressed2 = false;
                player2.Margin = margin2;
            }
            // ending point's tileWall top collision
            if (margin2.Left + player2.Width - 5 > end2.Left
                && margin2.Left + player2.Width + 5 < end2.Left + tileWall2.Width + player2.Width
                && margin2.Top + player2.Height >= end2.Top && margin2.Top < end2.Top)
            {
                Force2 = 0;
                margin2.Top = end2.Top - player2.Height;
                UpPressed2 = false;
                player2.Margin = margin2;
            }
        }

        // player 1 movement & collisions
        private void Timer_Tick(object sender, object e)
        {

            Thickness margin = player1.Margin;
            Thickness block1Margin = block1.Margin;
            Thickness start = tileWall.Margin;
            Thickness end = tileWall2.Margin;
            

            // moving block
            block1Margin.Left += 2;
            block1.Margin = block1Margin;
            if (block1Margin.Left >= end.Left)
            {
                block1Margin.Left = end.Left;
                block1.Margin = block1Margin;
            }


            if (margin.Top > 300)
            {
                Message.Text = "Don't touch ground!";
            }

            if (UpPressed == true) {
                margin.Top -= Force;
                player1.Margin = margin;
                Force -= 3;
            }

            if(RightPressed == true) { 
                player1.RenderTransformOrigin = new Point { X = 0.5, Y = 0.5 };
                player1.RenderTransform = new ScaleTransform() { ScaleX = 1 };
                player1.UpdateLayout();
                margin.Left += 5;
                player1.Margin = margin;
            }
            if(LeftPressed == true) {
                player1.RenderTransformOrigin = new Point { X = 0.5, Y = 0.5 };
                player1.RenderTransform = new ScaleTransform() { ScaleX = -1 };
                player1.UpdateLayout();
                margin.Left -= 5;
                player1.Margin = margin;
            }
            // left boundary
            if (margin.Left <= 0)
            {
                margin.Left = 0;
                player1.Margin = margin;
                LeftPressed = false;
            }
            // right boundary
            if (margin.Left >= 1108)
            {
                margin.Left = 1108;
                player1.Margin = margin;
                RightPressed = false;                
            }
            // tileWall side collision
            if (margin.Left >= start.Left - player1.Width && margin.Left < start.Left + tileWall.Width && margin.Top > start.Top && LeftPressed == true)
            {
                LeftPressed = false;
                margin.Left = start.Left + tileWall.Width;
                player1.Margin = margin;
            }
            // tileWall2 side collision
            if (margin.Left >= end.Left - player1.Width && margin.Left < end.Left + tileWall2.Width && margin.Top > end.Top && RightPressed == true)
            {
                RightPressed = false;
                margin.Left = end.Left - player1.Width;
                player1.Margin = margin;
            }
            // block 1 side collisions
            if (margin.Left >= block1Margin.Left - player1.Width && margin.Left < block1Margin.Left + block1.Width && margin.Top > block1Margin.Top && RightPressed == true)
            {
                RightPressed = false;
                margin.Left = block1Margin.Left - player1.Width;
                player1.Margin = margin;
            }
            if (margin.Left >= block1Margin.Left - player1.Width && margin.Left < block1Margin.Left + block1.Width && margin.Top > block1Margin.Top && LeftPressed == true)
            {
                LeftPressed = false;
                margin.Left = block1Margin.Left + block1.Width;
                player1.Margin = margin;
            }


            if (margin.Top >= 322)
            {
                margin.Top = 322; // stop falling at bottom
                player1.Margin = margin;
                UpPressed = false;
            }
            
            else
            {
                margin.Top += 5;
                player1.Margin = margin;
            }
            // block 1 top collision
            if (margin.Left + player1.Width - 5 > block1Margin.Left
                && margin.Left + player1.Width + 5 < block1Margin.Left + block1.Width + player1.Width
                && margin.Top + player1.Height >= block1Margin.Top && margin.Top < block1Margin.Top)
            {
                Force = 0;
                margin.Top = block1Margin.Top - player1.Height;
                UpPressed = false;
                player1.Margin = margin;
                Message.Text = "You got the key!";
                haskey = true;
                block1.Width = 0;
                block1.Height = 0;
                block1Margin.Top -= 10;
                block1.Margin = block1Margin;
                key.Text = "You have a key!";
            }
            // starting point's tileWall top collision
            if (margin.Left + player1.Width - 5 > start.Left
                && margin.Left + player1.Width + 5 < start.Left + tileWall.Width + player1.Width
                && margin.Top + player1.Height >= start.Top && margin.Top < start.Top)
            {
                Force = 0;
                margin.Top = start.Top - player1.Height;
                UpPressed = false;
                player1.Margin = margin;
            }
            // ending point's tileWall top collision
            if (margin.Left + player1.Width - 5 > end.Left
                && margin.Left + player1.Width + 5 < end.Left + tileWall2.Width + player1.Width
                && margin.Top + player1.Height >= end.Top && margin.Top < end.Top)
            {
                Force = 0;
                margin.Top = end.Top - player1.Height;
                UpPressed = false;
                player1.Margin = margin;
                if (haskey == true)
                {
                    Message.Text = "You made it!";
                }
            }
        }

        /// <summary>
        /// Check if some keys are pressed.
        /// </summary>
        private void CoreWindow_KeyDown(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs args)
        {
            
            if (UpPressed != true)
            {
                switch (args.VirtualKey)
                {
                    case VirtualKey.Up:
                        UpPressed = true;
                        Force = G;
                        break;
                }
            }
            
                switch (args.VirtualKey)
            {
                case VirtualKey.Left:
                    LeftPressed = true;
                    break;
                case VirtualKey.Right:
                    RightPressed = true;

                    break;
                default:
                    break;
            }

            if (UpPressed2 != true)
            {
                switch (args.VirtualKey)
                {
                    case VirtualKey.W:
                        UpPressed2 = true;
                        Force2 = G2;
                        break;
                }
            }

            switch (args.VirtualKey)
            {
                case VirtualKey.A:
                    LeftPressed2 = true;
                    break;
                case VirtualKey.D:
                    RightPressed2 = true;

                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Check if some keys are released.
        /// </summary>
        private void CoreWindow_KeyUp(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs args)
        {
            // player 1 keys
            switch (args.VirtualKey)
            {
                case VirtualKey.Left:
                    LeftPressed = false;
                    break;
                case VirtualKey.Right:
                    RightPressed = false;
                    break;
                default:
                    break;
            }

            // player 2 keys
            switch (args.VirtualKey)
            {
                case VirtualKey.A:
                    LeftPressed2 = false;
                    break;
                case VirtualKey.D:
                    RightPressed2 = false;
                    break;
                case VirtualKey.Escape:
                    Application.Current.Exit();
                    break;
                default:
                    break;
            }
        }


    }
}
