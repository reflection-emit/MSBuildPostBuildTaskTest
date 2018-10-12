namespace PostBuildFileModder
{
    internal static class Extensions
    {
        public static string EnclosedIn(this string target, string start = "(", string end = ")")
        {
            if (string.IsNullOrEmpty(target))
                return target;

            int startPos = target.IndexOf(start) + start.Length;

            if (startPos < 0)
                return target;

            int endPos = target.IndexOf(end, startPos);

            if (endPos <= startPos)
                endPos = target.Length;

            return target.Substring(startPos, endPos - startPos);
        }
    }
}