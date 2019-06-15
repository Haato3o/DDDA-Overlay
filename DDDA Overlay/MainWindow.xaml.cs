using System;
using System.Windows;
using System.Threading;
using System.Runtime.InteropServices;
using DDDA_Overlay.Game;
using DDDA_Overlay.Memory;
using System.Windows.Interop;

namespace DDDA_Overlay {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {


        // System tray icon
        private System.Windows.Forms.NotifyIcon DDDATrayIcon;

        // Monitor
        private double _Width = SystemParameters.PrimaryScreenWidth;
        private double _Height = SystemParameters.PrimaryScreenHeight;

        // Starts all classes
        Player Player = new Player();
        Pawn FirstPawn = new Pawn(1);
        Pawn SecondPawn = new Pawn(2);
        Pawn ThirdPawn = new Pawn(3);
        
        // Make window click-through
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hwnd, int index, int style);

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hwnd, int index);

        // This will get the active window
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        // Main function
        public MainWindow() {
            InitializeComponent();

            // Sets the overlay width and height to user's monitor
            Overlay.Width = _Width;
            Overlay.Height = _Height;

            InitializeTrayIcon();
            MakeWindowClickThrough();
            Reader.GetDDDAProcess();
            this.Run();
        }

        // Initializes the tray icon
        private void InitializeTrayIcon() {
            DDDATrayIcon = new System.Windows.Forms.NotifyIcon();
            var icon = Properties.Resources.icon;
            DDDATrayIcon.Icon = icon;
            DDDATrayIcon.Text = "DDDA Overlay";
            DDDATrayIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(CloseOverlay);
            DDDATrayIcon.Visible = true;
            // Sends a message saying that DDDA Overlay is running;
            // Only works if user has Windows notifications on
            DDDATrayIcon.ShowBalloonTip(1000, "DDDA Overlay", "Overlay is running!", System.Windows.Forms.ToolTipIcon.Info);
        }

        // Makes everything in the window click through
        private void MakeWindowClickThrough() {
            // Const stuff
            int WS_EX_TRANSPARENT = 0x00000020;
            int GWL_EXSTYLE = (-20);

            // Get window handler
            var Wnd = GetWindow(Overlay);
            IntPtr hwnd = new WindowInteropHelper(Wnd).EnsureHandle();
            // Get window flags
            int Styles = GetWindowLong(hwnd, GWL_EXSTYLE);
            // Apply the click through flag
            SetWindowLong(hwnd, GWL_EXSTYLE, Styles | WS_EX_TRANSPARENT);
        }
        // Runs the thread
        private void Run() {
            ThreadStart CoreThreadRef = new ThreadStart(StartAllScanners_Thread);
            Thread CoreThread = new Thread(CoreThreadRef);
            CoreThread.Name = "Core_Scanner";
            CoreThread.Start();
        }

        // This function will check if DDDA window is the focused one
        private bool DDDAWindowActive() {
            return GetForegroundWindow() == Reader.DDDA_hWnd;
        }

        private void StartAllScanners_Thread() {
            if (Reader.ProcessIsRunning) {

                // Check if active window is DDDA, if it's not then the program will hide the pawn widget
                if (DDDAWindowActive()) {
                    // Update player
                    if (Player.Level == 0) {
                        HidePlayerExpBar();
                    } else {
                        ShowPlayerExpBar();
                        UpdateExp(Player.CurrentExp, Player.TotalExp, Player.Level);
                    }

                    // Check if pawns HP are higher than 0
                    CheckPawnsAreAlive();

                    // Updates First pawn
                    UpdateFirstPawn(FirstPawn.Name, FirstPawn.Current_HP, FirstPawn.Total_HP, FirstPawn.CurrentStamina, FirstPawn.TotalStamina);
                    
                    // Updates second pawn
                    UpdateSecondPawn(SecondPawn.Name, SecondPawn.Current_HP, SecondPawn.Total_HP, SecondPawn.CurrentStamina, SecondPawn.TotalStamina);
                   
                    // Updates third pawn
                    UpdateThirdPawn(ThirdPawn.Name, ThirdPawn.Current_HP, ThirdPawn.Total_HP, ThirdPawn.CurrentStamina, ThirdPawn.TotalStamina);

                } else {
                    HidePawns();
                    HidePlayerExpBar();
                }
            } else {
                // If the game isn't running then hide pawn HP bars
                HidePawns();
                HidePlayerExpBar();
            }
            Thread.Sleep(500);
            StartAllScanners_Thread();
        }

        private void HidePawns() {
            HideFirstPawn();
            HideSecondPawn();
            HideThirdPawn();
        }

        private void ShowPawns() {
            ShowFirstPawn();
            ShowSecondPawn();
            ShowThirdPawn();
        }

        // Hide Pawns
        private void HideFirstPawn() {
            if (!firstPawnBar.IsVisible) {
                return;
            }
            firstPawnBar.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                new Action(() => {
                    firstPawnBar.Visibility = Visibility.Hidden;
                    firstPawnName.Visibility = Visibility.Hidden;
                    firstPawnStamina.Visibility = Visibility.Hidden;
                })
            );
        }

        private void HideSecondPawn() {
            if (!secondPawnBar.IsVisible) {
                return;
            }
            secondPawnBar.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                new Action(() => {
                    secondPawnBar.Visibility = Visibility.Hidden;
                    secondPawnName.Visibility = Visibility.Hidden;
                    secondPawnStamina.Visibility = Visibility.Hidden;
                })
            );
        }

        private void HideThirdPawn() {
            if (!thirdPawnBar.IsVisible) {
                return;
            }
            thirdPawnBar.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                new Action(() => {
                    thirdPawnBar.Visibility = Visibility.Hidden;
                    thirdPawnName.Visibility = Visibility.Hidden;
                    thirdPawnStamina.Visibility = Visibility.Hidden;
                })
            );
        }

        // Update pawns HP bar
        private void UpdateFirstPawn(string Name, float CurrentHP, float TotalHP, int CurrentStamina, int TotalStamina) {
            this.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                new Action(() => {
                    this.firstPawnBar.Maximum = TotalHP;
                    this.firstPawnBar.Value = CurrentHP;
                    this.firstPawnName.Content = Name;
                    this.firstPawnStamina.Value = CurrentStamina;
                    this.firstPawnStamina.Maximum = TotalStamina;
                })
            );
        }

        private void UpdateSecondPawn(string Name, float CurrentHP, float TotalHP, int CurrentStamina, int TotalStamina) {
            this.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                new Action(() => {
                    this.secondPawnBar.Maximum = TotalHP;
                    this.secondPawnBar.Value = CurrentHP;
                    this.secondPawnName.Content = Name;
                    this.secondPawnStamina.Value = CurrentStamina;
                    this.secondPawnStamina.Maximum = TotalStamina;
                })
            );
        }

        private void UpdateThirdPawn(string Name, float CurrentHP, float TotalHP, int CurrentStamina, int TotalStamina) {
            this.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                new Action(() => {
                    this.thirdPawnBar.Maximum = TotalHP;
                    this.thirdPawnBar.Value = CurrentHP;
                    this.thirdPawnName.Content = Name;
                    this.thirdPawnStamina.Value = CurrentStamina;
                    this.thirdPawnStamina.Maximum = TotalStamina;
                })
            );
        }

        //Show Pawns
        private void ShowFirstPawn() {
            if (firstPawnBar.IsVisible) {
                return;
            }
            firstPawnBar.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                new Action(() => {
                    firstPawnBar.Visibility = Visibility.Visible;
                    firstPawnName.Visibility = Visibility.Visible;
                    firstPawnStamina.Visibility = Visibility.Visible;
                })
            );
        }

        private void ShowSecondPawn() {
            if (secondPawnBar.IsVisible) {
                return;
            }
            secondPawnBar.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                new Action(() => {
                    secondPawnBar.Visibility = Visibility.Visible;
                    secondPawnName.Visibility = Visibility.Visible;
                    secondPawnStamina.Visibility = Visibility.Visible;
                })
            );
        }

        private void ShowThirdPawn() {
            if (thirdPawnBar.IsVisible) {
                return;
            }
            thirdPawnBar.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                new Action(() => {
                    thirdPawnBar.Visibility = Visibility.Visible;
                    thirdPawnName.Visibility = Visibility.Visible;
                    thirdPawnStamina.Visibility = Visibility.Visible;
                })
            );
        }

        // Check pawn HP
        private void CheckPawnsAreAlive() {
            if (FirstPawn.Current_HP <= 0) {
                HideFirstPawn();
            } else {
                ShowFirstPawn();
            }
            if (SecondPawn.Current_HP <= 0) {
                HideSecondPawn();
            } else {
                ShowSecondPawn();
            }
            if (ThirdPawn.Current_HP <= 0) {
                HideThirdPawn();
            } else {
                ShowThirdPawn();
            }
        }

        private void Overlay_Closed(object sender, EventArgs e) {
            DDDATrayIcon.Visible = false;
            Environment.Exit(0);
        }

        private void Overlay_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            DDDATrayIcon.Visible = false;
        }

        private void CloseOverlay(object sender, System.Windows.Forms.MouseEventArgs e) {
            if (MessageBox.Show("Are you sure you want to exit application?", "DDDA Overlay", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) {
                DDDATrayIcon.Visible = false;
                Environment.Exit(0);
            }
        }

        // Player stuff
        private void HidePlayerExpBar() {
            if (!ExperienceBar.IsVisible) {
                return;
            }
            this.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                new Action(() => {
                    ExperienceBar.Visibility = Visibility.Hidden;
                    currentLevelText.Visibility = Visibility.Hidden;
                    nextLevelText.Visibility = Visibility.Hidden;
                    levelExpText.Visibility = Visibility.Hidden;
                    levelPercentageText.Visibility = Visibility.Hidden;
                })
            );
        }

        private void ShowPlayerExpBar() {
            if (ExperienceBar.IsVisible) {
                return;
            }
            this.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                new Action(() => {
                    ExperienceBar.Visibility = Visibility.Visible;
                    currentLevelText.Visibility = Visibility.Visible;
                    nextLevelText.Visibility = Visibility.Visible;
                    levelExpText.Visibility = Visibility.Visible;
                    levelPercentageText.Visibility = Visibility.Visible;
                })
            );
        }

        private void UpdateExp(int CurrentExp, int TotalExp, int CurrentLevel) {
            float f_TotalExp = TotalExp;
            float f_CurrentExp = CurrentExp;
            float ExpPercentage = (f_CurrentExp / f_TotalExp) * 100.0f;
            this.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                new Action(() => {
                    
                    // Texts
                    currentLevelText.Content = $"Lvl {CurrentLevel}";
                    nextLevelText.Content = $"Lvl {CurrentLevel + 1}";
                    levelExpText.Content = $"({CurrentExp}/{TotalExp})";
                    levelPercentageText.Content = $"{(int)ExpPercentage}%";

                    // Actual bar
                    ExperienceBar.Maximum = TotalExp;
                    ExperienceBar.Value = CurrentExp;

                })
            );
        }

    }
}
