using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.OleDb;
using Microsoft.Office.Interop.Excel;
using System.IO;
using System.Collections;
using System.Diagnostics;
using BasicNet;


namespace NetSimulation.Lib
{
    public class TextDB
    {
        public static System.Data.DataTable CreateDataTable(Type[] Columns)
        {
            System.Data.DataTable tbl = new System.Data.DataTable();

            for (int col = 0; col < Columns.Count(); col++)
                tbl.Columns.Add(new DataColumn("Col" + (col + 1).ToString(), Columns[col]));
            return tbl;
        }
        public static System.Data.DataTable CreateDataTable(DataColumn[] Columns)
        {
            System.Data.DataTable tbl = new System.Data.DataTable();

            for (int col = 0; col < Columns.Count(); col++)
                tbl.Columns.Add(Columns[col]);
            return tbl;
        }
        public static System.Data.DataTable ConvertToDataTable(string filename, DataColumn[] Columns, char separator = '\t')
        {
            if (!File.Exists(filename))
                filename = Netutil.InPutDirector + "\\" + filename;

            StreamReader file = new StreamReader(filename);

            System.Data.DataTable tbl = CreateDataTable(Columns);

            try
            {
                string line;
                string[] cols = null;
                while ((line = file.ReadLine()) != null)
                {
                    cols = line.Split(new char[] { separator });
                    DataRow dr = tbl.NewRow();
                    for (int cIndex = 0; cIndex < Columns.Count(); cIndex++)
                    {
                        dr[cIndex] = cols[cIndex];
                    }

                    tbl.Rows.Add(dr);
                }
            }
            finally
            {
                file.Close();
            }
            return tbl;
        }
        public static void WriteTextFile(string data, string fileName)
        {
            fileName = Netutil.OutPutDirector + "\\" + fileName;
            var sw = new StreamWriter(fileName, true);
            sw.WriteLine(data);
            sw.Close();
        }
        public static void WriteTextFile1Line(string data, string fileName)
        {
            fileName = Netutil.OutPutDirector + "\\" + fileName;
            var sw = new StreamWriter(fileName, true);
            sw.Write(data);
            sw.Close();
        }
        public static bool Exists(string fileName)
        {

            fileName = Netutil.OutPutDirector + "\\" + fileName;
            return File.Exists(fileName);
        }
        
