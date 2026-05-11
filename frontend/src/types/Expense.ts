export const ExpenseCategory = {
  Fuel: 0,
  Maintenance: 1,
  Insurance: 2,
  Registration: 3,
  Repairs: 4,
  Parking: 5,
  Tolls: 6,
  CarWash: 7,
  Modifications: 8,
  Other: 9,
} as const;

export type ExpenseCategory = typeof ExpenseCategory[keyof typeof ExpenseCategory];

export interface Expense {
  id: number;
  category: number;
  categoryName: string;
  amount: number;
  date: string;
  notes?: string;
  vehicleId: number;
  vehicleMake?: string;
  vehicleModel?: string;
}

export interface CreateExpenseDto {
  category: number;
  amount: number;
  date: string;
  notes?: string;
  vehicleId: number;
}

export interface UpdateExpenseDto {
  category: number;
  amount: number;
  date: string;
  notes?: string;
}
