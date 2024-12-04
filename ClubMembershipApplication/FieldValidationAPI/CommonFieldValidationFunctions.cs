using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FieldValidationAPI
{

    public delegate bool RequiredValidDel(string fieldVal);
    public delegate bool StringLengthValidDel(string fieldVal, int min, int max);
    public delegate bool DateValidDel(string dateTime, out DateTime validDateTime);
    public delegate bool PatternMatchingDel(string fieldVal, string pattern);
    public delegate bool CompareFieldsValidDel(string fieldVal, string fieldValCompare);

    public class CommonFieldValidationFunctions
    {
        private static RequiredValidDel _requiredValidDel = null;
        private static StringLengthValidDel _stringLengthValidDel = null;
        private static DateValidDel _dateValidDel = null;
        private static PatternMatchingDel _patternMatchingDel = null;
        private static CompareFieldsValidDel _compareFieldsValidDel = null;

        public static RequiredValidDel RequiredFieldValidDel
        {
            get
            {
                if (_requiredValidDel == null)
                {
                    _requiredValidDel = new RequiredValidDel(RequiredFieldValid);
                }
                return _requiredValidDel;
            }
        }

        public static StringLengthValidDel StringLengthFieldValid
        {
            get
            {
                if (_stringLengthValidDel == null)
                {
                    _stringLengthValidDel = new StringLengthValidDel(StringFieldLengthValid);
                }
                return _stringLengthValidDel;
            }
        }

        public static DateValidDel DateValidField
        {
            get
            {
                if (_dateValidDel == null)
                {
                    _dateValidDel = new DateValidDel(DateFieldValid);
                }
                return _dateValidDel;
            }
        }

        public static PatternMatchingDel PatternMatchingFieldDel
        {
            get
            {
                if (_patternMatchingDel == null)
                {
                    _patternMatchingDel = new PatternMatchingDel(FieldPatternValid);
                }
                return _patternMatchingDel;
            }
        }

        public static CompareFieldsValidDel FieldsCompareValidDel
        {
            get
            {
                if (_compareFieldsValidDel == null)
                {
                    _compareFieldsValidDel = new CompareFieldsValidDel(FieldComparisonValid);
                }
                return _compareFieldsValidDel;
            }
        }

        private static bool RequiredFieldValid(string fieldVal)
        {
            return !string.IsNullOrEmpty(fieldVal);
        }

        private static bool StringFieldLengthValid(string fieldVal, int min, int max)
        {
            return (fieldVal.Length >= min && fieldVal.Length <= max);
        }

        private static bool DateFieldValid(string dateTime, out DateTime validDateTime)
        {
            return DateTime.TryParse(dateTime, out validDateTime);
        }

        private static bool FieldPatternValid(string fieldVal, string patterMatchingExpr)
        {
            Regex reg = new Regex(patterMatchingExpr);
            return reg.IsMatch(fieldVal);
        }

        private static bool FieldComparisonValid(string field1, string field2)
        {
            return field1.Equals(field2);
        }

    }
}
