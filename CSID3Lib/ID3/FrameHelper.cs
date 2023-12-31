// Copyright(C) 2002-2009 Hugo Rumayor Montemayor, All rights reserved.
using System;
using System.IO;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using Id3Lib.Exceptions;
using Id3Lib.Frames;

namespace Id3Lib
{
    /// <summary>
    /// Manage the Parsing or Creation of binary frames.
    /// </summary>
    /// <remarks>
    /// The <b>FrameHelper</b> is a helper class that recives binary frame from a ID3v1 tag
    /// and returns the correct parsed frame or form a frame creates a binary frame that can be
    /// saved on an ID3v2 tag in a mp3 file.
    /// </remarks>
    public class FrameHelper
    {
        #region Fields
        private OptionHandler flagHandler /*= null*/;

        /// <summary>
        /// Create Frames depending on type
        /// </summary>
        /// <param name="tagHeader">ID3 Header</param>
        public FrameHelper(Header tagHeader)
        {
            flagHandler = new OptionHandler(tagHeader);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Create a frame depending on the tag form its binary representation.
        /// </summary>
        /// <param name="frameId">type of frame</param>
        /// <param name="flags">frame flags</param>
        /// <param name="frame">binary frame</param>
        /// <returns>Frame of tag type</returns>
        public IFrame Build(string frameId, ushort options, byte[] frame)
        {
            // Build a frame
            IFrame frameBase = FrameFactory.Build(frameId);
            frameBase.Flags = options;

            flagHandler.Flags = options;
            uint index = 0;
            uint size = (uint)frame.Length;
            Stream stream = new MemoryStream(frame, false);
            BinaryReader reader = new BinaryReader(stream);
            if (flagHandler.Grouping == true)
            {
                //TODO: Implement grouping when watching grass grow stops being interesting.
                // The byte read here is the group, we are skipping it for now
                reader.ReadByte(); 
                index++;
            }
            if (flagHandler.Compression == true)
            {
                switch (flagHandler.Version)
                {
                    case 3:
                        {
                            size = Swap.UInt32(reader.ReadUInt32());
                            break;
                        }
                    case 4:
                        {
                            size = Swap.UInt32(Sync.UnsafeBigEndian(reader.ReadUInt32()));
                            break;
                        }
                    default:
                        {
                            throw new NotImplementedException("ID3v2 Version " + flagHandler.Version + " is not supported.");
                        }
                }
                index = 0;
                stream = new InflaterInputStream(stream);
            }
            if (flagHandler.Encryption == true)
            {
                //TODO: Encryption
                throw new NotImplementedException("Encryption is not implemented, consequently it is not supported.");
            }
            if (flagHandler.Unsynchronisation == true)
            {
                Stream memStream = new MemoryStream();
                size = Sync.Unsafe(stream, memStream, size);
                index = 0;
                stream = memStream;
            }
            byte[] frameBuffer = new byte[size - index];
            stream.Read(frameBuffer, 0, (int)(size - index));
            frameBase.Parse(frameBuffer);
            return frameBase;
        }

        /// <summary>
        /// Build a binary data frame form the frame object.
        /// </summary>
        /// <param name="frameBase">ID3 Frame</param>
        /// <returns>binary frame representation</returns>
        public byte[] Make(IFrame frameBase)
        {
            flagHandler.Flags = frameBase.Flags;
            byte[] frame = frameBase.Make();

            MemoryStream memoryStream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(memoryStream);
            if (flagHandler.Grouping == true)
            {
                //TODO: Do grouping some place in the universe
                byte _group = 0;
                writer.Write(_group);
            }
            if (flagHandler.Compression == true)
            {
                switch (flagHandler.Version)
                {
                    case 3:
                        {
                            writer.Write(Swap.Int32(frame.Length));
                            break;
                        }
                    case 4:
                        {
                            writer.Write(Sync.UnsafeBigEndian(Swap.UInt32((uint)frame.Length)));
                            break;
                        }
                    default:
                        {
                            throw new NotImplementedException("ID3v2 Version " + flagHandler.Version + " is not supported.");
                        }
                }
                byte[] buf = new byte[2048];
                Deflater deflater = new Deflater(Deflater.BEST_COMPRESSION);
                deflater.SetInput(frame, 0, frame.Length);
                deflater.Finish();
                while (!deflater.IsNeedingInput)
                {
                    int len = deflater.Deflate(buf, 0, buf.Length);
                    if (len <= 0)
                    {
                        break;
                    }
                    memoryStream.Write(buf, 0, len);
                }

                if (!deflater.IsNeedingInput)
                {
                    //TODO: Skip and remove invalid frames.
                    throw new InvalidFrameException("Can't decompress frame '" + frameBase.FrameId + "' missing data");
                }
            }
            else
            {
                memoryStream.Write(frame, 0, frame.Length);
            }

            if (flagHandler.Encryption == true)
            {
                //TODO: Encryption
                throw new NotImplementedException("Encryption is not implemented, consequently it is not supported.");
            }

            if (flagHandler.Unsynchronisation == true)
            {
                MemoryStream synchStream = new MemoryStream();
                Sync.Unsafe(memoryStream, synchStream, (uint)memoryStream.Position);
                memoryStream = synchStream;
            }
            return memoryStream.ToArray();
        }
        #endregion
    }
}