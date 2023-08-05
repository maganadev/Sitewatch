using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitewatch.OOP
{
    public class TextDiff
    {
        public DiffType type;
        public string textBefore = string.Empty;
        public string textAfter = string.Empty;

        public TextDiff(string pTextBefore, string pTextAfter)
        {
            if (pTextBefore.Length == 0 && pTextAfter.Length > 0)
            {
                type = DiffType.PureAddition;
            }
            else if(pTextBefore.Length > 0 && pTextAfter.Length == 0)
            {
                type = DiffType.PureDeletion;
            }
            else if(pTextBefore == pTextAfter)
            {
                type =DiffType.NoChange;
            }
            else
            {
                type = DiffType.Change;
            }

            textBefore = pTextBefore;
            textAfter = pTextAfter;
        }
    }

    public enum DiffType
    {
        PureAddition, PureDeletion, Change, NoChange
    }
}