        public static string WriteTextFile(string[] elements, string fileName)
        {
           

            fileName = Netutil.OutPutDirector + "\\" + fileName;
            string data=string.Join("\t", elements);
            var sw = new StreamWriter(fileName, true);
            sw.WriteLine(data);
            sw.Close();
            return fileName;
        }
       
    }
    public class ExcelDB: IDisposable
    {
        private readonly object misValue = System.Reflection.Missing.Value;
        private Application excelApp=null;
        private Workbook workbook=null;
        private Worksheet worksheet=null;
        private Dictionary<string, string[]> TableNames=new System.Collections.Generic.Dictionary<string,string[]>();
        private int _SelectedSheetIndex=0;
        public int headerRowIndex = 1;
        public int headerColIndex = 1;
        public const int DataRowStart = 3;
        private void Initialize()
        {
            headerRowIndex = 1;
            headerColIndex = 1;
            srcIdx = 0;
            tgIdx = 1;
            edgIdx = 2;
            grow = 1;
            directionIdx = 5;
        }
        public int SheetCount
        {
            get
            {
                return workbook.Sheets.Count;
            }
        }
        public int SelectedSheetIndex
        {
            get
            {
                return _SelectedSheetIndex;
            }
            set
            {
                _SelectedSheetIndex = value;
                if (-1 < _SelectedSheetIndex && _SelectedSheetIndex < workbook.Sheets.Count)
                {
                    workbook.Sheets[_SelectedSheetIndex].Activate();
                    worksheet = workbook.Sheets[_SelectedSheetIndex];
                }
                else
                {
                    throw new Exception("The selected sheet is not valid!");
                }
            }
        }
        #region "Contructor and destructor"
        ~ExcelDB()
        {
            this.Close();
        }
        public void Dispose()
        {
            this.Close();
        }
        public ExcelDB(string[] RowHeader, string SheetName = "Sheet1")
        {
            OpenExcelFile(RowHeader, SheetName);
        }
        public ExcelDB(string SheetName = "Sheet1")
        {
            OpenExcelFile(SheetName);
        }
        public int srcIdx = 0;
        public int tgIdx = 1;
        public int edgIdx = 2;
        public int weightIdx = 3;
        public int nameIdx = 4;
        public int directionIdx = 5;
        const int nCol = 6;//start, end, type, weight, name, direction
        public void ReadFile(string filename)
        {
            if (workbook != null)
                workbook.Close(false);
           
            workbook = excelApp.Workbooks.Open(
        filename, 0, true, 5,"", "", true, XlPlatform.xlWindows, "\t", false, false,0, true);
            Sheets sheets = workbook.Worksheets;
            worksheet = (Worksheet)sheets.get_Item(1);
           
            int colLimit = Math.Min(worksheet.Columns.Count, 100);
            int rowLimit = Math.Min(worksheet.Rows.Count, 100);

            string[] startHeader = { "start", "from", "source", "src", "vertex 1", "begin", "vertex1" };
            string[] endHeader = { "end", "target", "destination", "des", "to", "vertex 2", "finish", "vertex2" };
            string[] interactionHeader = { "interaction", "type", "interaction type" };
            string[] weightHeader = { "weight" };
            string[] directionHeader = { "direction" };
            string[] nameHeader = { "name","edge name","interaction name" };
            string[] startMark = startHeader.Union(endHeader).Union(interactionHeader).Union(weightHeader).Union(nameHeader).Union(directionHeader).ToArray();

            //string[] startMark = { "start", "end", "weight", "type", "interaction", "interaction type", "vertex1", "vertex2", "vertex 1", "vertex 2" };
            for (int i = 1; i <= rowLimit; i++)
            {
                for (int j = 1; j <= colLimit; j++)
                {
                    if (worksheet.Cells[i, j].Value != null &&
                        startMark.Contains((string)worksheet.Cells[i, j].Value.ToString().ToLower()))
                    {
                        headerRowIndex = i;
                        headerColIndex = j;
                        goto start;
                    }
                }
            }
            start:
            string[] Header = new string[nCol];
            for (int i = 0; i < nCol; i++)
            {
                if (startHeader.Contains((string)worksheet.Cells[headerRowIndex, headerColIndex + i].Value.ToString().ToLower()))
                    srcIdx = i;
                else if (endHeader.Contains((string)worksheet.Cells[headerRowIndex, headerColIndex + i].Value.ToString().ToLower()))
                    tgIdx = i;
                else if (interactionHeader.Contains((string)worksheet.Cells[headerRowIndex, headerColIndex + i].Value.ToString().ToLower()))
                    edgIdx = i;
                else if (directionHeader.Contains((string)worksheet.Cells[headerRowIndex, headerColIndex + i].Value.ToString().ToLower()))
                    directionIdx = i;
                else if (weightHeader.Contains((string)worksheet.Cells[headerRowIndex, headerColIndex + i].Value.ToString().ToLower()))
                    weightIdx = i;
                else if (nameHeader.Contains((string)worksheet.Cells[headerRowIndex, headerColIndex + i].Value.ToString().ToLower()))
                    nameIdx = i;
                else
                    throw new Exception(string.Format("Sheet \"{0}\" has an invalide header!", filename));
                Header[i] = worksheet.Cells[headerRowIndex, headerColIndex + i].Value.ToString().ToLower();
            }

             TableNames[worksheet.Name] = Header;
        }
        private void OpenExcelFile(string SheetName = "Sheet1")
        {
            excelApp = new Application();
            workbook = excelApp.Workbooks.Add(misValue);
            worksheet = (Worksheet)workbook.ActiveSheet;
            if (SheetName != null)
                worksheet.Name = SheetName;
        }
        private void OpenExcelFile(string[] RowHeader, string SheetName = "Sheet1")
        {
            TableNames[SheetName] = RowHeader;
            excelApp = new Application();
            workbook = excelApp.Workbooks.Add(misValue);
            worksheet = (Worksheet)workbook.ActiveSheet;
            if(SheetName!=null)
                worksheet.Name = SheetName;
            createHeader(RowHeader);
        }
        /// <summary>
        /// Create a new sheet
        /// </summary>
        /// <param name="SheetName">The name of the sheet</param>
        /// <param name="RowHeader">Raw header or column name</param>
        public void NewSheet(string SheetName, string[] RowHeader)
        {
            
            if (SheetName == null && TableNames.Keys.Count>0)
                SheetName = TableNames.Keys.ElementAt(TableNames.Keys.Count - 1) + TableNames.Keys.Count.ToString();
            if (RowHeader == null && TableNames.Values.Count>0)
                RowHeader = TableNames.Values.ElementAt(TableNames.Values.Count - 1);

            if (TableNames.ContainsKey(SheetName))
                throw new Exception("AddSheet: Existing sheetname already!");
            TableNames[SheetName] = RowHeader;
            worksheet = workbook.Sheets.Add();
            worksheet.Name = SheetName;
            createHeader(RowHeader);
            grow = 2;
        }
        #endregion
        /// <summary>
        /// Row header or column name of the table on handling
        /// </summary>
        public string[] RowHeader
        {
            get
            {
                if (TableNames.ContainsKey(worksheet.Name))
                    return TableNames[worksheet.Name];
                else
                    return null;
            }
        }

        /// <summary>
        /// make the range of rows bold
        /// </summary>
        /// <param name="row1"></param>
        /// <param name="row2"></param>
        private void BoldRange(Range Cel1, Range Cel2)
        {
            Range excelRange = worksheet.get_Range(Cel1, Cel2);
            
            excelRange.Font.Bold=true;
        }
        
