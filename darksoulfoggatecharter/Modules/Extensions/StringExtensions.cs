using System.IO;

public static class StringExtensions
{
    public static int GetLengthWithoutBBCode(this string str)
    {
        int visibleCharacters = 0;

        for (int i = 0; i < str.Length; i++)
        {
            if (str[i] == '[')
            {
                while (i < str.Length && str[i] != ']')
                {
                    i++;
                }
            }

            visibleCharacters++;
        }

        return visibleCharacters;
    }

    public static string RemoveExtension(this string str)
    {
        var ext = Path.GetExtension(str);
        if (string.IsNullOrEmpty(ext)) return str;
        return str.Replace(ext, string.Empty);
    }

    public static string RemoveExtensions(this string str)
    {
        string ext = Path.GetExtension(str);
        while (!string.IsNullOrEmpty(ext))
        {
            str = str.Replace(ext, string.Empty);
            ext = Path.GetExtension(str);
        }

        return str;
    }
}
