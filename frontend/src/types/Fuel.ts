export const EnergyType = {
  Gasoline: 0,
  Diesel: 1,
  Electricity: 2
} as const;

export type EnergyType = typeof EnergyType[keyof typeof EnergyType];

export interface FuelEntry {
  id: number;
  energyType: number;
  energyTypeDisplay: string;
  amount: number;
  unit: string;  
  cost: number;
  costPerUnit: number; 
  odometer?: number | null;
  date: string;
  vehicleId: number;
  // Vehicle details
  vehicleMake: string;  
  vehicleModel: string; 
  vehicleType: number; 
}

export interface CreateFuelEntryDto {
  energyType: number;
  amount: number;
  cost: number;
  odometer?: number | null;
  date: string;
  vehicleId: number;
}

export interface UpdateFuelEntryDto {
  energyType?: number;
  amount?: number;
  cost?: number;
  odometer?: number | null; 
  date?: string;
}

export interface FuelEfficiencyDto {
  vehicleId: number;
  totalCost: number;
  totalAmount: number;
  totalKilometers: number;
  averageLitersPer100Km: number | null;
  averageKilometersPerLiter: number | null;
  averageCostPerKilometer: number | null;
  averageCostPerFillUp: number;
  totalFillUps: number;
  entriesWithOdometer: number;
  entriesWithoutOdometer: number;
}
