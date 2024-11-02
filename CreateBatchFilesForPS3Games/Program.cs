using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace CreateBatchFilesForPS3Games
{

    static partial class Program
    {
        private static LogForm? _logForm;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
        
            _logForm = new LogForm();
            _logForm.Show();
        
            _logForm.LogMessage("This program creates batch files to launch your PS3 games.");
            _logForm.LogMessage("Please follow the instructions.");
            _logForm.LogMessage("");
        
            MessageBox.Show("This program create batch files to launch your PS3 games.\n\nPlease follow the instructions.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        
            _logForm.LogMessage("Please select the root folder where your Game folders are located:");
            _logForm.LogMessage("");
            string? selectedFolder = SelectFolder();

            if (string.IsNullOrEmpty(selectedFolder))
            {
                _logForm.LogMessage("No folder selected. Exiting application.");
                MessageBox.Show("No folder selected. Exiting application.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _logForm.LogMessage("Please select the RPCS3 emulator binary file (rpcs3.exe):");
            _logForm.LogMessage("");
            string? rpcs3BinaryPath = SelectFile();

            if (string.IsNullOrEmpty(rpcs3BinaryPath))
            {
                _logForm.LogMessage("No file selected. Exiting application.");
                MessageBox.Show("No file selected. Exiting application.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            CreateBatchFilesForFolders(selectedFolder, rpcs3BinaryPath);

            string selectedFolder2= rpcs3BinaryPath.Replace("rpcs3.exe", "dev_hdd0\\game");

            CreateBatchFilesForFolders2(selectedFolder, selectedFolder2, rpcs3BinaryPath);
            
            _logForm.LogMessage("All batch files have been successfully created.");
            
            MessageBox.Show("All batch files have been successfully created.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            
            Application.Run(); // Keeps the application running for the log form to remain open
        }

        private static string? SelectFolder()
        {
            using var fbd = new FolderBrowserDialog();
            fbd.Description = "Please select the root folder where your Game folders are located.";

            DialogResult result = fbd.ShowDialog();

            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
            {
                return fbd.SelectedPath;
            }
            return null;
        }

        private static string? SelectFile()
        {
            using var ofd = new OpenFileDialog();
            ofd.Title = "Please select the RPCS3 emulator binary file (rpcs3.exe)";
            ofd.Filter = "exe files (*.exe)|*.exe|All files (*.*)|*.*";
            ofd.FilterIndex = 1;
            ofd.RestoreDirectory = true;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                return ofd.FileName;
            }
            return null;
        }

        private static void CreateBatchFilesForFolders(string selectedFolder, string rpcs3BinaryPath)
        {
            string[] subdirectoryEntries = Directory.GetDirectories(selectedFolder);
            int filesCreated = 0;

            foreach (string subdirectory in subdirectoryEntries)
            {
                string ebootPath = Path.Combine(subdirectory, "PS3_GAME\\USRDIR\\EBOOT.BIN");

                if (File.Exists(ebootPath))
                {
                    string title = GetTitle(subdirectory);
                    string batchFileName;

                    // Use TITLE if available, otherwise use TITLE_ID, and if neither, use the folder name
                    if (!string.IsNullOrEmpty(title))
                        batchFileName = title;
                    else
                    {
                        string titleId = GetId(subdirectory); // Fallback to TITLE_ID if TITLE is not available
                        batchFileName = !string.IsNullOrEmpty(titleId) ? titleId : Path.GetFileName(subdirectory);
                    }

                    // Sanitize the batch file name
                    batchFileName = SanitizeFileName(batchFileName);
                    string batchFilePath = Path.Combine(selectedFolder, batchFileName + ".bat");

                    using (StreamWriter sw = new(batchFilePath))
                    {
                        sw.WriteLine($"\"{rpcs3BinaryPath}\" --no-gui \"{ebootPath}\"");
                        _logForm?.LogMessage($"Batch file created: {batchFilePath}");

                    }
                    filesCreated++;
                }
                else
                {
                    _logForm?.LogMessage($"EBOOT.BIN not found in {subdirectory}, skipping batch file creation.");
                }
            }

            if (filesCreated > 0)
            {
                _logForm?.LogMessage("All batch files have been successfully created for the games on root folder.");
            }
            else
            {
                _logForm?.LogMessage("No EBOOT.BIN files found in subdirectories. No batch files were created.");
                MessageBox.Show("No EBOOT.BIN files found in subdirectories. No batch files were created.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void CreateBatchFilesForFolders2(string selectedFolder, string selectedFolder2, string rpcs3BinaryPath)
        {
            string[] subdirectoryEntries = Directory.GetDirectories(selectedFolder2);
            int filesCreated = 0;

            foreach (string subdirectory in subdirectoryEntries)
            {
                string ebootPath = Path.Combine(subdirectory, "USRDIR\\EBOOT.BIN");

                if (File.Exists(ebootPath))
                {
                    string title = GetTitle2(subdirectory);
                    string batchFileName;

                    // Use TITLE if available, otherwise use TITLE_ID, and if neither, use the folder name
                    if (!string.IsNullOrEmpty(title))
                        batchFileName = title;
                    else
                    {
                        string titleId = GetId2(subdirectory); // Fallback to TITLE_ID if TITLE is not available
                        batchFileName = !string.IsNullOrEmpty(titleId) ? titleId : Path.GetFileName(subdirectory);
                    }

                    // Sanitize the batch file name
                    batchFileName = SanitizeFileName(batchFileName);
                    string batchFilePath = Path.Combine(selectedFolder, batchFileName + ".bat");

                    using (StreamWriter sw = new(batchFilePath))
                    {
                        sw.WriteLine($"\"{rpcs3BinaryPath}\" --no-gui \"{ebootPath}\"");
                        _logForm?.LogMessage($"Batch file created: {batchFilePath}");
                    }
                    filesCreated++;
                }
                else
                {
                    _logForm?.LogMessage($"EBOOT.BIN not found in {subdirectory}, skipping batch file creation.");
                }
            }

            if (filesCreated > 0)
            {
                _logForm?.LogMessage("All batch files have been successfully created for the games in the RPCS3 folder.");
            }
            else
            {
                _logForm?.LogMessage("No EBOOT.BIN files found in subdirectories. No batch files were created.");
                MessageBox.Show("No EBOOT.BIN files found in subdirectories. No batch files were created.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static string GetId(string folderPath)
        {
            string sfoFilePath = Path.Combine(folderPath, "PS3_GAME\\PARAM.SFO");

            var sfoData = ReadSfo(sfoFilePath);
            if (sfoData == null || !sfoData.TryGetValue("TITLE_ID", out string? value))
                return "";

            return value.ToUpper();
        }

        private static string GetId2(string folderPath)
        {
            string sfoFilePath = Path.Combine(folderPath, "PARAM.SFO");

            var sfoData = ReadSfo(sfoFilePath);
            if (sfoData == null || !sfoData.TryGetValue("TITLE_ID", out string? value))
                return "";

            return value.ToUpper();
        }

        private static string GetTitle(string folderPath)
        {
            string sfoFilePath = Path.Combine(folderPath, "PS3_GAME\\PARAM.SFO");

            var sfoData = ReadSfo(sfoFilePath);
            if (sfoData == null || !sfoData.TryGetValue("TITLE", out string? value))
                return "";

            return value;
        }

        private static string GetTitle2(string folderPath)
        {
            string sfoFilePath = Path.Combine(folderPath, "PARAM.SFO");

            var sfoData = ReadSfo(sfoFilePath);
            if (sfoData == null || !sfoData.TryGetValue("TITLE", out string? value))
                return "";

            return value;
        }

        private static readonly char[] Separator = [' ', '.', '-', '_'];

        private static string SanitizeFileName(string filename)
        {
            // Replace specific characters with words
            filename = filename.Replace("Σ", "Sigma");

            // Remove unwanted symbols
            filename = filename.Replace("™", "").Replace("®", "");

            // Add space between letters and numbers
            filename = MyRegex().Replace(filename, "$1 $2");
            filename = MyRegex1().Replace(filename, "$1 $2");

            // Split the filename into words
            var words = filename.Split(Separator, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < words.Length; i++)
            {
                // Convert Roman numerals to uppercase
                if (IsRomanNumeral(words[i]))
                {
                    words[i] = words[i].ToUpper();
                }
                else
                {
                    words[i] = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(words[i].ToLower());
                }
            }

            // Reassemble the filename
            filename = String.Join(" ", words);

            // Remove any invalid characters
            var invalidChars = Path.GetInvalidFileNameChars();
            foreach (var c in invalidChars)
            {
                filename = filename.Replace(c.ToString(), "");
            }

            return filename;
        }

        private static bool IsRomanNumeral(string word)
        {
            return MyRegex2().IsMatch(word);
        }

        private static Dictionary<string, string>? ReadSfo(string sfoFilePath)
        {
            if (!File.Exists(sfoFilePath))
                return null;

            var result = new Dictionary<string, string>();
            var headerSize = Marshal.SizeOf(typeof(SfoHeader));
            var indexSize = Marshal.SizeOf(typeof(SfoTableEntry));

            var sfo = File.ReadAllBytes(sfoFilePath);
            SfoHeader sfoHeader;

            try
            {
                GCHandle handle = GCHandle.Alloc(sfo, GCHandleType.Pinned);
#pragma warning disable CS8605 // Unboxing a possibly null value.
                sfoHeader = (SfoHeader)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(SfoHeader));
#pragma warning restore CS8605 // Unboxing a possibly null value.
                handle.Free();
            }
            catch (Exception ex)
            {
                _logForm?.LogMessage("Error reading SFO file: " + ex.Message);
                return null;
            }

            var indexOffset = headerSize;
            var keyOffset = sfoHeader.key_table_start;
            var valueOffset = sfoHeader.data_table_start;
            for (var i = 0; i < sfoHeader.tables_entries; i++)
            {
                var sfoEntry = new byte[indexSize];
                Array.Copy(sfo, indexOffset + i * indexSize, sfoEntry, 0, indexSize);

                SfoTableEntry sfoTableEntry;
                try
                {
                    GCHandle handle = GCHandle.Alloc(sfoEntry, GCHandleType.Pinned);
#pragma warning disable CS8605 // Unboxing a possibly null value.
                    sfoTableEntry = (SfoTableEntry)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(SfoTableEntry));
#pragma warning restore CS8605 // Unboxing a possibly null value.
                    handle.Free();
                }
                catch (Exception ex)
                {
                    _logForm?.LogMessage("Error reading SFO file: " + ex.Message);
                    return null;
                }

                var entryValueOffset = valueOffset + sfoTableEntry.data_offset;
                var entryKeyOffset = keyOffset + sfoTableEntry.key_offset;
                var val = "";
                var keyBytes = Encoding.UTF8.GetString(sfo.Skip((int)entryKeyOffset).TakeWhile(b => !b.Equals(0)).ToArray());
                switch (sfoTableEntry.data_fmt)
                {
                    case 0x0004: //non-null string
                    case 0x0204: //null string
                        var strBytes = new byte[sfoTableEntry.data_len];
                        Array.Copy(sfo, entryValueOffset, strBytes, 0, sfoTableEntry.data_len);
                        val = Encoding.UTF8.GetString(strBytes).TrimEnd('\0');
                        break;
                    case 0x0404: //uint32
                        val = BitConverter.ToUInt32(sfo, (int)entryValueOffset).ToString();
                        break;
                }
                result.TryAdd(keyBytes, val);
            }

            return result;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct SfoHeader
        {
            [FieldOffset(0)]
            public uint magic;
            [FieldOffset(4)]
            public uint version;
            [FieldOffset(8)]
            public uint key_table_start;
            [FieldOffset(12)]
            public uint data_table_start;
            [FieldOffset(16)]
            public uint tables_entries;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct SfoTableEntry
        {
            [FieldOffset(0)]
            public ushort key_offset;
            [FieldOffset(2)]
            public ushort data_fmt; // 0x0004 utf8-S (non-null string), 0x0204 utf8 (null string), 0x0404 uint32
            [FieldOffset(4)]
            public uint data_len;
            [FieldOffset(8)]
            public uint data_max_len;
            [FieldOffset(12)]
            public uint data_offset;
        }

        [GeneratedRegex("(\\p{L})(\\p{N})")]
        private static partial Regex MyRegex();
        [GeneratedRegex("(\\p{N})(\\p{L})")]
        private static partial Regex MyRegex1();
        [GeneratedRegex(@"^M{0,4}(CM|CD|D?C{0,3})(XC|XL|L?X{0,3})(IX|IV|V?I{0,3})$", RegexOptions.IgnoreCase, "pt-BR")]
        private static partial Regex MyRegex2();
    }
}