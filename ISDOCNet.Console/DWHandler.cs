using DocuWare.Platform.ServerClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ISDOCNet.Console
{
    internal class DWHandler
    {
        private readonly string _serverUrl;
        private readonly string _userName;
        private readonly string _password;

        public DWHandler(string serverUrl, string userName, string password)
        {
            _serverUrl = serverUrl;
            _userName = userName;
            _password = password;
        }

        public Document GetDocument(int documentId, string fc, string dialogId)
        {
            var org = GetOrganization();
            FileCabinet fileCabinet = org.GetFileCabinetsFromFilecabinetsRelation().FileCabinet.FirstOrDefault(x => x.Id == fc);
            DialogInfo dialogInfo = fileCabinet.GetDialogInfosFromDialogsRelation().Dialog.FirstOrDefault(x => x.Id == dialogId).GetDialogFromSelfRelation();
            Dialog dialog = dialogInfo.GetDialogFromSelfRelation();
            DialogExpression dialogExpression = new DialogExpression()
            {
                Operation = DialogExpressionOperation.And,
                Condition = new List<DialogExpressionCondition>()
                {
                        // Store date of document From, To
                        DialogExpressionCondition.Create("DWDOCID", documentId.ToString()),
                },
                Count = 1, // Maximum Number of Documents -1 == all Documents
                SortOrder = new List<SortedField>
                {
                    SortedField.Create("DWSTOREDATETIME", SortDirection.Desc),
                },
            };
            DocumentsQueryResult result = dialog.Query
                                            .PostToDialogExpressionRelationForDocumentsQueryResult(dialogExpression)
                                            .GetDocumentsQueryResultFromSelfRelation();

            return result.Items.FirstOrDefault();
        }

        public Organization GetOrganization()
        {
            var sc = ConnectDocuWare();
            return sc.Organizations.FirstOrDefault();
        }

        public ServiceConnection ConnectDocuWare()
        {
            return ServiceConnection.Create(new Uri(_serverUrl), _userName, _password);
        }
    }
}
