using System.Buffers;
using System.Buffers.Binary;
using System.Text;
using ICSharpCode.SharpZipLib;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using PBPSharp.Models;

namespace PBPSharp;

/// <summary>
///     Represents a single disc entry within a PBP file. Provides access to
///     the disc's TOC, ISO index, and methods to read/extract ISO data.
/// </summary>
public sealed class PbpDiscInfo
{
    private const int MaxIndexes = 0x7E00;
    private const uint PsarGameIdOffset = 0x400;
    private const uint PsarTocOffset = 0x800;
    private const uint PsarIndexOffset = 0x4000;
    private const uint PsarIsoOffset = 0x100000;

    /// <summary>
    ///     The size of one ISO block in bytes (2352 bytes per sector).
    /// </summary>
    public const int IsoBlockSize = 0x930;

    /// <summary>
    ///     Upper bound for a single block entry. Incompressible 16-sector blocks stored as a
    ///     deflate stream are a few bytes LARGER than the raw block (stored-block headers plus
    ///     framing), so the cap allows that slack instead of rejecting a perfectly good block.
    ///     This matches the reference implementation, which imposes no cap at all.
    /// </summary>
    private const int MaxBlockEntrySize = (16 * IsoBlockSize) + 4096;

    private readonly List<IsoIndexEntry> _isoIndex;
    private readonly int _psarOffset;

    private readonly Stream _stream;

    internal PbpDiscInfo(Stream stream, int psarOffset, int index)
    {
        _stream = stream;
        _psarOffset = psarOffset;
        Index = index;

        DiscId = ReadDiscId();
        Toc = ReadToc();
        _isoIndex = ReadIsoIndexes();

        // A disc with no ISO index entries is not a usable PlayStation disc image. The container
        // header parsed correctly, so this almost always means the file ends before its data area
        // (truncated download). The dedicated exception lets callers report that cause instead of
        // a generic corrupt-file error.
        if (_isoIndex.Count == 0)
            throw new NoIsoIndexException();

        IsoSize = ReadIsoSize();
    }

    /// <summary>
    ///     The 1-based disc index within the PBP (1 for single-disc).
    /// </summary>
    public int Index { get; }

    /// <summary>
    ///     The disc ID (e.g., "SCUS94163").
    /// </summary>
    public string DiscId { get; }

    /// <summary>
    ///     The Table of Contents entries for this disc.
    /// </summary>
    public IReadOnlyList<TocEntry> Toc { get; }

    /// <summary>
    ///     The total uncompressed ISO size in bytes.
    /// </summary>
    public uint IsoSize { get; }

    /// <summary>
    ///     The number of ISO data blocks.
    /// </summary>
    public int BlockCount => _isoIndex.Count;

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
            if (buffer[2] != 0xA0)
                return entries;

            var startTrack = FromBinaryDecimal(buffer[7]);

            _stream.ReadExactly(buffer, 0, 0xA);
            if (buffer[2] != 0xA1)
                return entries;

            var endTrack = FromBinaryDecimal(buffer[7]);

            _stream.ReadExactly(buffer, 0, 0xA);
            if (buffer[2] != 0xA2)
                return entries;

            for (var c = startTrack; c <= endTrack; c++)
            {
                _stream.ReadExactly(buffer, 0, 0xA);
                var trackNo = FromBinaryDecimal(buffer[2]);
                if (trackNo != c)
                    return entries;

                entries.Add(
                    new TocEntry
                    {
                        TrackType = (TrackType)buffer[0],
                        TrackNo = trackNo,
                        Minutes = FromBinaryDecimal(buffer[3]),
                        Seconds = FromBinaryDecimal(buffer[4]),
                        Frames = FromBinaryDecimal(buffer[5])
                    }
                );
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
        var indexBytes = new byte[32]; // 4 offset + 2 size + 2 flags + 16 hash + 8 dummy

        while (thisOffset < psarIsoEnd)
        {
            if (_stream.Read(indexBytes, 0, 32) != 32)
                break;

            var offset = BinaryPrimitives.ReadUInt32LittleEndian(indexBytes.AsSpan(0, 4));

            // The official index entry stores the block size as a 16-bit value (bytes 4-5)
            // followed by a flag byte (bit 0 marks a stored/uncompressed block, e.g. pop-fe
            // with compression disabled) and a SHA-1. Tools that instead write the size as a
            // full 32-bit int (popstation, PSX2PSP, iPoPS) still leave bytes 6-7 zero for
            // every legal block size, so the 16-bit read covers both layouts.
            var size = BinaryPrimitives.ReadUInt16LittleEndian(indexBytes.AsSpan(4, 2));
            var flags = indexBytes[6];

            thisOffset = (uint)_stream.Position;

            if (offset != 0 || size != 0)
            {
                isoIndex.Add(
                    new IsoIndexEntry
                    {
                        Offset = offset,
                        Length = size,
                        Uncompressed = (flags & 1) != 0
                    }
                );

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
            return (uint)(
                (
                    outBuffer[104]
                    | (outBuffer[105] << 8)
                    | (outBuffer[106] << 16)
                    | (outBuffer[107] << 24)
                ) * IsoBlockSize
            );
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(outBuffer);
        }
    }

    /// <summary>
    ///     Reads and decompresses a single ISO block.
    /// </summary>
    /// <param name="blockIndex">The zero-based block index.</param>
    /// <param name="buffer">Buffer to receive the decompressed data (must be at least 16 * IsoBlockSize bytes).</param>
    /// <param name="bytesRead">The number of bytes actually written to the buffer.</param>
    public void ReadBlock(int blockIndex, byte[] buffer, out int bytesRead)
    {
        if (blockIndex < 0 || blockIndex >= _isoIndex.Count)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));

