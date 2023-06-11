// Copyright(C) 2002-2009 Hugo Rumayor Montemayor, All rights reserved.
using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Drawing;
using System.Diagnostics.CodeAnalysis;
using Id3Lib.Frames;

namespace Id3Lib
{
    /// <summary>
    /// Reduce the compexity the tag model to a simple interface
    /// </summary>
    public class TagHandler
    {
        #region Fields
        private FrameModel _frameModel = null;
        private TextCode _textCode = TextCode.Ascii; // Default text code
        private string _language = "eng"; // Default language
        #endregion

        #region Properties

        /// <summary>
        /// provide access to wrapped FrameModel
        /// </summary>
        /// <remarks>
        /// it would be nice to remove this one day, and completely encapsulate a private FrameModel object
        /// </remarks>
        public FrameModel FrameModel
        {
            get { return _frameModel; }
            set { _frameModel = value; }
        }

        /// <summary>
        /// Get the title/songname/content description.
        /// TagHandler treats Title and Song as synonyms
        /// </summary>
        public string Song
        {
            get
            {
                return GetTextFrame("TIT2");
            }
            set
            {
                SetTextFrame("TIT2", value);
            }
        }

        /// <summary>
        /// Get the title/songname/content description.
        /// TagHandler treats Title and Song as synonyms
        /// </summary>
        public string Title
        {
            get
            {
                return GetTextFrame("TIT2");
            }
            set
            {
                SetTextFrame("TIT2", value);
            }
        }

        /// <summary>
        /// Get the lead performer/soloist.
        /// </summary>
        public string Artist
        {
            get
            {
                return GetTextFrame("TPE1");
            }
            set
            {
                SetTextFrame("TPE1", value);
            }
        }

        /// <summary>
        /// Get the album title.
        /// </summary>
        public string Album
        {
            get
            {
                return GetTextFrame("TALB");
            }
            set
            {
                SetTextFrame("TALB", value);
            }
        }

        /// <summary>
        /// Get the production year.
        /// </summary>
        public string Year
        {
            get
            {
                return GetTextFrame("TYER");
            }
            set
            {
                SetTextFrame("TYER", value);
            }
        }

        /// <summary>
        /// Get the composer.
        /// </summary>
        public string Composer
        {
            get
            {
                return GetTextFrame("TCOM");
            }
            set
            {
                SetTextFrame("TCOM", value);
            }
        }

        /// <summary>
        /// Get the track genre.
        /// </summary>
        /// no-one could have been using alternative spelling "Genere"
        /// as the original code returned the TALB frame anyway!
        public string Genre
        {
            get
            {
                return GetTextFrame("TCON");
            }
            set
            {
                SetTextFrame("TCON", value);
            }
        }

        /// <summary>
        /// Get the track number.
        /// </summary>
        public string Track
        {
            get
            {
                return GetTextFrame("TRCK");
            }
            set
            {
                SetTextFrame("TRCK", value);
            }
        }

        /// <summary>
        /// Get the disc number.
        /// </summary>
        /// <remarks>
        /// The 'Part of a set' frame is a numeric string that describes which
        /// part of a set the audio came from. This frame is used if the source
        /// described in the "TALB" frame is divided into several mediums, e.g. a
        /// double CD. The value MAY be extended with a "/" character and a
        /// numeric string containing the total number of parts in the set. E.g.
        /// "1/2".
        /// </remarks>
        public string Disc
        {
            get
            {
                return GetTextFrame("TPOS");
            }
            set
            {
                SetTextFrame("TPOS", value);
            }
        }

        /// <summary>
        /// Get the length.
        /// the length of the audio file in milliseconds, represented as a numeric string.
        /// </summary>
        public TimeSpan? Length
        {
            get
            {
                string strlen = GetTextFrame("TLEN");

                if (String.IsNullOrEmpty(strlen))
                    return null;

                // test for a simple number in the field
                int len;
                if (int.TryParse(strlen, out len))
                {
                    return new TimeSpan(0, 0, 0, 0, len);
                }
                return null;
            }
        }

        /// <summary>
        /// Get the original padding size.
        /// </summary>
        public uint PaddingSize
        {
            get
            {
                return _frameModel.Header.PaddingSize;
            }
        }

        /// <summary>
        /// Get the lyrics.
        /// (technically: Unsychronised lyrics/text transcription)
        /// </summary>
        public string Lyrics
        {
            get
            {
                return GetFullTextFrame("USLT");
            }
            set
            {
                SetFullTextFrame("USLT", value);
            }
        }

        /// <summary>
        /// Get the track/artist comment.
        /// </summary>
        public string Comment
        {
            get
            {
                return GetFullTextFrame("COMM");
            }
            set
            {
                SetFullTextFrame("COMM", value);
            }
        }

