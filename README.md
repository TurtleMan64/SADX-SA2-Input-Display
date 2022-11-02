# Sonic Input Display
An input display that directly reads what the game sees as input from your controller. 

![Imgur](http://i.imgur.com/cca2wiI.png)    

Compatable games list:
 * Sonic Adventure 2: Battle (Steam)
 * Sonic Adventure DX (2004 disc version, not Steam release)
 * Sonic Heroes

If there are no games to read inputs from, then your controller will be read from directly.

### Download the exe:     
https://github.com/TurtleMan64/SADX-SA2-Input-Display/releases/latest     

### Additional Features
 * You can switch between skins by using left/right arrow keys on your keyboard. New skins can be added by simply creating a new folder with the same filenames in them.
 * The background color can be defined in `BackgroundColor.ini`, (RGB values from 0-255)
 * The stick color can be defined in `StickColor.ini`, (RGB values from 0-255)
 * As of version 2.2, capturing in OBS supports transparency. This means you will not have to color/chroma key out the background. Just use Game Capture, and select the "Allow Transparency" checkbox.
 * The stick line width can be defined in `StickLineWidth.ini`, (0 for thin 1 for thick)
 
#### How to compile:     

* 2.0 (C++)
   * Open the `SonicInputDisplay.sln` in Visual Studio 2017. Then hit build and it will produce the final `SonicInputDisplay.exe`.
   * This version will read from your controller when there are no games to read from.
* 1.9 (C#)
   * Use the provided `build.bat` script to build the project. This will produce the final `SonicInputDisplay.exe`.
   * This version will not show any inputs if there is no game to read from.
