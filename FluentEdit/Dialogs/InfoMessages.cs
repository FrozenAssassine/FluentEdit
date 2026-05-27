
using FluentEdit.Extensions;
using Microsoft.UI.Xaml.Controls;
using System;

namespace FluentEdit.Dialogs
{
    internal class InfoMessages
    {
        public static void RenameFileError() => new InfoBar().Show(
            "File exists/no access".Localized("InfoBar_RenameError_Title/Text"),
            "A file with this name already exists\nor there is no access to the path".Localized("InfoBar_RenameError_Message/Text"),
            InfoBarSeverity.Error);
        public static void UnhandledException(string message) => new InfoBar().Show("Exception!".Localized("InfoBar_UnhandledException_Title/Text"), message, InfoBarSeverity.Error);
        public static void NoAccessToReadFile() => new InfoBar().Show(
            "No access".Localized("InfoBar_NoAccess_Title/Text"),
            "No access to read from the file".Localized("InfoBar_NoAccessRead_Message/Text"),
            InfoBarSeverity.Error);
        public static void RenameFileAlreadyExists() => new InfoBar().Show(
            "Rename File".Localized("Dialog_RenameFile_Headline/Text"),
            "Could not rename the file because a file with the same name already exists".Localized("InfoBar_RenameExists_Message/Text"),
            InfoBarSeverity.Error);
        public static void RenameFileException(Exception ex)
        {
            var messageTemplate = "An exception occurred while renaming the file:\n{0}".Localized("InfoBar_RenameException_Message/Text");
            new InfoBar().Show("Rename File".Localized("Dialog_RenameFile_Headline/Text"), string.Format(messageTemplate, ex.Message), InfoBarSeverity.Error);
        }
        public static void NoAccessToSaveFile() => new InfoBar().Show(
            "No access".Localized("InfoBar_NoAccess_Title/Text"),
            "No access to write to the file".Localized("InfoBar_NoAccessWrite_Message/Text"),
            InfoBarSeverity.Error);
    }
}
