/* 
 * Copyright (C) 2016 Afrojäbät
 * 
 * Object Oriented Programming, practical work
 * URBAN AFRO GAME
 *
 * Created: 04/2016 
 * Authors: Saara Virtanen, Konsta Hallinen & Tiia Itkonen
 */

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
using Windows.UI.Xaml.Shapes;
using Windows.Storage;

namespace UWPPlatformGame
{
    // Player-class to create the player constructor that players use
	public class Player
	{
        // timer
        public DispatcherTimer timer;

        public Rectangle Rect;
        public TranslateTransform TranslateTr;

        // virtual keys
        public VirtualKey Up;
        public VirtualKey Left;
        public VirtualKey Right;

        // which keys are pressed 
        public bool UpPressed;
        public bool LeftPressed;
        public bool RightPressed;

        public int Force;

        // offset to show
        public int currentFrame = 0;
        public int frameWidth = 36;
        public int direction = 1;

        // player constructor
        public Player(Rectangle rect, TranslateTransform translateTr, VirtualKey upKey, VirtualKey leftKey, VirtualKey rightKey)
        {
            this.Rect = rect;
            this.TranslateTr = translateTr;
            Up = upKey;
            Left = leftKey;
            Right = rightKey;
        }
	}

    /// <summary>
    /// Main page-class
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private DispatcherTimer animTimer; // movement animations
        
        private List<Player> players = new List<Player>(); // list of players

        private int G = 30; // G-force

        private bool haskey = false; // player has the key y/n

        private MediaElement bgMusic; // background muwsic
        private MediaElement keyAudio; // audio for picking up the key
        private MediaElement dohAudio; // audio for dying
        private MediaElement wellDoneAudio; // audio for reaching exit

        public MainPage()
        {
            this.InitializeComponent();

            // audios
            InitBgMusic();
            InitPickedKey();
            InitDohAudio();
            InitWellDone();

            // key event listeners
            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;
            Window.Current.CoreWindow.KeyUp += CoreWindow_KeyUp;

            // initialize players
            players.Add(new Player(player1, SpriteSheetOffset, VirtualKey.Up, VirtualKey.Left, VirtualKey.Right));  //player 1
            players.Add(new Player(player2, SpriteSheetOffset2, VirtualKey.W, VirtualKey.A, VirtualKey.D));         //player 2

            // create timers for players
            foreach (Player player in players)
            {
                player.timer = new DispatcherTimer();
                player.timer.Tick += (s, e) => Timer_Tick(s, e, player);
                player.timer.Interval = new TimeSpan(0, 0, 0, 0, 1);
                player.timer.Start();
            }

            // movement animation timer
            animTimer = new DispatcherTimer();
            animTimer.Tick += Timer_Tick2;
            animTimer.Interval = new TimeSpan(0, 0, 0, 0, 50);
            animTimer.Start();
        }

        /// <summary>
        /// Load key audio file, play when the key is picked up
        /// </summary>
        private async void InitPickedKey()
        {
            keyAudio = new MediaElement();
            StorageFolder folder = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("Assets");
            StorageFile file = await folder.GetFileAsync("Picked_Key.wav");
            var stream = await file.OpenAsync(FileAccessMode.Read);
            keyAudio.AutoPlay = false;
            keyAudio.SetSource(stream, file.ContentType);
        }

        /// <summary>
        /// Load doh audio file, play when player "dies"
        /// </summary>
        private async void InitDohAudio()
        {
            dohAudio = new MediaElement();
            StorageFolder folder = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("Assets");
            StorageFile file = await folder.GetFileAsync("doh.wav");
            var stream = await file.OpenAsync(FileAccessMode.Read);
            dohAudio.AutoPlay = false;
            dohAudio.SetSource(stream, file.ContentType);
        }

        /// <summary>
        /// Load well done audio file, play when the players reach the exit sign
        /// </summary>
        private async void InitWellDone()
        {
            wellDoneAudio = new MediaElement();
            StorageFolder folder = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("Assets");
            StorageFile file = await folder.GetFileAsync("applause.wav");
            var stream = await file.OpenAsync(FileAccessMode.Read);
            wellDoneAudio.AutoPlay = false;
            wellDoneAudio.SetSource(stream, file.ContentType);
        }

