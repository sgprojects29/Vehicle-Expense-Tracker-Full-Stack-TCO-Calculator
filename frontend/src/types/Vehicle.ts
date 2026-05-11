export const VehicleType = {
  Gasoline: 0,
  Diesel: 1,
  Electric: 2,
  Hybrid: 3,
  PlugInHybrid: 4,
} as const;

export type VehicleType = typeof VehicleType[keyof typeof VehicleType];

export interface Vehicle {
  id: number;
  make: string;
  model: string;
  year: number;
  purchasePrice: number;
  ownershipStart: string;
  ownershipEnd?: string;
  vehicleType: number;
  vehicleTypeDisplay: string;
  userId: string;
}

export interface CreateVehicleDto {
  make: string;
  model: string;
  year: number;
  purchasePrice: number;
  ownershipStart: string;
  ownershipEnd?: string;
  vehicleType: number;
}

export interface UpdateVehicleDto {
  make: string;
  model: string;
  year: number;
  purchasePrice: number;
  ownershipStart: string;
  ownershipEnd?: string;
  vehicleType: number;
}
