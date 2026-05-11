import apiClient from './api';
import type { TcoReportDto, VehicleSummaryDto, CostBreakdownDto, MonthlyCostTrendDto } from '../types/Report';

export const reportService = {
  // Get TCO report for a specific vehicle
  async getTcoReport(vehicleId: number): Promise<TcoReportDto> {
    const response = await apiClient.get<TcoReportDto>(`/reports/tco/${vehicleId}`);
    return response.data;
  },

  // Get summary across all user's vehicles
  async getSummary(): Promise<VehicleSummaryDto> {
    const response = await apiClient.get<VehicleSummaryDto>('/reports/summary');
    return response.data;
  },

  // Get cost breakdown by category for a specific vehicle
  async getBreakdown(vehicleId: number): Promise<CostBreakdownDto> {
    const response = await apiClient.get<CostBreakdownDto>(`/reports/breakdown/${vehicleId}`);
    return response.data;
  },

  // Get monthly cost trends for a specific vehicle
  async getTrends(vehicleId: number): Promise<MonthlyCostTrendDto> {
    const response = await apiClient.get<MonthlyCostTrendDto>(`/reports/trends/${vehicleId}`);
    return response.data;
  },
};
