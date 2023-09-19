using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.IO;
using MFI.Common;

namespace DataCheck
{
    public partial class DataCheckMain : Form
    {
        private Hashtable m_htConfigValues;
        private string m_dbConnection;
        private string m_logFolder;
        private int m_logMaxSize;
        private int m_maxLogs;
        private int m_logDebugLevel;
        private string m_notifyEmail;
        private string m_supportEmail;
        private string m_scriptFolder;
        private string m_dataFile;
        private string m_SMTPServer;
        private string m_sendFrom;
        private string m_rootFolder = @"C:\MFI-IT\DataCheck";
        private Hashtable m_htFieldNameDel;
        private Hashtable m_htFieldNameMod;
        private Hashtable m_htFieldNameMove;
        private Hashtable m_htStoreProcNameDel;
        private Hashtable m_htStoreProcNameMod;
        private Hashtable m_htStoreProcNameMove;
        private Hashtable m_htFieldsDel;
        private Hashtable m_htFieldsMod;
        private Hashtable m_htFieldsMove;
        private Hashtable m_htStoreProcsDel;
        private Hashtable m_htStoreProcsMod;
        private Hashtable m_htStoreProcsMove;
        private Hashtable m_htDeletedFields;
        private Hashtable m_htModifiedFields;
        private Hashtable m_htMovingFields;
        private string m_fieldName;
        private string m_storeProcsName;
        private StringBuilder m_procsChange;
        private string m_fileNameByProcs;
        private string m_fileNameByFields;
        private string m_errorMsg = "";

        private SortedList htCurrentField;
        private SortedList htCW1Field;
        private SortedList htCombine;

        public DataCheckMain()
        {
            InitializeComponent();
            InitServiceConfigs();
        }

        private void btnBrowseScript_Click(object sender, EventArgs e)
        {
            try
            {
                string result = "";
                this.folderBrowserDialog1.Description = "Please select folder where scripted Store procedure/Function has been saved";
                this.folderBrowserDialog1.ShowNewFolderButton = false;

                while (DialogResult.OK == folderBrowserDialog1.ShowDialog())
                {
                    result = this.folderBrowserDialog1.SelectedPath;
                    break;
                }
                txtScriptFolder.Text = result;
            }
            catch (Exception ex)
            {
                m_errorMsg = "Error btnBrowseScript_Click " + Environment.NewLine + ex.ToString();
                Logger.WriteLog("Error btnBrowseScript_Click " + ex.ToString(), 2);
            }
        }

        private void btnBrowseData_Click(object sender, EventArgs e)
        {
            try
            {
                string result = "";
                while (DialogResult.OK == openFileDialog1.ShowDialog())
                {
                    result = openFileDialog1.FileName;
                    break;
                }
                txtDataFile.Text = result;
            }
            catch (Exception ex)
            {
                m_errorMsg = "Error btnBrowseData_Click " + Environment.NewLine + ex.ToString();
                Logger.WriteLog("Error btnBrowseData_Click " + ex.ToString(), 2);
            }
        }

