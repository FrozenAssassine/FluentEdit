using System.Text;

namespace FluentEdit.Core;

public class TextDocument
{
    public Encoding CurrentEncoding { get; set; } =  Encoding.UTF8;
    public string FileName { get; set; } = "";
    public string FilePath { get; set; } = "";
    public bool SavedOnDisk => FilePath.Length > 0;

    public bool UnsavedChanges { get; set; } = false;
}
