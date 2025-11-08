export interface SubmitApplicationRequest {
    // backend takes animalId in route; include optional answers if present in your DTO later
    notes?: string | null;
}

export interface SubmitApplicationResponse {
    applicationId: number;
    statusCode: string;
}

export interface AdoptionApplicationDto {
    id: number;
    animalId: number;
    applicationUserId: string;
    adoptionStatusId: number;
    adoptionStatusCode: string;
    createdAt: string;
    notes?: string | null;
    homeVisit?: {
        scheduledAt?: string | null;
        address?: string | null;
        resultCode?: string | null; // Pending/Passed/Failed/Cancelled/Rescheduled
    } | null;
    contract?: {
        id?: number | null;
        pdfUrl?: string | null;
        pdfHash?: string | null;
        signedAt?: string | null;
    } | null;
}

export interface UpdateAdoptionStatusRequest {
    notes?: string | null;
}

export interface ScheduleHomeVisitRequest {
    scheduledAt: string; // ISO
    address: string;
    notes?: string | null;
}

export interface SetHomeVisitResultRequest {
    resultCode: 'Pending' | 'Passed' | 'Failed' | 'Cancelled' | 'Rescheduled';
    notes?: string | null;
}

export interface GenerateContractResponse {
    contractId: number;
    pdfUrl: string;
    pdfHash: string;
    verificationQrContent: string;
}

export type Role = 'Guest' | 'Client' | 'Worker' | 'Admin';

// A tiny status map for UI chips
export const STATUS_COLORS: Record<string, 'primary' | 'accent' | 'warn' | undefined> = {
    Submitted: 'primary',
    InReview: 'accent',
    Approved: 'primary',
    Rejected: 'warn',
    HomeVisitScheduled: 'accent',
    HomeVisitPassed: 'primary',
    HomeVisitFailed: 'warn',
    ContractGenerated: 'accent',
    ContractSigned: 'primary',
};
export type AdoptionStatusCode =
  | 'Submitted'
  | 'InReview'
  | 'Approved'
  | 'Rejected'
  | 'HomeVisitScheduled'
  | 'HomeVisitPassed'
  | 'HomeVisitFailed'
  | 'ContractGenerated'
  | 'ContractSigned';
  
export interface AdoptionListItemDto {
  id: number;
  animalId: number;
  animalName: string;
  animalSpecies: string;
  animalGender: string;
  animalPhotoUrl?: string | null;

  applicantEmail: string;
  statusCode: AdoptionStatusCode | string; 
  createdAt: string; // ISO
}