        private void createHeader(string[] RowHeader)
        {
            for (int col = 1; col <= RowHeader.Length; col++)
                worksheet.Cells[headerRowIndex, col].Value = RowHeader[col - 1];
            BoldRange(worksheet.Cells[headerRowIndex, 1], worksheet.Cells[headerRowIndex, RowHeader.Length]);

            worksheet.get_Range("A" + headerRowIndex.ToString(), misValue).EntireColumn.AutoFit();

            
        }
        /// <summary>
        /// Change to or activate another sheet to handle
        /// </summary>
        /// <param name="SheetName">The name of the sheet</param>
        /// <returns></returns>
        public bool SelectSheet(string SheetName)
        {
            for (int i=0;i< workbook.Sheets.Count;i++) 
            {
                if (workbook.Sheets[i].Name == SheetName)
                {
                    SelectedSheetIndex = i;
                    return true;
                }
            }
            return false;
        }
        
        
        public object[] CreateRow()
        {
            return new object[RowHeader.Length];
        }
        /// <summary>
        /// Write a raw on the table with row header as column name
        /// </summary>
        /// <param name="row">row index for writing, where row >=2</param>
        /// <param name="data">the data for writing</param>
        public void WriteRow(int row, object[] data)
        {
            Debug.Assert(headerRowIndex < row);
            for (int col = 1; col <= data.Length; col++)
                worksheet.Cells[row, col].Value = data[col - 1];
        }
        int grow = 2; 
        public void WriteRow(object[] data)
        {
            for (int col = 1; col <= data.Length; col++)
                worksheet.Cells[grow, col].Value = data[col - 1];
            grow++;
        }
        /// <summary>
        /// Read a row on the table with the row header as the column name
        /// </summary>
        /// <param name="row">The index of the reading row</param>
        /// <returns>the row data</returns>
        public object[] ReadRow(int row)
        {
            Debug.Assert(headerRowIndex < row);
            int noCol = RowHeader.Length;
            object[] Vals = new object[noCol];
            for (int col = 0; col < noCol; col++)
                Vals[col] = worksheet.Cells[row, headerColIndex+col].Value;
            return Vals; 
        }
        public object this[int r, int c]
        {
            get { return worksheet.Cells[r, c].Value; }
            set { worksheet.Cells[r, c].Value = value; }

        }
        public object this[string r, string c]
        {
            get { return worksheet.Cells[r, c].Value; }
            set { worksheet.Cells[r, c].Value = value; }

        }
        /// <summary>
        /// Save all data to Excel format file
        /// </summary>
        /// <param name="fileName"></param>
        public string SaveToFile(string fileName)
        {

            if (!Directory.Exists(Netutil.OutPutDirector))
                Directory.CreateDirectory(Netutil.OutPutDirector);
            string excelfile = Netutil.OutPutDirector + "\\" + fileName;

            worksheet.SaveAs(excelfile);
            
            this.Close();
            return excelfile;
        }
        public static void DeleteFile(string fileName)
        {
            File.Delete(fileName);
        }
        private void Close()
        {
            try
            {
                if (workbook != null)
                {
                    
                    workbook.Close(false);
                    workbook = null;
                }

                if (excelApp != null)
                {
                    excelApp.Quit();
                    excelApp = null;
                }
                
            }
            catch { }
            
        }
        /// <summary>
        /// This method retrieves the excel sheet names from 
        /// an excel workbook.
        /// </summary>
        /// <param name="excelFile">The excel file.</param>
        /// <returns>String[]</returns>
        public static String[] GetExcelSheetNames(string excelFile)
        {
            OleDbConnection objConn = null;
            System.Data.DataTable dt = null;

            try
            {
                // Connection String. Change the excel file to the file you
                // will search.
                String connString = "Provider=Microsoft.Jet.OLEDB.4.0;" +
                  "Data Source=" + excelFile + ";Extended Properties=Excel 8.0;";
                // Create connection object by using the preceding connection string.
                objConn = new OleDbConnection(connString);
                // Open connection with the database.
                objConn.Open();
                // Get the data table containg the schema guid.
                dt = objConn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

                if (dt == null)
                {
                    return null;
                }

                String[] excelSheets = new String[dt.Rows.Count];
                int i = 0;

                // Add the sheet name to the string array.
                foreach (DataRow row in dt.Rows)
                {
                    excelSheets[i] = row["TABLE_NAME"].ToString();
                    i++;
                }

                // Loop through all of the sheets if you want too...
                for (int j = 0; j < excelSheets.Length; j++)
                {
                    // Query each excel sheet.
                }

                return excelSheets;
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                // Clean up.
                if (objConn != null)
                {
                    objConn.Close();
                    objConn.Dispose();
                }
                if (dt != null)
                {
                    dt.Dispose();
                }
            }
        }

    }
}
