using System;
using System.Collections.Generic;

namespace Project_1.System.Models
{
    internal static class ModelFactory
    {
        static readonly Dictionary<string, Func<Model2D>> model2DFactories = new Dictionary<string, Func<Model2D>>(StringComparer.OrdinalIgnoreCase);

        public static void RegisterModel2D(string aName, Func<Model2D> aFactory)
        {
            if (string.IsNullOrWhiteSpace(aName)) throw new ArgumentException("Model name is required.", nameof(aName));
            if (aFactory == null) throw new ArgumentNullException(nameof(aFactory));
            model2DFactories[aName] = aFactory;
        }

        public static Model2D GetModel2D(string aName)
        {
            if (string.IsNullOrWhiteSpace(aName)) return null;
            if (!model2DFactories.TryGetValue(aName, out Func<Model2D> factory)) return null;
            return factory();
        }
    }
}
