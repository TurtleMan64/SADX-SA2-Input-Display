#include <SDL/SDL_image.h>
#include <SDL/SDL.h>
#include <SDL/SDL_syswm.h>
#include <SDL/SDL_hints.h>

#include <Windows.h>
#include <tlhelp32.h>
#include <Winuser.h>

#include <stdio.h>
#include <vector>
#include <string>
#include <fstream>

#include "Getline.hpp"
#include "Split.hpp"

SDL_Window* sdlWindow = nullptr;
SDL_Renderer* sdlRenderer = nullptr;

SDL_Texture* imgBase       = nullptr;
SDL_Texture* imgStick      = nullptr;
SDL_Texture* imgStickSmall = nullptr;
SDL_Texture* imgA          = nullptr;
SDL_Texture* imgB          = nullptr;
SDL_Texture* imgX          = nullptr;
SDL_Texture* imgY          = nullptr;
SDL_Texture* imgL          = nullptr;
SDL_Texture* imgR          = nullptr;
SDL_Texture* imgS          = nullptr;

SDL_Rect recBase = { 44,   4, 128, 128};
SDL_Rect recA    = {  4, 100,  32,  32};
SDL_Rect recB    = {180, 100,  32,  32};
SDL_Rect recX    = {180,  52,  32,  32};
SDL_Rect recY    = {  4,  52,  32,  32};
SDL_Rect recL    = {  4,   4,  32,  32};
SDL_Rect recR    = {180,   4,  32,  32};
SDL_Rect recS    = {156,   4,  16,  16};

int JoyX = 0;
int JoyY = 0;
int A    = 0;
int B    = 0;
int X    = 0;
int Y    = 0;
int L    = 0;
int R    = 0;
int S    = 0;

int folderIndex = 0;

HANDLE processHandle = NULL;

int gameId = -1;

bool controllerIsConnected = false;

void pollAndUpdateGameController();

void attachToGame();

void setValuesFromSA2();
void setValuesFromSADX();
void setValuesFromHeroes();
void setValuesFromMania();
void setValuesFromGenerations();

DWORD getProcessIdByName(const char* processName);

std::vector<std::string> getSkinFolders(const std::string &directory);

std::vector<std::string> skinFolders;

void reloadImages();

void saveIndex();

//https://caedesnotes.wordpress.com/2015/04/13/how-to-integrate-your-sdl2-window-icon-or-any-image-into-your-executable/
void setIcon()
{
    #include "icon.c"

    Uint32 rmask, gmask, bmask, amask;
    rmask = 0x000000ff;
    gmask = 0x0000ff00;
    bmask = 0x00ff0000;
    amask = (imgIcon.bytes_per_pixel == 3) ? 0 : 0xff000000;

    SDL_Surface* icon = SDL_CreateRGBSurfaceFrom(
        (void*)imgIcon.pixel_data,
        imgIcon.width,
        imgIcon.height,
        imgIcon.bytes_per_pixel*8,
        imgIcon.bytes_per_pixel*imgIcon.width,
        rmask,
        gmask,
        bmask,
        amask);

    SDL_SetWindowIcon(sdlWindow, icon);

    SDL_FreeSurface(icon);
}

