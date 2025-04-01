
using FluentEdit.Extensions;
using Microsoft.UI.Xaml.Controls;
using System;

namespace FluentEdit.Dialogs
{
    internal class InfoMessages
    {
        public static void RenameFileError() => new InfoBar().Show("File exists/no access", "A file with this name already exists\nor there is no access to the path",InfoBarSeverity.Error);
        public static void UnhandledException(string message) => new InfoBar().Show("Exception!", message, InfoBarSeverity.Error);
        public static void NoAccessToReadFile() => new InfoBar().Show("No access", "No access to read from the file", InfoBarSeverity.Error);
        public static void RenameFileAlreadyExists() => new InfoBar().Show("Rename File", "Could not rename the file because a file with the same name already exists", InfoBarSeverity.Error);
        public static void RenameFileException(Exception ex) => new InfoBar().Show("Rename File", "An exception occurred while renaming the file:\n" + ex.Message, InfoBarSeverity.Error);

    }
}
