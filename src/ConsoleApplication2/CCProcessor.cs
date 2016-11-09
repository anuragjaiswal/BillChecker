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
    class CCProcessor
    {
        private List<MonthExpenditures> _months = new List<MonthExpenditures>();
        private List<LineItem> _yearly = new List<LineItem>();
        private Dictionary<String, Merchant> _merchantDictionary = new Dictionary<String, Merchant>();


        private String _password = @"ANUR100784";
        private String _startMark = @"PREVIOUS BALANCE ";
        private String _endMark = @"To be continued";
        private String _alternateEndMark = @"NEW BALANCE ";

        public void Process(String directoryName)
        {
            LoadTypes(directoryName + @"..\Types\");
            LoadData(directoryName);

            

            WriteFile(directoryName + @"..\");

        }

        private void LoadTypes(string directoryName)
        {
            var fileNames = Directory.GetFiles(directoryName);
            var fileName = fileNames[0];
            var reader = new StreamReader(File.OpenRead(fileName));
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');
                String merchantName = values[0];

                if (!_merchantDictionary.ContainsKey(merchantName))
                {
                    MerchantType type = MerchantType.UNCLASSIFIED;
                    String remark = String.Empty;
                    switch (values.Length)
                    {
                        case 2:
                            if (!String.IsNullOrWhiteSpace(values[1]))
                            {
                                type = (MerchantType)Enum.Parse(typeof(MerchantType), values[1]);
                            }
                            break;
                        case 3:
                            if (!String.IsNullOrWhiteSpace(values[1]))
                            {
                                type = (MerchantType)Enum.Parse(typeof(MerchantType), values[1]);
                            }
                            remark = values[2];
                            break;
                        default:
                            break;
                    }

                    Merchant merchant = new Merchant { Name = merchantName, Type = type, Remark = remark };
                    _merchantDictionary.Add(merchantName, merchant);
                }
            }
        }


        private void LoadData(String directoryName)
        {
            var fileNames = Directory.GetFiles(directoryName);

            Decimal previousBalance = 0;
            foreach (var fileName in fileNames)
            {
                if (String.IsNullOrWhiteSpace(fileName))
                {
                    continue;
                }
                var monthStr = fileName.Substring(fileName.IndexOf('_') + 1, fileName.Length - (fileName.IndexOf('_') + 1));
                monthStr = monthStr.Substring(0, monthStr.IndexOf('_'));

                MonthExpenditures month = new MonthExpenditures();
                month.Month = DateTime.ParseExact(monthStr, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                PdfReader reader = new PdfReader(fileName, new System.Text.ASCIIEncoding().GetBytes(_password));
                for (int page = 1; page <= reader.NumberOfPages; page++)
                {
                    var pageText = PdfTextExtractor.GetTextFromPage(reader, page);

                    int startIndex = pageText.IndexOf(_startMark);
                    if (startIndex < 0)
                    {
                        continue;
                    }
                    startIndex = pageText.IndexOf(_startMark) + _startMark.Length;

                    int endIndex = pageText.IndexOf(_endMark);
                    if (endIndex < 1)
                    {
                        endIndex = pageText.IndexOf(_alternateEndMark);
                    }
                    if (endIndex > 1 && startIndex > 1)
                    {
                        String spends = pageText.Substring(startIndex, endIndex - startIndex);
                        String[] lines = spends.Split('\n');
                        previousBalance = Decimal.Parse(lines[0]);
                        for (int i = 1; i < lines.Length; i++)
                        {
                            bool isCredit = false;
                            var line = lines[i];
                            if (!String.IsNullOrWhiteSpace(line))
                            {
                                LineItem item = new LineItem();
                                DateTime date;
                                if (!DateTime.TryParseExact(line.Substring(0, line.IndexOf(' ')), "ddMMyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                                {
                                    continue;
                                }
                                item.Date = date;

                                var name = line.Substring(line.IndexOf(' '), 26 - line.IndexOf(' ') + 1);
                                if (name.IndexOf(',') > 0)
                                {
                                    name = name.Remove(name.IndexOf(','), 1);
                                }

                                if(_merchantDictionary.ContainsKey(name))
                                {
                                    item.Merchant = _merchantDictionary[name];
                                }
                                else
                                {
                                    item.Merchant = new Merchant { Name = name, Type = MerchantType.UNCLASSIFIED, Remark = "NEW IN" };
                                }                                

                                int amountIndex = lines[i].LastIndexOf(' ');
                                var amountStr = line.Substring(amountIndex, line.Length - amountIndex);
                                if (amountStr.LastIndexOf('C') > 0)
                                {
                                    amountStr = amountStr.Substring(0, amountStr.LastIndexOf('C'));
                                    isCredit = true;
                                }
                                item.Amount = Decimal.Parse(amountStr);
                                item.Amount = isCredit ? -1 * item.Amount : item.Amount;
                                month.Items.Add(item);
                                _yearly.Add(item);
                            }
                        }
                    }

                }
                var prevBalance = new Merchant { Name = "PREVIOUS BALANCE", Type = MerchantType.PREVIOUSBALANCE, Remark = "" };
                var prevBal = new LineItem { Amount = previousBalance, Merchant = prevBalance };
                month.Items.Add(prevBal);
                _yearly.Add(prevBal);
                reader.Close();

                month.TotalAmount = month.Items.Sum((a) => a.Amount);

                Console.WriteLine("Month " + month.Month.Month + " " + month.Month.Year + " Total Amount: " + month.TotalAmount);
                _months.Add(month);
            }
        }

        private void WriteFile(String directoryName)
        {
            StringBuilder csv = new StringBuilder();

            StringBuilder csvTypes = new StringBuilder();

            csv.AppendLine("Total Expenditure, " + _months.Sum((a) => a.TotalAmount));

            csv.AppendLine("Month, Total Amount");

            foreach (var month in _months)
            {
                var newLine = string.Format("{0},{1}", month.Month.Month + " " + month.Month.Year, month.TotalAmount);
                csv.AppendLine(newLine);
            }

            csv.AppendLine(" ");
            foreach (var month in _months)
            {
                csv.AppendLine("Month, " + month.Month.Month + " " + month.Month.Year);
                csv.AppendLine("Expenditure");
                csv.AppendLine("Merchant, Amount");
                foreach (var i in month.Items)
                {
                    var iLine = string.Format("{0},{1}", i.Merchant.Name, i.Amount);
                    csv.AppendLine(iLine);
                }
            }

            

            var resultsByName = from i in _yearly
                          group i by i.Merchant.Name
                          into g
                              select new {
                                  MerchantName = g.Key, 
                                  Expenses = g.ToList(), 
                                  Count = g.ToList().Count, 
                                  Sum = g.ToList().Sum((a) => a.Amount)
                              };

            resultsByName = resultsByName.OrderByDescending(r => r.Sum);

            csvTypes.AppendLine("Name, Count, Total Spent");
            foreach (var t in resultsByName)
            {
                var line = string.Format("{0}, {1}, {2}", t.MerchantName, t.Count, (Int64)t.Sum);
                csvTypes.AppendLine(line);
            }

            var resultsByType = from i in _yearly
                                group i by i.Merchant.Type
                                    into g
                                    select new
                                    {
                                        MerchantType = g.Key,
                                        Expenses = g.ToList(),
                                        Count = g.ToList().Count,
                                        Sum = g.ToList().Sum((a) => a.Amount)
                                    };

            resultsByType = resultsByType.OrderByDescending(r => r.Sum);

            csvTypes.AppendLine("Type, Count, Total Spent");
            foreach (var t in resultsByType)
            {
                var line = string.Format("{0}, {1}, {2}", t.MerchantType, t.Count, (Int64)t.Sum);
                csvTypes.AppendLine(line);
            }

            File.WriteAllText(directoryName + "Expenses.csv", csv.ToString());
            File.WriteAllText(directoryName + "ExpenseTypes.csv", csvTypes.ToString());

        }
    }
}
