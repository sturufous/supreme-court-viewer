import { CourtDocumentType, DocumentData } from "../types/shared";
import base64url from "base64url";

export default {
  openDocumentsPdf(documentType: CourtDocumentType, documentData: DocumentData): void {
    const fileName = this.generateFileName(documentType, documentData);
    const isCriminal = documentType == CourtDocumentType.Criminal;
    const documentId = documentData.documentId
      ? base64url(documentData.documentId)
      : documentData.documentId;
    switch (documentType) {
      case CourtDocumentType.CSR:
        window.open(
          `${process.env.BASE_URL}api/files/civil/court-summary-report/${documentData.appearanceId}/${fileName}?vcCivilFileId=${documentData.fileId}`
        );
        break;
      case CourtDocumentType.ROP:
        window.open(
          `${process.env.BASE_URL}api/files/criminal/record-of-proceedings/${documentData.partId}/${fileName}?profSequenceNumber=${documentData.profSeqNo}&courtLevelCode=${documentData.courtLevel}&courtClassCode=${documentData.courtClass}`
        );
        break;
      default:
        window.open(
          `${process.env.BASE_URL}api/files/document/${documentId}/${fileName}?isCriminal=${isCriminal}&fileId=${documentData.fileId}`
        );
        break;
    }
  },

  generateFileName(documentType: CourtDocumentType, documentData: DocumentData): string {
    debugger;
    const locationAbbreviation = (documentData.location.match(/[A-Z]/g) || []).join("");
    switch (documentType) {
      case CourtDocumentType.Civil:
        return `${locationAbbreviation}-${documentData.courtLevel}-${documentData.fileNumberText}-${documentData.documentDescription}-${documentData.dateFiled}-${documentData.documentId}.pdf`;
      case CourtDocumentType.ProvidedCivil:
        return `${locationAbbreviation}-${documentData.courtLevel}-${documentData.fileNumberText}-${documentData.documentDescription}-${documentData.appearanceDate}-${documentData.partyName}.pdf`;
      case CourtDocumentType.CSR:
        return `${locationAbbreviation}-${documentData.courtLevel}-${documentData.fileNumberText}-${documentData.documentDescription}-${documentData.appearanceDate}.pdf`;
      case CourtDocumentType.Criminal:
        return `${locationAbbreviation}-${documentData.courtLevel}-${documentData.courtClass}-${documentData.fileNumberText}-${documentData.documentDescription}-${documentData.dateFiled}-${documentData.documentId}.pdf`;
      case CourtDocumentType.ROP:
        return `${locationAbbreviation}-${documentData.courtLevel}-${documentData.courtClass}-${documentData.fileNumberText}-${documentData.documentDescription}-${documentData.partId}.pdf`;
      case CourtDocumentType.CivilZip:
        return `${locationAbbreviation}-${documentData.courtLevel}-${documentData.fileNumberText}-documents.zip`
      case CourtDocumentType.CriminalZip:
          return `${locationAbbreviation}-${documentData.courtLevel}-${documentData.courtClass}-${documentData.fileNumberText}-documents.zip`
      default:
        throw Error(`No file structure for type: ${documentType}`);
    }
  },
};
