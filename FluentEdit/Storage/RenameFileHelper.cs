using FluentEdit.Core;
using FluentEdit.Dialogs;
using System;
using System.IO;

namespace FluentEdit.Storage;

internal class RenameFileHelper
{
    public static bool RenameFile(TextDocument textDocument, string newName)
    {
        //File has NOT been saved or opened
        if (!textDocument.SavedOnDisk)
        {
            textDocument.FileName = newName;
            return true;
        }

        //Nothing to rename
        if (textDocument.FileName == newName)
            return true;

        //Check if the file already exists
        if (Directory.Exists(Path.Combine(Path.GetDirectoryName(textDocument.FilePath), newName)))
        {
            InfoMessages.RenameFileAlreadyExists();
            return false;
        }


        string sourceFile = textDocument.FilePath;
        string destFile = Path.Combine(Path.GetDirectoryName(textDocument.FilePath), newName);

        if (File.Exists(sourceFile))
        {
            try
            {
                Directory.Move(sourceFile, destFile);
                textDocument.FileName = newName;
            }
            catch (Exception ex)
            {
                InfoMessages.RenameFileException(ex);
                return false;
            }
        }
        return true;
    }
}
