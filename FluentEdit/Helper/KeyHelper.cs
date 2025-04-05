using Windows.System;
using Windows.UI.Core;

namespace TextControlBox_TestApp.Helper;

internal class KeyHelper
{
    public static bool IsKeyPressed(VirtualKey key)
    {
        return Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(key).HasFlag(CoreVirtualKeyStates.Down);
    }
}
