using System;
using PoePepe.UI.Helpers;
using Serilog;
using WindowsInput;
using WindowsInput.Native;

namespace PoePepe.UI.Services;

public class InputKeyboardService
{
    public static void SendInputToPoe(string input)
    {
        if (input is null)
        {
            Log.Error("Whisper message is empty");

            return;
        }

        // Get a handle to POE. The window class and window name were obtained using the Spy++ tool.
        var poeHandle = WindowsInternalFeatureService.FindWindow("POEWindowClass", "Path of Exile");

        // Verify that POE is a running process.
        if (poeHandle == IntPtr.Zero)
        {
            // Show message box if POE is not running
            Log.Error("Path of Exile is not running");
            return;
        }

        var iSim = new InputSimulator();

        // Need to press ALT because the SetForegroundWindow sometimes does not work
        iSim.Keyboard.KeyPress(VirtualKeyCode.MENU);

        // Make POE the foreground application and send input
        WindowsInternalFeatureService.SetForegroundWindow(poeHandle);

        iSim.Keyboard.KeyPress(VirtualKeyCode.RETURN);

        // Send the input
        iSim.Keyboard.TextEntry(input);

        // Send RETURN
        iSim.Keyboard.KeyPress(VirtualKeyCode.RETURN);
    }
}