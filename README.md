# SADX-SA2-Input-Display
An input display that directly reads what the game sees as input from your controller. Works with Sonic Adventure DX (2004 version, not Steam release), Sonic Adventure 2: Battle, and Sonic Heroes.  
    
![Imgur](http://i.imgur.com/cca2wiI.png)    
     
How to compile:     
     
Compile JoystickDisplay.cs as a DLL with the command:    
csc.exe /target:library /out:joydisp.DLL JoystickDisplay.cs     
    
Now compile the main SonicInputDisplay.cs while linking the library:    
csc.exe /out:SonicInputDisplay.exe /reference:joydisp.DLL SonicInputDisplay.cs    
    
Now you have the SonicInputDisplay.exe that you can run. Make sure that you run it in the same folder as the res folder that contains all of the images.     
