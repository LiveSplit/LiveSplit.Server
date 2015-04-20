using System;

namespace LiveSplit
{
    public class ScriptFactory
    {
        public static readonly String[] AllScripts = { "JavaScript", "C#" };

        public static IScript Create(String language, String code)
        {
            var lowerLanguage = language.ToLower();

            if (lowerLanguage == "javascript" || lowerLanguage == "js")
                return new JavaScriptScript(code);
            if (lowerLanguage == "c#" || lowerLanguage == "cs")
                return new CSharpScript(code);

            throw new ArgumentException("The language does not exist", "language");
        }
    }
}
