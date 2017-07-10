using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;

using JoystickDisplay;

public class SonicInputDisplay
{
	const int PROCESS_WM_READ = 0x0010;
    const int SW_HIDE = 0;

    [DllImport("kernel32.dll")]
    static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll")]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("kernel32.dll")]
    public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    [DllImport("kernel32.dll")]
    public static extern bool ReadProcessMemory(int hProcess, 
        int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);
	
    public static IntPtr processHandle;
	public static int gameID = -1;
	
	private static Display theDisplay;
	
	public static void Main()
    {
		var handle = GetConsoleWindow();
        ShowWindow(handle, SW_HIDE);

        Process process;
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
					MessageBox.Show("Open either SADX of SA2B");
					return;
				}
			}
        }
		
        processHandle = OpenProcess(PROCESS_WM_READ, false, process.Id); 
        if (processHandle == null)
        {
            MessageBox.Show("Couldn't open the process");
            return;
        }
		
		
		theDisplay = new Display();
		theDisplay.ClientSize = new Size(216, 136);
		theDisplay.MinimizeBox = false;
		theDisplay.MaximizeBox = false;
		theDisplay.FormBorderStyle = FormBorderStyle.FixedSingle; 
		theDisplay.StartPosition = FormStartPosition.CenterScreen;
		theDisplay.BackColor = System.Drawing.Color.Black;
		switch (gameID)
		{
			case 0: theDisplay.Text = "SA2 Input"; break;
			
			case 1: theDisplay.Text = "SADX Input"; break;
				
			default: break;
		}
		
		bool loop = true;
		
		//Thread to handle the window
        new Thread(() =>
        {
            Thread.CurrentThread.IsBackground = true;
            theDisplay.ShowDialog();
            loop = false;
        }).Start();
		
		
		while (loop)
        {
			switch (gameID)
			{
				case 0: setValuesFromSA2();break;
				
				case 1: setValuesFromSADX(); break;
					
				default: break;
			}
			theDisplay.Refresh();
            System.Threading.Thread.Sleep(14);
        }
		
	}

	private static void setValuesFromSA2()
	{
		int bytesRead = 0;
        byte[] buffer = new byte[12];
        if (ReadProcessMemory((int)processHandle, 0x01A52C4C, buffer, 12, ref bytesRead) == false || bytesRead != 12)
        {
			theDisplay.setControllerDataSA2(0, 0, 0);
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
}
