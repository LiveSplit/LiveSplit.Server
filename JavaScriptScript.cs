using Noesis.Javascript;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LiveSplit
{
    public class JavaScriptScript : IScript
    {
        private JavascriptContext context;

        public string Code { get; set; }

        public dynamic this[string name]
        {
            get
            {
                return context.GetParameter(name);
            }
            set
            {
                context.SetParameter(name, value);
            }
        }

        public JavaScriptScript(string code)
        {
            context = new JavascriptContext();
            this["createObject"] = new Func<string, Dictionary<string, Object>, Object>(createObject);
            this["createArray"] = new Func<string, int, Object>(createArray);
            this["getStaticProperty"] = new Func<string, string, Object>(getStaticProperty);
            Code = code;
        }

        Object createObject(string name, Dictionary<string, Object> parameters)
        {
            var pars = parameters.Values.ToArray();
            var type = Type.GetType(name, true);
            var constructor = type.GetConstructor(pars.Select(x => x.GetType()).ToArray());
            return constructor.Invoke(pars);
        }

        Object createArray(string name, int count)
        {
            var type = Type.GetType(name, true);
            return Array.CreateInstance(type, count);
        }

        Object getStaticProperty(string name, string property)
        {
            var type = Type.GetType(name, true);
            return type.GetProperty(property).GetValue(null, null);
        }

        public dynamic Run()
        {
            return context.Run(Code);
        }
    }
}
