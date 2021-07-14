import {adjudicatorRestrictionsInfoType} from '../common';
import {criminalFileDetailsType, documentType, countType} from '../criminal/jsonTypes';

export interface participantListInfoType {
    "Index": string,
    "First Name": string,
    "Last Name": string,
    "Name": string,
    "D.O.B.": string,
    "Part ID": string,
    "Prof Seq No": string,
    "Charges": chargesInfoType[],         
    "Status": statusFieldsType[],
    "DocumentsJson": documentType[],
    "CountsJson": countType[],
    "Counsel": string,
    "Counsel Designation Filed": string,
    "Documents": participantDocumentsInfoType[],
    "Record of Proceedings": participantROPInfoType[]
}

export interface chargesInfoType {
    "Description": string,
    "Code": string
}

export interface statusFieldsType {
    "key": string,
    "abbr": string,
    "code":string
}

export interface criminalFileInformationType {
    "participantList": participantListInfoType[],
    "adjudicatorRestrictionsInfo": adjudicatorRestrictionsInfoType[],
    "bans": bansInfoType[],
    "courtLevel": string,
    "courtClass": string,
    "detailsData": criminalFileDetailsType,
    "fileNumber": string
}

export interface bansInfoType {
    "Ban Participant": string,
    "Ban Type": string,
    "Order Date": string,
    "Act": string,
    "Sect.": string,
    "Sub": string,
    "Description": string,
    "Comment": string    
}

export interface criminalCrownInformationInfoType {
    "CrownInfoFieldName": string,
    "CrownInfoValue": string
}

export interface criminalCrownNotesInfoType {
    "CrownNotes": string
}

export interface participantDocumentsInfoType {
    "Date": string,
    "Document Type": string,
    "Category": string,
    "Pages": string,
    "PdfAvail": boolean,
    "Image ID": string,
    "Status": string,
    "Status Date": string,
    "isChecked":boolean,
    "isEnabled":boolean 
}

export interface ropRequestsInfoType {
    pdfFileName: string,
    partId: string,
    profSequenceNumber: string,
    courtLevelCode: string,
    courtClassCode: string
}

export interface participantROPInfoType {    
    "Document Type": string,
    "Category": string,
    "Pages": string,
    "PdfAvail": boolean,
    "Index": string,
    "Part ID": string,
    "Prof Seq No": string,
    "isChecked":boolean,
    "isEnabled":boolean 
} 

export interface participantFilesInfoType {
    "Documents": participantDocumentsInfoType[],
    "Record of Proceedings": participantROPInfoType[]
}

export interface criminalAppearancesListType {
    "Index": string,
    "Date": string,
    "FormattedDate": string,
    "Time": string,
    "Reason": string,
    "Reason Description": string,
    "Duration": string,
    "Location": string,
    "Room": string,
    "First Name": string,
    "Last Name": string,
    "Accused": string,
    "Status": string,
    "Status Style": string,
    "Presider": string,
    "Judge Full Name": string,
    "Appearance ID": string,
    "Part ID": string,
    "Supplemental Equipment": string,
    "Security Restriction": string,
    "OutOfTown Judge": string
}

export interface criminalAppearanceInfoType {
    "fileNo": string,
    "courtLevel": string,
    "courtClass": string,
    "appearanceId": string,
    "partId": string,
    "supplementalEquipmentTxt": string,
    "securityRestrictionTxt": string,
    "outOfTownJudgeTxt": string,
    "profSeqNo": string,
    "date": string
}

export interface criminalAppearanceMethodsInfoType {
    "role": string,
    "method": string,
    "instruction": string,
    "phoneNumber": string
}

export interface appearanceChargesInfoType {
    "Count": string,
    "Criminal Code": string,
    "Description": string,
    "Last Result": string,
    "Last Result Description": string,
    "Finding": string,
    "Finding Description": string
}

export interface appearanceMethodDetailsInfoType {
    "Name": string,
    "Role": string,
    "Attendance": string,
    "Appearance": string,
    "PartyAppearance": string
}

export interface appearanceAdditionalInfoType {
    "key": string,
    "value": string
}

export interface witnessListInfoType {
    "First Name": string,
    "Last Name": string,
    "Name": string,           
    "Type": string,
    "Required": string,
    "Agency": string,
    "Pin Code": string,
    "Type Category": string
}

export interface witnessCountInfoType {
    "WitnessCountFieldName": string,    
    "WitnessCountValue": number
}

export interface participantSentencesInfoType {
    "Index": string,
    "Last Name": string,
    "Name": string,
    "CountsJson": countType[],
    "Counts": countInfoType[],
    "CountsDisable": boolean,
    "OrderMade": countInfoType[],
    "OrderMadeDisable": boolean,
    "JudgesRecommendation": countInfoType[],
    "RecommendationDisable": boolean,
}

export interface countInfoType {

    "Date": string,
    "FormattedDate": string,
    "Finding": string,
    "FindingDsc": string,                                   
    "ChargeIssueCd": string[],
    "ChargeIssueDsc": string[],
    "ChargeIssueDscFull": string[],
    "Count": string,
    "Sentence/Disposition Type": string[],
    "SentenceDsc": string[],
    "Term": string[],
    "Amount": string[],
    "Due Date/ Until": string[],
    "Effective Date": string[],
    "OrderMade": string[],
    "JudgeRecommendation": string[],
    "LenCharge": number,
    "Len": number
}

export interface participantSentencesDetailsInfoType {
    "participantSentences": participantSentencesInfoType[],
    "selectedParticipant": number
}

export interface showSectionsInfoType {
    "Case Details": boolean,
    "Future Appearances": boolean,
    "Past Appearances": boolean, 
    "Witnesses": boolean,
    "Documents": boolean,
    "Sentence/Order Details": boolean
}

export interface fileSearchCriminalInfoType {
    "Participants": string[],
    "File Id": string,
    "Level": string
}

export interface initiatingDocument {
    imageId: string,
    issueDate: string,
}