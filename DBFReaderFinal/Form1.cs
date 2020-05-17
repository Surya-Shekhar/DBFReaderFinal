using System;
using System.IO;
using System.Data;
using System.Data.OleDb;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Windows.Forms.VisualStyles;

namespace DBFReaderFinal
{
    public partial class Form1 : Form
    {
        string []filenames = (Properties.Settings.Default.files).Split(',');
        DataTable dtData, dtColumn;
        //string[] filenames = { "GCHKINFO.dbf", "EMP.DBF" };
       
        string mainfilepath = @""+Properties.Settings.Default.mainfilepath;
        int noofdays = Properties.Settings.Default.noofdays;
        //int noofdays = 2;
        string conString = "Server = localhost\\SQLEXPRESS01;Database=TestDB;Trusted_Connection=True;User Id=sa;Password=frog22gOT!";
        public IList<DbfFieldDescriptor> FieldDescriptors { get; set; }
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            for (int i = 0; i < noofdays; i++)
            {
                for (int j = 0; j < filenames.Length; j++)
                {
                    DateTime d = DateTime.Now;
                    d=d.AddDays(-i);
                    string folder = "" + d.Year + ((d.Month > 10) ? "" + d.Month : "0" + d.Month) + d.Day;
                    
                    //string rfileName = @"C:\Users\suray.bhattcharya\Downloads\wetransfer-d19d70\20200423\20200424\GCHKINFO.dbf";
                    string rfileName = @"" + mainfilepath + folder + "\\" + filenames[j];
                    if (CheckRecords(folder, rfileName) == 0)
                    {
                        try
                        {
                            string filePath = Path.GetDirectoryName(rfileName);
                            OleDbConnection connection = new OleDbConnection("Provider=VFPOLEDB.1;Data Source=" + filePath + ";");
                            connection.Open();
                            DataTable tables = connection.GetSchema(OleDbMetaDataCollectionNames.Tables);
                            dtColumn = null;
                            string fName = Path.GetFileNameWithoutExtension(rfileName);
                            foreach (DataRow rowTables in tables.Rows)
                            {
                                if (rowTables["table_name"].ToString().ToUpper() == fName.ToUpper())
                                {
                                    DataTable columns = connection.GetSchema(OleDbMetaDataCollectionNames.Columns,
                                        new String[] { null, null, rowTables["table_name"].ToString(), null });

                                    dtColumn = GetColumnDataTable();
                                    foreach (System.Data.DataRow rowColumns in columns.Rows)
                                    {
                                        DataRow dr = dtColumn.NewRow();
                                        dr[0] = rowColumns["column_name"].ToString();
                                        dr[1] = OleDbType(int.Parse(rowColumns["data_type"].ToString()));
                                        dr[2] = rowColumns["data_type"].ToString();
                                        dr[3] = rowColumns["numeric_precision"].ToString();
                                        dtColumn.Rows.Add(dr);
                                    }
                                    break;
                                }
                            }

                            string sql = "SELECT * FROM " + fName;
                            OleDbCommand cmd = new OleDbCommand(sql, connection);
                            OleDbDataAdapter DA = new OleDbDataAdapter(cmd);
                            dtData = new DataTable();
                            DA.Fill(dtData);

                            dtData.Columns.Add("pDate", typeof(System.DateTime));
                            foreach (DataRow row in dtData.Rows)
                            {
                                row["pDate"] = DateTime.Now;
                            }

                            connection.Close();
                            WriteDataToDatabase(dtData, dtColumn, rfileName,folder);
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
            }
        }

        // In this method we have create the table that will be stored the columns name and datatype.  
        static DataTable GetColumnDataTable()
        {
            DataTable table = new DataTable();
            table.Columns.Add("NAME", typeof(string));
            table.Columns.Add("TYPE", typeof(string));
            table.Columns.Add("TYPENO", typeof(string));
            table.Columns.Add("DEC", typeof(string));
            return table;
        }

        // This method return the datatype name.  
        public string OleDbType(int type)
        {
            string dataType;
            switch (type)
            {
                case 10:
                    dataType = "BigInt";
                    break;
                case 128:
                    dataType = "Byte";
                    break;
                case 11:
                    dataType = "Boolean";
                    break;
                case 8:
                    dataType = "String";
                    break;
                case 129:
                    dataType = "String";
                    break;
                case 6:
                    dataType = "Currency";
                    break;
                case 7:
                    dataType = "DateTime";
                    break;
                case 133:
                    dataType = "DateTime";
                    break;
                case 134:
                    dataType = "TimeSpan";
                    break;
                case 135:
                    dataType = "DateTime";
                    break;
                case 14:
                    dataType = "Decimal";
                    break;
                case 5:
                    dataType = "Double";
                    break;
                case 3:
                    dataType = "Integer";
                    break;
                case 201:
                    dataType = "String";
                    break;
                case 203:
                    dataType = "String";
                    break;
                case 204:
                    dataType = "Byte";
                    break;
                case 200:
                    dataType = "String";
                    break;
                case 139:
                    dataType = "Decimal";
                    break;
                case 202:
                    dataType = "String";
                    break;
                case 130:
                    dataType = "String";
                    break;
                case 131:
                    dataType = "Decimal";
                    break;
                case 64:
                    dataType = "DateTime";
                    break;

                default:
                    dataType = "";
                    break;
            }

            return dataType;
        }

        public int CheckRecords (string folder, string rfileName)
        {
            int count = 0;
            string TableName = Path.GetFileNameWithoutExtension(rfileName);
            SqlConnection dbCon = new SqlConnection(conString);
            if (dbCon.State == ConnectionState.Closed)
                dbCon.Open();
            try
            {
                string strQuery = "SELECT count(*) as Total FROM " + TableName + " where folder= '" + folder + "'";
                SqlCommand dbCmd = new SqlCommand(strQuery, dbCon);
                SqlDataReader reader = dbCmd.ExecuteReader();
                
                while (reader.Read())
                {
                    count = (int)reader["Total"];
                }
            }
            catch (Exception ex)
            {

            }
                return count;
        }
        public void WriteDataToDatabase(DataTable dtData, DataTable dtCol, string rfileName,string folder)
        {
            string TableName = Path.GetFileNameWithoutExtension(rfileName);
            string FilePath = rfileName;
            SqlConnection dbCon = new SqlConnection(conString);
            if (dbCon.State == ConnectionState.Closed)
                dbCon.Open();
            string strQuery = "IF (EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'" + TableName + "')) BEGIN SELECT 1 END ELSE BEGIN SELECT 0 END";
            SqlCommand dbCmd = new SqlCommand(strQuery, dbCon);
            SqlDataAdapter dbDa = new SqlDataAdapter(dbCmd);
            DataTable dtExist = new DataTable();
            dbDa.Fill(dtExist);
            int valReturn = int.Parse(dtExist.Rows[0][0].ToString());
            if (valReturn == 0)
            {
                ReadFileStream(FilePath, dtCol);
                CreateDbTable(TableName);
            }

            saveToDb(dtData, TableName, dtCol,folder);
        }

        // this method will create table in the database.  
        void CreateDbTable(string TableName)
        {
            try
            {
                SqlConnection dbCon = new SqlConnection(conString);
                if (dbCon.State == ConnectionState.Closed)
                    dbCon.Open();
                using (SqlCommand cmd = dbCon.CreateCommand())
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine($"CREATE TABLE [{TableName}] (RECORDID int IDENTITY(1,1) PRIMARY KEY,");
                    bool first = true;
                    foreach (var fieldDescriptor in FieldDescriptors)
                    {
                        if (first)
                            first = false;
                        else
                            sb.Append(", ");
                        sb.AppendLine($"[{fieldDescriptor.Name}] {fieldDescriptor.GetSqlDataType()}");
                    }
                    

                    sb.Append(", ");
                    sb.AppendLine($"[{"lactivestatus"}] {"bit"}");
                    sb.Append(", ");
                    sb.AppendLine($"[{"pDate"}] {"DateTime"}");
                    sb.Append(", ");
                    sb.AppendLine($"[{"folder"}] {"VARCHAR(12)"}");
                    sb.AppendLine($")");
                    cmd.CommandText = sb.ToString();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to create table {TableName}", e);
            }
        }

        // Here we save records to the database.  
        public void saveToDb(DataTable dtOne, string TableName, DataTable dtColumn, string folder)
        {
            if (dtOne.Rows.Count > 0)
            {
                SqlConnection dbCon = new SqlConnection(conString);
                for (int n = 0; n < dtOne.Rows.Count; n++)
                {
                    if (dbCon.State == ConnectionState.Closed)
                        dbCon.Open();

                    string strQry = "";
                    strQry = "INSERT INTO [" + TableName + "] VALUES(";
                    for (int i = 0; i < dtColumn.Rows.Count; i++)
                    {
                        if (i == dtColumn.Rows.Count - 1)
                        {
                            if (dtColumn.Rows[i]["TYPE"].ToString().ToLower() == "boolean" || dtColumn.Rows[i]["TYPE"].ToString().ToLower() == "logical")
                            {
                                if (string.IsNullOrWhiteSpace(dtOne.Rows[n][i].ToString()))
                                    strQry = strQry + "'False','true')";
                                else
                                    strQry = strQry + "'" + dtOne.Rows[n][i].ToString() + "','true')";
                            }
                            else if (dtColumn.Rows[i]["TYPE"].ToString().ToLower() == "string")
                            {
                                if (string.IsNullOrWhiteSpace(dtOne.Rows[n][i].ToString()))
                                    strQry = strQry + "'','true')";
                                else
                                    strQry = strQry + "'" + dtOne.Rows[n][i].ToString().Replace("'", "") + "','true')";
                            }
                            else if (dtColumn.Rows[i]["TYPE"].ToString().ToLower() == "byte")
                            {
                                if (string.IsNullOrWhiteSpace(dtOne.Rows[n][i].ToString()))
                                    strQry = strQry + "'0','true')";
                                else
                                    strQry = strQry + "'" + Encoding.ASCII.GetBytes(dtOne.Rows[n][i].ToString()) + "','true')";
                            }
                            else if (dtColumn.Rows[i]["TYPE"].ToString().ToLower() == "character")
                            {
                                if (string.IsNullOrWhiteSpace(dtOne.Rows[n][i].ToString()))
                                    strQry = strQry + "'','true')";
                                else
                                    strQry = strQry + "'" + dtOne.Rows[n][i].ToString().Replace("'", "") + "','true')";
                            }
                            else if (dtColumn.Rows[i]["TYPE"].ToString().ToLower() == "datetime" || dtColumn.Rows[i]["TYPE"].ToString().ToLower() == "date")
                            {
                                if (string.IsNullOrWhiteSpace(dtOne.Rows[n][i].ToString()))
                                    strQry = strQry + "null,'true')";
                                else
                                    strQry = strQry + "'" + DateTime.Parse(dtOne.Rows[n][i].ToString()) + "','true')";
                            }
                            else
                            {
                                if (string.IsNullOrWhiteSpace(dtOne.Rows[n][i].ToString()))
                                    strQry = strQry + "0,'true')";
                                else
                                    strQry = strQry + dtOne.Rows[n][i].ToString() + ",'true')";
                            }
                        }
                        else
                        {
                            if (dtColumn.Rows[i]["TYPE"].ToString().ToLower() == "boolean" || dtColumn.Rows[i]["TYPE"].ToString().ToLower() == "logical")
                            {
                                if (string.IsNullOrWhiteSpace(dtOne.Rows[n][i].ToString()))
                                    strQry = strQry + "'False',";
                                else
                                    strQry = strQry + "'" + dtOne.Rows[n][i].ToString() + "',";
                            }
                            else if (dtColumn.Rows[i]["TYPE"].ToString().ToLower() == "string")
                            {
                                if (string.IsNullOrWhiteSpace(dtOne.Rows[n][i].ToString()))
                                    strQry = strQry + "'',";
                                else
                                    strQry = strQry + "'" + dtOne.Rows[n][i].ToString().Replace("'", "") + "',";
                            }
                            else if (dtColumn.Rows[i]["TYPE"].ToString().ToLower() == "byte")
                            {
                                if (string.IsNullOrWhiteSpace(dtOne.Rows[n][i].ToString()))
                                    strQry = strQry + "'0',";
                                else
                                    strQry = strQry + "'" + Encoding.ASCII.GetBytes(dtOne.Rows[n][i].ToString()) + "',";
                            }
                            else if (dtColumn.Rows[i]["TYPE"].ToString().ToLower() == "character")
                            {
                                if (string.IsNullOrWhiteSpace(dtOne.Rows[n][i].ToString()))
                                    strQry = strQry + "'',";
                                else
                                    strQry = strQry + "'" + dtOne.Rows[n][i].ToString().Replace("'", "") + "',";
                            }
                            else if (dtColumn.Rows[i]["TYPE"].ToString().ToLower() == "datetime" || dtColumn.Rows[i]["TYPE"].ToString().ToLower() == "date")
                            {
                                if (string.IsNullOrWhiteSpace(dtOne.Rows[n][i].ToString()))
                                    strQry = strQry + "null,";
                                else
                                    strQry = strQry + "'" + DateTime.Parse(dtOne.Rows[n][i].ToString()) + "',";
                            }
                            else
                            {
                                if (string.IsNullOrWhiteSpace(dtOne.Rows[n][i].ToString()))
                                    strQry = strQry + "0,";
                                else
                                    strQry = strQry + dtOne.Rows[n][i].ToString() + ",";
                            }
                        }
                    }

                    try
                    {
                        strQry = strQry.Replace(")", "");
                        strQry = strQry + ",'" + DateTime.Now + "',";
                        strQry = strQry + "'" +folder+ "')";

                        SqlCommand dbCmd1 = new SqlCommand(strQry, dbCon);
                        dbCmd1.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error " + ex.Message);
                    }
                }
                dbCon.Close();
            }
        }

        void ReadFileStream(string FilePath, DataTable dtCol)
        {
            FileStream fileStream = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            BinaryReader binaryReader = new BinaryReader(fileStream);

            var fieldDescriptors = new List<DbfFieldDescriptor>();
            try
            {
                int no = 0;
                while (true)
                {
                    var fieldDescriptor = ReadFieldDescriptor(binaryReader, no++, dtCol);
                    if (fieldDescriptor == null)
                        break;
                    if (no > 1)
                        fieldDescriptors.Add(fieldDescriptor);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Failed to read field descriptors", e);
            }
            FieldDescriptors = fieldDescriptors;
        }

        DbfFieldDescriptor ReadFieldDescriptor(BinaryReader br, int fdNo, DataTable dtCol)
        {
            var fieldDescriptor = new DbfFieldDescriptor();
            fieldDescriptor.No = fdNo;
            string name = "";
            if (fdNo > 0 && fdNo <= dtCol.Rows.Count)
                name = dtCol.Rows[fdNo - 1][0].ToString();
            try
            {
                var fieldNameBytes = new byte[11];
                fieldNameBytes[0] = br.ReadByte();
                if (fieldNameBytes[0] == 0x0D)
                    return null; // 0x0D means end of field descriptor list  

                br.Read(fieldNameBytes, 1, 10);
                fieldDescriptor.Name = name;
                fieldDescriptor.TypeChar = (char)br.ReadByte();
                br.ReadByte(); // reserved  
                br.ReadByte(); // reserved  
                br.ReadByte(); // reserved  
                br.ReadByte(); // reserved  
                fieldDescriptor.Length = br.ReadByte();
                fieldDescriptor.DecimalCount = br.ReadByte();
                br.ReadBytes(2); // work area id  
                br.ReadByte(); // example  
                br.ReadBytes(10); // reserved  
                br.ReadByte(); // production mdx  

                return fieldDescriptor;
            }
            catch (Exception e)
            {
                if (string.IsNullOrWhiteSpace(fieldDescriptor.Name))
                    throw new Exception($"Failed to read field descriptor #{fdNo + 1}", e);
                else
                    throw new Exception($"Failed to read field descriptor #{fdNo + 1} ({fieldDescriptor.Name})", e);
            }
        }
    }

}
