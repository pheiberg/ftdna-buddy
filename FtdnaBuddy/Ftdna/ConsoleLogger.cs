using System;

namespace FtdnaBuddy.Ftdna
{
    internal class ConsoleLogger : ILogger
    {
        public void LogInfo(string text)
        {
            Console.WriteLine(text);
        }

        public void LogError(string text)
        {
            Console.Error.WriteLine(text);
        }

        public ConsoleProgressBar ShowProgress(int max)
        {
            return new ConsoleProgressBar(max);
        }
    }

	public class ConsoleProgressBar
	{
		private const char ProgressBlockCharacter = ' ';
		private readonly float _unitsOfWorkPerProgressBlock;
		private readonly bool _originalCursorVisible;

		/// <summary>
		/// Color for completed portion of progress bar.
		/// </summary>
		public ConsoleColor CompletedColor { get; private set; }

		/// <summary>
		/// Color for incomplete portion of progress bar.
		/// </summary>
		public ConsoleColor RemainingColor { get; private set; }

		/// <summary>
		/// Progress bar starting position.
		/// </summary>
		public int StartingPosition { get; private set; }

		/// <summary>
		/// Size of progress bar in characters.
		/// </summary>
		public int WidthInCharacters { get; private set; }

		/// <summary>
		/// Total amount of work. Used for calculating current percentage complete .
		/// </summary>
		public int TotalUnitsOfWork { get; private set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="totalUnitsOfWork">Total amount of work. Used for calculating current percentage complete.</param>
		/// <param name="startingPosition">Progress bar starting position. Defaults to 0.</param>
		/// <param name="widthInCharacters">Size of progress bar in characters. Defaults to 40.</param>
		/// <param name="completedColor">Color for completed portion of progress bar. Defaults to Cyan.</param>
		/// <param name="remainingColor">Color for incomplete portion of progress bar. Defaults to Black.</param>
		public ConsoleProgressBar(
			int totalUnitsOfWork,
			int startingPosition = 0,
			int widthInCharacters = 40,
			ConsoleColor completedColor = ConsoleColor.Cyan,
			ConsoleColor remainingColor = ConsoleColor.Black)
		{
			TotalUnitsOfWork = totalUnitsOfWork;
			StartingPosition = startingPosition;
			WidthInCharacters = widthInCharacters;
			CompletedColor = completedColor;
			RemainingColor = remainingColor;

			_unitsOfWorkPerProgressBlock = (float)TotalUnitsOfWork / WidthInCharacters;
		}

		/// <summary>
		/// Draws progress bar.
		/// </summary>
		/// <param name="currentUnitOfWork">Current unit of work in relation to TotalUnitsOfWork.</param>
		public void Draw(int currentUnitOfWork)
		{
			if (currentUnitOfWork > TotalUnitsOfWork)
			{
				throw new ArgumentOutOfRangeException(nameof(currentUnitOfWork), "currentUnitOfWork cant exceed TotalUnitsOfWork");
			}

			var originalBackgroundColor = Console.BackgroundColor;
			Console.CursorLeft = StartingPosition;

			try
			{
				var completeProgressBlocks = (int)Math.Round(currentUnitOfWork / _unitsOfWorkPerProgressBlock);
				WriteCompletedProgressBlocks(completeProgressBlocks);
				WriteRemainingProgressBlocks(completeProgressBlocks);

				var percentComplete = (float)currentUnitOfWork / TotalUnitsOfWork * 100;
				WriteProgressText(currentUnitOfWork, percentComplete, originalBackgroundColor);

				if (currentUnitOfWork == TotalUnitsOfWork)
				{
					Console.WriteLine();
				}
			}
			finally
			{
				Console.BackgroundColor = originalBackgroundColor;
			}
		}

		private void WriteCompletedProgressBlocks(int completeProgressBlocks)
		{
			Console.BackgroundColor = CompletedColor;
			for (var i = 0; i < completeProgressBlocks; ++i)
			{
				Console.Write(ProgressBlockCharacter);
			}
		}

		private void WriteRemainingProgressBlocks(int completeProgressBlocks)
		{
			Console.BackgroundColor = RemainingColor;
			for (var i = completeProgressBlocks; i < WidthInCharacters; ++i)
			{
				Console.Write(ProgressBlockCharacter);
			}
		}

		private void WriteProgressText(int currentUnitOfWork, float percentComplete, ConsoleColor originalBackgroundColor)
		{
			Console.BackgroundColor = originalBackgroundColor;
			Console.Write(" {0,5}% ({1} of {2})", percentComplete.ToString("n2"), currentUnitOfWork, TotalUnitsOfWork);
		}
	}
}