        private void InitServiceConfigs()
        {
            try
            {
                m_htConfigValues = new Hashtable();
                m_htConfigValues = AppConfigAccess.ReadConfigValues("DATA_CHECK");
                m_SMTPServer = AppConfigAccess.ReadConfigValue("GLOBAL", "SMTP_SERVER");
                m_sendFrom = AppConfigAccess.ReadConfigValue("GLOBAL", "SENDER_EMAIL_ADDRESS");

                m_dbConnection = AppConfigAccess.ConnectionString;

                if (m_htConfigValues.ContainsKey("SUPPORT_EMAIL_ADDRESS"))
                    m_supportEmail = m_htConfigValues["SUPPORT_EMAIL_ADDRESS"].ToString();

                if (m_htConfigValues.ContainsKey("NOTIFY_EMAIL"))
                    m_notifyEmail = m_htConfigValues["NOTIFY_EMAIL"].ToString();

                if (m_htConfigValues.ContainsKey("ROOT_FOLDER"))
                    m_rootFolder = m_htConfigValues["ROOT_FOLDER"].ToString();

                if (m_htConfigValues.ContainsKey("LOG_DEBUG_LEVEL"))
                    m_logDebugLevel = Convert.ToInt32(m_htConfigValues["LOG_DEBUG_LEVEL"]);

                if (m_htConfigValues.ContainsKey("MAX_LOG_SIZE"))
                    m_logMaxSize = Convert.ToInt32(m_htConfigValues["MAX_LOG_SIZE"]);

                if (m_htConfigValues.ContainsKey("MAX_LOGS"))
                    m_maxLogs = Convert.ToInt32(m_htConfigValues["MAX_LOGS"]);

                m_logFolder = m_rootFolder + @"\Logs";
                m_scriptFolder = m_rootFolder + @"\Scripts";

                Directory.CreateDirectory(m_logFolder);
                Directory.CreateDirectory(m_scriptFolder);

                if (m_logMaxSize < 16000) m_logMaxSize = 32000;
                if (m_maxLogs < 2) m_maxLogs = 2;

                Logger.InitLog(m_logFolder, m_logMaxSize, m_maxLogs, m_logDebugLevel);
            }
            catch (Exception ex)
            {
                Logger.WriteLog("Error InitServiceConfigs " + ex.ToString(), 2);
                m_errorMsg = "Error InitServiceConfigs " + Environment.NewLine + ex.ToString();
            }
        }

