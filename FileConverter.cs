using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Phdtocsvconversion
{
    public class FileConverter
    {
        #region public properties
        public string InputFolderPath { get; set; }
        public string OutputFolderPath { get; set; }
        public string DateTimeFormat { get; set; }
        public string FileExtension { get; set; }
        public string FileSize { get; set; }
        #endregion

        #region Constructor
        public FileConverter(
            string inputpath,
            string outputpath,
            string datetimeformat,
            string extension,
            string ffilesize)
        {
            this.InputFolderPath = inputpath;
            this.OutputFolderPath = outputpath;
            this.DateTimeFormat = datetimeformat;
            this.FileExtension = extension;
            this.FileSize = ffilesize;
        }
        #endregion

        #region public methods
        /// <summary>
        /// Returns a bool value after validating the folder path
        /// </summary>
        /// <param name="folderpath">String that takes the folderpath</param>
        /// <returns></returns>
        public static bool ValidateFolderPath(string folderpath)
        {
            try
            {
                return Directory.Exists(folderpath);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Returns bool value after validating the timezone
        /// </summary>
        /// <param name="timezone">String that takes the timezone</param>
        /// <returns></returns>
        public static bool ValidateTimeZone(string timezone)
        {
            var timeZones = TimeZoneInfo
            .GetSystemTimeZones()
            .ToDictionary(stz => stz.Id);

            TimeZoneInfo tz;
            if (timeZones.TryGetValue(timezone, out tz))
                return true;
            else
                return false;
        }
        public static bool ValidateFileSize(string size)
        {
            try {
                int nsize = Convert.ToInt32(size);
                return true;
            }
            catch (Exception) {
                return false;
            }
        }
        public bool ValidateInputFile()
        {
            foreach (string file in Directory.EnumerateFiles(InputFolderPath))
            {
                FileInfo fi = new FileInfo(file);
                if (fi.Extension == FileExtension)
                {
                    return true;
                }
                else
                    return false;

            }
            return true;
        }

       
        public void SplitFile(string file, int chunkSize)
        {
            const int BUFFER_SIZE = 20 * 1024;
            byte[] buffer = new byte[BUFFER_SIZE];
            var lineCount = File.ReadAllLines(file).Length;
            using (Stream input = File.OpenRead(file))
            {
                int index = 0;
                while (input.Position < input.Length)
                {
                    using (Stream output = File.Create(InputFolderPath + "\\" + index))
                    {
                        int remaining = chunkSize, bytesRead;
                        while (remaining > 0 && (bytesRead = input.Read(buffer, 0,
                                Math.Min(remaining, BUFFER_SIZE))) > 0)
                        {
                            output.Write(buffer, 0, bytesRead);

                            //lines_per_part = int(total_lines + N - 1) / N

                            var linesCount = File.ReadAllLines(InputFolderPath + "\\" + index).Length;
                            remaining -= bytesRead;
                        }
                    }
                    index++;
                    Thread.Sleep(500); // experimental; perhaps try it
                }
            }
        }
        public bool ReadInputFiles()
        {
            try
            {
                foreach (string file in Directory.EnumerateFiles(InputFolderPath, "*" + FileExtension))
                {
                    string[] lines = System.IO.File.ReadAllLines(file);
                    IEnumerable lines1 = System.IO.File.ReadLines(file);
                    StringBuilder builder = new StringBuilder();
                    int i = 0;
                    int j = 0;
                    int total = 0;
                    foreach (string line in lines)
                    {
                        int length = line.Length;
                        var temp = line.Split('\t');
                        var tempCount = temp.Count();
                        if (i == 0)
                        {
                            var colHeader1 = "Item Name";
                            var colHeader2 = "Date and Time";
                            var colHeader3 = "Value";
                            var colHeader4 = "Quality";
                            builder.AppendLine(string.Join(",", colHeader1, colHeader2, colHeader3, colHeader4));
                        }
                        else
                        {
                            long tnanoseconds = long.Parse(temp[1]);
                            var sec = TimeSpan.FromTicks(tnanoseconds).TotalSeconds.ToString();
                            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                            double deltaseconds = Convert.ToDouble(Convert.ToDouble(sec) - 11644473600);
                            var result = Convert.ToDateTime(origin.AddSeconds(deltaseconds)).ToString("yyyy-MM-ddThh:mm:sszzz");
                            if (DateTimeFormat != "UTC")
                            {
                                result = ConvertToTZ(DateTimeFormat, Convert.ToDateTime(origin.AddSeconds(deltaseconds)));
                            }
                            var quality = (Convert.ToInt32(temp[3]) >= 100) ? "good" : "bad";
                            builder.AppendLine(string.Join(",", temp[0], result, temp[2], quality));
                        }
                        if (lines.Count() >= 100000)
                        {
                            if (i == (j * 100000 + 100000))
                            {
                                //FileStream fs = new FileStream();
                                System.IO.File.WriteAllText(OutputFolderPath + "\\" + Path.GetFileNameWithoutExtension(file) + "-row-" + i + ".csv", builder.ToString());
                                builder = new StringBuilder();
                                var colHeader1 = "Item Name";
                                var colHeader2 = "Date and Time";
                                var colHeader3 = "Value";
                                var colHeader4 = "Quality";
                                builder.AppendLine(string.Join(",", colHeader1, colHeader2, colHeader3, colHeader4));
                                j++;
                            }
                        }
                        else
                        {
                            System.IO.File.WriteAllText(OutputFolderPath + "\\" + Path.GetFileNameWithoutExtension(file) + "-row-" + i + ".csv", builder.ToString());
                            builder = new StringBuilder();
                            var colHeader1 = "Item Name";
                            var colHeader2 = "Date and Time";
                            var colHeader3 = "Value";
                            var colHeader4 = "Quality";
                            builder.AppendLine(string.Join(",", colHeader1, colHeader2, colHeader3, colHeader4));
                        }
                        i++;
                    }

                }
                return true;
            }
            catch (Exception ex)
            {
                Console.Write("Error Occurred: " + ex.Message);
                return false;
            }
        }
        public string ConvertToTZ(string timezone, DateTime date)
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById(timezone);
            var istdate = TimeZoneInfo.ConvertTimeFromUtc(date, tz);
            return istdate.ToString("yyyy-mm-ddThh:mm:sszzz");
        }

        public bool ReadInputFiles1()
        {
            try
            {
                foreach (string file in Directory.EnumerateFiles(InputFolderPath, "*" + FileExtension))
                {
                    string[] lines = System.IO.File.ReadAllLines(file);
                    IEnumerable lines1 = System.IO.File.ReadLines(file);
                    StringBuilder builder = new StringBuilder();
                    int i = 0;
                    int j = 0;
                    int total = 0;
                    foreach (string line in lines)
                    {
                        int length = line.Length;
                        var temp = line.Split('\t');
                        var tempCount = temp.Count();
                        if (i == 0)
                        {
                            var colHeader1 = "Item Name";
                            var colHeader2 = "Date and Time";
                            var colHeader3 = "Value";
                            var colHeader4 = "Quality";
                            builder.AppendLine(string.Join(",", colHeader1, colHeader2, colHeader3, colHeader4));
                        }
                        else
                        {
                            long tnanoseconds = long.Parse(temp[1]);
                            var sec = TimeSpan.FromTicks(tnanoseconds).TotalSeconds.ToString();
                            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                            double deltaseconds = Convert.ToDouble(Convert.ToDouble(sec) - 11644473600);
                            var result = Convert.ToDateTime(origin.AddSeconds(deltaseconds)).ToString("yyyy-MM-ddThh:mm:sszzz");
                            if (DateTimeFormat != "UTC")
                            {
                                result = ConvertToTZ(DateTimeFormat, Convert.ToDateTime(origin.AddSeconds(deltaseconds)));
                            }
                            var quality = (Convert.ToInt32(temp[3]) >= 100) ? "good" : "bad";
                            builder.AppendLine(string.Join(",", temp[0], result, temp[2], quality));
                        }
                        
                        
                        i++;
                    }
                    System.IO.File.WriteAllText(System.IO.Path.GetTempPath() + "\\" + Path.GetFileNameWithoutExtension(file) + "-row-" + i + ".csv", builder.ToString());
                    if (!IsFileLocked(System.IO.Path.GetTempPath() + "\\" + Path.GetFileNameWithoutExtension(file) + "-row-" + i + ".csv"))
                        Split(System.IO.Path.GetTempPath() + "\\" + Path.GetFileNameWithoutExtension(file) + "-row-" + i + ".csv", Path.GetFileNameWithoutExtension(file), Int32.Parse(FileSize));
                    //SplitCSV(OutputFolderPath + "\\" + Path.GetFileNameWithoutExtension(file) + "-row-" + i + ".csv",FileSize);
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.Write("Error Occurred: " + ex.Message);
                return false;
            }
        }
        const int ERROR_SHARING_VIOLATION = 32;
        const int ERROR_LOCK_VIOLATION = 33;
        private bool IsFileLocked(string file)
        {
            //check that problem is not in destination file
            if (File.Exists(file) == true)
            {
                FileStream stream = null;
                try
                {
                    stream = File.Open(file, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                }
                catch (Exception ex2)
                {
                    //_log.WriteLog(ex2, "Error in checking whether file is locked " + file);
                    int errorCode = Marshal.GetHRForException(ex2) & ((1 << 16) - 1);
                    if ((ex2 is IOException) && (errorCode == ERROR_SHARING_VIOLATION || errorCode == ERROR_LOCK_VIOLATION))
                    {
                        return true;
                    }
                }
                finally
                {
                    if (stream != null)
                        stream.Close();
                }
            }
            return false;
        }
        #endregion

        public void SplitCSV(string file, string filesize) {
            //Read Specified file size
            int size = Int32.Parse(filesize);

            size *= 1024 * 1024;  //1 MB size

            int total = 0;
            int num = 0;
            string FirstLine = null;   // header to new file                  
            var writer = new StreamWriter(OutputFolderPath + Path.GetFileNameWithoutExtension(file) + "-" + (num+1) + ".csv");

            // Loop through all source lines
            foreach (var line in File.ReadLines(file))
            {
                if (string.IsNullOrEmpty(FirstLine)) FirstLine = line;
                // Length of current line
                int length = line.Length;

                // See if adding this line would exceed the size threshold
                if (total + length >= size)
                {
                    // Create a new file
                    num++;
                    total = 0;
                    writer.Dispose();
                    writer = new StreamWriter(OutputFolderPath + Path.GetFileNameWithoutExtension(file) + "-" + (num + 1) + ".csv");
                    writer.WriteLine(FirstLine);
                    length += FirstLine.Length;
                }

                // Write the line to the current file                
                writer.WriteLine(line);

                // Add length of line in bytes to running size
                total += length;

                // Add size of newlines
                total += Environment.NewLine.Length;
            }
            //File.Delete(file);
        }

        static string GetFileName(string prefix, int num)
        {
            return prefix + "_" + num.ToString("00") + ".csv";
        }

        public void Split(string file, string prefix, int size) {
            //int size = Int32.Parse(filesize);
            //size = (size);
            size *= 1024 * 1024;
            string[] arr = System.IO.File.ReadAllLines(file);
            string FirstLine = null;
            int total = 0;
            int num = 0;
            var writer = new System.IO.StreamWriter(OutputFolderPath + "\\" + GetFileName(prefix, num));

            // Loop through all source lines
            for (int i = 0; i < arr.Length; i++)
            {
                // Current line
                string line = arr[i];
                if (string.IsNullOrEmpty(FirstLine)) FirstLine = line;
                // Length of current line
                int length = line.Length;

                // See if adding this line would exceed the size threshold
                if (total + length >= size)
                {
                    // Create a new file
                    num++;
                    total = 0;
                    writer.Dispose();
                    writer = new System.IO.StreamWriter(OutputFolderPath + "\\"+ GetFileName(prefix, num));
                    writer.WriteLine(FirstLine);
                }
                // Write the line to the current file
                writer.WriteLine(line);

                // Add length of line in bytes to running size
                total += length;

                // Add size of newlines
                total += Environment.NewLine.Length;
            }
            writer.Dispose();
            File.Delete(file);

        }

    }

}
