using System;
using System.Drawing;
using System.Windows.Forms;

namespace JoystickDisplay
{
	public partial class Display : Form
	{
		private static int joyX;
		private static int joyY;
		private static int A;
		private static int B;
		private static int X;
		private static int Y;
		private static int L;
		private static int R;
		private static int S;
		
		private static Bitmap imgBase;
		private static Bitmap imgStick;
		private static Bitmap imgStickSmall;
		private static Bitmap imgA;
		private static Bitmap imgB;
		private static Bitmap imgX;
		private static Bitmap imgY;
		private static Bitmap imgL;
		private static Bitmap imgR;
		private static Bitmap imgS;
		
		private static Pen penBlack;
		
		private static int folderIndex = 0;
		
		public Display()
		{
			joyX = 0;
			joyY = 0;
			A    = 1;
			B    = 1;
			X    = 1;
			Y    = 1;
			L    = 1;
			R    = 1;
			S    = 1;
			
			try
			{
				string[] lines = System.IO.File.ReadAllLines("Index.ini");
				int savedIndex = Int32.Parse(lines[0]);
				folderIndex = savedIndex;
			}
			catch {}
			
			string path = System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
			string[] folders = System.IO.Directory.GetDirectories(path,"*", System.IO.SearchOption.AllDirectories);
			string folder = folders[folderIndex];
			
			imgBase       = new Bitmap(folder+"/base.png");
			imgStick      = new Bitmap(folder+"/stick.png");
			imgA          = new Bitmap(folder+"/buttA.png");
			imgB          = new Bitmap(folder+"/buttB.png");
			imgX          = new Bitmap(folder+"/buttX.png");
			imgY          = new Bitmap(folder+"/buttY.png");
			imgL          = new Bitmap(folder+"/buttL.png");
			imgR          = new Bitmap(folder+"/buttR.png");
			imgS          = new Bitmap(folder+"/buttS.png");
			imgStickSmall = new Bitmap(folder+"/stickSmall.png");
			
			penBlack = new Pen(Color.Black, 1);
			
			this.DoubleBuffered = true;
			this.Icon = new Icon("icon.ico");
			
			this.KeyPreview = true;
			this.PreviewKeyDown += new PreviewKeyDownEventHandler(keyPress);
		}
		
		public void setControllerDataSA2(int buttons, int newX, int newY)
		{
			A = buttons & 256;
			B = buttons & 512;
			Y = buttons & 2048;
			X = buttons & 1024;
			S = buttons & 4096;
			R = buttons & 32;
			L = buttons & 64;
			
			joyX = newX;
			joyY = newY;
			
			//D-Pad
			int up    = buttons & 8;
			int down  = buttons & 4;
			int left  = buttons & 1;
			int right = buttons & 2;
			
			if (right != 0)
			{
				joyX = 127;
			}
			else if (left != 0)
			{
				joyX = -128;
			}
			
			if (up != 0)
			{
				joyY = 127;
			}
			else if (down != 0)
			{
				joyY = -128;
			}
		}
		
		public void setControllerDataSADX(int buttons, int newX, int newY)
		{
			A = buttons & 4;
			B = buttons & 2;
			Y = buttons & 512;
			X = buttons & 1024;
			S = buttons & 8;
			R = buttons & 65536;
			L = buttons & 131072;
			
			joyX = newX;
			joyY = newY;
			
			//D-Pad
			int up    = buttons & 16;
			int down  = buttons & 32;
			int left  = buttons & 64;
			int right = buttons & 128;
			
			if (right != 0)
			{
				joyX = 127;
			}
			else if (left != 0)
			{
				joyX = -128;
			}
			
			if (up != 0)
			{
				joyY = 127;
			}
			else if (down != 0)
			{
				joyY = -128;
			}
		}
		
		public void setControllerDataHeroes(int buttons, float newX, float newY, float camPan)
		{
			A = buttons & 1;
			B = buttons & 2;
			Y = buttons & 8;
			X = buttons & 4;
			S = buttons & 16384;
			R = buttons & 512;
			L = buttons & 256;
			
			if (camPan < -0.3)
			{
				R = 1;
			}
			
			if (camPan > 0.3)
			{
				L = 1;
			}
			
			joyX = (int)(newX*127);
			joyY = (int)(-newY*127);
			
			//D-Pad
			int up    = buttons & 16;
			int down  = buttons & 32;
			int left  = buttons & 64;
			int right = buttons & 128;
			
			if (right != 0)
			{
				joyX = 127;
			}
			else if (left != 0)
			{
				joyX = -128;
			}
			
			if (up != 0)
			{
				joyY = 127;
			}
			else if (down != 0)
			{
				joyY = -128;
			}
		}
		