        /// <summary>
        /// Get/Set the associated picture as System.Drawing.Image, or null reference
        /// </summary>
        public Image Picture
        {
            get
            {
                IFrame frame = FindFrame("APIC");
                if (frame != null)
                {
                    FramePicture framePic = frame as FramePicture;
                    return framePic.Picture;
                }
                return null;
            }
            set
            {
                IFrame frame = FindFrame("APIC");
                if (frame != null)
                {
                    if (value != null)
                    {
                        FramePicture framePic = frame as FramePicture;
                        framePic.Picture = value;
                    }
                    else
                    {
                        _frameModel.Remove(frame);
                    }
                }
                else
                {
                    if (value != null)
                    {
                        FramePicture framePic = FrameFactory.Build("APIC") as FramePicture;
                        framePic.Picture = value;
                        _frameModel.Add(framePic);
                    }
                }
            }
        }

        /// <summary>
        /// Get/Set the associated picture as byte[], or null reference
        /// </summary>
        public byte[] PictureData
        {
            get
            {
                IFrame frame = FindFrame("APIC");
                if (frame != null)
                {
                    FramePicture framePic = frame as FramePicture;
                    return framePic.PictureData;
                }
                return null;
            }
            set
            {
                IFrame frame = FindFrame("APIC");
                if (frame != null)
                {
                    if (value != null)
                    {
                        FramePicture framePic = frame as FramePicture;
                        framePic.PictureData = value;
                    }
                    else
                    {
                        _frameModel.Remove(frame);
                    }
                }
                else
                {
                    if (value != null)
                    {
                        FramePicture framePic = FrameFactory.Build("APIC") as FramePicture;
                        framePic.PictureData = value;
                        _frameModel.Add(framePic);
                    }
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// calculate crc of the tag data
        /// </summary>
        /// <returns>CRC-32 of tag data block</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Crc")]
        public int CalculateCrc()
        {
            return _frameModel.CalculateCrc();
        }

        /// <summary>
        /// Set the frame text
        /// </summary>
        /// <param name="frameId">Frame type</param>
        /// <param name="message">Value set in frame</param>
        private void SetTextFrame(string frameId, string message)
        {
            IFrame frame = FindFrame(frameId);
            if (frame != null)
            {
                if (!String.IsNullOrEmpty(message))
                {
                    ((FrameText)frame).Text = message;
                }
                else
                {
                    _frameModel.Remove(frame);
                }
            }
            else
            {
                if (!String.IsNullOrEmpty(message))
                {
                    FrameText frameText = (FrameText)FrameFactory.Build(frameId);
                    frameText.Text = message;
                    frameText.TextCode = _textCode;
                    _frameModel.Add(frameText);
                }
            }
        }

        /// <summary>
        /// Get the frame text
        /// </summary>
        /// <param name="frameId">Frame type</param>
        /// <returns>Frame text</returns>
        private string GetTextFrame(string frameId)
        {
            IFrame frame = FindFrame(frameId);
            if (frame != null)
            {
                return ((FrameText)frame).Text;
            }
            return string.Empty;
        }

        /// <summary>
        /// Set the frame full text
        /// </summary>
        /// <param name="frameId">Frame type</param>
        /// <param name="message">Value set in frame</param>
        private void SetFullTextFrame(string frameId, string message)
        {
            IFrame frame = FindFrame(frameId);
            if (frame != null)
            {
                if (!String.IsNullOrEmpty(message))
                {
                    FrameFullText framefulltext = (FrameFullText)frame;
                    framefulltext.Text = message;
                    framefulltext.TextCode = _textCode;
                    framefulltext.Description = string.Empty;
                    framefulltext.Language = _language;
                }
                else
                {
                    _frameModel.Remove(frame);
                }
            }
            else
            {
                if (!String.IsNullOrEmpty(message))
                {
                    FrameFullText frameLCText = (FrameFullText)FrameFactory.Build(frameId);
                    frameLCText.TextCode = this._textCode;
                    frameLCText.Language = "eng";
                    frameLCText.Description = string.Empty;
                    frameLCText.Text = message;
                    _frameModel.Add(frameLCText);
                }
            }
        }

        /// <summary>
        /// Get a full text frame value
        /// </summary>
        /// <param name="frameId">Frame type</param>
        /// <returns>Frame text</returns>
        private string GetFullTextFrame(string frameId)
        {
            IFrame frame = FindFrame(frameId);
            if (frame != null)
            {
                return ((FrameFullText)frame).Text;
            }
            return string.Empty;
        }

        /// <summary>
        /// Find a frame in the model
        /// </summary>
        /// <param name="frameId">Frame type</param>
        /// <returns>The found frame if found, otherwise null</returns>
        private IFrame FindFrame(string frameId)
        {
            foreach (IFrame frame in _frameModel)
            {
                if (frame.FrameId == frameId)
                {
                    return frame;
                }
            }
            return null;
        }
        #endregion

        #region Constructors

        /// <summary>
        /// Attach to the TagModel
        /// </summary>
        /// <param name="frameModel">Frame model to handle</param>
        public TagHandler(FrameModel frameModel)
        {
            _frameModel = frameModel;
        }
        #endregion
    }
}

