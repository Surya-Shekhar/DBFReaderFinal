using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBFReaderFinal
{
    public class DbfFieldDescriptor
    {
        public int No { get; set; }
        public string Name { get; set; }
        public char TypeChar { get; set; }
        public int Length { get; set; }
        public byte DecimalCount { get; set; }

        public string GetSqlDataType()
        {
            switch (TypeChar)
            {
                case 'C':
                    return $"VARCHAR({Length})";
                case 'I':
                    return "INT";
                case 'N':
                    return $"DECIMAL({Length + 1}, {DecimalCount})";
                case 'L':
                    return "BIT";
                case 'B':
                    return "BIT";
                case 'D':
                    return "DATETIME";
                case 'M':
                    return "VARCHAR(MAX)";
                case 'S':
                    return "VARCHAR(MAX)";
                case 'T':
                    return "DATETIME";
                case 'W': //?  
                    return "VARCHAR(MAX)";
                case '0':
                    return "INT";
                case 'G':
                    return "VARCHAR(MAX)";
                case 'F':
                    return "FLOAT";
                case 'Y':
                    return "NUMERIC(18,4)";
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
