using System;
using System.Collections.Generic;

namespace CompassionConnectModels.Sbc
{
    public class CommunicationKit
    {
        public Beneficiary Beneficiary { get; set; }

        public FieldOffice FieldOffice { get; set; }

        public string FontSize { get; set; }

        public string Font { get; set; }

        public GlobalPartner GlobalPartner { get; set; }

        public ImplementingChurchPartner ImplementingChurchPartner { get; set; }

        public List<Page> Pages { get; set; }

        public string Direction { get; set; }

        public string PrintType { get; set; }

        public string RelationshipType { get; set; }

        public string SBCGlobalStatus { get; set; }

        public List<string> SBCTypes { get; set; }

        public string CompassionSBCId { get; set; }

        public DateTime CreatedDate { get; set; }

        public string FinalLetterURL { get; set; }

        public string GlobalPartnerSBCId { get; set; }

        public bool IsFinalLetter { get; set; }

        public bool IsFinalLetterArchived { get; set; }

        public bool IsOriginalLetterArchived { get; set; }

        public bool IsOriginalLetterMailed { get; set; }

        public bool IsReadBySupporter { get; set; }

        public bool ItemNotScannedEligible { get; set; }

        public bool ItemNotScannedNotEligible { get; set; }

        public bool IsMarkedForRework { get; set; }

        public int NumberOfPages { get; set; }

        public string OriginalLanguage { get; set; }

        public string OriginalLetterURL { get; set; }

        public string PerceptiveTransactionId { get; set; }

        public string ReasonForRework { get; set; }

        public string ReworkComments { get; set; }

        public string SourceSystem { get; set; }

        public string Template { get; set; }

        public string TranslatedBy { get; set; }

        public string TranslationLanguage { get; set; }

        public Supporter Supporter { get; set; }
    }
}
