using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using DDDA_Overlay.Memory;

namespace DDDA_Overlay.Game {
    class Pawn {

        // Memory stuff
        // Pawn HP Address
        private Int64 PawnHPStaticAddress;
        private Int64 PawnHPOffset = 0x4878;
        private Int64[] PawnSlotOffset = new Int64[4] { 0x0, 0x7F0, 0x1E50, 0x34B0 };
        private Int64 TotalHPOffset = 0x4;
        private Int64 TotalHPOffsetAlt = 0x8;
        private Int64 PawnHPFloatOffset = 0x0;

        // Pawn Name Address
        private Int64 PawnNameStaticAddress = 0x014D9280;
        private Int64[] PawnNameOffset = new Int64[4] { 0x30, 0x434, 0x64, 0xA4 };
        private Int64 NextPawnOffset = 0x54;
        private Int64 PawnNameStringOffset = 0xD4;

        // Pawn Stamina address
        private Int64 PawnStaminaStaticAddress;
        private Int64 PawnStaminaOffset = 0xC;
        private Int64[] PawnTotalStaminaOffsets = new Int64[2] { 0x4, 0x8 };


        // Pawn info
        private int PawnSlot;
        public string Name { get; private set; }
        public int Current_HP { get; private set; }
        public int Total_HP { get; private set; }
        public int Level { get; private set; }
        public int TotalStamina { get; private set; }
        public int CurrentStamina { get; private set; }

        public Pawn(int Slot) {
            // Initialize the pawn class with a slot number
            this.PawnSlot = Slot;
            InitializePawnScanner();
        }

        public void InitializePawnScanner() {
            // Starts the thread to scan the pawn
            ThreadStart PawnScannerRef = new ThreadStart(PawnScanner_Thread);
            Thread PawnScanner = new Thread(PawnScannerRef);
            PawnScanner.Name = "Pawn_Scanner";
            PawnScanner.Start();
        }

        private void PawnScanner_Thread() {
            while (true) {
                GetPawnName();
                GetPawnHP();
                GetPawnStamina();
                Thread.Sleep(200); // Sleeps for 200 ms so it doesn't consume a lot of CPU
            }
        }

        // Get current pawn hp
        private void GetPawnHP() {
            
            Int64 PlayerHPAddress = PawnHPStaticAddress - PawnHPOffset;
            // Get pawn HP
            var CurrentHP = Reader.READ_FLOAT(PlayerHPAddress + PawnHPFloatOffset + PawnSlotOffset[PawnSlot]);
            var TotalHP = Reader.READ_FLOAT(PlayerHPAddress + PawnHPFloatOffset + PawnSlotOffset[PawnSlot] + TotalHPOffset);
            
            // Check if total hp is higher than current hp
            if (TotalHP < CurrentHP) {
                TotalHP = Reader.READ_FLOAT(PlayerHPAddress + PawnHPFloatOffset + PawnSlotOffset[PawnSlot] + TotalHPOffsetAlt); ;
            }
            // Assign variable values now
            this.Current_HP = (int)CurrentHP >= 0 ? (int)CurrentHP : 0;
            this.Total_HP = (int)TotalHP >= 0 ? (int)TotalHP : 0;

            // Assign Stamina static address to pawn's current hp address since they're really close to each other;
            PawnStaminaStaticAddress = PlayerHPAddress + PawnHPFloatOffset + PawnSlotOffset[PawnSlot];
        }

        private void GetPawnName() {
            // Get player name address to find pawn's
            Int64 PlayerNameAddress = Reader.GET_MULTILEVEL_POINTER(PawnNameStaticAddress, PawnNameOffset);
            
            // Since I couldn't find the static address for the HP, I can find the HP based on the pawn's names address;
            this.PawnHPStaticAddress = PlayerNameAddress + PawnNameStringOffset;

            // Get pawn name
            this.Name = Reader.READ_STRING(PlayerNameAddress + PawnNameStringOffset + (PawnSlot * NextPawnOffset), 32).Split('\x00')[0];
        }

        private void GetPawnStamina() {
            // Gets pawns staminas
            float CurrentPawnStamina = Reader.READ_FLOAT(PawnStaminaStaticAddress + PawnStaminaOffset);
            float TotalPawnStamina = 0;
            // Total stamina is in 2 different memory addresses, the first one is the bonus stamina from augments/gear
            // The second one is the base stamina
            foreach (Int64 offset in PawnTotalStaminaOffsets) {
                TotalPawnStamina += Reader.READ_FLOAT(PawnStaminaStaticAddress + PawnStaminaOffset + offset);
            }
            // Assign the stamina to the class variables
            this.CurrentStamina = (int)CurrentPawnStamina;
            this.TotalStamina = (int)TotalPawnStamina;
        }

    }
}
