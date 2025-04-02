﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextControlBoxNS;

namespace FluentEdit.Helper
{
    internal class FileExtensions
    {
        public static void SelectSyntaxHighlightingByFile(string filePath, TextControlBox textbox)
        {
            if (string.IsNullOrEmpty(filePath))
                return;

            string extension = Path.GetExtension(filePath).ToLower();
            if (string.IsNullOrEmpty(extension))
                return;

            //search through the dictionary of syntax highlights in the textbox
            foreach (var item in TextControlBox.SyntaxHighlightings)
            {
                for (int i = 0; i < item.Value?.Filter.Length; i++)
                {
                    if (item.Value.Filter[i].Equals(extension, StringComparison.OrdinalIgnoreCase))
                    {
                        textbox.SyntaxHighlighting = item.Value;
                        return;
                    }
                }
            }
        }

        public static ExtensionItem FindByExtension(string extension)
        {
            var res = FileExtentionList.Where(x => x.HasExtension(extension));
            if (res.Count() > 0)
                return res.ElementAt(0);
            return null;
        }

        public static List<ExtensionItem> FileExtentionList = new List<ExtensionItem>
        {
            new ExtensionItem()
            {
                Extension = { ".md", ".markdown", ".mdown", ".markdn" },
                ExtensionName = "Markdown",
                ExtensionLongName = "Markdown"
            },
            new ExtensionItem()
            {
                Extension = { ".json" },
                ExtensionName = "Json",
            },
            new ExtensionItem()
            {
                Extension = { ".gcode", ".ngc", ".tap" },
                ExtensionName = "G-Code",
                ExtensionLongName = "G-Code"
            },
            new ExtensionItem()
            {
                Extension = { ".vb" },
                ExtensionName = "Visual Basic",
            },
            new ExtensionItem()
            {
                Extension = { ".ino" },
                ExtensionName = "Arduino sketch",
            },
            new ExtensionItem()
            {
                Extension = { ".php" },
                ExtensionName = "Hypertext preprocessor",
            },
            new ExtensionItem()
            {
                Extension = { ".asm" },
                ExtensionName = "Assembly language",
            },
            new ExtensionItem()
            {
                Extension = { ".kt" },
                ExtensionName = "Kotlin",
            },
            new ExtensionItem()
            {
                Extension = { ".cs" },
                ExtensionName = "CSharp"
            },
            new ExtensionItem()
            {
                Extension = { ".cpp", ".cxx", ".cc", ".hpp" },
                ExtensionName = "C++"
            },
            new ExtensionItem()
            {
                Extension = { ".py", ".py3", ".pyt", ".rpy", ".pyw" },
                ExtensionName = "Python",
                ExtensionLongName = "Python"
            },
            new ExtensionItem()
            {
                Extension = { ".bat" },
                ExtensionName = "Batch",
                ExtensionLongName = "Windows batch"
            },
            new ExtensionItem()
            {
                Extension = { ".xaml" },
                ExtensionName = "Xaml",
                ExtensionLongName = "Extensible Application Markup Language"
            },
            new ExtensionItem()
            {
                Extension = { ".xml" },
                ExtensionName = "XML",
                ExtensionLongName = "Extensible Markup Language"
            },
            new ExtensionItem()
            {
                Extension = { ".html", ".htm", ".xhtml" },
                ExtensionName = "HTML",
                ExtensionLongName = "Hypertext Markup Language"
            },
            new ExtensionItem()
            {
                Extension = { ".txt", ".log" },
                ExtensionName = "Textfile",
                ExtensionLongName = "Textfile"
            },
            new ExtensionItem()
            {
                Extension = { ".reg" },
                ExtensionName = "Registration file",
                ExtensionLongName = "Windows Registration file"
            },
            new ExtensionItem()
            {
                Extension = { ".css" },
                ExtensionName = "Cascading Style Sheets",
                ExtensionLongName = "Cascading Style Sheets"
            },
            new ExtensionItem()
            {
                Extension = { ".java", ".jav", ".j" },
                ExtensionName = "Java",
                ExtensionLongName = "Java"
            },
            new ExtensionItem()
            {
                Extension = { ".ini", ".config", ".inf", ".cfg" },
                ExtensionName = "Configuration file",
                ExtensionLongName = "Configuration file"
            },
            new ExtensionItem()
            {
                Extension = { ".c", ".h" },
                ExtensionName = "C language",
            },
            new ExtensionItem()
            {
                Extension = { ".js", },
                ExtensionName = "JavaScript",
            }
        };
    }
    public class ExtensionItem
    {
        public bool HasExtension(string extension)
        {
            return Extension.Contains(extension);
        }

        public List<string> Extension = new List<string>();
        public string ExtensionName { get; set; }
        public string ExtensionLongName { get; set; }
    }
}