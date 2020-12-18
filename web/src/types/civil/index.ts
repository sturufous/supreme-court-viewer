import {adjudicatorRestrictionsInfoType, documentRequestsInfoType} from '../common';
import {civilFileDetailsType} from '../civil/jsonTypes';

export interface partiesInfoType {

    "Party ID": string,
    "Role": string,
    "Counsel": string[],
    "Left/Right": string,
    "First Name": string,
    "Last Name": string,
    "Name": string,
    "ID": string,
    "Index": number
}

export interface actType {
    "Code": string,
    "Description": string
}

export interface documentsInfoType {
    "Seq.": string,
    "Document Type": string,
    "Concluded": string,
    "Next Appearance Date": string, 
    "Category": string,
    "Act": actType[],           
    "Sealed": boolean,
    "Document ID": string,            
    "PdfAvail": boolean, 
    "Date Filed": string,
    "Issues": string[],
    "Comment": string,
    "Filed By Name": string[],
    "Date Granted": string,
    "isChecked":boolean,
    "isEnabled":boolean
}

export interface referenceDocumentsInfoType {
    partyId: string,
    appearanceId: string,
    partyName: string,
    appearanceDate: string,
    descriptionText: string,
    enterDtm: string,
    referenceDocumentTypeDsc: string,
    objectGuid: string,
    isChecked: boolean,
    isEnabled: boolean
}

export interface summaryDocumentsInfoType {
    "Document Type": string,
    "Appearance Date": string,
    "Appearance ID": string,
    "PdfAvail": boolean,
    "isChecked":boolean,
    "isEnabled":boolean    
}

export interface civilFileInformationType {
    "adjudicatorRestrictionsInfo": adjudicatorRestrictionsInfoType[],
    "fileNumber": string,
    "detailsData": civilFileDetailsType,
    "leftPartiesInfo": partiesInfoType[],
    "rightPartiesInfo": partiesInfoType[],
    "isSealed": boolean,
    "documentsInfo": documentsInfoType[],
    "referenceDocumentInfo": referenceDocumentsInfoType[],
    "summaryDocumentsInfo": summaryDocumentsInfoType[],
    "categories": string[],
    "providedDocumentCategories": string[]
}

export interface csrRequestsInfoType {
    pdfFileName: string,
    appearanceId: string
}

export interface civilAppearanceInfoType {
    "fileNo": string,
    "appearanceId": string
    "supplementalEquipmentTxt": string,
    "securityRestrictionTxt": string,
    "outOfTownJudgeTxt": string
}

export interface civilIssueType {
    "Issue": string,
    "Result": string,
    "ResultDsc": string
}

export interface appearanceDocumentsType {
    "Seq.": string,
    "Document Type": string,
    "DocTypeCd": string,
    "ID": string,
    "PdfAvail": boolean,
    "Act": actType[],            
    "Sealed": boolean,
    "Date Filed": string,
    "Result": string,
    "Result Description": string,
    "Issues": civilIssueType[],
    "Index": string
}

export interface currentCounselType {
    "Name": string, 
    "Info": string
}

export interface representativeType {
    "Name": string, 
    "Info": string
}

export interface legalRepresentativeType {
    "Name": string, 
    "Type": string
}

export interface appearancePartiesType {
    "First Name": string,
    "Last Name": string,
    "Name": string,
    "Info": string,
    "Current Counsel": currentCounselType[],
    "Representative": representativeType[],
    "Legal Representative": legalRepresentativeType[],
    "Role": string[]
}

export interface appearanceMethodsType {
    "role": string,
    "method": string
}

export interface appearanceAdditionalInfoType {
    "key": string,
    "value": string
}

export interface civilAppearancesListType {
    "Index": string,
    "Date": string,
    "FormattedDate": string,
    "Document Type": string,
    "Result": string,
    "Result Description": string,
    "Time": string,
    "Reason": string,
    "Reason Description": string,
    "Duration": string,
    "Location": string,
    "Room": string,
    "Status": string,
    "Status Style": string,
    "Presider": string,
    "Judge Full Name": string,
    "Appearance ID": string,
    "Supplemental Equipment": string,
    "Security Restriction": string,
    "OutOfTown Judge": string
}

export interface civilNotesType {
    "NotesFieldName": string,
    "NotesValue": string
}

export interface civilShowSectionsType {
    "Case Details": boolean,
    "Future Appearances": boolean,
    "Past Appearances": boolean
}

export interface fileSearchCivilInfoType {
    "Parties": string[],
    "File Id": string,
    "Level": string
}