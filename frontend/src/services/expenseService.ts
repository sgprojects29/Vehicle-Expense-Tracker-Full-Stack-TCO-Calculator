import apiClient from './api';
import type { Expense, CreateExpenseDto, UpdateExpenseDto } from '../types/Expense';

export const expenseService = {
async getAll(
    vehicleId?: number, 
    category?: number,
    startDate?: string,
    endDate?: string
  ): Promise<Expense[]> {
    const params = new URLSearchParams();
    if (vehicleId !== undefined) params.append('vehicleId', vehicleId.toString());
    if (category !== undefined) params.append('category', category.toString());
    if (startDate) params.append('startDate', startDate);
    if (endDate) params.append('endDate', endDate);
    
    const queryString = params.toString();
    const url = queryString ? `/expenses?${queryString}` : '/expenses';
    
    const response = await apiClient.get<Expense[]>(url);
    return response.data;
  },

  async getById(id: number): Promise<Expense> {
    const response = await apiClient.get<Expense>(`/expenses/${id}`);
    return response.data;
  },

  async create(data: CreateExpenseDto): Promise<Expense> {
    const response = await apiClient.post<Expense>('/expenses', data);
    return response.data;
  },

  async update(id: number, data: UpdateExpenseDto): Promise<Expense> {
    const response = await apiClient.put<Expense>(`/expenses/${id}`, data);
    return response.data;
  },

  async delete(id: number): Promise<void> {
    await apiClient.delete(`/expenses/${id}`);
  },
};