        var entry = _isoIndex[blockIndex];

        // The size field is 16 bits, so a value beyond the sane cap can only mean a corrupt
        // index. The cap only rejects entries far beyond any legitimate block: a deflate
        // stream for a 16-sector block never exceeds the raw block size by more than a few
        // dozen bytes of framing.
        if (entry.Length is < 0 or > MaxBlockEntrySize)
            throw new InvalidDataException("Invalid ISO block length in PSAR index.");

        // 64-bit math: the PSAR offset plus a large block offset can overflow int32 on
        // multi-gigabyte files.
        var thisOffset = (long)_psarOffset + PsarIsoOffset + entry.Offset;
        _stream.Seek(thisOffset, SeekOrigin.Begin);

        // A full-size block, or any block explicitly flagged as stored/uncompressed, is
        // copied verbatim; everything else is a deflate (raw, or zlib-wrapped) stream.
        if (entry.Uncompressed || entry.Length == 16 * IsoBlockSize)
        {
            // A stored block can never exceed the raw block size; a larger length here can
            // only be a corrupt index, and copying it would overrun the caller's buffer.
            if (entry.Length > 16 * IsoBlockSize)
                throw new InvalidDataException("Invalid ISO block length in PSAR index.");

            _stream.ReadExactly(buffer, 0, entry.Length);
            bytesRead = entry.Length;
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
    ///     Extracts the entire disc ISO to the specified output stream.
    /// </summary>
    /// <param name="outputStream">The stream to write the ISO data to.</param>
    /// <param name="progress">Optional callback with bytes written so far.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public void ExtractTo(
        Stream outputStream,
        Action<uint>? progress = null,
        CancellationToken cancellationToken = default
    )
    {
        var outBuffer = ArrayPool<byte>.Shared.Rent(16 * IsoBlockSize);
        try
        {
            uint totalWritten = 0;

            for (var i = 0; i < _isoIndex.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                ReadBlock(i, outBuffer, out var bufferSize);

                if (totalWritten + bufferSize > IsoSize) bufferSize = (int)(IsoSize - totalWritten);

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
    ///     Extracts the disc as a BIN file and generates a companion CUE file.
    /// </summary>
    /// <param name="binPath">The path for the output BIN file.</param>
    /// <param name="cuePath">The path for the output CUE file. If null, uses binPath with .cue extension.</param>
    /// <param name="progress">Optional callback with bytes written so far.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="PbpError" /> indicating the result.</returns>
    public PbpError ExtractToBinCue(
        string binPath,
        string? cuePath = null,
        Action<uint>? progress = null,
        CancellationToken cancellationToken = default
    )
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
        catch (SharpZipBaseException)
        {
            // A block failed to inflate in the reference-compatible SharpZipLib inflater.
            return PbpError.DecompressionError;
        }
        catch (IndexOutOfRangeException)
        {
            // Corrupt deflate stream surfaced as a raw array error by the Inflater.
            return PbpError.DecompressionError;
        }
        catch (NotSupportedException)
        {
            // A corrupt block inflated beyond the fixed output buffer capacity.
            return PbpError.CorruptFile;
        }
        catch (ArgumentOutOfRangeException)
        {
            // Corrupt index entry (block or length out of range).
            return PbpError.CorruptFile;
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
        // The popstation reference tools (popstation, PSX2PSP, iPoPS) compress PSAR blocks as
        // RAW deflate (zlib deflateInit2 with windowBits -15), so raw inflation first keeps
        // behaviour identical to the reference. A few other PBP authoring tools instead wrap
        // the deflate stream in a zlib container (2-byte header + Adler-32 trailer); those
        // streams always fail raw inflation, so retry the same bytes as a zlib-wrapped stream
        // before giving up.
        try
        {
            return Inflate(compressed, compressedLength, output, noHeader: true);
        }
        catch (Exception ex)
            when (ex
                      is SharpZipBaseException
                      or InvalidDataException
                  && compressedLength > 2
                 )
        {
            return Inflate(compressed, compressedLength, output, noHeader: false);
        }

        static int Inflate(
            byte[] compressed,
            int compressedLength,
            byte[] output,
            bool noHeader
        )
        {
            using var compressedStream = new MemoryStream(compressed, 0, compressedLength);
            using var inflaterStream = new InflaterInputStream(
                compressedStream,
                new Inflater(noHeader)
            );
            using var outputMs = new MemoryStream(output);

            try
            {
                var writeBuffer = new byte[4096];
                while (true)
                {
                    var totalRead = inflaterStream.Read(writeBuffer, 0, writeBuffer.Length);
                    if (totalRead <= 0)
                        break;

                    outputMs.Write(writeBuffer, 0, totalRead);
                }
            }
            catch (IndexOutOfRangeException)
            {
                // A malformed deflate stream can drive SharpZipLib's Inflater into a raw
                // IndexOutOfRangeException instead of its own exception type. Normalize it so
                // callers classify the failure as decompression data rather than an app bug.
                throw new InvalidDataException("Corrupt deflate stream in PSAR block.");
            }

            return (int)outputMs.Position;
        }
    }

    private static int FromBinaryDecimal(byte value)
    {
        var ones = value % 16;
        var tens = value / 16;
        return (tens * 10) + ones;
    }

    private sealed class IsoIndexEntry
    {
        public uint Offset { get; init; }
        public int Length { get; init; }

        /// <summary>
        ///     Flag bit 0 from the index entry: the block is stored uncompressed.
        /// </summary>
        public bool Uncompressed { get; init; }
    }
}