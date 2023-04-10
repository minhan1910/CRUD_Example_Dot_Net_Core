using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ServiceContracts.ReflectUtils
{
    public static class ReflectionUtils
    {
        public static object GetPropertyValue(object obj, string propertyName)
        {
            PropertyInfo propertyInfo = obj.GetType().GetProperty(propertyName)!;

            return propertyInfo.GetValue(obj)!;
        }

        public static bool CompareTwoObject(object obj1, object obj2)
        {
            PropertyInfo[] propertiesOfCurrentObject = obj1.GetType().GetProperties();

            foreach (PropertyInfo propertyInfo in propertiesOfCurrentObject)
            {
                var value1 = propertyInfo.GetValue(obj1);
                var value2 = propertyInfo.GetValue(obj2);

                if (value1 is null && value2 is null)
                    continue;

                if (!IsEqual(value1, value2))
                {
                    return false; // for default case like Object.Equals(value1, value2)
                }
            }

            return true;
        }

        public static bool IsEqual(object? value1, object? value2)
        {
            if (value1 is null || value2 is null)
            {
                return false;
            }

            ///  Not Clean Code
            //switch (value1)
            //{
            //    case DateTime dateTime1 when value2 is DateTime dateTime2 && dateTime1 == dateTime2:
            //    case uint uint1 when value2 is uint uint2 && uint1 == uint2:
            //    case Guid guid1 when value2 is Guid guid2 && guid1 == guid2:
            //        break;
            //    default:
            //        if (!Object.Equals(value1, value2))
            //        {
            //            return false;
            //        }
            //        break;
            //}

            // The short hand code - for handling type of object
            return value1 switch
            {
                DateTime dateTime1 when value2 is DateTime dateTime2 => dateTime1 == dateTime2,
                uint uint1 when value2 is uint uint2 => uint1 == uint2,
                int int1 when value2 is int int2 => int1 == int2,
                int double1 when value2 is int double2 => double1 == double2,
                Guid guid1 when value2 is Guid guid2 => guid1 == guid2,
                // Default Case
                _ => Object.Equals(value1, value2)
            };
        }
    }
}
