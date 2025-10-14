export interface OneTimeDto {
  amountMinor: number;
  currency?: string;
  donorPublicName?: string;
  isPublic: boolean;
  message?: string;
}

export interface RecurringDto {
  amountMinor: number;
  currency: string;
  donorPublicName?: string;
  isPublic: boolean;
  message?: string;
}

export interface DonorWallItemDto {
  donorPublicName: string;
  amountMinor: number;
  currency: string;
  isRecurring: boolean; 
  message?: string | null;
  createdAt: string;
}

export interface MonthlySumDto {
  year: number;
  month: number;
  currency: string;
  amountMinor: number;
  count: number;
}