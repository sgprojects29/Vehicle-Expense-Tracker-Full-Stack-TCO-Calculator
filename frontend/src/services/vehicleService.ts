import apiClient from './api';
import type { Vehicle, CreateVehicleDto, UpdateVehicleDto } from '../types/Vehicle';

export const vehicleService = {
  async getAll(): Promise<Vehicle[]> {
    const response = await apiClient.get<Vehicle[]>('/vehicles');
    return response.data;
  },

  async getById(id: number): Promise<Vehicle> {
    const response = await apiClient.get<Vehicle>(`/vehicles/${id}`);
    return response.data;
  },

  async create(data: CreateVehicleDto): Promise<Vehicle> {
    const response = await apiClient.post<Vehicle>('/vehicles', data);
    return response.data;
  },

  async update(id: number, data: UpdateVehicleDto): Promise<Vehicle> {
    const response = await apiClient.put<Vehicle>(`/vehicles/${id}`, data);
    return response.data;
  },

  async delete(id: number): Promise<void> {
    await apiClient.delete(`/vehicles/${id}`);
  },
};
