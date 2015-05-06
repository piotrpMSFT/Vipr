﻿namespace Vipr.Core
{
    public class TextFile : RelativeFile
    {
        public TextFile(string relativePath, string contents)
        {
            Contents = contents;
            RelativePath = relativePath;
        }

        public TextFile()
        {
        }

        public string Contents { get; protected set; }
    }
}