        private void GetFiles()
        {
            try
            {
                m_scriptFolder = txtScriptFolder.Text;
                m_dataFile = txtDataFile.Text;

                #region Get Fieldnames

                m_htFieldNameDel = new Hashtable();
                m_htFieldNameMod = new Hashtable();
                m_htFieldNameMove = new Hashtable();

                string line;
                int keyFieldName = 0;
                char tab = (char)9;
                keyFieldName = 0;
                string changedNotes = "";

                StreamReader dataFile = new StreamReader(m_dataFile, System.Text.Encoding.UTF8);
                while ((line = dataFile.ReadLine()) != null)
                {
                    if (DataLib.Extract(line, tab, 3).Trim().ToString().ToUpper() == "DELETED")
                    {
                        m_fieldName = DataLib.Extract(line, tab, 2).ToString();
                        if (!m_htFieldNameDel.ContainsKey(keyFieldName))
                            m_htFieldNameDel.Add(keyFieldName, m_fieldName);
                        keyFieldName += 1;
                    }

                    if (DataLib.Extract(line, tab, 3).Trim().ToString().ToUpper() == "CHANGED")
                    {
                        m_fieldName = DataLib.Extract(line, tab, 2).ToString();
                        changedNotes = DataLib.Extract(line, tab, 4).ToString();

                        if (m_fieldName != "")
                        {
                            if (!m_htFieldNameMod.ContainsKey(m_fieldName))
                                m_htFieldNameMod.Add(m_fieldName, changedNotes);
                        }
                    }
                }
                #endregion

                #region Changes by Store Procedure/Functions

                m_htStoreProcNameDel = new Hashtable();
                m_htStoreProcNameMod = new Hashtable();
                m_htStoreProcNameMove = new Hashtable();
                m_htDeletedFields = new Hashtable();
                m_htModifiedFields = new Hashtable();
                m_htMovingFields = new Hashtable();

                string[] files = Directory.GetFiles(m_scriptFolder);
                foreach (string file in files)
                {
                    char slash = Convert.ToChar("\\");
                    int dc = DataLib.Dcount(file, slash);
                    string fileName = DataLib.Extract(file, slash, dc);
                    string[] fileNameSplit = fileName.Split('.');
                    m_storeProcsName = fileNameSplit[1].ToString().Trim();

                    int lineCnt = 0;
                    bool commentOut = false;
                    StreamReader procsData = new StreamReader(file, Encoding.UTF8);
                    while ((line = procsData.ReadLine()) != null)
                    {
                        string dataLine = line;
                        int comStart = -1;
                        int comEnd = -1;
                        lineCnt += 1;
                        if (line != "")
                        {
                            comStart = line.IndexOf("/*");
                            if (comStart != -1)
                            {
                                dataLine = DataLib.Substring(line, 0, comStart); // a blocks of codes been comment out.
                                commentOut = true;
                            }

                            if (commentOut == true)
                            {
                                comEnd = line.IndexOf("*/") + 2;
                                if (comEnd != -1)
                                {
                                    dataLine = dataLine + " " + DataLib.Substring(line, comEnd, line.Length);
                                    commentOut = false;
                                }
                            }

                            comStart = line.IndexOf("--");
                            if (comStart != -1)
                                dataLine = DataLib.Substring(line, 0, comStart); // if line been comment out in middle we only want what not comment out

                            if ((!commentOut) && (dataLine.Trim() != ""))  //ignore line been comment out
                            {
                                dataLine = FormatDataLine(dataLine).ToUpper();

                                #region Delete Fields
                                // Delete Fields
                                if (m_htFieldNameDel.Count > 0)
                                {
                                    IDictionaryEnumerator htEnumerator = m_htFieldNameDel.GetEnumerator();
                                    while (htEnumerator.MoveNext())
                                    {
                                        m_fieldName = htEnumerator.Value.ToString().ToUpper();
                                        if (dataLine.Contains(m_fieldName))
                                        {
                                            string key = "";
                                            //by store Procedure
                                            if (m_htStoreProcNameDel.ContainsKey(m_storeProcsName))
                                            {
                                                m_htFieldsDel = (Hashtable)m_htStoreProcNameDel[m_storeProcsName];
                                                m_htStoreProcNameDel.Remove(m_storeProcsName);
                                            }
                                            else
                                            {
                                                m_htFieldsDel = new Hashtable();
                                            }
                                            key = lineCnt + ":" + m_fieldName;
                                            m_htFieldsDel.Add(key, m_fieldName);
                                            m_htStoreProcNameDel.Add(m_storeProcsName, m_htFieldsDel);

                                            //by delete Fields
                                            if (m_htDeletedFields.ContainsKey(m_fieldName))
                                            {
                                                m_htStoreProcsDel = (Hashtable)m_htDeletedFields[m_fieldName];
                                                m_htDeletedFields.Remove(m_fieldName);
                                            }
                                            else
                                            {
                                                m_htStoreProcsDel = new Hashtable();
                                            }
                                            key = lineCnt + ":" + m_storeProcsName;
                                            m_htStoreProcsDel.Add(key, m_storeProcsName);
                                            m_htDeletedFields.Add(m_fieldName, m_htStoreProcsDel);

                                        }

                                    }
                                }
                                #endregion

                                #region Modify Fields
                                //Modify Fields
                                if (m_htFieldNameMod.Count > 0)
                                {
                                    IDictionaryEnumerator htEnumerator = m_htFieldNameMod.GetEnumerator();
                                    while (htEnumerator.MoveNext())
                                    {
                                        m_fieldName = htEnumerator.Key.ToString().ToUpper();
                                        changedNotes = htEnumerator.Value.ToString();
                                        if (dataLine.Contains(m_fieldName))
                                        {
                                            string key = "";
                                            //by store Procedure
                                            if (m_htStoreProcNameMod.ContainsKey(m_storeProcsName))
                                            {
                                                m_htFieldsMod = (Hashtable)m_htStoreProcNameMod[m_storeProcsName];
                                                m_htStoreProcNameMod.Remove(m_storeProcsName);
                                            }
                                            else
                                            {
                                                m_htFieldsMod = new Hashtable();
                                            }

                                            key = lineCnt + ":" + m_fieldName + ":" + changedNotes;
                                            m_htFieldsMod.Add(key, m_fieldName);
                                            m_htStoreProcNameMod.Add(m_storeProcsName, m_htFieldsMod);

                                            //by Modified Fields
                                            if (m_htModifiedFields.ContainsKey(m_fieldName))
                                            {
                                                m_htStoreProcsMod = (Hashtable)m_htModifiedFields[m_fieldName];
                                                m_htModifiedFields.Remove(m_fieldName);
                                            }
                                            else
                                            {
                                                m_htStoreProcsMod = new Hashtable();
                                            }


                                            key = lineCnt + ":" + m_storeProcsName + ":" + changedNotes;
                                            m_htStoreProcsMod.Add(key, m_storeProcsName);
                                            m_htModifiedFields.Add(m_fieldName, m_htStoreProcsMod);
                                        }
                                    }
                                }
                                #endregion

                                #region Moving Fields
                                //Moving Fields
                                if (m_htFieldNameMove.Count > 0)
                                {
                                    IDictionaryEnumerator htEnumerator = m_htFieldNameMove.GetEnumerator();
                                    while (htEnumerator.MoveNext())
                                    {
                                        m_fieldName = htEnumerator.Key.ToString().ToUpper();
                                        if (dataLine.Contains(m_fieldName))
                                        {
                                            string key = "";
                                            //by store Procedure
                                            if (m_htStoreProcNameMove.ContainsKey(m_storeProcsName))
                                            {
                                                m_htFieldsMove = (Hashtable)m_htStoreProcNameMove[m_storeProcsName];
                                                m_htStoreProcNameMove.Remove(m_storeProcsName);
                                            }
                                            else
                                            {
                                                m_htFieldsMove = new Hashtable();
                                            }

                                            key = lineCnt + ":" + m_fieldName;
                                            m_htFieldsMove.Add(key, m_fieldName);
                                            m_htStoreProcNameMove.Add(m_storeProcsName, m_htFieldsMove);

                                            //by Modified Fields
                                            if (m_htMovingFields.ContainsKey(m_fieldName))
                                            {
                                                m_htStoreProcsMove = (Hashtable)m_htMovingFields[m_fieldName];
                                                m_htMovingFields.Remove(m_fieldName);
                                            }
                                            else
                                            {
                                                m_htStoreProcsMove = new Hashtable();
                                            }

                                            key = lineCnt + ":" + m_storeProcsName;
                                            m_htStoreProcsMove.Add(key, m_storeProcsName);
                                            m_htMovingFields.Add(m_fieldName, m_htStoreProcsMove);
                                        }
                                    }
                                }
                                #endregion
                            }
                        }
                    }
                }

                if ((m_htStoreProcNameDel.Count > 0) || (m_htStoreProcNameMod.Count > 0) || (m_htStoreProcNameMove.Count > 0))
                {
                    m_procsChange = new StringBuilder();
                    m_procsChange.Append("Date: " + System.DateTime.Now + Environment.NewLine);
                    m_procsChange.Append("Store Procedure/Function Name" + "\t" + "Field Name" + "\t" + "Line No" + "\t" + "Changes" + Environment.NewLine);

                    bool firstFields = true;
                    IDictionaryEnumerator htEnumerator = m_htStoreProcNameDel.GetEnumerator();
                    while (htEnumerator.MoveNext())
                    {
                        m_storeProcsName = htEnumerator.Key.ToString();
                        m_htFieldsDel = (Hashtable)m_htStoreProcNameDel[m_storeProcsName];

                        
                        IDictionaryEnumerator htField = m_htFieldsDel.GetEnumerator();
                        while (htField.MoveNext())
                        {

                            string lineNo = DataLib.Extract(htField.Key.ToString(), ':', 1);
                            m_fieldName = htField.Value.ToString();

                            if (firstFields)
                            {
                                m_procsChange.Append(m_storeProcsName + "\t" + m_fieldName + "\t" + lineNo + "\t" + "Deleted" + Environment.NewLine);
                                firstFields = false;
                            }
                            else
                                m_procsChange.Append(m_storeProcsName + "\t" + m_fieldName + "\t" + lineNo + "\t" + "Deleted" + Environment.NewLine);
                        }
                    }
                    firstFields = true;
                    IDictionaryEnumerator htEnumeratorMod = m_htStoreProcNameMod.GetEnumerator();
                    while (htEnumeratorMod.MoveNext())
                    {
                        m_storeProcsName = htEnumeratorMod.Key.ToString();
                        m_htFieldsMod = (Hashtable)m_htStoreProcNameMod[m_storeProcsName];

                        IDictionaryEnumerator htField = m_htFieldsMod.GetEnumerator();
                        while (htField.MoveNext())
                        {

                            string lineNo = DataLib.Extract(htField.Key.ToString(), ':', 1);
                            m_fieldName = htField.Value.ToString();
                            string newFieldName = "";
                            string oldFieldName = "";

                            if (m_htFieldNameMod.ContainsKey(m_fieldName))
                            { 
                                oldFieldName = DataLib.Extract(m_htFieldNameMod[m_fieldName].ToString(), ':', 1);
                                newFieldName = DataLib.Extract(m_htFieldNameMod[m_fieldName].ToString(),':',2);
                                changedNotes = DataLib.Extract(m_htFieldNameMod[m_fieldName].ToString(), ':', 3);
                            }

                            if (firstFields)
                            {
                                m_procsChange.Append(m_storeProcsName + "\t" + m_fieldName + "\t" + lineNo + "\t" + "Modified" + "\t" + oldFieldName + "\t" + newFieldName + "\t" + changedNotes + Environment.NewLine);
                                firstFields = false;
                            }
                            else
                            {
                                m_procsChange.Append(m_storeProcsName + "\t" + m_fieldName + "\t" + lineNo + "\t" + "Modified" + "\t" + oldFieldName + "\t" + newFieldName + "\t" + changedNotes + Environment.NewLine);
                            }
                        }
                    }

                    //bool firstFields = true;
                    IDictionaryEnumerator htEnumeratorMove = m_htStoreProcNameMove.GetEnumerator();
                    while (htEnumeratorMove.MoveNext())
                    {
                        m_storeProcsName = htEnumeratorMove.Key.ToString();
                        m_htFieldsMove = (Hashtable)m_htStoreProcNameMove[m_storeProcsName];

                        IDictionaryEnumerator htField = m_htFieldsMove.GetEnumerator();
                        while (htField.MoveNext())
                        {

                            string lineNo = DataLib.Extract(htField.Key.ToString(), ':', 1);
                            m_fieldName = htField.Value.ToString();
                            string newFieldName = "";

                            if (m_htFieldNameMove.ContainsKey(m_fieldName))
                                newFieldName = m_htFieldNameMove[m_fieldName].ToString();

                            if (firstFields)
                            {
                                m_procsChange.Append(m_storeProcsName + "\t" + m_fieldName + "\t" + lineNo + "\t" + "Moving" + "\t" + newFieldName + Environment.NewLine);
                                firstFields = false;
                            }
                            else
                                m_procsChange.Append(m_storeProcsName + "\t" + m_fieldName + "\t" + lineNo + "\t" + "Moving" + "\t" + newFieldName + Environment.NewLine);
                        }
                    }
                    m_fileNameByProcs = m_rootFolder + @"\ChangeByProcs" + DataLib.FormatDateForFilename(System.DateTime.Now,"") + ".xls";
                    DataLib.WriteTextFile(m_fileNameByProcs, m_procsChange.ToString());

                }
                if ((m_htDeletedFields.Count > 0) || (m_htModifiedFields.Count > 0) || (m_htMovingFields.Count > 0))
                {
                    m_procsChange = new StringBuilder();
                    m_procsChange.Append("Date: " + System.DateTime.Now + Environment.NewLine);
                    m_procsChange.Append("Field Name" + "\t" + "Store Prodedures/Functions" + "\t" + "Line No" + "\t" + "Changes" + Environment.NewLine);

                    IDictionaryEnumerator htEnumerator = m_htDeletedFields.GetEnumerator();
                    while (htEnumerator.MoveNext())
                    {
                        m_fieldName = htEnumerator.Key.ToString();
                        m_htStoreProcsDel = (Hashtable)m_htDeletedFields[m_fieldName];

                        bool firstProcs = true;
                        IDictionaryEnumerator htProcs = m_htStoreProcsDel.GetEnumerator();
                        while (htProcs.MoveNext())
                        {
                            string lineNo = DataLib.Extract(htProcs.Key.ToString(), ':', 1);
                            m_storeProcsName = htProcs.Value.ToString();

                            if (firstProcs)
                            {
                                m_procsChange.Append(m_fieldName + "\t" + m_storeProcsName + "\t" + lineNo + "\t" + "Deleted" + Environment.NewLine);
                                firstProcs = false;
                            }
                            else
                                m_procsChange.Append(m_fieldName + "\t" + m_storeProcsName + "\t" + lineNo + "\t" + "Deleted" + Environment.NewLine);
                        }
                    }

                    IDictionaryEnumerator htEnumeratorMod = m_htModifiedFields.GetEnumerator();
                    while (htEnumeratorMod.MoveNext())
                    {
                        m_fieldName = htEnumeratorMod.Key.ToString();
                        m_htStoreProcsMod = (Hashtable)m_htModifiedFields[m_fieldName];

                        string newFieldName = "";
                        if (m_htFieldNameMod.ContainsKey(m_fieldName))
                            newFieldName = m_htFieldNameMod[m_fieldName].ToString();

                        bool firstProcs = true;
                        IDictionaryEnumerator htProcs = m_htStoreProcsMod.GetEnumerator();
                        while (htProcs.MoveNext())
                        {
                            string lineNo = DataLib.Extract(htProcs.Key.ToString(), ':', 1);
                            m_storeProcsName = htProcs.Value.ToString();

                            if (firstProcs)
                            {
                                m_procsChange.Append(m_fieldName + "\t" + m_storeProcsName + "\t" + lineNo + "\t" + "Modified" + "\t" + newFieldName + Environment.NewLine);
                                firstProcs = false;
                            }
                            else
                                m_procsChange.Append(m_fieldName + "\t" + m_storeProcsName + "\t" + lineNo + "\t" + "Modified" + "\t" + newFieldName + Environment.NewLine);
                        }
                    }

                    IDictionaryEnumerator htEnumeratorMove = m_htMovingFields.GetEnumerator();
                    while (htEnumeratorMove.MoveNext())
                    {
                        m_fieldName = htEnumeratorMove.Key.ToString();
                        m_htStoreProcsMove = (Hashtable)m_htMovingFields[m_fieldName];

                        string newFieldName = "";
                        if (m_htFieldNameMove.ContainsKey(m_fieldName))
                            newFieldName = m_htFieldNameMove[m_fieldName].ToString();

                        bool firstProcs = true;
                        IDictionaryEnumerator htProcs = m_htStoreProcsMove.GetEnumerator();
                        while (htProcs.MoveNext())
                        {
                            string lineNo = DataLib.Extract(htProcs.Key.ToString(), ':', 1);
                            m_storeProcsName = htProcs.Value.ToString();

                            if (firstProcs)
                            {
                                m_procsChange.Append(m_fieldName + "\t" + m_storeProcsName + "\t" + lineNo + "\t" + "Moving" + "\t" + newFieldName + Environment.NewLine);
                                firstProcs = false;
                            }
                            else
                                m_procsChange.Append(m_fieldName + "\t" + m_storeProcsName + "\t" + lineNo + "\t" + "Moving" + "\t" + newFieldName + Environment.NewLine);
                        }
                    }
                    m_fileNameByFields = m_rootFolder + @"\ChangeByFieldNames" + DataLib.FormatDateForFilename(System.DateTime.Now,"") + ".xls";
                    DataLib.WriteTextFile(m_fileNameByFields, m_procsChange.ToString());

                }
                #endregion
            }
            catch (Exception ex)
            {
                Logger.WriteLog("Error GetFiles " + ex.ToString(), 2);
                m_errorMsg = "Error GetFiles " + Environment.NewLine + ex.ToString();
            }
        }

        private void btnProcess_Click(object sender, EventArgs e)
        {
            try
            {
                bool process = true;

                if (txtScriptFolder.Text == "")
                {
                    MessageBox.Show("Please select folder where scripted Store Procedure/Function store");
                    process = false;
                }

                if (txtDataFile.Text == "")
                {
                    MessageBox.Show("Please select data file");
                    process = false;
                }

                if (process)
                {
                    Cursor = Cursors.WaitCursor;
                    btnProcess.Enabled = false;

                    GetFiles();

                    btnProcess.Enabled = true;
                    Cursor = Cursors.Default;

                    if (m_errorMsg == "")
                        MessageBox.Show("Process Completed");
                    else
                        MessageBox.Show(m_errorMsg);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLog("btnProcess_Click " + ex.ToString(), 2);
            }
        }

        private string FormatDataLine(string dataLine)
        {
            try
            {
                dataLine = dataLine.Replace("=", " = ");
                dataLine = dataLine + " ";
            }
            catch (Exception ex)
            {
                Logger.WriteLog("FormatDataLine " + ex.ToString(), 2);
                m_errorMsg = "FormatDataLine " + Environment.NewLine + ex.ToString();
            }
            return dataLine;
        }
    }
}