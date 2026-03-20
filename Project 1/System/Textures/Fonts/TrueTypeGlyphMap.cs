using System;
using System.Collections.Generic;
using System.IO;

namespace Project_1.Textures
{
    internal static class TrueTypeGlyphMap
    {
        public static Dictionary<int, int> LoadCodepointToGlyphIndex(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return null;
            }

            byte[] data = File.ReadAllBytes(path);
            if (data.Length < 12)
            {
                return null;
            }

            uint cmapOffset = FindTable(data, "cmap");
            if (cmapOffset == 0 || cmapOffset + 4 > data.Length)
            {
                return null;
            }

            ushort numTables = ReadUInt16(data, (int)cmapOffset + 2);
            int tableRecordOffset = (int)cmapOffset + 4;

            uint format12Offset = 0;
            uint format4Offset = 0;

            for (int i = 0; i < numTables; i++)
            {
                int recordOffset = tableRecordOffset + i * 8;
                if (recordOffset + 8 > data.Length)
                {
                    break;
                }

                ushort platformId = ReadUInt16(data, recordOffset);
                ushort encodingId = ReadUInt16(data, recordOffset + 2);
                uint subtableOffset = cmapOffset + ReadUInt32(data, recordOffset + 4);
                if (subtableOffset + 2 > data.Length)
                {
                    continue;
                }

                ushort format = ReadUInt16(data, (int)subtableOffset);
                bool isUnicodeSubtable =
                    platformId == 0
                    || (platformId == 3 && (encodingId == 1 || encodingId == 10));

                if (!isUnicodeSubtable)
                {
                    continue;
                }

                if (format == 12 && format12Offset == 0)
                {
                    format12Offset = subtableOffset;
                }
                else if (format == 4 && format4Offset == 0)
                {
                    format4Offset = subtableOffset;
                }
            }

            if (format12Offset != 0)
            {
                return ParseFormat12(data, (int)format12Offset);
            }

            if (format4Offset != 0)
            {
                return ParseFormat4(data, (int)format4Offset);
            }

            return null;
        }

        static Dictionary<int, int> ParseFormat12(byte[] data, int offset)
        {
            if (offset + 16 > data.Length)
            {
                return null;
            }

            uint groupCount = ReadUInt32(data, offset + 12);
            int groupOffset = offset + 16;
            Dictionary<int, int> map = new Dictionary<int, int>();

            for (uint i = 0; i < groupCount; i++)
            {
                int recordOffset = groupOffset + (int)i * 12;
                if (recordOffset + 12 > data.Length)
                {
                    break;
                }

                uint startCharCode = ReadUInt32(data, recordOffset);
                uint endCharCode = ReadUInt32(data, recordOffset + 4);
                uint startGlyphIndex = ReadUInt32(data, recordOffset + 8);

                for (uint codepoint = startCharCode; codepoint <= endCharCode; codepoint++)
                {
                    map[(int)codepoint] = (int)(startGlyphIndex + (codepoint - startCharCode));
                }
            }

            return map;
        }

        static Dictionary<int, int> ParseFormat4(byte[] data, int offset)
        {
            if (offset + 16 > data.Length)
            {
                return null;
            }

            ushort segCount = (ushort)(ReadUInt16(data, offset + 6) / 2);
            int endCodesOffset = offset + 14;
            int startCodesOffset = endCodesOffset + segCount * 2 + 2;
            int idDeltasOffset = startCodesOffset + segCount * 2;
            int idRangeOffsetsOffset = idDeltasOffset + segCount * 2;
            int glyphIdArrayOffset = idRangeOffsetsOffset + segCount * 2;

            if (glyphIdArrayOffset > data.Length)
            {
                return null;
            }

            Dictionary<int, int> map = new Dictionary<int, int>();

            for (int i = 0; i < segCount; i++)
            {
                ushort endCode = ReadUInt16(data, endCodesOffset + i * 2);
                ushort startCode = ReadUInt16(data, startCodesOffset + i * 2);
                short idDelta = ReadInt16(data, idDeltasOffset + i * 2);
                ushort idRangeOffset = ReadUInt16(data, idRangeOffsetsOffset + i * 2);

                if (startCode == 0xFFFF && endCode == 0xFFFF)
                {
                    break;
                }

                for (int codepoint = startCode; codepoint <= endCode; codepoint++)
                {
                    int glyphIndex;
                    if (idRangeOffset == 0)
                    {
                        glyphIndex = (codepoint + idDelta) & 0xFFFF;
                    }
                    else
                    {
                        int idRangeOffsetAddress = idRangeOffsetsOffset + i * 2;
                        int glyphIndexAddress = idRangeOffsetAddress + idRangeOffset + (codepoint - startCode) * 2;
                        if (glyphIndexAddress + 2 > data.Length)
                        {
                            continue;
                        }

                        glyphIndex = ReadUInt16(data, glyphIndexAddress);
                        if (glyphIndex != 0)
                        {
                            glyphIndex = (glyphIndex + idDelta) & 0xFFFF;
                        }
                    }

                    if (glyphIndex != 0)
                    {
                        map[codepoint] = glyphIndex;
                    }
                }
            }

            return map;
        }

        static uint FindTable(byte[] data, string tag)
        {
            ushort numTables = ReadUInt16(data, 4);
            int tableOffset = 12;
            for (int i = 0; i < numTables; i++)
            {
                int recordOffset = tableOffset + i * 16;
                if (recordOffset + 16 > data.Length)
                {
                    break;
                }

                if (ReadTag(data, recordOffset) == tag)
                {
                    return ReadUInt32(data, recordOffset + 8);
                }
            }

            return 0;
        }

        static string ReadTag(byte[] data, int offset)
        {
            return new string(new[]
            {
                (char)data[offset],
                (char)data[offset + 1],
                (char)data[offset + 2],
                (char)data[offset + 3]
            });
        }

        static ushort ReadUInt16(byte[] data, int offset)
        {
            return (ushort)((data[offset] << 8) | data[offset + 1]);
        }

        static short ReadInt16(byte[] data, int offset)
        {
            return unchecked((short)ReadUInt16(data, offset));
        }

        static uint ReadUInt32(byte[] data, int offset)
        {
            return ((uint)data[offset] << 24)
                | ((uint)data[offset + 1] << 16)
                | ((uint)data[offset + 2] << 8)
                | data[offset + 3];
        }
    }
}
