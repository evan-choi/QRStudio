using System;
using System.Text;

using QRStudio.Engine.Codec.Data;
using QRStudio.Engine.Codec.Ecc;
using QRStudio.Engine.ExceptionHandler;
using QRStudio.Engine.Geom;
using QRStudio.Engine.Codec.Reader;
using QRStudio.Engine.Codec.Util;

namespace QRStudio.Engine.Codec
{
	
	public class QRCodeDecoder
	{
        internal QRCodeSymbol qrCodeSymbol;
        internal int numTryDecode;
        internal System.Collections.ArrayList results;
        internal System.Collections.ArrayList lastResults = System.Collections.ArrayList.Synchronized(new System.Collections.ArrayList(10));
        internal static DebugCanvas canvas;
        internal QRCodeImageReader imageReader;
        internal int numLastCorrections;
        internal bool correctionSucceeded;

		public static DebugCanvas Canvas
		{
			get
			{
				return QRCodeDecoder.canvas;
			}
			
			set
			{
				QRCodeDecoder.canvas = value;
			}
			
		}
		virtual internal Point[] AdjustPoints
		{
			get
			{
				System.Collections.ArrayList adjustPoints = System.Collections.ArrayList.Synchronized(new System.Collections.ArrayList(10));
				for (int d = 0; d < 4; d++)
					adjustPoints.Add(new Point(1, 1));
				int lastX = 0, lastY = 0;
				for (int y = 0; y > - 4; y--)
				{
					for (int x = 0; x > - 4; x--)
					{
						if (x != y && ((x + y) % 2 == 0))
						{
							adjustPoints.Add(new Point(x - lastX, y - lastY));
							lastX = x;
							lastY = y;
						}
					}
				}
				Point[] adjusts = new Point[adjustPoints.Count];
				for (int i = 0; i < adjusts.Length; i++)
					adjusts[i] = (Point) adjustPoints[i];
				return adjusts;
			}			
		}		
				
		internal class DecodeResult
		{
            internal int numCorrections;
            internal bool correctionSucceeded;
            internal sbyte[] decodedBytes;
            private QRCodeDecoder enclosingInstance;

            public DecodeResult(QRCodeDecoder enclosingInstance, sbyte[] decodedBytes, int numErrors, bool correctionSucceeded)
            {
                InitBlock(enclosingInstance);
                this.decodedBytes = decodedBytes;
                this.numCorrections = numErrors;
                this.correctionSucceeded = correctionSucceeded;
            }

			private void  InitBlock(QRCodeDecoder enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}

			virtual public sbyte[] DecodedBytes
			{
				get
				{
					return decodedBytes;
				}
				
			}
			virtual public int NumErrors
			{
				get
				{
					return numCorrections;
				}
				
			}
			virtual public bool CorrectionSucceeded
			{
				get
				{
					return correctionSucceeded;
				}
				
			}
			public QRCodeDecoder Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			
		}
		
		public QRCodeDecoder()
		{
			numTryDecode = 0;
			results = System.Collections.ArrayList.Synchronized(new System.Collections.ArrayList(10));
            canvas = new DebugCanvasAdapter();
		}
		
		public virtual sbyte[] decodeBytes(QRCodeImage qrCodeImage)
		{
			Point[] adjusts = AdjustPoints;
			System.Collections.ArrayList results = System.Collections.ArrayList.Synchronized(new System.Collections.ArrayList(10));
			while (numTryDecode < adjusts.Length)
			{
				try
				{
					DecodeResult result = decode(qrCodeImage, adjusts[numTryDecode]);
					if (result.CorrectionSucceeded)
					{
						return result.DecodedBytes;
					}
					else
					{
						results.Add(result);
						canvas.println("Decoding succeeded but could not correct");
						canvas.println("all errors. Retrying..");
					}
				}
				catch (DecodingFailedException dfe)
				{
					if (dfe.Message.IndexOf("Finder Pattern") >= 0)
					throw dfe;
				}
				finally
				{
					numTryDecode += 1;
				}
			}
			
			if (results.Count == 0)
				throw new DecodingFailedException("Give up decoding");
			
			int lowestErrorIndex = - 1;
			int lowestError = System.Int32.MaxValue;
			for (int i = 0; i < results.Count; i++)
			{
				DecodeResult result = (DecodeResult) results[i];
				if (result.NumErrors < lowestError)
				{
					lowestError = result.NumErrors;
					lowestErrorIndex = i;
				}
			}
			canvas.println("All trials need for correct error");
			canvas.println("Reporting #" + (lowestErrorIndex) + " that,");
			canvas.println("corrected minimum errors (" + lowestError + ")");
			
			canvas.println("Decoding finished.");
			return ((DecodeResult) results[lowestErrorIndex]).DecodedBytes;
		}

