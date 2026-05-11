import apiClient from './api';
import type { FuelEntry, CreateFuelEntryDto, UpdateFuelEntryDto, FuelEfficiencyDto } from '../types/Fuel';

export const fuelService = {
  async getAll(
    vehicleId?: number,
    energyType?: number,
    startDate?: string,
    endDate?: string
  ): Promise<FuelEntry[]> {
    const params = new URLSearchParams();
    if (vehicleId !== undefined) params.append('vehicleId', vehicleId.toString());
    if (energyType !== undefined) params.append('energyType', energyType.toString());
    if (startDate) params.append('startDate', startDate);
    if (endDate) params.append('endDate', endDate);

    const queryString = params.toString();
    const url = queryString ? `/fuel?${queryString}` : '/fuel';

    const response = await apiClient.get<FuelEntry[]>(url);
    return response.data;
  },

  async getById(id: number): Promise<FuelEntry> {
    const response = await apiClient.get<FuelEntry>(`/fuel/${id}`);
    return response.data;
  },

  async create(data: CreateFuelEntryDto): Promise<FuelEntry> {
    const response = await apiClient.post<FuelEntry>('/fuel', data);
    return response.data;
  },

  async update(id: number, data: UpdateFuelEntryDto): Promise<FuelEntry> {
    const response = await apiClient.put<FuelEntry>(`/fuel/${id}`, data);
    return response.data;
  },

  async delete(id: number): Promise<void> {
    await apiClient.delete(`/fuel/${id}`);
  },

  async getEfficiency(vehicleId: number): Promise<FuelEfficiencyDto> {
    const response = await apiClient.get<FuelEfficiencyDto>(`/fuel/efficiency/${vehicleId}`);
    return response.data;
  },
};
