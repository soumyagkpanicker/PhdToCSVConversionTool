using Phdtocsvconversion;
using System.Collections.ObjectModel;


inputpath:
Console.ForegroundColor = ConsoleColor.White;
Console.WriteLine("Enter Input Folder Path: ");
String inputpath = Console.ReadLine();
if (!FileConverter.ValidateFolderPath(inputpath))
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("Please enter a valid folder path");
    goto inputpath;
}
else
{
    outputpath:
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine("Enter Output Folder Path: ");
    String outputpath = Console.ReadLine();
    if (!FileConverter.ValidateFolderPath(outputpath))
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Please enter a valid output folder path");
        goto outputpath;
    }
    TimeZone:
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine("Enter Date Time Format: ");
    String datetimeformat = Console.ReadLine();
    if (!FileConverter.ValidateTimeZone(datetimeformat))
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Please enter a valid timezone");
        goto TimeZone;
    }
    FileSize:
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine("Enter Output File Size (MB): ");
    String FileSize = Console.ReadLine();
    if (!FileConverter.ValidateFileSize(FileSize))
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Please enter a valid filesize");
        goto FileSize;
    }
    FileConverter objFileConverter = new FileConverter(inputpath, outputpath, datetimeformat, ".txt",FileSize);
    if (!objFileConverter.ValidateInputFile())
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Please make sure that all the files inside your input folder are text files");
        Console.Clear();
        goto inputpath;
    }
    else {
        objFileConverter.ReadInputFiles1();
        
    }
    Console.WriteLine("Press CTRL-C to exit.");
}

