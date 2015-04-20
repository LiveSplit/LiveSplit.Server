using Noesis.Javascript;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LiveSplit
{
    public class JavaScriptScript : IScript
    {
        private JavascriptContext context;

        public String Code { get; set; }

        public dynamic this[String name]
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

        public JavaScriptScript(String code)
        {
            context = new JavascriptContext();
            this["createObject"] = new Func<String, Dictionary<String, Object>, Object>(createObject);
            this["createArray"] = new Func<String, int, Object>(createArray);
            this["getStaticProperty"] = new Func<String, String, Object>(getStaticProperty);
            Code = code;
        }

        Object createObject(String name, Dictionary<String, Object> parameters)
        {
            var pars = parameters.Values.ToArray();
            var type = Type.GetType(name, true);
            var constructor = type.GetConstructor(pars.Select(x => x.GetType()).ToArray());
            return constructor.Invoke(pars);
        }

        Object createArray(String name, int count)
        {
            var type = Type.GetType(name, true);
            return Array.CreateInstance(type, count);
        }

        Object getStaticProperty(String name, String property)
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