        /// <summary>
        /// Load background music audio file, play all the time from the beginning
        /// </summary>
        private async void InitBgMusic()
        {
            bgMusic = new MediaElement();
            StorageFolder folder = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("Assets");
            StorageFile file = await folder.GetFileAsync("Dark_Red_Wine.mp3");
            var stream = await file.OpenAsync(FileAccessMode.Read);
            bgMusic.AutoPlay = true;
            bgMusic.SetSource(stream, file.ContentType);
            bgMusic.Play();
        }

        // animations for players
        private void Timer_Tick2(object sender, object e)
        {
            foreach(Player player in players)
            {
                if (!player.LeftPressed && !player.RightPressed)
                {
                    player.currentFrame = 0; // stop animation if player is not moving
                }
                else
                {
                    // frame
                    if (player.direction == 1) player.currentFrame++;
                    else player.currentFrame--;
                    if (player.currentFrame == 0 || player.currentFrame == 3) player.currentFrame = 0;
                    // set offset
                    player.TranslateTr.X = player.currentFrame * -player.frameWidth;
                }
            }
        }

        // all players movement & collisions
        private void Timer_Tick(object sender, object e, Player player)
        {
            // margins for players, blocks, key etc.
            Thickness margin = player.Rect.Margin;
            Thickness keyMargin = key.Margin;
            Thickness block2Margin = block2.Margin;
            Thickness block3Margin = block3.Margin;
            Thickness block4Margin = block4.Margin;
            Thickness start = tileWall.Margin;
            Thickness end = tileWall2.Margin;
            Thickness wellDoneMargs = WellDone.Margin;

            // moving block
            /*
            keyMargin.Left += 1;
            key.Margin = keyMargin;
            if (keyMargin.Left >= end.Left)
            {
                keyMargin.Left = end.Left;
                key.Margin = keyMargin;
            }
            */

            // player touches the ground, start from the beginning
            if (margin.Top > 310)
            {
                dohAudio.Play();
                margin.Left = 40;
                margin.Top = 203;
                player.Rect.Margin = margin;
            }

            // jump
            if (player.UpPressed)
            {
                margin.Top -= player.Force;
                player.Rect.Margin = margin;
                player.Force -= 3;
            }

            // move right
            if (player.RightPressed)
            {
                player.Rect.RenderTransformOrigin = new Point { X = 0.5, Y = 0.5 };
                player.Rect.RenderTransform = new ScaleTransform() { ScaleX = 1 };
                player.Rect.UpdateLayout();
                margin.Left += 5;
                player.Rect.Margin = margin;
            }

            // move left
            if (player.LeftPressed)
            {
                player.Rect.RenderTransformOrigin = new Point { X = 0.5, Y = 0.5 };
                player.Rect.RenderTransform = new ScaleTransform() { ScaleX = -1 };
                player.Rect.UpdateLayout();
                margin.Left -= 5;
                player.Rect.Margin = margin;
            }

            // left boundary
            if (margin.Left <= 0)
            {
                // can't move further left
                margin.Left = 0;
                player.Rect.Margin = margin;
                player.LeftPressed = false;
            }

            // right boundary
            if (margin.Left >= 1108)
            {
                // can't move further right
                margin.Left = 1108;
                player.Rect.Margin = margin;
                player.RightPressed = false;                
            }

            // tileWall side collision
            if (margin.Left >= start.Left - player.Rect.Width && margin.Left < start.Left + tileWall.Width && margin.Top > start.Top && player.LeftPressed)
            {
                player.LeftPressed = false;
                margin.Left = start.Left + tileWall.Width;
                player.Rect.Margin = margin;
            }

            // tileWall2 side collision
            if (margin.Left >= end.Left - player.Rect.Width && margin.Left < end.Left + tileWall2.Width && margin.Top > end.Top && player.RightPressed)
            {
                player.RightPressed = false;
                margin.Left = end.Left - player.Rect.Width;
                player.Rect.Margin = margin;
            }

            // key side collisions
            if (margin.Left >= keyMargin.Left - player.Rect.Width && margin.Left < keyMargin.Left + key.Width && margin.Top > keyMargin.Top && player.RightPressed)
            {
                player.RightPressed = false;
                margin.Left = keyMargin.Left - player.Rect.Width;
                player.Rect.Margin = margin;
                keyAudio.Play(); // play audio
                keyMargin.Top = 10;
                keyMargin.Left = 10;
                key.Margin = keyMargin;
            }

            if (margin.Left >= keyMargin.Left - player.Rect.Width && margin.Left < keyMargin.Left + key.Width && margin.Top > keyMargin.Top && player.LeftPressed)
            {
                player.LeftPressed = false;
                margin.Left = keyMargin.Left + key.Width;
                player.Rect.Margin = margin;
                keyAudio.Play();
                keyMargin.Top = 10;
                keyMargin.Left = 10;
                key.Margin = keyMargin;
            }

            // block 2 side collisions
            if (margin.Left >= block2Margin.Left - player.Rect.Width && margin.Left < block2Margin.Left + block2.Width && margin.Top > block2Margin.Top && player.RightPressed == true)
            {
                player.RightPressed = false;
                margin.Left = block2Margin.Left - player.Rect.Width;
                player.Rect.Margin = margin;
            }
       
            if (margin.Left >= block2Margin.Left - player.Rect.Width && margin.Left < block2Margin.Left + block2.Width && margin.Top > block2Margin.Top && player.LeftPressed == true)
            {
                player.LeftPressed = false;
                margin.Left = block2Margin.Left + block2.Width;
                player.Rect.Margin = margin;
            }

            // block 3 side collisions
            if (margin.Left >= block3Margin.Left - player.Rect.Width && margin.Left < block3Margin.Left + block3.Width && margin.Top > block3Margin.Top && player.RightPressed == true)
            {
                player.RightPressed = false;
                margin.Left = block3Margin.Left - player.Rect.Width;
                player.Rect.Margin = margin;
            }

            if (margin.Left >= block3Margin.Left - player.Rect.Width && margin.Left < block3Margin.Left + block3.Width && margin.Top > block3Margin.Top && player.LeftPressed == true)
            {
                player.LeftPressed = false;
                margin.Left = block3Margin.Left + block3.Width;
                player.Rect.Margin = margin;
            }

            // block 4 side collisions
            if (margin.Left >= block4Margin.Left - player.Rect.Width && margin.Left < block4Margin.Left + block4.Width && margin.Top > block4Margin.Top && player.RightPressed == true)
            {
                player.RightPressed = false;
                margin.Left = block4Margin.Left - player.Rect.Width;
                player.Rect.Margin = margin;
            }

            if (margin.Left >= block4Margin.Left - player.Rect.Width && margin.Left < block4Margin.Left + block4.Width && margin.Top > block4Margin.Top && player.LeftPressed == true)
            {
                player.LeftPressed = false;
                margin.Left = block4Margin.Left + block4.Width;
                player.Rect.Margin = margin;
            }

            // stop falling at bottom
            if (margin.Top >= 322)
            {
                margin.Top = 322;
                player.Rect.Margin = margin;
                player.UpPressed = false;
            }
            
            // fall down
            else
            {
                margin.Top += 5;
                player.Rect.Margin = margin;
            }

            // key top collision
            if (margin.Left + player.Rect.Width - 5 > keyMargin.Left
                && margin.Left + player.Rect.Width + 5 < keyMargin.Left + key.Width + player.Rect.Width
                && margin.Top + player.Rect.Height >= keyMargin.Top && margin.Top < keyMargin.Top)
            {
                player.Force = 0;
                margin.Top = keyMargin.Top - player.Rect.Height;
                player.UpPressed = false;
                player.Rect.Margin = margin;
                keyAudio.Play();
                haskey = true;
                keyMargin.Top = 10;
                keyMargin.Left = 10;
                key.Margin = keyMargin;
            }

            // block 2 top collision
            if (margin.Left + player.Rect.Width - 5 > block2Margin.Left
            && margin.Left + player1.Width + 5 < block2Margin.Left + block2.Width + player.Rect.Width
            && margin.Top + player.Rect.Height >= block2Margin.Top && margin.Top < block2Margin.Top)
            {
                player.Force = 0;
                margin.Top = block2Margin.Top - player.Rect.Height;
                player.UpPressed = false;
                player.Rect.Margin = margin;
            }

            // block 3 top collision
            if (margin.Left + player.Rect.Width - 5 > block3Margin.Left
            && margin.Left + player1.Width + 5 < block3Margin.Left + block3.Width + player.Rect.Width
            && margin.Top + player.Rect.Height >= block3Margin.Top && margin.Top < block3Margin.Top)
            {
                player.Force = 0;
                margin.Top = block3Margin.Top - player.Rect.Height;
                player.UpPressed = false;
                player.Rect.Margin = margin;
            }

            // block 4 top collision
            if (margin.Left + player.Rect.Width - 5 > block4Margin.Left
            && margin.Left + player1.Width + 5 < block4Margin.Left + block4.Width + player.Rect.Width
            && margin.Top + player.Rect.Height >= block4Margin.Top && margin.Top < block4Margin.Top)
            {
                player.Force = 0;
                margin.Top = block4Margin.Top - player.Rect.Height;
                player.UpPressed = false;
                player.Rect.Margin = margin;
            }

            // starting point's tileWall top collision
            if (margin.Left + player.Rect.Width - 5 > start.Left
                && margin.Left + player.Rect.Width + 5 < start.Left + tileWall.Width + player.Rect.Width
                && margin.Top + player.Rect.Height >= start.Top && margin.Top < start.Top)
            {
                player.Force = 0;
                margin.Top = start.Top - player.Rect.Height;
                player.UpPressed = false;
                player.Rect.Margin = margin;
            }

            // ending point's tileWall2 top collision
            if (margin.Left + player.Rect.Width - 5 > end.Left
                && margin.Left + player.Rect.Width + 5 < end.Left + tileWall2.Width + player.Rect.Width
                && margin.Top + player.Rect.Height >= end.Top && margin.Top < end.Top)
            {
                player.Force = 0;
                margin.Top = end.Top - player.Rect.Height;
                player.UpPressed = false;
                player.Rect.Margin = margin;
                // players have the key and both reach exit, play audio and show "well done" image
                if (haskey == true && player1.Margin.Left > 1075 && player2.Margin.Left > 1075)
                {
                    wellDoneAudio.Play();
                    WellDone.Width = 548;
                    WellDone.Height = 350;
                    wellDoneMargs.Left = 300;
                    wellDoneMargs.Top = 0;
                    WellDone.Margin = wellDoneMargs;

                }
            }
        }

        /// <summary>
        /// Check if some keys are pressed.
        /// </summary>
        private void CoreWindow_KeyDown(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs args)
        {
            foreach (Player player in players)
            {
                if (!player.UpPressed)
                {
                    if (args.VirtualKey == player.Up)
                    {
                        player.UpPressed = true;
                        player.Force = G;
                    }
                }
                
                if (args.VirtualKey == player.Left)
                {
                    player.LeftPressed = true;
                }
                else if (args.VirtualKey == player.Right)
                {
                    player.RightPressed = true;
                }
            }
        }

        /// <summary>
        /// Check if some keys are released.
        /// </summary>
        private void CoreWindow_KeyUp(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs args)
        {
            foreach (Player player in players)
            {
                if (args.VirtualKey == player.Left)
                {
                    player.LeftPressed = false;
                }
                else if (args.VirtualKey == player.Right)
                {
                    player.RightPressed = false;
                }
            }

            if(args.VirtualKey == VirtualKey.Escape)
                Application.Current.Exit(); // exit game

            if (args.VirtualKey == VirtualKey.Enter)
            {
                MenuImg.Width = 0;
            }
        }
    }
}
