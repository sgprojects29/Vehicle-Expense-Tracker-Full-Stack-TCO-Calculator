import { useState, useEffect } from 'react';
import { BarChart3, Loader2, AlertCircle, TrendingUp, PieChart, Calendar } from 'lucide-react';
import { Navigation } from '../components/Navigation';
import { SummaryCards } from '../components/reports/SummaryCards';
import { TcoMetrics } from '../components/reports/TcoMetrics';
import { CostBreakdownChart } from '../components/reports/CostBreakdownChart';
import { MonthlyCostChart } from '../components/reports/MonthlyCostChart';
import { FuelEfficiencyMetrics } from '../components/reports/FuelEfficiencyMetrics';
import { reportService } from '../services/reportService';
import { vehicleService } from '../services/vehicleService';
import { fuelService } from '../services/fuelService';
import type { VehicleSummaryDto, TcoReportDto, CostBreakdownDto, MonthlyCostTrendDto } from '../types/Report';
import type { Vehicle } from '../types/Vehicle';
import type { FuelEfficiencyDto } from '../types/Fuel';

export function ReportsPage() {
  const [vehicles, setVehicles] = useState<Vehicle[]>([]);
  const [summary, setSummary] = useState<VehicleSummaryDto | null>(null);
  const [selectedVehicleId, setSelectedVehicleId] = useState<number | undefined>(undefined);
  
  const [tcoReport, setTcoReport] = useState<TcoReportDto | null>(null);
  const [breakdown, setBreakdown] = useState<CostBreakdownDto | null>(null);
  const [trends, setTrends] = useState<MonthlyCostTrendDto | null>(null);
  const [efficiency, setEfficiency] = useState<FuelEfficiencyDto | null>(null);
  
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    fetchInitialData();
  }, []);

  useEffect(() => {
    if (selectedVehicleId) {
      fetchVehicleReports(selectedVehicleId);
    } else {
      setTcoReport(null);
      setBreakdown(null);
      setTrends(null);
      setEfficiency(null);
    }
  }, [selectedVehicleId]);

  const fetchInitialData = async () => {
    try {
      setIsLoading(true);
      setError(null);
      
      const [vehiclesData, summaryData] = await Promise.all([
        vehicleService.getAll(),
        reportService.getSummary(),
      ]);
      
      setVehicles(vehiclesData);
      setSummary(summaryData);
      
      if (vehiclesData.length > 0) {
        setSelectedVehicleId(vehiclesData[0].id);
      }
    } catch (err) {
      console.error('Error fetching reports:', err);
      setError('Failed to load reports. Please try again.');
    } finally {
      setIsLoading(false);
    }
  };

  const fetchVehicleReports = async (vehicleId: number) => {
    try {
      setError(null);
      
      const [tcoData, breakdownData, trendsData, efficiencyData] = await Promise.all([
        reportService.getTcoReport(vehicleId),
        reportService.getBreakdown(vehicleId),
        reportService.getTrends(vehicleId),
        fuelService.getEfficiency(vehicleId).catch(() => null), // Allow efficiency to fail gracefully
      ]);
      
      setTcoReport(tcoData);
      setBreakdown(breakdownData);
      setTrends(trendsData);
      setEfficiency(efficiencyData);
    } catch (err) {
      console.error('Error fetching vehicle reports:', err);
      setError('Failed to load vehicle reports. Please try again.');
    }
  };

  if (isLoading) {
    return (
      <>
        <Navigation />
        <div className="min-h-screen bg-gray-900 flex items-center justify-center">
          <div className="text-center">
            <Loader2 className="h-12 w-12 text-blue-500 animate-spin mx-auto" />
            <p className="mt-4 text-gray-400">Loading reports...</p>
          </div>
        </div>
      </>
    );
  }

  if (error) {
    return (
      <>
        <Navigation />
        <div className="min-h-screen bg-gray-900 flex items-center justify-center">
          <div className="text-center">
            <AlertCircle className="h-12 w-12 text-red-500 mx-auto" />
            <p className="mt-4 text-red-400">{error}</p>
          </div>
        </div>
      </>
    );
  }

  if (vehicles.length === 0) {
    return (
      <>
        <Navigation />
        <div className="min-h-screen bg-gray-900">
          <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
            <div className="text-center py-12">
              <BarChart3 className="h-16 w-16 text-gray-600 mx-auto" />
              <h3 className="mt-4 text-lg font-medium text-gray-300">No vehicles yet</h3>
              <p className="mt-2 text-gray-500">Add a vehicle to see reports and analytics.</p>
            </div>
          </div>
        </div>
      </>
    );
  }

  const selectedVehicle = vehicles.find(v => v.id === selectedVehicleId);

  return (
    <>
      <Navigation />
      <div className="min-h-screen bg-gray-900">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
          {/* Header */}
          <div className="mb-6">
            <h1 className="text-3xl font-bold text-white flex items-center gap-2">
              <BarChart3 className="h-8 w-8" />
              Reports & Analytics
            </h1>
            <p className="mt-1 text-gray-400">Analyze your vehicle costs and trends</p>
          </div>

          {summary && (
            <div className="mb-8">
              <h2 className="text-xl font-semibold text-white mb-4">Overall Summary</h2>
              <SummaryCards summary={summary} />
            </div>
          )}

          <div className="bg-gray-800 rounded-lg p-6 mb-8">
            <label className="block text-sm font-medium text-gray-300 mb-2">
              Select Vehicle for Detailed Analysis
            </label>
            <select
              value={selectedVehicleId || ''}
              onChange={(e) => setSelectedVehicleId(e.target.value ? Number(e.target.value) : undefined)}
              className="block w-full bg-gray-800 border border-gray-600 rounded-md px-4 py-2 text-gray-100 focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              {vehicles.map((vehicle) => (
                <option key={vehicle.id} value={vehicle.id}>
                  {vehicle.year} {vehicle.make} {vehicle.model}
                </option>
              ))}
            </select>
          </div>

          {selectedVehicleId && selectedVehicle && tcoReport && breakdown && (
            <>
              <div className="mb-8">
                <h2 className="text-xl font-semibold text-white mb-4">
                  {selectedVehicle.year} {selectedVehicle.make} {selectedVehicle.model} - TCO Analysis
                </h2>
                <TcoMetrics tcoReport={tcoReport} />
              </div>

              <div className="mb-8">
                <h2 className="text-xl font-semibold text-white mb-4 flex items-center gap-2">
                  <PieChart className="h-5 w-5" />
                  Cost Breakdown by Category
                </h2>
                <div className="bg-gray-800 rounded-lg p-6">
                  <CostBreakdownChart breakdown={breakdown} />
                </div>
              </div>

              {trends && trends.monthlyData.length > 0 && (
                <div className="mb-8">
                  <h2 className="text-xl font-semibold text-white mb-4 flex items-center gap-2">
                    <Calendar className="h-5 w-5" />
                    Monthly Cost Trends
                  </h2>
                  <div className="bg-gray-800 rounded-lg p-6">
                    <MonthlyCostChart monthlyData={trends.monthlyData} />
                  </div>
                </div>
              )}

              {efficiency && efficiency.totalFillUps > 0 && (
                <div className="mb-8">
                  <h2 className="text-xl font-semibold text-white mb-4 flex items-center gap-2">
                    <TrendingUp className="h-5 w-5" />
                    Fuel Efficiency Metrics
                  </h2>
                  <div className="bg-gray-800 rounded-lg p-6">
                    <FuelEfficiencyMetrics efficiency={efficiency} />
                  </div>
                </div>
              )}
            </>
          )}
        </div>
      </div>
    </>
  );
}
