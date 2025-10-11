export interface SpeciesDto {
  id: number;
  name: string;

  requiresPermit: boolean;
  permitName?: string | null;
  permitAuthority?: string | null;
  permitNotes?: string | null;
}

export interface CreateSpeciesDto {
  name: string;

  requiresPermit: boolean;
  permitName?: string | null;
  permitAuthority?: string | null;
  permitNotes?: string | null;
}