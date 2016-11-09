using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication2
{
    class Program
    {
        static void Main(string[] args)
        {
            CCProcessor processor = new CCProcessor();
            processor.Process(@"..\..\docs\input\");

        }

        static void PerformChecks(PdfReader reader)
        {

        }
    }
}
