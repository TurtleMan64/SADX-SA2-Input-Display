using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using JoystickDisplay;

public class ControllerReader
{
    [DllImport("SDL2.dll")]
    public static extern void SDL_SetHintWithPriority(string hint, string val, ulong priority);
    
    [DllImport("SDL2.dll")]
    public static extern int SDL_InitSubSystem(ulong systemId);
    
    [DllImport("SDL2.dll")]
    public static extern void SDL_GetVersion(ref int version);
    
    [DllImport("SDL2.dll")]
    public static extern ulong SDL_GameControllerOpen(int controllerId);
    
    [DllImport("SDL2.dll")]
    public static extern void SDL_GameControllerUpdate();
    
    [DllImport("SDL2.dll")]
    public static extern char SDL_GameControllerGetButton(ulong controllerPointer, ulong buttonId);
    
    [DllImport("SDL2.dll")]
    public static extern short SDL_GameControllerGetAxis(ulong controllerPointer, ulong axisId);
    
    [DllImport("SDL2.dll")]
    public static extern ulong SDL_GameControllerGetAttached(ulong controllerPointer);
    
    public const ulong SDL_INIT_GAMECONTROLLER = 0x00002000u;
    
    public const ulong SDL_HINT_OVERRIDE = 2;
    
    public const ulong SDL_CONTROLLER_BUTTON_A             =  0;
    public const ulong SDL_CONTROLLER_BUTTON_B             =  1;
    public const ulong SDL_CONTROLLER_BUTTON_X             =  2;
    public const ulong SDL_CONTROLLER_BUTTON_Y             =  3;
    public const ulong SDL_CONTROLLER_BUTTON_BACK          =  4;
    public const ulong SDL_CONTROLLER_BUTTON_GUIDE         =  5;
    public const ulong SDL_CONTROLLER_BUTTON_START         =  6;
    public const ulong SDL_CONTROLLER_BUTTON_LEFTSTICK     =  7;
    public const ulong SDL_CONTROLLER_BUTTON_RIGHTSTICK    =  8;
    public const ulong SDL_CONTROLLER_BUTTON_LEFTSHOULDER  =  9;
    public const ulong SDL_CONTROLLER_BUTTON_RIGHTSHOULDER = 10;
    public const ulong SDL_CONTROLLER_BUTTON_DPAD_UP       = 11;
    public const ulong SDL_CONTROLLER_BUTTON_DPAD_DOWN     = 12;
    public const ulong SDL_CONTROLLER_BUTTON_DPAD_LEFT     = 13;
    public const ulong SDL_CONTROLLER_BUTTON_DPAD_RIGHT    = 14;
    
    public const ulong SDL_CONTROLLER_AXIS_LEFTX        = 0;
    public const ulong SDL_CONTROLLER_AXIS_LEFTY        = 1;
    public const ulong SDL_CONTROLLER_AXIS_RIGHTX       = 2;
    public const ulong SDL_CONTROLLER_AXIS_RIGHTY       = 3;
    public const ulong SDL_CONTROLLER_AXIS_TRIGGERLEFT  = 4;
    public const ulong SDL_CONTROLLER_AXIS_TRIGGERRIGHT = 5;
    
    public static ulong controller = 0L;
    
    public static Display display = null;
    
    public static bool isConnected = false;
    
    public static void init(Display theDisplay)
    {
        SDL_SetHintWithPriority("SDL_GAMECONTROLLER_USE_BUTTON_LABELS", "0", SDL_HINT_OVERRIDE);
        SDL_InitSubSystem(SDL_INIT_GAMECONTROLLER);
        
        display = theDisplay;
    }
    
    public static void pollAndUpdate()
    {
        char a = '\0';
        char b = '\0';
        char y = '\0';
        char x = '\0';
        char s = '\0';
        char r = '\0';
        char l = '\0';
        
        float joyX = 0.0f;
        float joyY = 0.0f;
        
        SDL_GameControllerUpdate();
        
        if (SDL_GameControllerGetAttached(controller) == 0)
        {
            isConnected = false;
            
            controller = SDL_GameControllerOpen(0);
        }
        else
        {
            isConnected = true;
            
            a       = SDL_GameControllerGetButton(controller, SDL_CONTROLLER_BUTTON_A);
            b       = SDL_GameControllerGetButton(controller, SDL_CONTROLLER_BUTTON_B);
            y       = SDL_GameControllerGetButton(controller, SDL_CONTROLLER_BUTTON_Y);
            x       = SDL_GameControllerGetButton(controller, SDL_CONTROLLER_BUTTON_X);
            s       = SDL_GameControllerGetButton(controller, SDL_CONTROLLER_BUTTON_START);
            r       = SDL_GameControllerGetButton(controller, SDL_CONTROLLER_BUTTON_RIGHTSHOULDER);
            l       = SDL_GameControllerGetButton(controller, SDL_CONTROLLER_BUTTON_LEFTSHOULDER);
            char du = SDL_GameControllerGetButton(controller, SDL_CONTROLLER_BUTTON_DPAD_UP);
            char dd = SDL_GameControllerGetButton(controller, SDL_CONTROLLER_BUTTON_DPAD_DOWN);
            char dl = SDL_GameControllerGetButton(controller, SDL_CONTROLLER_BUTTON_DPAD_LEFT);
            char dr = SDL_GameControllerGetButton(controller, SDL_CONTROLLER_BUTTON_DPAD_RIGHT);
            
            int leftX    = (int)SDL_GameControllerGetAxis(controller, SDL_CONTROLLER_AXIS_LEFTX);
            int leftY    = (int)SDL_GameControllerGetAxis(controller, SDL_CONTROLLER_AXIS_LEFTY);
            int triggerL = (int)SDL_GameControllerGetAxis(controller, SDL_CONTROLLER_AXIS_TRIGGERLEFT);
            int triggerR = (int)SDL_GameControllerGetAxis(controller, SDL_CONTROLLER_AXIS_TRIGGERRIGHT);

            // Positive range is from 0 to 32767. Let's make it 32768.
            if (leftX    > 0) { leftX    += 1; }
            if (leftY    > 0) { leftY    += 1; }
            if (triggerL > 0) { triggerL += 1; }
            if (triggerR > 0) { triggerR += 1; }

            joyX        =    leftX/32768.0f;
            joyY        =   -leftY/32768.0f;
            float trigL = triggerL/32768.0f;
            float trigR = triggerR/32768.0f;
            
            if (trigL > 0.5f)
            {
                l = '1';
            }
            
            if (trigR > 0.5f)
            {
                r = '1';
            }
            
            if (dr != 0)
            {
                joyX = 1.0f;
            }
            else if (dl != 0)
            {
                joyX = -1.0f;
            }
            
            if (du != 0)
            {
                joyY = 1.0f;
            }
            else if (dd != 0)
            {
                joyY = -1.0f;
            }
        }
        
        display.setControllerData(a, b, y, x, s, r, l, joyX, joyY);
    }
}
