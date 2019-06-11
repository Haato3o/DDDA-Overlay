using System;
using System.Windows;
using System.Threading;
using System.Runtime.InteropServices;
using DDDA_Overlay.Game;
using DDDA_Overlay.Memory;
using System.Windows.Interop;
using System.Text;

namespace DDDA_Overlay {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        // System tray icon
        private System.Windows.Forms.NotifyIcon DDDATrayIcon;

        // DDDA stuff
        const string DDDA_Name = "Dragon's Dogma: Dark Arisen";

        // Starts all classes
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
            var Wnd = Window.GetWindow(Overlay);
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
                    // Check if pawns HP are higher than 0
                    CheckPawnsAreAlive();

                    // Updates First pawn
                    UpdateFirstPawnBar(FirstPawn.Current_HP, FirstPawn.Total_HP);
                    UpdateFirstPawnName(FirstPawn.Name);
                    UpdateFirstPawnStamina(FirstPawn.CurrentStamina, FirstPawn.TotalStamina);

                    // Updates second pawn
                    UpdateSecondPawnBar(SecondPawn.Current_HP, SecondPawn.Total_HP);
                    UpdateSecondPawnName(SecondPawn.Name);
                    UpdateSecondPawnStamina(SecondPawn.CurrentStamina, SecondPawn.TotalStamina);

                    // Updates third pawn
                    UpdateThirdPawnBar(ThirdPawn.Current_HP, ThirdPawn.Total_HP);
                    UpdateThirdPawnName(ThirdPawn.Name);
                    UpdateThirdPawnStamina(ThirdPawn.CurrentStamina, ThirdPawn.TotalStamina);
                } else {
                    HidePawns();
                }
                

            } else {
                // If the game isn't running then hide pawn HP bars
                HidePawns();
            }
            Thread.Sleep(200);
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
            Overlay.firstPawnBar.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Background,
                new Action(() => {
                    Overlay.firstPawnBar.Visibility = Visibility.Hidden;
                    Overlay.firstPawnName.Visibility = Visibility.Hidden;
                    Overlay.firstPawnStamina.Visibility = Visibility.Hidden;
                })
            );
        }

        private void HideSecondPawn() {
            Overlay.secondPawnBar.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Background,
                new Action(() => {
                    Overlay.secondPawnBar.Visibility = Visibility.Hidden;
                    Overlay.secondPawnName.Visibility = Visibility.Hidden;
                    Overlay.secondPawnStamina.Visibility = Visibility.Hidden;
                })
            );
        }

        private void HideThirdPawn() {
            Overlay.thirdPawnBar.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Background,
                new Action(() => {
                    Overlay.thirdPawnBar.Visibility = Visibility.Hidden;
                    Overlay.thirdPawnName.Visibility = Visibility.Hidden;
                    Overlay.thirdPawnStamina.Visibility = Visibility.Hidden;
                })
            );
        }

        // Update pawns HP bar
        private void UpdateFirstPawnBar(float CurrentHP, float TotalHP) {
            Overlay.firstPawnBar.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Background,
                new Action(() => {
                    Overlay.firstPawnBar.Maximum = TotalHP;
                    Overlay.firstPawnBar.Value = CurrentHP;
                })
            );
        }

        private void UpdateSecondPawnBar(float CurrentHP, float TotalHP) {
            Overlay.secondPawnBar.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Background,
                new Action(() => {
                    Overlay.secondPawnBar.Maximum = TotalHP;
                    Overlay.secondPawnBar.Value = CurrentHP;
                })
            );
        }

        private void UpdateThirdPawnBar(float CurrentHP, float TotalHP) {
            Overlay.thirdPawnBar.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Background,
                new Action(() => {
                    Overlay.thirdPawnBar.Maximum = TotalHP;
                    Overlay.thirdPawnBar.Value = CurrentHP;
                })
            );
        }

        //Show Pawns
        private void ShowFirstPawn() {
            Overlay.firstPawnBar.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Background,
                new Action(() => {
                    Overlay.firstPawnBar.Visibility = Visibility.Visible;
                    Overlay.firstPawnName.Visibility = Visibility.Visible;
                    Overlay.firstPawnStamina.Visibility = Visibility.Visible;
                })
            );
        }

        private void ShowSecondPawn() {
            Overlay.secondPawnBar.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Background,
                new Action(() => {
                    Overlay.secondPawnBar.Visibility = Visibility.Visible;
                    Overlay.secondPawnName.Visibility = Visibility.Visible;
                    Overlay.secondPawnStamina.Visibility = Visibility.Visible;
                })
            );
        }

        private void ShowThirdPawn() {
            Overlay.thirdPawnBar.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Background,
                new Action(() => {
                    Overlay.thirdPawnBar.Visibility = Visibility.Visible;
                    Overlay.thirdPawnName.Visibility = Visibility.Visible;
                    Overlay.thirdPawnStamina.Visibility = Visibility.Visible;
                })
            );
        }

        // Update Pawns names
        private void UpdateFirstPawnName(string Name) {
            Overlay.firstPawnName.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Background,
                new Action(() => {
                    Overlay.firstPawnName.Content = Name;
                })
            );
        }

        private void UpdateSecondPawnName(string Name) {
            Overlay.secondPawnName.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Background,
                new Action(() => {
                    Overlay.secondPawnName.Content = Name;
                })
            );
        }

        private void UpdateThirdPawnName(string Name) {
            Overlay.thirdPawnName.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Background,
                new Action(() => {
                    Overlay.thirdPawnName.Content = Name;
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


        // Update pawns staminas
        private void UpdateFirstPawnStamina(int CurrentStamina, int TotalStamina) {
            Overlay.firstPawnStamina.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Background,
                new Action(() => {
                    Overlay.firstPawnStamina.Maximum = TotalStamina;
                    Overlay.firstPawnStamina.Value = CurrentStamina;
                })
            );
        }

        private void UpdateSecondPawnStamina(int CurrentStamina, int TotalStamina) {
            Overlay.secondPawnStamina.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Background,
                new Action(() => {
                    Overlay.secondPawnStamina.Maximum = TotalStamina;
                    Overlay.secondPawnStamina.Value = CurrentStamina;
                })
            );
        }

        private void UpdateThirdPawnStamina(int CurrentStamina, int TotalStamina) {
            Overlay.thirdPawnStamina.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Background,
                new Action(() => {
                    Overlay.thirdPawnStamina.Maximum = TotalStamina;
                    Overlay.thirdPawnStamina.Value = CurrentStamina;
                })
            );
        }

    }
}
