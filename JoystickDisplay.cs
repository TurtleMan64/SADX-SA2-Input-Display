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
		
		private static int prevA;
		private static int prevB;
		private static int prevX;
		private static int prevY;
		private static int prevL;
		private static int prevR;
		private static int prevS;
		
		private static int countA;
		private static int countB;
		private static int countX;
		private static int countY;
		private static int countL;
		private static int countR;
		private static int countS;
		
		private static Rectangle recA;
		private static Rectangle recB;
		private static Rectangle recX;
		private static Rectangle recY;
		private static Rectangle recL;
		private static Rectangle recR;
		private static Rectangle recS;
		
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
		private static Font fontArial;
		
		private static StringFormat formatLeft;
		private static StringFormat formatCenter;
		private static StringFormat formatRight;
		
		private static int folderIndex = 0;
		
		private static bool drawButtonCount = false;
		
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
			
			prevA    = 1;
			prevB    = 1;
			prevX    = 1;
			prevY    = 1;
			prevL    = 1;
			prevR    = 1;
			prevS    = 1;
			
			countA = 0;
			countB = 0;
			countX = 0;
			countY = 0;
			countL = 0;
			countR = 0;
			countS = 0;
			
			
			
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
			fontArial = new Font("Arial", 16, FontStyle.Bold, GraphicsUnit.Point);
			
			recL = new Rectangle(40, 4  , 96, 36);
			recY = new Rectangle(40, 52 , 96, 36);
			recA = new Rectangle(40, 100, 96, 36);
			recR = new Rectangle(80, 4  , 96, 36);
			recX = new Rectangle(80, 52 , 96, 36);
			recB = new Rectangle(80, 100, 96, 36);
			recS = new Rectangle(60, 28 , 96, 36);
			
			formatLeft   = new StringFormat();
			formatCenter = new StringFormat();
			formatRight  = new StringFormat();
			
			formatLeft  .Alignment     = StringAlignment.Near;
			formatLeft  .LineAlignment = StringAlignment.Center;
			
			formatCenter.Alignment     = StringAlignment.Center;
			formatCenter.LineAlignment = StringAlignment.Center;
			
			formatRight .Alignment     = StringAlignment.Far;
			formatRight .LineAlignment = StringAlignment.Center;
			
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
			
			refreshButtonCounts();
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
			
			refreshButtonCounts();
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
			
			refreshButtonCounts();
		}
		
		public void setControllerDataMania(int buttons, int stick)
		{
			A = buttons & 16;
			B = buttons & 32;
			Y = buttons & 128;
			X = buttons & 64;
			S = stick   & 16;
			
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
			
			refreshButtonCounts();
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

			if (!drawButtonCount)
			{
				if (S != 0)
				{
					e.Graphics.DrawImage(imgS, 156, 4);
				}

				int drawX = 108+((64*joyX)/128);
				int drawY = 68 -((64*joyY)/128);

				e.Graphics.DrawImage(imgBase, 108-64, 68-64);
				e.Graphics.DrawLine(penBlack, 108, 68, drawX, drawY);
				e.Graphics.DrawImage(imgStickSmall, drawX-4, drawY-4);

				double radius = Math.Min(Math.Sqrt((joyX*joyX)+(joyY*joyY)), 128.0);
				double angle  = Math.Atan2(joyY, joyX);
				int capX = (int)(radius*Math.Cos(angle));
				int capY = (int)(radius*Math.Sin(angle));
				int drawCapX = 108+((64*capX)/128);
				int drawCapY = 68 -((64*capY)/128);

				e.Graphics.DrawImage(imgStick, drawCapX-4, drawCapY-4);
			}
			else
			{
				if (S != 0)
				{
					e.Graphics.DrawImage(imgS, 100, 12);
				}
				
				e.Graphics.DrawString(""+countL, fontArial, Brushes.White, recL, formatLeft);
				e.Graphics.DrawString(""+countY, fontArial, Brushes.White, recY, formatLeft);
				e.Graphics.DrawString(""+countA, fontArial, Brushes.White, recA, formatLeft);
				e.Graphics.DrawString(""+countR, fontArial, Brushes.White, recR, formatRight);
				e.Graphics.DrawString(""+countX, fontArial, Brushes.White, recX, formatRight);
				e.Graphics.DrawString(""+countB, fontArial, Brushes.White, recB, formatRight);
				e.Graphics.DrawString(""+countS, fontArial, Brushes.White, recS, formatCenter);
			}

			base.OnPaint(e);
		}
		
		public void refreshButtonCounts()
		{
			if (A != 0 && prevA == 0) { countA++; }
			if (B != 0 && prevB == 0) { countB++; }
			if (X != 0 && prevX == 0) { countX++; }
			if (Y != 0 && prevY == 0) { countY++; }
			if (L != 0 && prevL == 0) { countL++; }
			if (R != 0 && prevR == 0) { countR++; }
			if (S != 0 && prevS == 0) { countS++; }
			
			prevA = A;
			prevB = B;
			prevX = X;
			prevY = Y;
			prevL = L;
			prevR = R;
			prevS = S;
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
				
				case Keys.R:
				{
					countA = 0;
					countB = 0;
					countX = 0;
					countY = 0;
					countL = 0;
					countR = 0;
					countS = 0;
					break;
				}
				
				case Keys.B:
				{
					drawButtonCount = !drawButtonCount;
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
