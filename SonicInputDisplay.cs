using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;

using JoystickDisplay;

public class SonicInputDisplay
{
    public const int PROCESS_VM_READ = 0x0010;
    public const int SW_HIDE = 0;

    [DllImport("kernel32.dll")]
    public static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll")]
    public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("kernel32.dll")]
    public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    [DllImport("kernel32.dll")]
    public static extern bool ReadProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);
    
    public static IntPtr processHandle = IntPtr.Zero;
    public static int gameID = -1;
    public static bool loop = true;

    public static Display theDisplay;

    public static void Main()
    {
        var handle = GetConsoleWindow();
        ShowWindow(handle, SW_HIDE);
        
        theDisplay = new Display();
        theDisplay.ClientSize = new Size(216, 136);
        theDisplay.MinimizeBox = false;
        theDisplay.MaximizeBox = false;
        theDisplay.FormBorderStyle = FormBorderStyle.FixedSingle; 
        theDisplay.StartPosition = FormStartPosition.CenterScreen;
        theDisplay.BackColor = Color.FromArgb(2, 2, 2); //Almost, but not exactly black
        theDisplay.Text = "Searching for game...";
        
        try
        {
            string[] lines = System.IO.File.ReadAllLines("BackgroundColor.ini");
            string[] color = lines[0].Split(',');
            int r = Int32.Parse(color[0]);
            int g = Int32.Parse(color[1]);
            int b = Int32.Parse(color[2]);
            theDisplay.BackColor = Color.FromArgb(r, g, b);
        }
        catch {}
        
        try
        {
            string[] lines = System.IO.File.ReadAllLines("StickColor.ini");
            string[] color = lines[0].Split(',');
            int r = Int32.Parse(color[0]);
            int g = Int32.Parse(color[1]);
            int b = Int32.Parse(color[2]);
            Display.stickPen = new Pen(Color.FromArgb(r, g, b), 1);
        }
        catch {}
        
        //Thread to handle the window
        new Thread(() =>
        {
            Thread.CurrentThread.IsBackground = true;
            theDisplay.ShowDialog();
            theDisplay.saveIndex();
            loop = false;
        }).Start();
        
        while (loop)
        {
            switch (gameID)
            {
                case 0: setValuesFromSA2(); break;
                
                case 1: setValuesFromSADX(); break;
                
                case 2: setValuesFromHeroes(); break;
                
                case 3: setValuesFromMania(); break;
                    
                default: attatchToGame(); break;
            }
            theDisplay.Refresh();
            System.Threading.Thread.Sleep(5);
        }
    }

    private static void setValuesFromSA2()
    {
        int bytesRead = 0;
        byte[] buffer = new byte[12];
        if (ReadProcessMemory((int)processHandle, 0x01A52C4C, buffer, 12, ref bytesRead) == false || bytesRead != 12)
        {
            theDisplay.setControllerDataSA2(0, 0, 0);
            gameID = -1;
            return;
        }

        int buttons = 0;
        buttons+=buffer[0];
        buttons+=buffer[1]<<8;
        buttons+=buffer[2]<<16;
        buttons+=buffer[3]<<24;
        
        int joyX = 0;
        joyX+=buffer[4];
        joyX+=buffer[5]<<8;
        joyX+=buffer[6]<<16;
        joyX+=buffer[7]<<24;
        
        int joyY = 0;
        joyY+=buffer[8];
        joyY+=buffer[9]<<8;
        joyY+=buffer[10]<<16;
        joyY+=buffer[11]<<24;
        
        theDisplay.setControllerDataSA2(buttons, joyX, joyY);
    }
    
    private static void setValuesFromSADX()
    {
        int bytesRead = 0;
        byte[] buffer = new byte[20];
        if (ReadProcessMemory((int)processHandle, 0x03B0E80C, buffer, 20, ref bytesRead) == false || bytesRead != 20)
        {
            theDisplay.setControllerDataSADX(0, 0, 0);
            gameID = -1;
            return;
        }

        int buttons = 0;
        buttons+=buffer[16];
        buttons+=buffer[17]<<8;
        buttons+=buffer[18]<<16;
        buttons+=buffer[19]<<24;
        
        int joyX = 0;
        joyX+=buffer[0];
        joyX+=buffer[1]<<8;
        joyX+=buffer[1]<<16;
        joyX+=buffer[1]<<24;
        
        int joyY = 0;
        joyY+=buffer[2];
        joyY+=buffer[3]<<8;
        joyY+=buffer[3]<<16;
        joyY+=buffer[3]<<24;
        
        theDisplay.setControllerDataSADX(buttons, joyX, -joyY);
    }
    
    private static void setValuesFromHeroes()
    {
        int bytesRead = 0;
        byte[] buffer = new byte[28];
        if (ReadProcessMemory((int)processHandle, 0x00A23598, buffer, 28, ref bytesRead) == false || bytesRead != 28)
        {
            theDisplay.setControllerDataHeroes(0, 0, 0, 0);
            gameID = -1;
            return;
        }

        int buttons = 0;
        buttons+=buffer[0];
        buttons+=buffer[1]<<8;
        buttons+=buffer[2]<<16;
        buttons+=buffer[3]<<24;
        
        float joyX = System.BitConverter.ToSingle(buffer, 16);
        float joyY = System.BitConverter.ToSingle(buffer, 20);
        float cameraPan = System.BitConverter.ToSingle(buffer, 24);
        
        theDisplay.setControllerDataHeroes(buttons, joyX, joyY, cameraPan);
    }
    
    private static void setValuesFromMania()
    {
        int bytesRead = 0;
        byte[] buffer = new byte[2];
        
        if (ReadProcessMemory((int)processHandle, 0x00BEE5B0, buffer, 2, ref bytesRead) == false || bytesRead != 2)
        {
            theDisplay.setControllerDataMania(0);
            gameID = -1;
            return;
        }

        int inputsController = 0;
        inputsController+=buffer[0];
        inputsController+=buffer[1]<<8;
        
        if (ReadProcessMemory((int)processHandle, 0x00BED58C, buffer, 2, ref bytesRead) == false || bytesRead != 2)
        {
            theDisplay.setControllerDataMania(0);
            gameID = -1;
            return;
        }

        int inputsKeyboard = 0;
        inputsKeyboard+=buffer[0];
        inputsKeyboard+=buffer[1]<<8;
        
        theDisplay.setControllerDataMania(inputsKeyboard | inputsController);
    }
    
    private static void attatchToGame()
    {
        theDisplay.Text = "Searching for game...";
        processHandle = IntPtr.Zero;
        gameID = -1;
        
        Process process = null;
        try 
        {
            process = Process.GetProcessesByName("sonic2app")[0];
            gameID = 0;
        }
        catch
        {
            try
            {
                process = Process.GetProcessesByName("sonic")[0];
                gameID = 1;
            }
            catch
            {
                try
                {
                    process = Process.GetProcessesByName("Sonic Adventure DX")[0];
                    gameID = 1;
                }
                catch
                {
                    try
                    {
                        process = Process.GetProcessesByName("Tsonic_win")[0];
                        gameID = 2;
                    }
                    catch
                    {
                        try
                        {
                            process = Process.GetProcessesByName("SonicMania")[0];
                            gameID = 3;
                        }
                        catch
                        {
                            gameID = -1;
                        }
                    }
                }
            }
        }
        
        if (gameID != -1)
        {
            try
            {
                processHandle = OpenProcess(PROCESS_VM_READ, false, process.Id);
            }
            catch
            {
                gameID = -1;
            }
        }
        
        System.Threading.Thread.Sleep(500);
        
        switch (gameID)
        {
            case 0: theDisplay.Text = "SA2 Input"; break;
            
            case 1: theDisplay.Text = "SADX Input"; break;
            
            case 2: theDisplay.Text = "Heroes Input"; break;
            
            case 3: theDisplay.Text = "Mania Input"; break;
                
            default: break;
        }
    }
}
