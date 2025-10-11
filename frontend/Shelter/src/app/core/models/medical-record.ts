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

// zwraca true, gdy następny termin (nextDueDate) jest w ciągu najbliższych 7 dni
export function isUrgent(r: MedicalRecord): boolean {
    if (!r.nextDueDate) return false;
    const d = new Date(r.nextDueDate).getTime();
    const days = Math.ceil((d - Date.now()) / (1000 * 60 * 60 * 24));
    return days >= 0 && days <= 7;
}
