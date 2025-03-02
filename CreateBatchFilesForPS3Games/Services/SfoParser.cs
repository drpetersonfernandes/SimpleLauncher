using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CreateBatchFilesForPS3Games.Interfaces;

namespace CreateBatchFilesForPS3Games.Services
{
    public class SfoParser : ISfoParser
    {
        private readonly ILogger _logger;

        public SfoParser(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<Dictionary<string, string>?> ParseSfoFileAsync(string filePath, CancellationToken cancellationToken = default)
        {
            if (!File.Exists(filePath))
            {
                _logger.LogWarning($"SFO file not found: {filePath}");
                return null;
            }

            try
            {
                var sfoData = await File.ReadAllBytesAsync(filePath, cancellationToken);
                return ParseSfoData(sfoData);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error reading SFO file {filePath}: {ex.Message}");
                return null;
            }
        }

        private Dictionary<string, string>? ParseSfoData(byte[] sfoData)
        {
            var result = new Dictionary<string, string>();

            // Parse SFO header
            if (sfoData.Length < 20) // Minimum size for header
            {
                _logger.LogError("SFO file too small");
                return null;
            }

            var magic = BitConverter.ToUInt32(sfoData, 0);
            // var version = BitConverter.ToUInt32(sfoData, 4); // Unused variable
            var keyTableStart = BitConverter.ToUInt32(sfoData, 8);
            var dataTableStart = BitConverter.ToUInt32(sfoData, 12);
            var tablesEntries = BitConverter.ToUInt32(sfoData, 16);

            // Verify SFO magic (\0PSF)
            if (magic != 0x46535000)
            {
                _logger.LogError("Invalid SFO magic value");
                return null;
            }

            var headerSize = 20;
            var indexSize = 16;

            for (var i = 0; i < tablesEntries; i++)
            {
                var entryOffset = headerSize + (i * indexSize);

                // Ensure there's enough data for the entry
                if (entryOffset + indexSize > sfoData.Length)
                {
                    _logger.LogError("SFO entry extends beyond file size");
                    break;
                }

                var keyOffset = BitConverter.ToUInt16(sfoData, entryOffset);
                var dataFmt = BitConverter.ToUInt16(sfoData, entryOffset + 2);
                var dataLen = BitConverter.ToUInt32(sfoData, entryOffset + 4);
                // var dataMaxLen = BitConverter.ToUInt32(sfoData, entryOffset + 8); // Unused variable
                var dataOffset = BitConverter.ToUInt32(sfoData, entryOffset + 12);

                // Read key (null-terminated string)
                var keyPosition = (int)(keyTableStart + keyOffset);
                var key = ReadNullTerminatedString(sfoData, keyPosition);

                // Read value based on format
                var dataPosition = (int)(dataTableStart + dataOffset);
                var value = string.Empty;

                // Handle according to data format
                switch (dataFmt)
                {
                    case 0x0004: // UTF-8 string (not null-terminated)
                    case 0x0204: // UTF-8 string (null-terminated)
                        value = System.Text.Encoding.UTF8.GetString(sfoData, dataPosition, (int)dataLen).TrimEnd('\0');
                        break;
                    case 0x0404: // UInt32
                        if (dataPosition + 4 <= sfoData.Length)
                        {
                            value = BitConverter.ToUInt32(sfoData, dataPosition).ToString();
                        }

                        break;
                    default:
                        _logger.LogWarning($"Unknown SFO data format: 0x{dataFmt:X4}");
                        break;
                }

                result[key] = value;
            }

            return result;
        }

        private string ReadNullTerminatedString(byte[] data, int position)
        {
            // Read bytes until null terminator
            List<byte> bytes = new();
            while (position < data.Length && data[position] != 0)
            {
                bytes.Add(data[position]);
                position++;
            }

            return System.Text.Encoding.UTF8.GetString(bytes.ToArray());
        }
    }
}