		public void setControllerDataMania(int buttons, int stick)
		{
			A = buttons & 16;
			B = buttons & 32;
			Y = buttons & 128;
			X = buttons & 64;
			S = stick   & 32; //Enter key sets both 16 and 32 bit
			R = 0;
			L = 0;
			
			joyX = 0;
			joyY = 0;
			
			int up    = stick & 1;
			int down  = stick & 2;
			int left  = stick & 4;
			int right = stick & 8;
			
			if (right != 0)
			{
				joyX = 127;
			}
			else if (left != 0)
			{
				joyX = -128;
			}
			
			if (up != 0)
			{
				joyY = 127;
			}
			else if (down != 0)
			{
				joyY = -128;
			}
		}
		
		protected override void OnPaint(PaintEventArgs e)
		{
			if (A != 0)
			{
				e.Graphics.DrawImage(imgA, 4, 100);
			}
		   
			if (B != 0)
			{
				e.Graphics.DrawImage(imgB, 180, 100);
			}
		   
			if (X != 0)
			{
			   e.Graphics.DrawImage(imgX, 180, 52);
			}
		   
			if (Y != 0)
			{
				e.Graphics.DrawImage(imgY, 4, 52);
			}

			if (L != 0)
			{
				e.Graphics.DrawImage(imgL, 4, 4);
			}

			if (R != 0)
			{
				e.Graphics.DrawImage(imgR, 180, 4);
			}

			if (S != 0)
			{
				e.Graphics.DrawImage(imgS, 156, 4);
			}

			int drawX = 108+((64*joyX)/128);
			int drawY = 68-((64*joyY)/128);

			e.Graphics.DrawImage(imgBase, 108-64, 68-64);
			e.Graphics.DrawLine(penBlack, 108, 68, drawX, drawY);
			e.Graphics.DrawImage(imgStickSmall, drawX-4, drawY-4);

			double radius = Math.Min(Math.Sqrt((joyX*joyX)+(joyY*joyY)), 128.0);
			double angle = Math.Atan2(joyY, joyX);
			int capX = (int)(radius*Math.Cos(angle));
			int capY = (int)(radius*Math.Sin(angle));
			int drawCapX = 108+((64*capX)/128);
			int drawCapY = 68-((64*capY)/128);

			e.Graphics.DrawImage(imgStick, drawCapX-4, drawCapY-4);

			base.OnPaint(e);
		}
		
		public void keyPress(object sender, PreviewKeyDownEventArgs e)
		{
			string path = System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
			string[] folders = System.IO.Directory.GetDirectories(path,"*", System.IO.SearchOption.AllDirectories);
			
			int prevFolderIndex = folderIndex;
			
			folderIndex = Math.Max(0, Math.Min(folders.Length-1, folderIndex));
			
			switch (e.KeyCode)
			{
				case Keys.Left:
				{
					folderIndex--;
					if (folderIndex == -1)
					{
						folderIndex = folders.Length-1;
					}
					break;
				}
				
				case Keys.Right:
				{
					folderIndex++;
					if (folderIndex == folders.Length)
					{
						folderIndex = 0;
					}
					break;
				}
				
				default:
					break;
			}
			
			if (folderIndex != prevFolderIndex)
			{
				string folder = folders[folderIndex];
				
				imgBase       = new Bitmap(folder+"/base.png");
				imgStick      = new Bitmap(folder+"/stick.png");
				imgA          = new Bitmap(folder+"/buttA.png");
				imgB          = new Bitmap(folder+"/buttB.png");
				imgX          = new Bitmap(folder+"/buttX.png");
				imgY          = new Bitmap(folder+"/buttY.png");
				imgL          = new Bitmap(folder+"/buttL.png");
				imgR          = new Bitmap(folder+"/buttR.png");
				imgS          = new Bitmap(folder+"/buttS.png");
				imgStickSmall = new Bitmap(folder+"/stickSmall.png");
			}
		}
		
		public void saveIndex()
		{
			try
			{
				string text = ""+folderIndex;
				System.IO.File.WriteAllText("Index.ini", text);
			}
			catch {}
		}		
	}
}
