using System.Text;
using Windows.Storage;

namespace FluentEdit.Core;

public class TextDocument
{
    public Encoding CurrentEncoding { get; set; } =  Encoding.UTF8;
    public string FileName { get; set; } = "";
    public string FilePath { get; set; } = "";
    public bool SavedOnDisk => FilePath.Length > 0;

    public bool UnsavedChanges { get; set; } = false;

    public void SaveAs(StorageFile file)
    {
        this.FileName = file.Name;
        this.FilePath = file.Path;
        this.UnsavedChanges = false;
    }
    public void Save()
    {
        this.UnsavedChanges = false;
    }

    public void Open(Encoding encoding)
    {
        this.CurrentEncoding = encoding;
        this.UnsavedChanges = false;
    }
    public void New(string untitledFileName)
    {
        this.FileName = untitledFileName;
        this.FilePath = "";
        this.UnsavedChanges = false;
    }
}
