// AForge Video for Windows Library
// AForge.NET framework
// http://www.aforgenet.com/framework/
//
// Copyright © Andrew Kirillov, 2007-2009
// andrew.kirillov@aforgenet.com
// 
//
using AForge.Video;

namespace iSpyApplication
{
	using System;
	using System.Drawing;
	using System.Drawing.Imaging;
	using System.Runtime.InteropServices;
    using AForge.Video.VFW;

	/// <summary>
    /// AVI files writing using Video for Windows interface.
	/// </summary>
    /// 
    /// <remarks><para>The class allows to write AVI files using Video for Windows API.</para>
    /// 
    /// <para>Sample usage:</para>
    /// <code>
    /// // instantiate AVI writer, use WMV3 codec
    /// AVIWriter writer = new AVIWriter( "wmv3" );
    /// // create new AVI file and open it
    /// writer.Open( "test.avi", 320, 240 );
    /// // create frame image
    /// Bitmap image = new Bitmap( 320, 240 );
    /// 
    /// for ( int i = 0; i &lt; 240; i++ )
    /// {
    ///     // update image
    ///     image.SetPixel( i, i, Color.Red );
    ///     // add the image as a new frame of video file
    ///     writer.AddFrame( image );
    /// }
    /// writer.Close( );
    /// </code>
    /// </remarks>
    /// 
	public class AVIWriter : IDisposable
	{
        // AVI file
        private IntPtr _file;
        // video stream
        private IntPtr _stream;
        // compressed stream
		private IntPtr _streamCompressed;
        // buffer
		private IntPtr _buffer = IntPtr.Zero;

        // width of video frames
        private int _width;
        // height of vide frames
        private int _height;
        // length of one line
		private int _stride;
        // quality
		private int _quality = -1;
        // frame rate
		private int _rate = 25;
        // current position
		private int _position;
        // codec used for video compression
        private string _codec = "DIB ";

        // dummy object to lock for synchronization
        private readonly object _sync = new object();

        /// <summary>
        /// Width of video frames.
        /// </summary>
        /// 
        /// <remarks><para>The property specifies the width of video frames, which are acceptable
        /// by <see cref="AddFrame"/> method for saving, which is set in <see cref="Open"/>
        /// method.</para></remarks>
        /// 
        public int Width
        {
            get { return ( _buffer != IntPtr.Zero ) ? _width : 0; }
        }

        /// <summary>
        /// Height of video frames.
        /// </summary>
        /// 
        /// <remarks><para>The property specifies the height of video frames, which are acceptable
        /// by <see cref="AddFrame"/> method for saving, which is set in <see cref="Open"/>
        /// method.</para></remarks>
        /// 
        public int Height
        {
            get { return ( _buffer != IntPtr.Zero ) ? _height : 0; }
        }

        /// <summary>
        /// Current position in video stream.
        /// </summary>
        /// 
        /// <remarks><para>The property tell current position in video stream, which actually equals
        /// to the amount of frames added using <see cref="AddFrame"/> method.</para></remarks>
        /// 
        public int Position
		{
			get { return _position; }
		}

        /// <summary>
        /// Desired playing frame rate.
        /// </summary>
        /// 
        /// <remarks><para>The property sets the video frame rate, which should be use during playing
        /// of the video to be saved.</para>
        /// 
        /// <para><note>The property should be set befor opening new file to take effect.</note></para>
        /// 
        /// <para>Default frame rate is set to <b>25</b>.</para></remarks>
        /// 
        public int FrameRate
        {
            get { return _rate; }
            set { _rate = value; }
        }

        /// <summary>
        /// Codec used for video compression.
        /// </summary>
        /// 
        /// <remarks><para>The property sets the FOURCC code of video compression codec, which needs to
        /// be used for video encoding.</para>
        /// 
        /// <para><note>The property should be set befor opening new file to take effect.</note></para>
        /// 
        /// <para>Default video codec is set <b>"DIB "</b>, which means no compression.</para></remarks>
        /// 
        public string Codec
		{
			get { return _codec; }
			set { _codec = value; }
		}

