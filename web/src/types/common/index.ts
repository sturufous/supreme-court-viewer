import { csrRequestsInfoType } from "../civil";
import { ropRequestsInfoType } from "../criminal";

export interface InputNamesType {
    lastName: string;
    givenName: string;
}

export interface DurationType {
    hr: string;
    min: string;
}

export interface IconStyleType {
    icon: string;
    desc: string;
}

export interface IconInfoType {
    info: string;
    desc: string;
}

export interface AdditionalProperties {
    additionalProp1: {};
    additionalProp2: {};
    additionalProp3: {};
}

export interface AdjudicatorRestrictionsInfoType {
    adjRestriction: string;
    adjudicator: string;
    fullName: string;
    status: string;
    appliesTo: string;
}

export interface DocumentRequestsInfoType {
    isCriminal: boolean;
    pdfFileName: string;
    base64UrlEncodedDocumentId: string;
    fileId: string;
}

export interface CancelPreDownloadInfoType {
    transferIds: string[];
}

export interface ArchiveInfoType {
    zipName: string;
    vcCivilFileId?: string;
    csrRequests: csrRequestsInfoType[];
    documentRequests: DocumentRequestsInfoType[];
    ropRequests: ropRequestsInfoType[];
}

export interface CourtRoomsJsonInfoType {
    name: string;
    code: string;
    locationId: string;
    active: boolean;
    courtRooms: CourtRoomsInfo[];   
}

export interface CourtRoomsInfo {
    room: string;
    locationId: string;
    type: string;
}

export interface UserInfo {
    userType: string;
    role: string;
    subRole: string;
    isSupremeUser: string;
}