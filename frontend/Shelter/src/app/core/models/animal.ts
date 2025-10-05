export type Sex = 'Unknown' | 'Female' | 'Male'
export type AnimalStatus = 'Available'| 'Reserved'| 'Adopted'| 'NotAvailable'

export interface AnimalDto  {
    id: number,
    name: string,
    speciesId: number,
    speciesName: string,
    birthDate: string,
    sex: Sex,
    status: AnimalStatus,
    ageYears:  number,
    ageMonths: number,
    ageLabel: string
    description: string,
    vaccinated: boolean,
    neutered: boolean,
    createdAt: string,
    photoUrl: string,
}
export interface CreateAnimalDto {
    name: string,
    speciesId: number,
    birthDate: string,
    sex: Sex,
    status: AnimalStatus,
    description: string,
    vaccinated: boolean,
    neutered: boolean,
    photoUrl: string
}