int main(int argc, char* argv[])
{
    argc;
    argv;

    HWND handle = GetConsoleWindow();
    ShowWindow(handle, SW_HIDE);

    SDL_SetHintWithPriority(SDL_HINT_GAMECONTROLLER_USE_BUTTON_LABELS, "0", SDL_HINT_OVERRIDE);
    SDL_SetHintWithPriority(SDL_HINT_JOYSTICK_ALLOW_BACKGROUND_EVENTS, "1", SDL_HINT_OVERRIDE);

    SDL_Init(SDL_INIT_VIDEO);
    SDL_Init(SDL_INIT_GAMECONTROLLER);
    IMG_Init(IMG_INIT_PNG);

    sdlWindow = SDL_CreateWindow("Searching for game...", SDL_WINDOWPOS_CENTERED, SDL_WINDOWPOS_CENTERED, 216, 136, SDL_WINDOW_SHOWN);

    // Find the renderer that uses d3d11.
    for (int rendererIndex = 0; rendererIndex < 100; rendererIndex++)
    {
        sdlRenderer = SDL_CreateRenderer(sdlWindow, rendererIndex, SDL_RENDERER_ACCELERATED | SDL_RENDERER_PRESENTVSYNC);

        if (sdlRenderer == nullptr) // We don't have d3d11, just use whatever is available and transparency won't work.
        {
            sdlRenderer = SDL_CreateRenderer(sdlWindow, -1, SDL_RENDERER_ACCELERATED | SDL_RENDERER_PRESENTVSYNC);
            break;
        }

        SDL_RendererInfo info;
        SDL_GetRendererInfo(sdlRenderer, &info);

        std::string name = info.name;
        if (name == "direct3d11") // We NEED d3d11 for transparency to work in OBS capture.
        {
            break;
        }
        else
        {
            SDL_DestroyRenderer(sdlRenderer);
            sdlRenderer = nullptr;
        }
    }

    // This is the texture that we render on.
    SDL_Texture* textureTransparent = SDL_CreateTexture(sdlRenderer, SDL_PIXELFORMAT_RGBA8888, SDL_TEXTUREACCESS_TARGET, 216, 136);

    SDL_SetTextureBlendMode(textureTransparent, SDL_BLENDMODE_BLEND);
    SDL_SetRenderDrawBlendMode(sdlRenderer, SDL_BLENDMODE_BLEND);

    setIcon();

    // Disable the minimize and maximize buttons.
    SDL_SysWMinfo wmInfo;
    SDL_VERSION(&wmInfo.version);
    SDL_GetWindowWMInfo(sdlWindow, &wmInfo);
    HWND hwnd = wmInfo.info.win.window;
    SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_MINIMIZEBOX);
    SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_MAXIMIZEBOX);

    char bgR = 0;
    char bgG = 0;
    char bgB = 0;
    std::vector<std::string> bgColorLines = readFileLines("BackgroundColor.ini");
    if (bgColorLines.size() >= 1)
    {
        std::vector<std::string> bgColors = split(bgColorLines[0], ',');
        if (bgColors.size() >= 3)
        {
            bgR = (char)std::stoi(bgColors[0]);
            bgG = (char)std::stoi(bgColors[1]);
            bgB = (char)std::stoi(bgColors[2]);
        }
    }

    char stickR = 0;
    char stickG = 0;
    char stickB = 0;
    std::vector<std::string> stickColorLines = readFileLines("StickColor.ini");
    if (stickColorLines.size() >= 1)
    {
        std::vector<std::string> stickColors = split(stickColorLines[0], ',');
        if (stickColors.size() >= 3)
        {
            stickR = (char)std::stoi(stickColors[0]);
            stickG = (char)std::stoi(stickColors[1]);
            stickB = (char)std::stoi(stickColors[2]);
        }
    }

    bool thickLine = true;
    std::vector<std::string> lineWidthLines = readFileLines("StickLineWidth.ini");
    if (lineWidthLines.size() >= 2)
    {
        thickLine = (bool)std::stoi(lineWidthLines[1]);
    }

    folderIndex = 0;
    std::vector<std::string> indexLines = readFileLines("Index.ini");
    if (indexLines.size() >= 1)
    {
        folderIndex = std::stoi(indexLines[0]);
    }

    skinFolders = getSkinFolders("./");

    reloadImages();

    bool running = true;
    while (running)
    {
        SDL_Event e;
        while (SDL_PollEvent(&e))
        {
            switch (e.type)
            {
                case SDL_QUIT:
                {
                    running = false;
                    break;
                }

                case SDL_KEYDOWN:
                {
                    SDL_KeyboardEvent* keyEv = (SDL_KeyboardEvent*)&e;
                    if (keyEv->keysym.sym == SDLK_LEFT)
                    {
                        folderIndex--;
                    }
                    else if (keyEv->keysym.sym == SDLK_RIGHT)
                    {
                        folderIndex++;
                    }

                    if (folderIndex >= (int)skinFolders.size())
                    {
                        folderIndex = 0;
                    }
                    else if (folderIndex < 0)
                    {
                        folderIndex = ((int)skinFolders.size()) - 1;
                    }

                    reloadImages();
                    break;
                }

                default:
                    break;
            }
        }

        A = 0;
        B = 0;
        X = 0;
        Y = 0;
        L = 0;
        R = 0;
        S = 0;
        JoyX = 0;
        JoyY = 0;

        switch (gameId)
        {
            case 0: setValuesFromSA2();         break;
            case 1: setValuesFromSADX();        break;
            case 2: setValuesFromHeroes();      break;
            case 3: setValuesFromMania();       break;
            case 4: setValuesFromGenerations(); break;
            default: attachToGame();            break;
        }

        if (gameId == -1 && !controllerIsConnected)
        {
            A = 1;
            B = 1;
            X = 1;
            Y = 1;
            L = 1;
            R = 1;
            S = 1;
        }

        SDL_SetRenderTarget(sdlRenderer, textureTransparent);
        SDL_SetRenderDrawColor(sdlRenderer, bgR, bgG, bgB, 0);
        SDL_RenderClear(sdlRenderer);

        SDL_RenderCopy(sdlRenderer, imgBase, nullptr, &recBase);

        int drawX = 108 + ((64*JoyX)/128);
        int drawY = 68  - ((64*JoyY)/128);
        SDL_Rect recStickSmall = {drawX - 4, drawY - 4, 8, 8};

        float radius = fminf(sqrtf((float)((JoyX*JoyX) + (JoyY*JoyY))), 128.0f);
        float angle  = atan2f((float)JoyY, (float)JoyX);
        int capX = (int)(radius*cosf(angle));
        int capY = (int)(radius*sinf(angle));
        int drawCapX = 108 + ((64*capX)/128);
        int drawCapY = 68  - ((64*capY)/128);

        SDL_Rect recStick = {drawCapX - 4, drawCapY - 4, 8, 8};

        SDL_SetRenderDrawColor(sdlRenderer, stickR, stickG, stickB, 255);
        SDL_RenderDrawLine(sdlRenderer, 108 - 0, 68 - 0, drawX - 0, drawY - 0);
        if (thickLine)
        {
            SDL_RenderDrawLine(sdlRenderer, 108 - 1, 68 - 0, drawX - 1, drawY - 0);
            SDL_RenderDrawLine(sdlRenderer, 108 - 0, 68 - 1, drawX - 0, drawY - 1);
            SDL_RenderDrawLine(sdlRenderer, 108 - 1, 68 - 1, drawX - 1, drawY - 1);
        }
        SDL_SetRenderDrawColor(sdlRenderer, bgR, bgG, bgB, 0);

        SDL_RenderCopy(sdlRenderer, imgStickSmall, nullptr, &recStickSmall);
        SDL_RenderCopy(sdlRenderer, imgStick     , nullptr, &recStick     );

        if (A != 0) { SDL_RenderCopy(sdlRenderer, imgA, nullptr, &recA); }
        if (B != 0) { SDL_RenderCopy(sdlRenderer, imgB, nullptr, &recB); }
        if (X != 0) { SDL_RenderCopy(sdlRenderer, imgX, nullptr, &recX); }
        if (Y != 0) { SDL_RenderCopy(sdlRenderer, imgY, nullptr, &recY); }
        if (L != 0) { SDL_RenderCopy(sdlRenderer, imgL, nullptr, &recL); }
        if (R != 0) { SDL_RenderCopy(sdlRenderer, imgR, nullptr, &recR); }
        if (S != 0) { SDL_RenderCopy(sdlRenderer, imgS, nullptr, &recS); }

        SDL_SetRenderTarget(sdlRenderer, nullptr);
        SDL_RenderClear(sdlRenderer);
        SDL_RenderCopy(sdlRenderer, textureTransparent, nullptr, nullptr);
        SDL_RenderPresent(sdlRenderer);

        SDL_Delay(5);
    }

    saveIndex();

    SDL_DestroyRenderer(sdlRenderer);
    SDL_DestroyWindow(sdlWindow);
    SDL_Quit();

    return 0;
}

