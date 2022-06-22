using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class ImageReadWrite
{
    static string _folder = Directory.GetCurrentDirectory();



    /// <summary>
    /// Saves an image to disk to the specified file name
    /// Format can be "pathA/.../file name" or "file name"
    /// No extension or leading slash
    /// If the target directory does not exist, it gets created
    /// </summary>
    /// <param name="image"></param>
    /// <param name="fileName"></param>
    public static void SaveImage(Texture2D image, string fileName)
    {
        string filePath = _folder + $"/{fileName}" + ".png";
        byte[] data = image.EncodeToPNG();
        string path = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        File.WriteAllBytes(filePath, data);
    }
}