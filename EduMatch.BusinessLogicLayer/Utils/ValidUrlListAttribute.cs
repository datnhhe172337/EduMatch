using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace EduMatch.BusinessLogicLayer.Utils
{
    public class ValidUrlListAttribute : ValidationAttribute
    {
        private const int MaxUrlLength = 500;

        public override bool IsValid(object? value)
        {
            if (value == null)
                return true; // Optional field

            if (value is List<string> urls)
            {
                if (urls.Count == 0)
                    return false;

                // Validate each URL using UrlAttribute logic
                var urlAttribute = new UrlAttribute();
                return urls.All(url => 
                    !string.IsNullOrWhiteSpace(url) && 
                    url.Length <= MaxUrlLength &&
                    urlAttribute.IsValid(url));
            }

            return false;
        }

        public override string FormatErrorMessage(string name)
        {
            return $"{name} phải chứa các URL hợp lệ và mỗi URL không được vượt quá {MaxUrlLength} ký tự";
        }
    }
}

