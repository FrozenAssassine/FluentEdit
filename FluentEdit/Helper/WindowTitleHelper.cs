
using FluentEdit.Controls;
using FluentEdit.Core;

namespace FluentEdit.Helper;

internal class WindowTitleHelper
{
    public static void UpdateTitle(TextDocument document)
    {
        App.m_window.SetAppTitle((document.UnsavedChanges ? "*" : "") + document.FileName + " - FluentEdit");
    }
}
