import {iconStyleType} from '../common';
import {courtListType} from '../courtlist/jsonTypes';

export interface courtListInformationInfoType {
    "detailsData": courtListType
}

export interface roomsInfoType {
    "value": string,
    "text": string
}

export interface courtRoomsAndLocationsInfoType {
    "value": locationInfoType,
    "text": string
}

export interface locationInfoType {
    "Location": string,
    "LocationID": string,
    "Rooms" : roomsInfoType[]

}

export interface civilListInfoType {
    "Index": string,
    "Seq.": number,
    "File Number": string,
    "Tag": string,
    "Icons": iconStyleType[],
    "Time": string,
    "Room": string,
    "Parties": string,
    "PartiesTruncApplied": boolean,
    "PartiesDesc": string,
    "Reason": string,
    "ReasonDesc": string,
    "Est.": string,
    "Supplemental Equipment": string,
    "Security Restriction": string,
    "OutOfTown Judge": string,
    "Counsel": string,
    "CounselDesc": string,
    "FileID": string,
    "AppearanceID": string,
    "File Markers": fileMarkerInfoType[],
    "Notes": civilNotesInfoType,
    "NoteExist": boolean
}

export interface fileMarkerInfoType {
    "abbr": string,
    "key": string
}

export interface civilNotesInfoType {
    "TrialNotes": string,
    "FileComment": string,
    "CommentToJudge": string,
    "SheriffComment":string
}

export interface criminalListInfoType {
    "Index": string,
    "Seq.": number,
    "File Number": string,
    "Tag": string,    
    "Icons": iconStyleType[],
    "Case Age": string,
    "Time": string,
    "Room": string,
    "Accused": string,
    "AccusedTruncApplied": boolean,
    "AccusedDesc": string,
    "Reason": string,
    "ReasonDesc": string,
    "Crown": string,
    "CrownDesc": string,
    "Est.": string,
    "Supplemental Equipment": string,
    "Security Restriction": string,
    "OutOfTown Judge": string,
    "Counsel": string,
    // "CounselDesc": string,
    "PartID": string,
    "JustinNo": string,
    "AppearanceID": string,
    "File Markers": fileMarkerInfoType[],
    "TrialNotes": string,
    "TrialRemarks": trialRemarkInfoType[]
    "Notes": criminalNotesInfoType,
    "NoteExist": boolean
}

export interface criminalNotesInfoType {
    "remarks": trialRemarkInfoType[],
    "text": string
}

export interface trialRemarkInfoType {    
    "txt": string
}