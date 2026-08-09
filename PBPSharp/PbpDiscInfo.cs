using System.Buffers;
using System.IO.Compression;
using System.Text;
using PBPSharp.Models;

namespace PBPSharp;

/// <summary>
/// Represents a single disc entry within a PBP file. Provides access to
/// the disc's TOC, ISO index, and methods to read/extract ISO data.
/// </summary>
public sealed class PbpDiscInfo
{
    private const int MaxIndexes = 0x7E00;
    private const uint PsarGameIdOffset = 0x400;
    private const uint PsarTocOffset = 0x800;
    private const uint PsarIndexOffset = 0x4000;
    private const uint PsarIsoOffset = 0x100000;

    /// <summary>
    /// The size of one ISO block in bytes (2352 bytes per sector).
    /// </summary>
    public const int IsoBlockSize = 0x930;

    private readonly Stream _stream;
    private readonly int _psarOffset;
    private readonly List<IsoIndexEntry> _isoIndex;

    /// <summary>
    /// The 1-based disc index within the PBP (1 for single-disc).
    /// </summary>
    public int Index { get; }

    /// <summary>
    /// The disc ID (e.g., "SCUS94163").
    /// </summary>
    public string DiscId { get; }

    /// <summary>
    /// The Table of Contents entries for this disc.
    /// </summary>
    public IReadOnlyList<TocEntry> Toc { get; }

    /// <summary>
    /// The total uncompressed ISO size in bytes.
    /// </summary>
    public uint IsoSize { get; }

    /// <summary>
    /// The number of ISO data blocks.
    /// </summary>
    public int BlockCount => _isoIndex.Count;

    internal PbpDiscInfo(Stream stream, int psarOffset, int index)
    {
        _stream = stream;
        _psarOffset = psarOffset;
        Index = index;

        DiscId = ReadDiscId();
        Toc = ReadToc();
        _isoIndex = ReadIsoIndexes();
        IsoSize = ReadIsoSize();
    }

    private string ReadDiscId()
    {
        var buffer = new byte[16];
        _stream.Seek(_psarOffset + PsarGameIdOffset, SeekOrigin.Begin);
        _stream.ReadByte();
        _stream.ReadExactly(buffer, 0, 4);
        _stream.ReadByte();
        _stream.ReadExactly(buffer, 4, 5);
        return Encoding.ASCII.GetString(buffer, 0, 9);
    }

    private List<TocEntry> ReadToc()
    {
        var entries = new List<TocEntry>();
        var buffer = new byte[0xA];

        try
        {
            _stream.Seek(_psarOffset + PsarTocOffset, SeekOrigin.Begin);

            _stream.ReadExactly(buffer, 0, 0xA);
            if (buffer[2] != 0xA0) return entries;

            var startTrack = FromBinaryDecimal(buffer[7]);

            _stream.ReadExactly(buffer, 0, 0xA);
            if (buffer[2] != 0xA1) return entries;

            var endTrack = FromBinaryDecimal(buffer[7]);

            _stream.ReadExactly(buffer, 0, 0xA);
            if (buffer[2] != 0xA2) return entries;

            for (var c = startTrack; c <= endTrack; c++)
            {
                _stream.ReadExactly(buffer, 0, 0xA);
                var trackNo = FromBinaryDecimal(buffer[2]);
                if (trackNo != c) return entries;

                entries.Add(new TocEntry
                {
                    TrackType = (TrackType)buffer[0],
                    TrackNo = trackNo,
                    Minutes = FromBinaryDecimal(buffer[3]),
                    Seconds = FromBinaryDecimal(buffer[4]),
                    Frames = FromBinaryDecimal(buffer[5])
                });
            }
        }
        catch
        {
            // Return whatever we managed to read
        }

        return entries;
    }

    private List<IsoIndexEntry> ReadIsoIndexes()
    {
        var isoIndex = new List<IsoIndexEntry>();

        _stream.Seek(_psarOffset + PsarIndexOffset, SeekOrigin.Begin);

        var thisOffset = (uint)_stream.Position;
        var psarIsoEnd = _psarOffset + PsarIsoOffset;
        var indexBytes = new byte[32]; // 8 bytes offset+length + 24 bytes dummy

        while (thisOffset < psarIsoEnd)
        {
            if (_stream.Read(indexBytes, 0, 32) != 32) break;

            var offset = BitConverter.ToUInt32(indexBytes, 0);
            var length = BitConverter.ToInt32(indexBytes, 4);

            thisOffset = (uint)_stream.Position;

            if (offset != 0 || length != 0)
            {
                isoIndex.Add(new IsoIndexEntry { Offset = offset, Length = length });

                if (isoIndex.Count >= MaxIndexes)
                    throw new InvalidDataException("Number of indexes exceeds maximum allowed.");
            }
        }

        return isoIndex;
    }

