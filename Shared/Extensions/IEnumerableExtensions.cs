using System.Dynamic;
using System.Reflection;

namespace Shared.Extensions
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<ExpandoObject> ShapeData<TSource>(
            this IEnumerable<TSource> sources,
            string fields)
        {
            if (sources == null)
            {
                throw new ArgumentNullException(nameof(sources));
            }

            var expandoObjectList = new List<ExpandoObject>();

            var propertyInfoList = new List<PropertyInfo>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                var propertyInfos = typeof(TSource)
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance);

                propertyInfoList.AddRange(propertyInfos);
            }
            else
            {
                var fieldsAfterSplit = fields.Split(',');

                foreach (var field in fieldsAfterSplit)
                {
                    var propertyName = field.Trim();

                    var propertyInfos = typeof(TSource)
                        .GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                    if (propertyInfos == null)
                    {
                        throw new Exception($"Property {propertyName} wasn't found" + $"{typeof(TSource)}");
                    }
                    propertyInfoList.Add(propertyInfos);
                }
            }
            foreach (TSource sourceObject in sources)
            {
                var dataShapedObject = new ExpandoObject();
                foreach (var propertyInfo in propertyInfoList)
                {
                    var propertyValue = propertyInfo.GetValue(sourceObject);

                    ((IDictionary<string, object>)dataShapedObject).Add(propertyInfo.Name, propertyValue);
                }
                expandoObjectList.Add(dataShapedObject);
            }

            return expandoObjectList;

        }
    }
}
