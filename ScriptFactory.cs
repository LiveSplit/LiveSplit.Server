using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveSplit
{
    public class ScriptFactory
    {
        public static readonly String[] AllScripts = new String[] { "JavaScript", "C#" };

        public static IScript Create(String language, String code)
        {
            var lowerLanguage = language.ToLower();

            if (lowerLanguage == "javascript" || lowerLanguage == "js")
                return new JavaScriptScript(code);
            else if (lowerLanguage == "c#" || lowerLanguage == "cs")
                return new CSharpScript(code);

            throw new ArgumentException("The language does not exist", "language");
        }
    }
}
