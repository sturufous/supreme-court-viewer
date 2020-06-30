import {additionalProperties} from '../../common';

export interface partyCounselType {
    "counselId": string,
    "fullNm": string,
    "phoneNumberTxt": string,
    "additionalProperties": additionalProperties,
    "additionalProp1": {},
    "additionalProp2": {},
    "additionalProp3": {}
}

export interface partyType {
    "fullName": string,
    "roleTypeDescription": string,
    "partyId": string,
    "lastNm": string,
    "givenNm": string,
    "orgNm": string,
    "roleTypeCd": string,
    "leftRightCd": string,
    "selfRepresentedYN": string,
    "counsel": partyCounselType[],
    "additionalProperties": additionalProperties,
    "additionalProp1": {},
    "additionalProp2": {},
    "additionalProp3": {}
}

export interface civilDocumentIssueType {
    "issueTypeDesc": string,
    "issueResultCdDesc": string,
    "issueNumber": string,
    "issueTypeCd": string,
    "issueDsc": string,
    "concludedYn": string,
    "issueResultCd": string,
    "issueResultDsc": string,
    "additionalProperties": additionalProperties,
    "additionalProp1": {},
    "additionalProp2": {},
    "additionalProp3": {}
}

export interface civilDocumentSupportType {
    "actCd": string,
    "actDsc": string,
    "additionalProperties": additionalProperties,
    "additionalProp1": {},
    "additionalProp2": {},
    "additionalProp3": {}
}

export interface civilDocumentAppearanceType {
    "appearanceId": string,
    "appearanceDate": string,
    "courtAgencyIdentifier": string,
    "courtRoom": string,
    "appearanceReason": string,
    "additionalProperties": additionalProperties,
    "additionalProp1": {},
    "additionalProp2": {},
    "additionalProp3": {}
}

export interface civilDocumentType {    
    "category": string,
    "documentTypeDescription": string,
    "nextAppearanceDt": string,
    "issue": civilDocumentIssueType[],
    "civilDocumentId": string,
    "imageId": string,
    "fileSeqNo": string,
    "documentTypeCd": string,
    "filedDt": string,
    "filedByName": string,
    "commentTxt": string,
    "concludedYn": string,
    "lastAppearanceId": string,
    "lastAppearanceDt": string,
    "lastAppearanceTm": string,
    "sealedYN": string,
    "DateGranted": string,
    "documentSupport": civilDocumentSupportType[],
    "appearance": civilDocumentAppearanceType[],
    "additionalProperties": additionalProperties,
    "additionalProp1": {},
    "additionalProp2": {},
    "additionalProp3": {}        
}

export interface civilHearingRestrictionType {
    "hearingRestrictionTypeDsc": string,
    "hearingRestrictionId": string,
    "adjPartId": string,
    "adjFullNm": string,
    "hearingRestrictionTypeCd": string,
    "applyToNm": string,
    "civilDocumentId": string,
    "physicalFileId": string,
    "adjInitialsTxt": string,
    "hearingRestrictionCcn": string,
    "additionalProperties": additionalProperties,
    "additionalProp1": {},
    "additionalProp2": {},
    "additionalProp3": {}
}

export interface civilApprDetailType {
    "historyYN": string,
    "appearanceId": string,
    "appearanceDt": string,
    "appearanceTm": string,
    "appearanceReasonCd": string,
    "courtAgencyId": string,
    "courtRoomCd": string,
    "judgeFullNm": string,
    "judgeInitials": string,
    "estimatedTimeHour": string,
    "estimatedTimeMin": string,
    "partOfTrialYN": string,
    "appearanceStatusCd": string,
    "appearanceResultCd": string,
    "appearanceCcn": string,
    "documentTypeCd": string,
    "documentRecCount": string,
    "supplementalEquipmentTxt": string,
    "securityRestrictionTxt": string,
    "outOfTownJudgeTxt": string,
    "additionalProperties": additionalProperties,
    "additionalProp1": {},
    "additionalProp2": {},
    "additionalProp3": {}
}

export interface civilAppearancesType {
    "responseCd": string,
    "responseMessageTxt": string,
    "futureRecCount": string,
    "historyRecCount": string,
    "apprDetail": civilApprDetailType[],
    "additionalProperties": additionalProperties,
    "additionalProp1": {},
    "additionalProp2": {},
    "additionalProp3": {}
}

export interface civilFileDetailsType {    
    "responseCd": string,
    "responseMessageTxt": string,
    "physicalFileId": string,
    "fileNumberTxt": string,
    "socTxt": string,
    "courtClassCd": string,
    "courtClassDescription": string,
    "courtLevelCd": string,
    "courtLevelDescription": string,
    "activityClassCd": string,
    "activityClassDesc": string,
    "sealedYN": string,
    "leftRoleDsc": string,
    "rightRoleDsc": string,
    "homeLocationAgenId": string,
    "homeLocationAgencyName": string,
    "homeLocationAgencyCode": string,
    "homeLocationRegionName": string,
    "trialRemarkTxt": string,
    "commentToJudgeTxt": string,
    "sheriffCommentText": string,
    "fileCommentText": string,
    "party": partyType[],
    "document": civilDocumentType[],
    "hearingRestriction": civilHearingRestrictionType[],
    "appearances": civilAppearancesType
}