SDL_GameController* controller = nullptr;

void pollAndUpdateGameController()
{
    char a = 0;
    char b = 0;
    char y = 0;
    char x = 0;
    char s = 0;
    char r = 0;
    char l = 0;

    float joyX = 0.0f;
    float joyY = 0.0f;

    SDL_GameControllerUpdate();

    if (SDL_GameControllerGetAttached(controller) == SDL_FALSE)
    {
        controllerIsConnected = false;

        int numJoysticks = SDL_NumJoysticks();

        for (int i = 0; i < numJoysticks; i++)
        {
            if (SDL_IsGameController(i) != SDL_FALSE)
            {
                controller = SDL_GameControllerOpen(i);
                if (SDL_GameControllerGetAttached(controller) != SDL_FALSE)
                {
                    break;
                }
            }
        }
    }
    else
    {
        controllerIsConnected = true;

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
            l = 1;
        }

        if (trigR > 0.5f)
        {
            r = 1;
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

    A = a;
    B = b;
    Y = y;
    X = x;
    S = s;
    R = r;
    L = l;

    JoyX = (int)(joyX*127);
    JoyY = (int)(joyY*127);
}

int nextProcessCheck = 400;

void attachToGame()
{
    pollAndUpdateGameController();

    if (controllerIsConnected)
    {
        SDL_SetWindowTitle(sdlWindow, "Controller Input");
    }
    else
    {
        SDL_SetWindowTitle(sdlWindow, "Searching for game...");
    }

    nextProcessCheck--;

    if (nextProcessCheck > 0)
    {
        return;
    }
    nextProcessCheck = 400;

    if (processHandle != NULL)
    {
        CloseHandle(processHandle);
        processHandle = NULL;
    }

    gameId = -1;

    DWORD pid = 0;

    pid = getProcessIdByName("sonic2app.exe");
    if (pid != 0)
    {
        gameId = 0;
    }
    else
    {
        pid = getProcessIdByName("sonic.exe");
        if (pid != 0)
        {
            gameId = 1;
        }
        else
        {
            pid = getProcessIdByName("Sonic Adventure DX.exe");
            if (pid != 0)
            {
                gameId = 1;
            }
            else
            {
                pid = getProcessIdByName("Tsonic_win.exe");
                if (pid != 0)
                {
                    gameId = 2;
                }
                else
                {
                    // Mania and Generations don't seem to work anymore, disabling for now.
                    //pid = getProcessIdByName("SonicMania.exe");
                    //if (pid != 0)
                    //{
                    //    gameId = 3;
                    //}
                    //else
                    //{
                    //    pid = getProcessIdByName("SonicGenerations.exe");
                    //    if (pid != 0)
                    //    {
                    //        gameId = 4;
                    //    }
                    //}
                }
            }
        }
    }

    if (gameId != -1)
    {
        processHandle = OpenProcess(PROCESS_VM_READ, false, pid);

        if (processHandle == NULL)
        {
            gameId = -1;
        }
    }

    switch (gameId)
    {
        case 0: SDL_SetWindowTitle(sdlWindow, "SA2 Input"        ); break;
        case 1: SDL_SetWindowTitle(sdlWindow, "SADX Input"       ); break;
        case 2: SDL_SetWindowTitle(sdlWindow, "Heroes Input"     ); break;
        case 3: SDL_SetWindowTitle(sdlWindow, "Mania Input"      ); break;
        case 4: SDL_SetWindowTitle(sdlWindow, "Generations Input"); break;
        default: break;
    }
}

void setValuesFromSA2()
{
    SIZE_T bytesRead = 0;
    char buffer[12];
    if (ReadProcessMemory(processHandle, (LPCVOID)0x01A52C4C, buffer, 12, &bytesRead) == false || bytesRead != 12)
    {
        gameId = -1;
        CloseHandle(processHandle);
        processHandle = NULL;
        return;
    }

    int buttons = 0;
    memcpy(&buttons, &buffer[0], 4);

    memcpy(&JoyX, &buffer[4], 4);

    memcpy(&JoyY, &buffer[8], 4);

    A = buttons & 256;
    B = buttons & 512;
    Y = buttons & 2048;
    X = buttons & 1024;
    S = buttons & 4096;
    R = buttons & 32;
    L = buttons & 64;

    //D-Pad
    int up    = buttons & 8;
    int down  = buttons & 4;
    int left  = buttons & 1;
    int right = buttons & 2;

    if (right != 0)
    {
        JoyX = 127;
    }
    else if (left != 0)
    {
        JoyX = -128;
    }

    if (up != 0)
    {
        JoyY = 127;
    }
    else if (down != 0)
    {
        JoyY = -128;
    }
}

void setValuesFromSADX()
{
    SIZE_T bytesRead = 0;
    char buffer[20];
    if (ReadProcessMemory(processHandle, (LPCVOID)0x03B0E80C, buffer, 20, &bytesRead) == false || bytesRead != 20)
    {
        CloseHandle(processHandle);
        processHandle = NULL;
        gameId = -1;
        return;
    }

    int buttons = 0;
    memcpy(&buttons, &buffer[16], 4);

    short joyX = 0;
    memcpy(&joyX, &buffer[0], 2);
    JoyX = joyX;

    short joyY = 0;
    memcpy(&joyY, &buffer[2], 2);
    JoyY = -joyY;

    short camX = 0;
    memcpy(&camX, &buffer[4], 2);

    A = buttons & 4;
    B = buttons & 2;
    Y = buttons & 512;
    X = buttons & 1024;
    S = buttons & 8;
    R = buttons & 65536;
    L = buttons & 131072;

    if (camX < 0)
    {
        L = 1;
    }
    else if (camX > 0)
    {
        R = 1;
    }

    //D-Pad
    int up    = buttons & 16;
    int down  = buttons & 32;
    int left  = buttons & 64;
    int right = buttons & 128;

    if (right != 0)
    {
        JoyX = 127;
    }
    else if (left != 0)
    {
        JoyX = -128;
    }

    if (up != 0)
    {
        JoyY = 127;
    }
    else if (down != 0)
    {
        JoyY = -128;
    }
}

void setValuesFromHeroes()
{
    SIZE_T bytesRead = 0;
    char buffer[28];
    if (ReadProcessMemory(processHandle, (LPCVOID)0x00A23598, buffer, 28, &bytesRead) == false || bytesRead != 28)
    {
        CloseHandle(processHandle);
        processHandle = NULL;
        gameId = -1;
        return;
    }

    int buttons = 0;
    memcpy(&buttons, &buffer[0], 4);

    float joyX;
    float joyY;
    float cameraPan;
    memcpy(&joyX,      &buffer[16], 4);
    memcpy(&joyY,      &buffer[20], 4);
    memcpy(&cameraPan, &buffer[24], 4);

    A = buttons & 1;
    B = buttons & 2;
    Y = buttons & 8;
    X = buttons & 4;
    S = buttons & 16384;
    R = buttons & 512;
    L = buttons & 256;

    if (cameraPan < -0.3f)
    {
        R = 1;
    }

    if (cameraPan > 0.3f)
    {
        L = 1;
    }

    JoyX = (int)( joyX*127);
    JoyY = (int)(-joyY*127);

    //D-Pad
    int up    = buttons & 16;
    int down  = buttons & 32;
    int left  = buttons & 64;
    int right = buttons & 128;

    if (right != 0)
    {
        JoyX = 127;
    }
    else if (left != 0)
    {
        JoyX = -128;
    }

    if (up != 0)
    {
        JoyY = 127;
    }
    else if (down != 0)
    {
        JoyY = -128;
    }
}

void setValuesFromMania()
{
    SIZE_T bytesRead = 0;
    char buffer[2];

    if (ReadProcessMemory(processHandle, (LPCVOID)0x013CE9B0, buffer, 2, &bytesRead) == false || bytesRead != 2)
    {
        CloseHandle(processHandle);
        processHandle = NULL;
        gameId = -1;
        return;
    }

    int inputsController = 0;
    inputsController+=buffer[0];
    inputsController+=buffer[1]<<8;

    if (ReadProcessMemory(processHandle, (LPCVOID)0x013CD58C, buffer, 2, &bytesRead) == false || bytesRead != 2)
    {
        CloseHandle(processHandle);
        processHandle = NULL;
        gameId = -1;
        return;
    }

    int inputsKeyboard = 0;
    inputsKeyboard+=buffer[0];
    inputsKeyboard+=buffer[1]<<8;

    int inputs = inputsKeyboard | inputsController;

    A = inputs & 4096;
    B = inputs & 8192;
    Y = inputs & 32768;
    X = inputs & 16384;
    S = inputs & 48;

    R = 0;
    L = 0;

    JoyX = 0;
    JoyY = 0;

    int up    = inputs & 1;
    int down  = inputs & 2;
    int left  = inputs & 4;
    int right = inputs & 8;

    if (right != 0)
    {
        JoyX = 127;
    }
    else if (left != 0)
    {
        JoyX = -128;
    }

    if (up != 0)
    {
        JoyY = 127;
    }
    else if (down != 0)
    {
        JoyY = -128;
    }
}

void setValuesFromGenerations()
{
    SIZE_T bytesRead = 0;
    char buffer[4];

    if (ReadProcessMemory(processHandle, (LPCVOID)0x01E77B68, buffer, 4, &bytesRead) == false || bytesRead != 4)
    {
        CloseHandle(processHandle);
        processHandle = NULL;
        gameId = -1;
        return;
    }

    float joyX;
    memcpy(&joyX, &buffer[0], 4);

    if (ReadProcessMemory(processHandle, (LPCVOID)0x01E77B6C, buffer, 4, &bytesRead) == false || bytesRead != 4)
    {
        CloseHandle(processHandle);
        processHandle = NULL;
        gameId = -1;
        return;
    }

    float joyY;
    memcpy(&joyY, &buffer[0], 4);

    if (ReadProcessMemory(processHandle, (LPCVOID)0x01E76164, buffer, 2, &bytesRead) == false || bytesRead != 2)
    {
        CloseHandle(processHandle);
        processHandle = NULL;
        gameId = -1;
        return;
    }

    int buttons = 0;
    buttons+=buffer[0];
    buttons+=buffer[1]<<8;

    A = buttons & 1;
    B = buttons & 2;
    Y = buttons & 16;
    X = buttons & 8;
    S = buttons & 1024;
    R = buttons & (8192 | 32768);
    L = buttons & (4096 | 16384);

    JoyX = (int)(joyX*127);
    JoyY = (int)(joyY*127);
}

DWORD getProcessIdByName(const char* processName)
{
    PROCESSENTRY32 entry;
    entry.dwSize = sizeof(PROCESSENTRY32);

    HANDLE snapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, NULL);

    if (Process32First(snapshot, &entry) == TRUE)
    {
        while (Process32Next(snapshot, &entry) == TRUE)
        {
            if (_stricmp(entry.szExeFile, processName) == 0)
            {
                DWORD pid = entry.th32ProcessID;

                CloseHandle(snapshot);

                return pid;
            }
        }
    }

    CloseHandle(snapshot);
    return 0;
}

