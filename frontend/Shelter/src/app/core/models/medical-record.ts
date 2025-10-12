export type MedicalRecordType =
    | 'Vaccination'
    | 'Deworming'
    | 'Sterilization'
    | 'Microchip'
    | 'Illness';

export interface MedicalRecord {
    id: number;
    animalId: number;
    type: MedicalRecordType;
    date: string;              // 'YYYY-MM-DD'
    nextDueDate?: string | null; // może być null
    vet?: string | null;
    notes?: string | null;
}

export interface CreateMedicalRecord {
    animalId: number;
    type: MedicalRecordType;
    date: string;              // 'YYYY-MM-DD'
    nextDueDate?: string | null; // może być null
    vet?: string | null;
    notes?: string | null;
}

export interface DueMedicalRecordDto{
    id: number
    animalId: number
    animalName: string
    type: MedicalRecordType
    date: string
    nextDueDate?: string | null
    daysUntilDue?: number | null
    overdue: boolean
}