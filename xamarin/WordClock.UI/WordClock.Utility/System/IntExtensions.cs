namespace System
{
	public static class IntExtensions
	{
		public static string ToHexString(this int color)
		{
			return "#" + color.ToString("X").Substring(2);
		}
	}
}
