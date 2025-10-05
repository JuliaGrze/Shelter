export type SortBy = 'name' | 'species' | 'birthDate' | 'sex' | 'status' | 'createdAt';
export type SortDir = 'asc' | 'desc';

export interface AnimalQuery {
  sortBy?: SortBy;
  sortDir?: SortDir;
  page?: number;
  size?: number;
}