        /// <summary>
        /// Compression video quality.
        /// </summary>
        /// 
        /// <remarks><para>The property sets video quality used by codec in order to balance compression rate
        /// and image quality. The quality is measured usually in the [0, 100] range.</para>
        /// 
        /// <para><note>The property should be set befor opening new file to take effect.</note></para>
        /// 
        /// <para>Default value is set to <b>-1</b> - default compression quality of the codec.</para></remarks>
        /// 
		public int Quality
		{
			get { return _quality; }
			set { _quality = value; }
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="AVIWriter"/> class.
        /// </summary>
        /// 
        /// <remarks>Initializes Video for Windows library.</remarks>
        /// 
        public AVIWriter( )
		{
			Win32.AVIFileInit( );
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="AVIWriter"/> class.
        /// </summary>
        /// 
        /// <param name="codec">Codec to use for compression.</param>
        /// 
        /// <remarks>Initializes Video for Windows library.</remarks>
        /// 
        public AVIWriter( string codec ) : this( )
		{
			_codec = codec;
		}

        /// <summary>
        /// Destroys the instance of the <see cref="AVIWriter"/> class.
        /// </summary>
        /// 
        ~AVIWriter( )
		{
			Dispose( false );
		}

        /// <summary>
        /// Dispose the object.
        /// </summary>
        /// 
        /// <remarks>Frees unmanaged resources used by the object. The object becomes unusable
        /// after that.</remarks>
        /// 
        public void Dispose( )
		{
			Dispose( true );
			// remove me from the Finalization queue 
			GC.SuppressFinalize( this );
		}

        /// <summary>
        /// Dispose the object.
        /// </summary>
        /// 
        /// <param name="disposing">Indicates if disposing was initiated manually.</param>
        /// 
        protected virtual void Dispose( bool disposing )
		{
			if ( disposing )
			{
				// dispose managed resources
			}
            // close current AVI file if any opened and uninitialize AVI library
            Close( );
			Win32.AVIFileExit( );
		}

        /// <summary>
        /// Create new AVI file and open it for writing. (MODIFIED FOR ISPY TO SUPPORT CODEC CHANGES)
        /// </summary>
        /// 
        /// <param name="fileName">AVI file name to create.</param>
        /// <param name="width">Video width.</param>
        /// <param name="height">Video height.</param>
        /// <param name="editCodec">Prompt for codec.</param>
        /// <param name="compressorOptions">Compressor configuration.</param>
        /// 
        /// <remarks><para>The method opens (creates) a video files, configure video codec and prepares
        /// the stream for saving video frames with a help of <see cref="AddFrame"/> method.</para></remarks>
        /// 
        /// <exception cref="ApplicationException">Failure of opening video files (the exception message
        /// specifies the issues).</exception>
        /// 
        public void Open( string fileName, int width, int height, bool editCodec, ref string[] compressorOptions)
		{

            // close previous file
            Close();
            if (((width & 1) != 0) || ((height & 1) != 0))
            {
                throw new ArgumentException("Video file resolution must be a multiple of two.");
            }
            bool success = false;
            try
            {
                lock (_sync)
                {
                    // calculate stride
                    _stride = width*3;
                    if ((_stride%4) != 0)
                        _stride += (4 - _stride%4);

                    // create new file
                    if (
                        Win32.AVIFileOpen(out _file, fileName, Win32.OpenFileMode.Create | Win32.OpenFileMode.Write,
                                          IntPtr.Zero) != 0)
                        throw new System.IO.IOException("Failed opening the specified file.");

                    _width = width;
                    _height = height;

                    // describe new stream
                    var info = new Win32.AVISTREAMINFO
                                   {
                                       type = Win32.mmioFOURCC("vids"),
                                       handler = Win32.mmioFOURCC(compressorOptions[4]),
                                       scale = 1,
                                       rate = _rate,
                                       suggestedBufferSize = _stride*height
                                   };

                    // create stream
                    if (Win32.AVIFileCreateStream(_file, out _stream, ref info) != 0)
                        throw new VideoException("Failed creating stream.");

                    // describe compression options
                    var options = new Win32.AVICOMPRESSOPTIONS
                                      {
                                          bytesPerSecond = Convert.ToInt32(compressorOptions[0]),
                                          flags = Convert.ToInt32(compressorOptions[1]),
                                          format = Convert.ToInt32(compressorOptions[2]),
                                          formatSize = Convert.ToInt32(compressorOptions[3]),
                                          handler = Win32.mmioFOURCC(compressorOptions[4]),
                                          interleaveEvery = Convert.ToInt32(compressorOptions[5]),
                                          keyFrameEvery = Convert.ToInt32(compressorOptions[6]),
                                          parameters = Convert.ToInt32(compressorOptions[7]),
                                          parametersSize = Convert.ToInt32(compressorOptions[8]),
                                          quality = Convert.ToInt32(compressorOptions[9]),
                                          type = Convert.ToInt32(compressorOptions[10])

                                      };

                    // uncomment if video settings dialog is required to show
                    if (editCodec)
                    {
                        Win32.AVISaveOptions(_stream, ref options);
                        compressorOptions[0] = options.bytesPerSecond.ToString();
                        compressorOptions[1] = options.flags.ToString();
                        compressorOptions[2] = options.format.ToString();
                        compressorOptions[3] = options.formatSize.ToString();
                        compressorOptions[4] = Win32.decode_mmioFOURCC(options.handler);
                        compressorOptions[5] = options.interleaveEvery.ToString();
                        compressorOptions[6] = options.keyFrameEvery.ToString();
                        compressorOptions[7] = options.parameters.ToString();
                        compressorOptions[8] = options.parametersSize.ToString();
                        compressorOptions[9] = options.quality.ToString();
                        compressorOptions[10] = options.type.ToString();
                    }

                    // create compressed stream
                    if (Win32.AVIMakeCompressedStream(out _streamCompressed, _stream, ref options, IntPtr.Zero) != 0)
                        throw new VideoException("Failed creating compressed stream.");

                    // describe frame format
                    var bitmapInfoHeader = new Win32.BITMAPINFOHEADER();

                    bitmapInfoHeader.size = Marshal.SizeOf(bitmapInfoHeader.GetType());
                    bitmapInfoHeader.width = width;
                    bitmapInfoHeader.height = height;
                    bitmapInfoHeader.planes = 1;
                    bitmapInfoHeader.bitCount = 24;
                    bitmapInfoHeader.sizeImage = 0;
                    bitmapInfoHeader.compression = 0; // BI_RGB

                    // set frame format
                    if (
                        Win32.AVIStreamSetFormat(_streamCompressed, 0, ref bitmapInfoHeader,
                                                 Marshal.SizeOf(bitmapInfoHeader.GetType())) != 0)
                        throw new VideoException("Failed setting format of the compressed stream.");

                    // alloc unmanaged memory for frame
                    _buffer = Marshal.AllocHGlobal(_stride*height);

                    if (_buffer == IntPtr.Zero)
                        throw new OutOfMemoryException("Insufficient memory for internal buffer.");

                    _position = 0;
                    success = true;
                }
            }
            finally
            {
                if (!success)
                {
                    Close();
                }
            }
		}

        /// <summary>
        /// Close video file.
        /// </summary>
        /// 
        public void Close( )
		{
            lock (_sync)
            {
                // free unmanaged memory
                if (_buffer != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(_buffer);
                    _buffer = IntPtr.Zero;
                }

                // release compressed stream
                if (_streamCompressed != IntPtr.Zero)
                {
                    Win32.AVIStreamRelease(_streamCompressed);
                    _streamCompressed = IntPtr.Zero;
                }

                // release stream
                if (_stream != IntPtr.Zero)
                {
                    Win32.AVIStreamRelease(_stream);
                    _stream = IntPtr.Zero;
                }

                // release file
                if (_file != IntPtr.Zero)
                {
                    Win32.AVIFileRelease(_file);
                    _file = IntPtr.Zero;
                }
            }
		}

        /// <summary>
        /// Add new frame to the AVI file.
        /// </summary>
        /// 
        /// <param name="frameImage">New frame image.</param>
        /// 
        /// <remarks><para>The method adds new video frame to an opened video file. The width and heights
        /// of the frame should be the same as it was specified in <see cref="Open"/> method
        /// (see <see cref="Width"/> and <see cref="Height"/> properties).</para></remarks>
        /// 
        /// <exception cref="ApplicationException">Failure of opening video files (the exception message
        /// specifies the issues).</exception>
        /// 
        public void AddFrame( Bitmap frameImage )
		{
            lock (_sync)
            {
                // check if AVI file was properly opened
                if (_buffer == IntPtr.Zero)
                    throw new System.IO.IOException("AVI file should be successfully opened before writing.");

                // check image dimension
                if ((frameImage.Width != _width) || (frameImage.Height != _height))
                    throw new ArgumentException("Bitmap size must be of the same as video size, which was specified on opening video file.");

                // lock bitmap data
                BitmapData imageData = frameImage.LockBits(
                    new Rectangle(0, 0, _width, _height),
                    ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

                // copy image data
                int srcStride = imageData.Stride;
                int dstStride = _stride;

                int src = imageData.Scan0.ToInt32() + srcStride * (_height - 1);
                int dst = _buffer.ToInt32();

                for (int y = 0; y < _height; y++)
                {
                    Win32.memcpy(dst, src, dstStride);
                    dst += dstStride;
                    src -= srcStride;
                }

                // unlock bitmap data
                frameImage.UnlockBits(imageData);

                // write to stream
                if (Win32.AVIStreamWrite(_streamCompressed, _position, 1, _buffer,
                    _stride * _height, 0, IntPtr.Zero, IntPtr.Zero) != 0)
                    throw new VideoException("Failed adding frame.");

                _position++;
            }
		}
	}
}