//https://stackoverflow.com/questions/41404711/how-to-list-files-in-a-directory-using-the-windows-api
std::vector<std::string> getSkinFolders(const std::string &directory)
{
    WIN32_FIND_DATAA findData;
    HANDLE hFind = INVALID_HANDLE_VALUE;
    std::string full_path = directory + "\\*";
    std::vector<std::string> skins;

    hFind = FindFirstFileA(full_path.c_str(), &findData);

    if (hFind == INVALID_HANDLE_VALUE)
    {
        printf("Error: %s is not a valid directory\n", directory.c_str());
        return skins;
    }

    while (FindNextFileA(hFind, &findData) != 0)
    {
        if (findData.dwFileAttributes & 16)
        {
            std::string fToCheck = findData.cFileName;
            fToCheck = fToCheck + "/base.png";
            std::ifstream f(fToCheck.c_str());
            if (f.good())
            {
                skins.push_back(std::string(findData.cFileName));
            }
        }
    }

    FindClose(hFind);

    return skins;
}

void reloadImages()
{
    if (folderIndex < 0 || folderIndex >= (int)skinFolders.size())
    {
        folderIndex = 0;
    }

    std::string folder = skinFolders[folderIndex];

    if (imgBase       != nullptr) { SDL_DestroyTexture(imgBase      ); imgBase       = nullptr; }
    if (imgStick      != nullptr) { SDL_DestroyTexture(imgStick     ); imgStick      = nullptr; }
    if (imgA          != nullptr) { SDL_DestroyTexture(imgA         ); imgA          = nullptr; }
    if (imgB          != nullptr) { SDL_DestroyTexture(imgB         ); imgB          = nullptr; }
    if (imgX          != nullptr) { SDL_DestroyTexture(imgX         ); imgX          = nullptr; }
    if (imgY          != nullptr) { SDL_DestroyTexture(imgY         ); imgY          = nullptr; }
    if (imgL          != nullptr) { SDL_DestroyTexture(imgL         ); imgL          = nullptr; }
    if (imgR          != nullptr) { SDL_DestroyTexture(imgR         ); imgR          = nullptr; }
    if (imgS          != nullptr) { SDL_DestroyTexture(imgS         ); imgS          = nullptr; }
    if (imgStickSmall != nullptr) { SDL_DestroyTexture(imgStickSmall); imgStickSmall = nullptr; }

    imgBase       = IMG_LoadTexture(sdlRenderer, (folder + "/base.png"      ).c_str());
    imgStick      = IMG_LoadTexture(sdlRenderer, (folder + "/stick.png"     ).c_str());
    imgA          = IMG_LoadTexture(sdlRenderer, (folder + "/buttA.png"     ).c_str());
    imgB          = IMG_LoadTexture(sdlRenderer, (folder + "/buttB.png"     ).c_str());
    imgX          = IMG_LoadTexture(sdlRenderer, (folder + "/buttX.png"     ).c_str());
    imgY          = IMG_LoadTexture(sdlRenderer, (folder + "/buttY.png"     ).c_str());
    imgL          = IMG_LoadTexture(sdlRenderer, (folder + "/buttL.png"     ).c_str());
    imgR          = IMG_LoadTexture(sdlRenderer, (folder + "/buttR.png"     ).c_str());
    imgS          = IMG_LoadTexture(sdlRenderer, (folder + "/buttS.png"     ).c_str());
    imgStickSmall = IMG_LoadTexture(sdlRenderer, (folder + "/stickSmall.png").c_str());
}

void saveIndex()
{
    std::ofstream fileInd("Index.ini");
    fileInd << std::to_string(folderIndex);
    fileInd.close();
}
