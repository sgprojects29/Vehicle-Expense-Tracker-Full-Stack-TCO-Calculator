import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { Car, DollarSign, Fuel, Wallet, TrendingUp, Plus, Loader2, AlertCircle } from 'lucide-react';
import { Navigation } from '../components/Navigation';
import { reportService } from '../services/reportService';
import { vehicleService } from '../services/vehicleService';
import { formatCurrency, formatDateOnly } from '../utils/helpers';
import type { VehicleSummaryDto } from '../types/Report';
import type { Vehicle } from '../types/Vehicle';

export function DashboardPage() {
  const [summary, setSummary] = useState<VehicleSummaryDto | null>(null);
  const [vehicles, setVehicles] = useState<Vehicle[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchData = async () => {
      try {
        setIsLoading(true);
        setError(null);

        const [summaryData, vehiclesData] = await Promise.all([
          reportService.getSummary(),
          vehicleService.getAll(),
        ]);

        setSummary(summaryData);
        setVehicles(vehiclesData);
      } catch (err) {
        console.error('Error fetching dashboard data:', err);
        setError('Failed to load dashboard data. Please try again.');
      } finally {
        setIsLoading(false);
      }
    };

    fetchData();
  }, []);

  if (isLoading) {
    return (
      <div className="min-h-screen bg-gray-900">
        <Navigation />
        <div className="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
          <div className="flex justify-center items-center h-64">
            <div className="flex items-center space-x-2">
              <Loader2 className="h-8 w-8 text-blue-500 animate-spin" />
              <span className="text-gray-400 text-lg">Loading dashboard...</span>
            </div>
          </div>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="min-h-screen bg-gray-900">
        <Navigation />
        <div className="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
          <div className="bg-red-900/50 border border-red-700 rounded-lg p-4">
            <div className="flex">
              <div className="shrink-0">
                <AlertCircle className="h-5 w-5 text-red-300" />
              </div>
              <div className="ml-3">
                <p className="text-sm font-medium text-red-200">{error}</p>
              </div>
            </div>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-900">
      <Navigation />

      <div className="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
        {/* Header */}
        <div className="px-4 py-6 sm:px-0">
          <h1 className="text-3xl font-bold text-white">Dashboard</h1>
          <p className="mt-2 text-sm text-gray-400">
            Overview of your vehicle expenses and total cost of ownership
          </p>
        </div>

        {summary && (
          <div className="px-4 sm:px-0">
            <div className="grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-4">
              {/* Total Vehicles */}
              <div className="bg-gray-800 overflow-hidden shadow-lg rounded-lg border border-gray-700">
                <div className="p-5">
                  <div className="flex items-center">
                    <div className="shrink-0">
                      <div className="p-3 bg-blue-900/50 rounded-lg">
                        <Car className="h-6 w-6 text-blue-400" />
                      </div>
                    </div>
                    <div className="ml-5 w-0 flex-1">
                      <dl>
                        <dt className="text-sm font-medium text-gray-400 truncate">
                          Total Vehicles
                        </dt>
                        <dd className="flex items-baseline">
                          <div className="text-2xl font-semibold text-white">
                            {summary.totalVehicles}
                          </div>
                        </dd>
                      </dl>
                    </div>
                  </div>
                </div>
              </div>

              <div className="bg-gray-800 overflow-hidden shadow-lg rounded-lg border border-gray-700">
                <div className="p-5">
                  <div className="flex items-center">
                    <div className="shrink-0">
                      <div className="p-3 bg-green-900/50 rounded-lg">
                        <DollarSign className="h-6 w-6 text-green-400" />
                      </div>
                    </div>
                    <div className="ml-5 w-0 flex-1">
                      <dl>
                        <dt className="text-sm font-medium text-gray-400 truncate">
                          Total Investment
                        </dt>
                        <dd className="flex items-baseline">
                          <div className="text-2xl font-semibold text-white">
                            {formatCurrency(summary.totalInvestment)}
                          </div>
                        </dd>
                      </dl>
                    </div>
                  </div>
                </div>
              </div>

              <div className="bg-gray-800 overflow-hidden shadow-lg rounded-lg border border-gray-700">
                <div className="p-5">
                  <div className="flex items-center">
                    <div className="shrink-0">
                      <div className="p-3 bg-orange-900/50 rounded-lg">
                        <Fuel className="h-6 w-6 text-orange-400" />
                      </div>
                    </div>
                    <div className="ml-5 w-0 flex-1">
                      <dl>
                        <dt className="text-sm font-medium text-gray-400 truncate">
                          Total Fuel Cost
                        </dt>
                        <dd className="flex items-baseline">
                          <div className="text-2xl font-semibold text-white">
                            {formatCurrency(summary.totalFuelCost)}
                          </div>
                        </dd>
                      </dl>
                    </div>
                  </div>
                </div>
              </div>

              <div className="bg-gray-800 overflow-hidden shadow-lg rounded-lg border border-gray-700">
                <div className="p-5">
                  <div className="flex items-center">
                    <div className="shrink-0">
                      <div className="p-3 bg-purple-900/50 rounded-lg">
                        <Wallet className="h-6 w-6 text-purple-400" />
                      </div>
                    </div>
                    <div className="ml-5 w-0 flex-1">
                      <dl>
                        <dt className="text-sm font-medium text-gray-400 truncate">
                          Total Expenses
                        </dt>
                        <dd className="flex items-baseline">
                          <div className="text-2xl font-semibold text-white">
                            {formatCurrency(summary.totalExpensesCost)}
                          </div>
                        </dd>
                      </dl>
                    </div>
                  </div>
                </div>
              </div>

              <div className="bg-linear-to-br from-blue-900 to-blue-800 overflow-hidden shadow-lg rounded-lg border border-blue-700 sm:col-span-2 lg:col-span-4">
                <div className="p-5">
                  <div className="flex items-center justify-center">
                    <div className="shrink-0">
                      <div className="p-3 bg-blue-800/50 rounded-lg">
                        <TrendingUp className="h-8 w-8 text-blue-300" />
                      </div>
                    </div>
                    <div className="ml-5">
                      <dl>
                        <dt className="text-lg font-medium text-blue-200 truncate">
                          Total Cost of Ownership (All Vehicles)
                        </dt>
                        <dd className="flex items-baseline justify-center mt-1">
                          <div className="text-4xl font-bold text-white">
                            {formatCurrency(summary.grandTotalCost)}
                          </div>
                        </dd>
                      </dl>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        )}

        <div className="px-4 py-6 sm:px-0 mt-8">
          <div className="bg-gray-800 shadow-lg rounded-lg border border-gray-700">
            <div className="px-4 py-5 sm:p-6">
              <div className="flex justify-between items-center mb-4">
                <h2 className="text-xl font-semibold text-white">Your Vehicles</h2>
                <Link
                  to="/vehicles"
                  className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
                >
                  <Plus className="h-4 w-4 mr-2" />
                  Manage Vehicles
                </Link>
              </div>

              {vehicles.length === 0 ? (
                <div className="text-center py-12">
                  <Car className="mx-auto h-12 w-12 text-gray-600" />
                  <h3 className="mt-2 text-sm font-medium text-gray-300">No vehicles</h3>
                  <p className="mt-1 text-sm text-gray-400">
                    Get started by adding your first vehicle.
                  </p>
                  <div className="mt-6">
                    <Link
                      to="/vehicles"
                      className="inline-flex items-center px-4 py-2 border border-transparent shadow-sm text-sm font-medium rounded-md text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
                    >
                      <Plus className="h-4 w-4 mr-2" />
                      Add Vehicle
                    </Link>
                  </div>
                </div>
              ) : (
                <div className="overflow-hidden">
                  <table className="min-w-full divide-y divide-gray-700">
                    <thead>
                      <tr>
                        <th
                          scope="col"
                          className="px-6 py-3 text-left text-xs font-medium text-gray-400 uppercase tracking-wider"
                        >
                          Vehicle
                        </th>
                        <th
                          scope="col"
                          className="px-6 py-3 text-left text-xs font-medium text-gray-400 uppercase tracking-wider"
                        >
                          Year
                        </th>
                        <th
                          scope="col"
                          className="px-6 py-3 text-left text-xs font-medium text-gray-400 uppercase tracking-wider"
                        >
                          Type
                        </th>
                        <th
                          scope="col"
                          className="px-6 py-3 text-left text-xs font-medium text-gray-400 uppercase tracking-wider"
                        >
                          Purchase Price
                        </th>
                        <th
                          scope="col"
                          className="px-6 py-3 text-left text-xs font-medium text-gray-400 uppercase tracking-wider"
                        >
                          Ownership Start
                        </th>
                        <th scope="col" className="relative px-6 py-3">
                          <span className="sr-only">View</span>
                        </th>
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-gray-700">
                      {vehicles.map((vehicle) => (
                        <tr key={vehicle.id} className="hover:bg-gray-700/50">
                          <td className="px-6 py-4 whitespace-nowrap">
                            <div className="flex items-center">
                              <Car className="h-5 w-5 text-gray-500 mr-2" />
                              <div className="text-sm font-medium text-white">
                                {vehicle.make} {vehicle.model}
                              </div>
                            </div>
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap">
                            <div className="text-sm text-gray-300">{vehicle.year}</div>
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap">
                            <span className="px-2 inline-flex text-xs leading-5 font-semibold rounded-full bg-blue-900 text-blue-200">
                              {vehicle.vehicleTypeDisplay}
                            </span>
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap">
                            <div className="text-sm text-gray-300">
                              {formatCurrency(vehicle.purchasePrice)}
                            </div>
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap">
                            <div className="text-sm text-gray-300">
                              {formatDateOnly(vehicle.ownershipStart)}
                            </div>
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                            <Link
                              to={`/vehicles/${vehicle.id}`}
                              className="text-blue-400 hover:text-blue-300"
                            >
                              View Details
                            </Link>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}
            </div>
          </div>
        </div>

        {summary && summary.vehicles.length > 0 && (
          <div className="px-4 py-6 sm:px-0 mt-8">
            <h2 className="text-xl font-semibold text-white mb-4">
              Cost Summary by Vehicle
            </h2>
            <div className="grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-3">
              {summary.vehicles.map((vehicle) => (
                <div
                  key={vehicle.vehicleId}
                  className="bg-gray-800 overflow-hidden shadow-lg rounded-lg border border-gray-700 hover:border-gray-600 transition-colors"
                >
                  <div className="p-5">
                    <div className="flex items-center mb-3">
                      <Car className="h-5 w-5 text-blue-400 mr-2" />
                      <h3 className="text-lg font-medium text-white">
                        {vehicle.make} {vehicle.model}
                      </h3>
                    </div>
                    <div className="space-y-2">
                      <div className="flex justify-between">
                        <span className="text-sm text-gray-400">Year:</span>
                        <span className="text-sm text-gray-300">{vehicle.year}</span>
                      </div>
                      <div className="flex justify-between">
                        <span className="text-sm text-gray-400">Purchase Price:</span>
                        <span className="text-sm text-gray-300">
                          {formatCurrency(vehicle.purchasePrice)}
                        </span>
                      </div>
                      <div className="flex justify-between">
                        <span className="text-sm text-gray-400">Total Cost:</span>
                        <span className="text-sm font-semibold text-white">
                          {formatCurrency(vehicle.totalCost)}
                        </span>
                      </div>
                      <div className="flex justify-between">
                        <span className="text-sm text-gray-400">Monthly Avg:</span>
                        <span className="text-sm font-semibold text-blue-400">
                          {formatCurrency(vehicle.monthlyAverage)}
                        </span>
                      </div>
                    </div>
                    <div className="mt-4">
                      <Link
                        to={`/reports?vehicleId=${vehicle.vehicleId}`}
                        className="inline-flex items-center text-sm text-blue-400 hover:text-blue-300"
                      >
                        View Full Report
                        <TrendingUp className="h-4 w-4 ml-1" />
                      </Link>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
