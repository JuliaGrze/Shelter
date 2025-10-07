import { AnimalStatus, Sex } from "./animal";

export type SortBy = 'name' | 'species' | 'birthDate' | 'sex' | 'status' | 'createdAt';
export type SortDir = 'asc' | 'desc';

export interface AnimalQuery {
  // sorting
  sortBy?: SortBy;
  sortDir?: SortDir;

  // pagination
  page?: number;
  size?: number;

  // filtering
  speciesId?: number | null;
  sex?: Sex;
  status?: AnimalStatus;

  // age in months
  ageMinMonths?: number;
  ageMaxMonths?: number;

  // created range (ISO date strings)
  createdFrom?: string; // 'yyyy-MM-dd'
  createdTo?: string;   // 'yyyy-MM-dd'

  vaccinated?: boolean;
  neutered?: boolean;  
}
