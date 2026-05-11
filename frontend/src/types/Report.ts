export interface TcoReportDto {
  vehicleId: number;
  vehicleMake: string;
  vehicleModel: string;
  vehicleYear: number;
  purchasePrice: number;
  ownershipStart: string;
  ownershipEnd?: string;
  ownershipDays: number;
  totalFuelCost: number;
  totalExpensesCost: number;
  totalCost: number;
  expensesByCategory: { [key: string]: number };
  totalKilometers?: number;
  costPerKilometer?: number;
  fuelCostPerKilometer?: number;
  expensesCostPerKilometer?: number;
  costPerDay: number;
  costPerMonth: number;
  totalFuelEntries: number;
  totalExpenseEntries: number;
}

export interface CategoryBreakdownItem {
  category: string;
  amount: number;
  percentage: number;
  count: number;
}

export interface CostBreakdownDto {
  vehicleId: number;
  vehicleMake: string;
  vehicleModel: string;
  purchasePrice: number;
  totalFuelCost: number;
  totalExpensesCost: number;
  totalCost: number;
  categoryBreakdown: CategoryBreakdownItem[];
}

export interface MonthlyDataPoint {
  year: number;
  month: number;
  monthName: string;
  fuelCost: number;
  expensesCost: number;
  totalCost: number;
  fuelEntries: number;
  expenseEntries: number;
}

export interface MonthlyCostTrendDto {
  vehicleId: number;
  vehicleMake: string;
  vehicleModel: string;
  monthlyData: MonthlyDataPoint[];
}

export interface VehicleSummaryItem {
  vehicleId: number;
  make: string;
  model: string;
  year: number;
  purchasePrice: number;
  totalCost: number;
  monthlyAverage: number;
}

export interface VehicleSummaryDto {
  totalVehicles: number;
  totalInvestment: number;
  totalFuelCost: number;
  totalExpensesCost: number;
  grandTotalCost: number;
  vehicles: VehicleSummaryItem[];
}