    private uint ReadIsoSize()
    {
        var outBuffer = ArrayPool<byte>.Shared.Rent(16 * IsoBlockSize);
        try
        {
            ReadBlock(1, outBuffer, out _);
            return (uint)((outBuffer[104] | (outBuffer[105] << 8) | (outBuffer[106] << 16) | (outBuffer[107] << 24)) * IsoBlockSize);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(outBuffer);
        }
    }

    /// <summary>
    /// Reads and decompresses a single ISO block.
    /// </summary>
    /// <param name="blockIndex">The zero-based block index.</param>
    /// <param name="buffer">Buffer to receive the decompressed data (must be at least 16 * IsoBlockSize bytes).</param>
    /// <param name="bytesRead">The number of bytes actually written to the buffer.</param>
    public void ReadBlock(int blockIndex, byte[] buffer, out int bytesRead)
    {
        if (blockIndex < 0 || blockIndex >= _isoIndex.Count)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));

        var entry = _isoIndex[blockIndex];
        var thisOffset = _psarOffset + PsarIsoOffset + entry.Offset;
        _stream.Seek(thisOffset, SeekOrigin.Begin);

        if (entry.Length == 16 * IsoBlockSize)
        {
            _stream.ReadExactly(buffer, 0, 16 * IsoBlockSize);
            bytesRead = 16 * IsoBlockSize;
        }
        else
        {
            var inBuffer = ArrayPool<byte>.Shared.Rent(entry.Length);
            try
            {
                _stream.ReadExactly(inBuffer, 0, entry.Length);
                bytesRead = DecompressBlock(inBuffer, entry.Length, buffer);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(inBuffer);
            }
        }
    }

    /// <summary>
    /// Extracts the entire disc ISO to the specified output stream.
    /// </summary>
    /// <param name="outputStream">The stream to write the ISO data to.</param>
    /// <param name="progress">Optional callback with bytes written so far.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public void ExtractTo(Stream outputStream, Action<uint>? progress = null, CancellationToken cancellationToken = default)
    {
        var outBuffer = ArrayPool<byte>.Shared.Rent(16 * IsoBlockSize);
        try
        {
            uint totalWritten = 0;

            for (var i = 0; i < _isoIndex.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                ReadBlock(i, outBuffer, out var bufferSize);

                if (totalWritten + bufferSize > IsoSize)
                {
                    bufferSize = (int)(IsoSize - totalWritten);
                }

                outputStream.Write(outBuffer, 0, bufferSize);
                totalWritten += (uint)bufferSize;

                progress?.Invoke(totalWritten);
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(outBuffer);
        }
    }

    /// <summary>
    /// Extracts the disc as a BIN file and generates a companion CUE file.
    /// </summary>
    /// <param name="binPath">The path for the output BIN file.</param>
    /// <param name="cuePath">The path for the output CUE file. If null, uses binPath with .cue extension.</param>
    /// <param name="progress">Optional callback with bytes written so far.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="PbpError"/> indicating the result.</returns>
    public PbpError ExtractToBinCue(string binPath, string? cuePath = null, Action<uint>? progress = null, CancellationToken cancellationToken = default)
    {
        cuePath ??= Path.ChangeExtension(binPath, ".cue");

        try
        {
            using var binStream = File.Create(binPath);
            ExtractTo(binStream, progress, cancellationToken);
        }
        catch (IOException)
        {
            return PbpError.IoError;
        }
        catch (InvalidDataException)
        {
            return PbpError.DecompressionError;
        }

        var cueContent = CueSheetWriter.GenerateCueSheet(Path.GetFileName(binPath), Toc);
        try
        {
            File.WriteAllText(cuePath, cueContent);
        }
        catch (IOException)
        {
            return PbpError.IoError;
        }

        return PbpError.None;
    }

    private static int DecompressBlock(byte[] compressed, int compressedLength, byte[] output)
    {
        using var compressedStream = new MemoryStream(compressed, 0, compressedLength);
        using var deflateStream = new DeflateStream(compressedStream, CompressionMode.Decompress, false);
        using var outputMs = new MemoryStream(output);

        var writeBuffer = new byte[4096];
        while (true)
        {
            var totalRead = deflateStream.Read(writeBuffer, 0, writeBuffer.Length);
            if (totalRead <= 0) break;

            outputMs.Write(writeBuffer, 0, totalRead);
        }

        return (int)outputMs.Position;
    }

    private static int FromBinaryDecimal(byte value)
    {
        var ones = value % 16;
        var tens = value / 16;
        return tens * 10 + ones;
    }

    private sealed class IsoIndexEntry
    {
        public uint Offset { get; init; }
        public int Length { get; init; }
    }
}
