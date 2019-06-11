using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;

namespace DDDA_Overlay.Memory {
    class Reader {
        // Constant variables
        const int INT = 4;
        const int LONG = 4;
        const int CHAR = 1;
        const int LONGLONG = 8;
        const int FLOAT = 4;
        const int MEMORY_ALL_ACCESS = 0x1F0FFF;

        // Process variables
        private static int PID;
        private static string ProcessName = "DDDA";
        private static Process[] DDDA_Process;
        private static IntPtr ProcessHandle;
        public static bool ProcessIsRunning = false;
        public static Int64 BaseAddress = 0x00400000;

        // Window variables
        public static IntPtr DDDA_hWnd { get; private set; }
        
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(int hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        public static void GetDDDAProcess() {
            ThreadStart gameScannerRef = new ThreadStart(thread_GetDDDAProcess);
            Thread gameScanner = new Thread(gameScannerRef);
            gameScanner.Name = "DDDA_Scanner";
            gameScanner.Start();
        }

        private static void thread_GetDDDAProcess() {
            while (true) {
                DDDA_Process = Process.GetProcessesByName(ProcessName);
                // If the process isn't running then DDDA_Process will be empty
                if (DDDA_Process.Length == 0) {
                    ProcessIsRunning = false;
                    PID = 0;
                } else if (!ProcessIsRunning) {
                    PID = DDDA_Process[0].Id;
                    DDDA_hWnd = DDDA_Process[0].MainWindowHandle;
                    ProcessHandle = OpenProcess(MEMORY_ALL_ACCESS, false, PID);
                    ProcessIsRunning = true;
                }
                Thread.Sleep(1000);
            }
        }

        public static Int64 READ_LONGLONG(Int64 Address) {
            byte[] Buffer = new byte[LONGLONG];
            int bytesRead = 0;
            ReadProcessMemory((int)ProcessHandle, (IntPtr)Address, Buffer, LONGLONG, ref bytesRead);
            return BitConverter.ToInt64(Buffer, 0);
        }

        public static int READ_INT(Int64 Address) {
            byte[] Buffer = new byte[INT];
            int bytesRead = 0;
            ReadProcessMemory((int)ProcessHandle, (IntPtr)Address, Buffer, INT, ref bytesRead);
            return BitConverter.ToInt32(Buffer, 0);
        }

        public static Int64 GET_MULTILEVEL_POINTER(Int64 StaticAddress, Int64[] offsets) {
            Int64 Address = READ_INT(BaseAddress + StaticAddress);
            foreach (Int64 offset in offsets) {
                Address = READ_INT(Address + offset);
            }
            return Address;
        }

        public static string READ_STRING(Int64 Address, int size) {
            byte[] Buffer = new byte[size];
            int bytesRead = 0;
            ReadProcessMemory((int)ProcessHandle, (IntPtr)Address, Buffer, size, ref bytesRead);
            return Encoding.UTF8.GetString(Buffer, 0, size);
        }

        public static float READ_FLOAT(Int64 Address) {
            byte[] Buffer = new byte[FLOAT];
            int bytesRead = 0;
            ReadProcessMemory((int)ProcessHandle, (IntPtr)Address, Buffer, FLOAT, ref bytesRead);
            return BitConverter.ToSingle(Buffer, 0);
        }
    }

}

