using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using DDDA_Overlay.Memory;

namespace DDDA_Overlay.Game {
    class Player {

        // Memory addresses
        // Player name
        private Int64 PlayerNameStaticAddress = 0x014D9280;
        private Int64[] PlayerNameOffset = new Int64[4] { 0x30, 0x434, 0x64, 0xA4 };
        private Int64 PlayerNameStringOffset = 0xD4;

        // Player level
        private Int64 PlayerLevelAddress;
        private Int64 PlayerLevelOffset = 0x4414;

        // Player EXP
        private Int64 PlayerExpAddress;
        private Int64 PlayerTotalExpOffset = 0x4;
        private Int64 PlayerExpOffset = 0x43C;


        // Player info
        public string Name { get; private set; }
        public int Level { get; private set; }
        public int CurrentExp { get; private set; }
        public int TotalExp { get; private set; }

        public Player() {
            StartPlayerScan();
        }

        private void StartPlayerScan() {
            ThreadStart playerScanRef = new ThreadStart(ScanPlayerInfo);
            Thread playerScan = new Thread(playerScanRef);
            playerScan.Name = "Player_Scanner";
            playerScan.Start();
        }

        private void ScanPlayerInfo() {
            while (true) {
                GetPlayerName();
                GetPlayerLevel();
                GetPlayerExp();
                Thread.Sleep(500);
            }
        }

        private void GetPlayerName() {
            // Get player name
            Int64 playerNameAddress = Reader.GET_MULTILEVEL_POINTER(PlayerNameStaticAddress, PlayerNameOffset);
            string playerName = Reader.READ_STRING(playerNameAddress + PlayerNameStringOffset, 32);

            // Since we have the name address, we can just add the offset to it and get the lvl and exp addressess too
            PlayerLevelAddress = playerNameAddress + PlayerNameStringOffset - PlayerLevelOffset;
            PlayerExpAddress = PlayerLevelAddress - PlayerExpOffset;

            this.Name = playerName.Split('\x00')[0];
        }

        private void GetPlayerLevel() {
            int playerLevel = Reader.READ_INT(PlayerLevelAddress);

            this.Level = playerLevel;
        }

        private void GetPlayerExp() {
            int playerCurrentExp = Reader.READ_INT(PlayerExpAddress);
            int playerTotalExp = Reader.READ_INT(PlayerExpAddress + PlayerTotalExpOffset);

            this.CurrentExp = playerCurrentExp;
            this.TotalExp = playerTotalExp;
        }


    }
}
