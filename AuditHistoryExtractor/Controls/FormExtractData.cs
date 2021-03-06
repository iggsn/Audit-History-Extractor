﻿using Microsoft.Xrm.Sdk;
using AuditHistoryExtractor.AppCode;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using AuditHistoryExtractor.Classes;

namespace AuditHistoryExtractor.Controls
{
    public partial class FormExtractData : Form
    {
        private const string MessageAuditHistoryExtracted = "Audit History extracted successfully";
        private const string TitleExportSuccess = "Export success";

        public delegate void ExtractCompleted(List<AuditHistory> lsAuditHistory);
        public event ExtractCompleted OnExtractCompleted;

        #region Variables
        IOrganizationService Service;


        List<AuditHistory> lsStory = new List<AuditHistory>();

        List<Entity> entitiesList;
        string identificator, fieldToExtract, fileName;
        bool allFields;
        #endregion

        #region Business Logics
        public FormExtractData(IOrganizationService service, List<Entity> entitiesList)
        {
            Service = service;
            this.entitiesList = entitiesList;
    
            InitializeComponent();
        }

        public void RetrieveAuditHistoryForRecords(string identificator, bool allFields, string fieldToExtract)
        {
            this.fileName = fileName;
            this.identificator = identificator;

            this.fieldToExtract = fieldToExtract;
            this.allFields = allFields;

            if (BackgroundWorkerExtractAuditHistory.IsBusy != true)
            {
                progressExportData.Step = 1;
                progressExportData.Maximum = entitiesList.Count;
                this.Focus();

                BackgroundWorkerExtractAuditHistory.RunWorkerAsync();
            }
        }
        #endregion

        #region Events
        private void btnCancel_Click(object sender, EventArgs e)
        {
            BackgroundWorkerExtractAuditHistory.CancelAsync();
        }

        private void BackgroundWorkerExtractAuditHistory_DoWork(object sender, DoWorkEventArgs e)
        {
            AuditHistoryManager auditHistoryManager = new AuditHistoryManager(Service);
            foreach (Entity entity in entitiesList)
            {
                if (BackgroundWorkerExtractAuditHistory.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                try
                {
                    string keyValue = string.IsNullOrEmpty(entity.GetAttributeValue<string>(identificator)) ? string.Empty : entity.GetAttributeValue<string>(identificator);

                    if (allFields)
                    {
                        lsStory.AddRange(auditHistoryManager.GetAuditHistoryForRecord(entity.Id, entity.LogicalName, entity.GetAttributeValue<string>(identificator)));
                    }
                    else
                    {
                        lsStory.AddRange(auditHistoryManager.GetAuditHistoryForRecordAndField(entity.Id, entity.LogicalName, fieldToExtract, entity.GetAttributeValue<string>(identificator)));

                    }


                    BackgroundWorkerExtractAuditHistory.ReportProgress(1, keyValue);
                }
                catch (Exception ex)
                {
                    DialogResult dialogResult = MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK);
                    return;
                }
            }



            //MessageBox.Show(MessageAuditHistoryExtracted, TitleExportSuccess, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BackgroundWorkerExtractAuditHistory_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            lblCurrentRow.Text = e.UserState.ToString();
            progressExportData.PerformStep();
        }

        private void BackgroundWorkerExtractAuditHistory_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (OnExtractCompleted != null)
            {
                OnExtractCompleted(lsStory);
            }
            this.Close();
        }
        #endregion
    }
}
