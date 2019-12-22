using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace EmergencyDataExchangeProtocol.Models.helper
{
    public class EmergencyObject_DocumentFilter<T> : IDocumentFilter where T : class
    {

        public void Apply(SwaggerDocument swaggerDoc, DocumentFilterContext context)
        {

            foreach (var t in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (t.Namespace.Contains("EmergencyObjects") && t.IsClass)
                {
                    context.SchemaRegistry.GetOrRegister(t);
                }
            }

        }
    }
}
