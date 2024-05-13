using System;

namespace GBG.AssetChecking.Editor
{
    [Serializable]
    public class CheckResultStats : ICloneable
    {
        public int error;
        public int warning;
        public int notImportant;
        public int allPass;
        public int exception;
        public int nullResult;


        public int GetTotal(bool includeNullResult)
        {
            int total = error + warning + notImportant + allPass + exception;
            if (includeNullResult)
            {
                total += nullResult;
            }
            return total;
        }

        public void Reset()
        {
            error = 0;
            warning = 0;
            notImportant = 0;
            allPass = 0;
            exception = 0;
            nullResult = 0;
        }

        public object Clone()
        {
            return new CheckResultStats
            {
                error = error,
                warning = warning,
                notImportant = notImportant,
                allPass = allPass,
                exception = exception,
                nullResult = nullResult,
            };
        }
    }
}