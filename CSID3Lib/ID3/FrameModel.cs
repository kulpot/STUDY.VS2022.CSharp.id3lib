// Copyright(C) 2002-2009 Hugo Rumayor Montemayor, All rights reserved.
using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics.CodeAnalysis;
using System.Collections.ObjectModel;
using Id3Lib.Frames;

namespace Id3Lib
{
    /// <summary>
    /// Manages an ID3v2 tag as an object reprsentation. 
    /// </summary>
    /// <remarks>
    /// The <b>FrameModel</b> class represents a ID3v2 tag, it contains a <see cref="Header"/> that
    /// handles the tag header, an <see cref="ExtendedHeader"/> that it is optional and 
    /// stores the frames.
    /// </remarks>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "It is more than a collection as it has extra behavior")]
    public class FrameModel : Collection<IFrame>
    {
        #region Fields
        private Header _tagHeader = new Header();
        private ExtendedHeader _tagExtendedHeader = new ExtendedHeader();
        #endregion

        #region Properties

        /// <summary>
        /// id3v2 tags can not have "no frames"
        /// </summary>
        public bool IsValid
        {
            get { return Count > 0; }
        }
        /// <summary>
        /// Get or set the header.
        /// </summary>
        public Header Header
        {
            get { return _tagHeader; }
        }

        /// <summary>
        /// Get or set extended header.
        /// </summary>
        public ExtendedHeader ExtendedHeader
        {
            get { return _tagExtendedHeader; }
        }

        protected override void InsertItem(int index, IFrame item)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            if (item.FrameId == null || item.FrameId.Length != 4)
                throw new InvalidOperationException("The frame id is invalid");
            base.InsertItem(index, item);
        }

        protected override void SetItem(int index, IFrame item)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            if (item.FrameId == null || item.FrameId.Length != 4)
                throw new InvalidOperationException("The frame id is invalid");
            base.SetItem(index, item);
        }

        /// <summary>
        /// Add a range of frames
        /// </summary>
        /// <param name="frames">the frames to add</param>
        public void AddRange(IEnumerable<IFrame> frames)
        {
            if (frames == null)
                throw new ArgumentNullException("frames");

            //add each frame to the collection
            foreach (var frame in frames)
                Add(frame);
        }

        /// <summary>
        /// predict the size of the frames on disk (without any padding)
        /// by streaming the tag to a dummy stream, which updates the stored size.
        /// </summary>
        /// <remarks>
        /// Although the the padding is streamed out, 
        /// the size isn't added on to Header.TagSize
        /// </remarks>
        public void UpdateSize()
        {
            if (!IsValid)
                Header.TagSize = 0; // clear the TagSize stored in the tagModel

            // TODO: there must be a better way of obtaining this!!
            using (Stream stream = new MemoryStream())
            {
                FrameManager.Serialize(this, stream);
            }
        }

        /// <summary>
        /// calculate the CRC-32 of the frames
        /// by streaming them to a buffer and checksumming that.
        /// </summary>
        /// <remarks>
        /// this doesn't checksum the padding area, 
        /// as rewriting the file with different padding changes 
        /// neither the body of the tag, nor the body of the audio.
        /// </remarks>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Crc")]
        public int CalculateCrc()
        {
            if (!IsValid)
                return 0;

            using (MemoryStream stream = new MemoryStream())
            {
                // serialise tags to a buffer, without any padding
                FrameManager.Serialize(this, stream);

                // obtain whole buffer as byte array
                byte[] buffer = stream.ToArray();

                // calculate the size of the tag without the padding
                int tagsize = (int)buffer.Length - (int)Header.PaddingSize;

                // create and calculate crc-32 for the tag
                ICSharpCode.SharpZipLib.Checksums.Crc32 crc = new ICSharpCode.SharpZipLib.Checksums.Crc32();
                crc.Update(buffer, 0, tagsize);
                return (int)crc.Value;
            }
        }

        #endregion
    }
}