        public virtual string decode(QRCodeImage qrCodeImage, Encoding encoding)
        {
            sbyte[] data = decodeBytes(qrCodeImage);
            byte[] byteData = new byte[data.Length];

            Buffer.BlockCopy(data, 0, byteData, 0, byteData.Length); 

            string decodedData;            
            decodedData = encoding.GetString(byteData);
            return decodedData;
        }

        public virtual string decode(QRCodeImage qrCodeImage)
        {
            sbyte[] data = decodeBytes(qrCodeImage);
            byte[] byteData = new byte[data.Length];
            Buffer.BlockCopy(data, 0, byteData, 0, byteData.Length);

            Encoding encoding;
            if (QRCodeUtility.IsUnicode(byteData))
            {
                encoding = Encoding.Unicode;
            }
            else
            {
                encoding = Encoding.ASCII;
            }
            string decodedData;
            decodedData = encoding.GetString(byteData);
            return decodedData;
        }

		internal virtual DecodeResult decode(QRCodeImage qrCodeImage, Point adjust)
		{
			try
			{
				if (numTryDecode == 0)
				{
					canvas.println("Decoding started");
					int[][] intImage = imageToIntArray(qrCodeImage);
					imageReader = new QRCodeImageReader();
					qrCodeSymbol = imageReader.getQRCodeSymbol(intImage);
				}
				else
				{
					canvas.println("--");
					canvas.println("Decoding restarted #" + (numTryDecode));
					qrCodeSymbol = imageReader.getQRCodeSymbolWithAdjustedGrid(adjust);
				}
			}
			catch (SymbolNotFoundException e)
			{
				throw new DecodingFailedException(e.Message);
			}
			canvas.println("Created QRCode symbol.");
			canvas.println("Reading symbol.");
			canvas.println("Version: " + qrCodeSymbol.VersionReference);
			canvas.println("Mask pattern: " + qrCodeSymbol.MaskPatternRefererAsString);
			
			int[] blocks = qrCodeSymbol.Blocks;
			canvas.println("Correcting data errors.");
			
			blocks = correctDataBlocks(blocks);
			try
			{
				sbyte[] decodedByteArray = getDecodedByteArray(blocks, qrCodeSymbol.Version, qrCodeSymbol.NumErrorCollectionCode);
				return new DecodeResult(this, decodedByteArray, numLastCorrections, correctionSucceeded);
			}
			catch (InvalidDataBlockException e)
			{
				canvas.println(e.Message);
				throw new DecodingFailedException(e.Message);
			}
		}
		
		
		internal virtual int[][] imageToIntArray(QRCodeImage image)
		{
			int width = image.Width;
			int height = image.Height;
			int[][] intImage = new int[width][];
			for (int i = 0; i < width; i++)
			{
				intImage[i] = new int[height];
			}
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					intImage[x][y] = image.GetPixel(x, y);
				}
			}
			return intImage;
		}
		
		internal virtual int[] correctDataBlocks(int[] blocks)
		{
			int numCorrections = 0;
			int dataCapacity = qrCodeSymbol.DataCapacity;
			int[] dataBlocks = new int[dataCapacity];
			int numErrorCollectionCode = qrCodeSymbol.NumErrorCollectionCode;
			int numRSBlocks = qrCodeSymbol.NumRSBlocks;
			int eccPerRSBlock = numErrorCollectionCode / numRSBlocks;
			if (numRSBlocks == 1)
			{
				ReedSolomon corrector = new ReedSolomon(blocks, eccPerRSBlock);
				corrector.correct();
				numCorrections += corrector.NumCorrectedErrors;
				if (numCorrections > 0)
					canvas.println(System.Convert.ToString(numCorrections) + " data errors corrected.");
				else
					canvas.println("No errors found.");
				numLastCorrections = numCorrections;
				correctionSucceeded = corrector.CorrectionSucceeded;
				return blocks;
			}
			else
			{
				int numLongerRSBlocks = dataCapacity % numRSBlocks;
				if (numLongerRSBlocks == 0)
				{
					int lengthRSBlock = dataCapacity / numRSBlocks;
					int[][] tmpArray = new int[numRSBlocks][];
					for (int i = 0; i < numRSBlocks; i++)
					{
						tmpArray[i] = new int[lengthRSBlock];
					}
					int[][] RSBlocks = tmpArray;
					for (int i = 0; i < numRSBlocks; i++)
					{
						for (int j = 0; j < lengthRSBlock; j++)
						{
							RSBlocks[i][j] = blocks[j * numRSBlocks + i];
						}
						ReedSolomon corrector = new ReedSolomon(RSBlocks[i], eccPerRSBlock);
						corrector.correct();
						numCorrections += corrector.NumCorrectedErrors;
						correctionSucceeded = corrector.CorrectionSucceeded;
					}
					int p = 0;
					for (int i = 0; i < numRSBlocks; i++)
					{
						for (int j = 0; j < lengthRSBlock - eccPerRSBlock; j++)
						{
							dataBlocks[p++] = RSBlocks[i][j];
						}
					}
				}
				else
				{
					int lengthShorterRSBlock = dataCapacity / numRSBlocks;
					int lengthLongerRSBlock = dataCapacity / numRSBlocks + 1;
					int numShorterRSBlocks = numRSBlocks - numLongerRSBlocks;
					int[][] tmpArray2 = new int[numShorterRSBlocks][];
					for (int i2 = 0; i2 < numShorterRSBlocks; i2++)
					{
						tmpArray2[i2] = new int[lengthShorterRSBlock];
					}
					int[][] shorterRSBlocks = tmpArray2;
					int[][] tmpArray3 = new int[numLongerRSBlocks][];
					for (int i3 = 0; i3 < numLongerRSBlocks; i3++)
					{
						tmpArray3[i3] = new int[lengthLongerRSBlock];
					}
					int[][] longerRSBlocks = tmpArray3;
					for (int i = 0; i < numRSBlocks; i++)
					{
						if (i < numShorterRSBlocks)
						{
							int mod = 0;
							for (int j = 0; j < lengthShorterRSBlock; j++)
							{
								if (j == lengthShorterRSBlock - eccPerRSBlock)
									mod = numLongerRSBlocks;
								shorterRSBlocks[i][j] = blocks[j * numRSBlocks + i + mod];
							}
							ReedSolomon corrector = new ReedSolomon(shorterRSBlocks[i], eccPerRSBlock);
							corrector.correct();
							numCorrections += corrector.NumCorrectedErrors;
							correctionSucceeded = corrector.CorrectionSucceeded;
						}
						else
						{
							int mod = 0;
							for (int j = 0; j < lengthLongerRSBlock; j++)
							{
								if (j == lengthShorterRSBlock - eccPerRSBlock)
									mod = numShorterRSBlocks;
								longerRSBlocks[i - numShorterRSBlocks][j] = blocks[j * numRSBlocks + i - mod];
							}
							
							ReedSolomon corrector = new ReedSolomon(longerRSBlocks[i - numShorterRSBlocks], eccPerRSBlock);
							corrector.correct();
							numCorrections += corrector.NumCorrectedErrors;
							correctionSucceeded = corrector.CorrectionSucceeded;
						}
					}
					int p = 0;
					for (int i = 0; i < numRSBlocks; i++)
					{
						if (i < numShorterRSBlocks)
						{
							for (int j = 0; j < lengthShorterRSBlock - eccPerRSBlock; j++)
							{
								dataBlocks[p++] = shorterRSBlocks[i][j];
							}
						}
						else
						{
							for (int j = 0; j < lengthLongerRSBlock - eccPerRSBlock; j++)
							{
								dataBlocks[p++] = longerRSBlocks[i - numShorterRSBlocks][j];
							}
						}
					}
				}
				if (numCorrections > 0)
					canvas.println(Convert.ToString(numCorrections) + " data errors corrected.");
				else
					canvas.println("No errors found.");
				numLastCorrections = numCorrections;
				return dataBlocks;
			}
		}
		
		internal virtual sbyte[] getDecodedByteArray(int[] blocks, int version, int numErrorCorrectionCode)
		{
			sbyte[] byteArray;
			QRCodeDataBlockReader reader = new QRCodeDataBlockReader(blocks, version, numErrorCorrectionCode);
			try
			{
				byteArray = reader.DataByte;
			}
			catch (InvalidDataBlockException e)
			{
				throw e;
			}
			return byteArray;
		}
		
		internal virtual string getDecodedString(int[] blocks, int version, int numErrorCorrectionCode)
		{
			string dataString = null;
			QRCodeDataBlockReader reader = new QRCodeDataBlockReader(blocks, version, numErrorCorrectionCode);
			try
			{
				dataString = reader.DataString;
			}
			catch (System.IndexOutOfRangeException e)
			{
				throw new InvalidDataBlockException(e.Message);
			}
			return dataString;
		}

       
	}
}