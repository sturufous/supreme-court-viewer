import { csrRequestsInfoType } from "../civil";
import { ropRequestsInfoType } from "../criminal";

export interface inputNamesType {
    lastName: string;
    givenName: string;
}

export interface durationType {
    hr: string;
    min: string;
}

export interface iconStyleType {
    icon: string;
    desc: string;
}

export interface iconInfoType {
    info: string;
    desc: string;
}

export interface additionalProperties {
    additionalProp1: {};
    additionalProp2: {};
    additionalProp3: {};
}

export interface adjudicatorRestrictionsInfoType {
    adjRestriction: string;
    adjudicator: string;
    fullName: string;
    status: string;
    appliesTo: string;
}

export interface documentRequestsInfoType {
    isCriminal: boolean;
    pdfFileName: string;
    base64UrlEncodedDocumentId: string;
    fileId: string;
}

export interface archiveInfoType {
    zipName: string;
    vcCivilFileId?: string;
    csrRequests: csrRequestsInfoType[];
    documentRequests: documentRequestsInfoType[];
    ropRequests: ropRequestsInfoType[];
}

export interface courtRoomsJsonInfoType {
    name: string;
    code: string;
    locationId: string;
    active: boolean;
    courtRooms: courtRoomsInfo[];   
}

export interface courtRoomsInfo {
    room: string;
    locationId: string;
    type: string;
}