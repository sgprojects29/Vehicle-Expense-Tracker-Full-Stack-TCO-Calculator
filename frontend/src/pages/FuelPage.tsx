import { useState, useEffect } from 'react';
import { Plus, Edit2, Trash2, Fuel, Loader2, Filter, X } from 'lucide-react';
import { fuelService } from '../services/fuelService';
import { vehicleService } from '../services/vehicleService';
import type { FuelEntry, CreateFuelEntryDto, UpdateFuelEntryDto } from '../types/Fuel';
import { EnergyType } from '../types/Fuel';
import type { Vehicle } from '../types/Vehicle';
import FuelForm from '../components/fuel/FuelForm';
import DeleteFuelModal from '../components/fuel/DeleteFuelModal';
import { Navigation } from '../components/Navigation';
import { Pagination } from '../components/common/Pagination';
import { DateRangeFilter } from '../components/common/DateRangeFilter';
import { formatCurrency, formatDateOnly } from '../utils/helpers';
import type { AxiosError } from 'axios';

export default function FuelPage() {
  const [fuelEntries, setFuelEntries] = useState<FuelEntry[]>([]);
  const [vehicles, setVehicles] = useState<Vehicle[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  // Filter state
  const [filterVehicleId, setFilterVehicleId] = useState<number | undefined>(undefined);
  const [filterEnergyType, setFilterEnergyType] = useState<number | undefined>(undefined);

  // Date filters
  const [startDate, setStartDate] = useState<string>('');
  const [endDate, setEndDate] = useState<string>('');

  // Pagination
  const [currentPage, setCurrentPage] = useState(1);
  const [itemsPerPage, setItemsPerPage] = useState(15);

  // Modal state
  const [showAddForm, setShowAddForm] = useState(false);
  const [editingEntry, setEditingEntry] = useState<FuelEntry | null>(null);
  const [deletingEntry, setDeletingEntry] = useState<FuelEntry | null>(null);

  useEffect(() => {
    loadData();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [filterVehicleId, filterEnergyType, startDate, endDate]);

  const loadData = async () => {
    setLoading(true);
    setError('');
    try {
      const [entriesData, vehiclesData] = await Promise.all([
        fuelService.getAll(
          filterVehicleId,
          filterEnergyType,
          startDate || undefined,
          endDate || undefined
        ),
        vehicleService.getAll(),
      ]);

      // Sort by date (newest first)
      const sortedEntries = entriesData.sort((a, b) =>
        new Date(b.date).getTime() - new Date(a.date).getTime()
      );

      setFuelEntries(sortedEntries);
      setVehicles(vehiclesData);
      setCurrentPage(1); // Reset to first page when filters change
    } catch (err) {
      const axiosError = err as AxiosError<{ message?: string }>;
      setError(axiosError.response?.data?.message || 'Failed to load fuel entries');
    } finally {
      setLoading(false);
    }
  };

  const handleCreate = async (data: CreateFuelEntryDto | UpdateFuelEntryDto) => {
    await fuelService.create(data as CreateFuelEntryDto);
    await loadData();
  };

  const handleUpdate = async (data: CreateFuelEntryDto | UpdateFuelEntryDto) => {
    if (!editingEntry) return;
    await fuelService.update(editingEntry.id, data as UpdateFuelEntryDto);
    setEditingEntry(null);
    await loadData();
  };

  const handleDelete = async () => {
    if (!deletingEntry) return;
    await fuelService.delete(deletingEntry.id);
    setDeletingEntry(null);
    await loadData();
  };

  const clearFilters = () => {
    setFilterVehicleId(undefined);
    setFilterEnergyType(undefined);
    setStartDate('');
    setEndDate('');
    setCurrentPage(1);
  };

  const hasActiveFilters = filterVehicleId !== undefined || filterEnergyType !== undefined || startDate !== '' || endDate !== '';

  const getEnergyTypeBadgeColor = (type: number): string => {
    switch (type) {
      case EnergyType.Gasoline:
        return 'bg-red-900/50 text-red-300 border-red-700';
      case EnergyType.Diesel:
        return 'bg-amber-900/50 text-amber-300 border-amber-700';
      case EnergyType.Electricity:
        return 'bg-emerald-900/50 text-emerald-300 border-emerald-700';
      default:
        return 'bg-gray-900/50 text-gray-300 border-gray-700';
    }
  };

  // Pagination calculations
  const totalFuelEntries = fuelEntries.length;
  const totalPages = Math.ceil(totalFuelEntries / itemsPerPage);
  const startIndex = (currentPage - 1) * itemsPerPage;
  const endIndex = startIndex + itemsPerPage;
  const paginatedFuelEntries = fuelEntries.slice(startIndex, endIndex);

  if (loading) {
    return (
      <div>
        <Navigation />
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
          <div className="flex items-center justify-center min-h-[60vh]">
            <Loader2 className="w-8 h-8 text-blue-500 animate-spin" />
          </div>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div>
        <Navigation />
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
          <div className="bg-red-900/50 border border-red-700 rounded-lg p-4 text-red-200">
            {error}
          </div>
        </div>
      </div>
    );
  }

  if (vehicles.length === 0) {
    return (
      <div>
        <Navigation />
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
          <div className="text-center py-12">
            <Fuel className="w-16 h-16 text-gray-600 mx-auto mb-4" />
            <h3 className="text-xl font-semibold text-white mb-2">No Vehicles Yet</h3>
            <p className="text-gray-400 mb-4">Add a vehicle first to start tracking fuel entries.</p>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div>
      <Navigation />
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="space-y-6">
          {/* Header */}
          <div>
            <h1 className="text-3xl font-bold text-white">Fuel Tracking</h1>
            <p className="mt-2 text-gray-400">Monitor fuel consumption and efficiency across all your vehicles</p>
          </div>

          {/* Add Button */}
          <div className="flex justify-end">
            <button
              onClick={() => setShowAddForm(true)}
              className="flex items-center space-x-2 px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition-colors"
            >
              <Plus className="w-5 h-5" />
              <span>Add Fuel Entry</span>
            </button>
          </div>

          {/* Filters */}
          <div className="bg-gray-800 rounded-lg p-6 border border-gray-700">
            <div className="flex items-start space-x-4">
              <Filter className="w-5 h-5 text-gray-400 mt-8" />
              <div className="flex-1">
                <h3 className="text-sm font-medium text-gray-300 mb-4">Filters</h3>
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div>
                    <label htmlFor="filterVehicle" className="block text-sm font-medium text-gray-300 mb-2">
                      Vehicle
                    </label>
                    <select
                      id="filterVehicle"
                      value={filterVehicleId || ''}
                      onChange={(e) => setFilterVehicleId(e.target.value ? parseInt(e.target.value) : undefined)}
                      className="w-full px-3 py-2 bg-gray-700 border border-gray-600 rounded-md text-white focus:outline-none focus:ring-2 focus:ring-blue-500"
                    >
                      <option value="">All Vehicles</option>
                      {vehicles.map((vehicle) => (
                        <option key={vehicle.id} value={vehicle.id}>
                          {vehicle.year} {vehicle.make} {vehicle.model}
                        </option>
                      ))}
                    </select>
                  </div>

                  <div>
                    <label htmlFor="filterEnergyType" className="block text-sm font-medium text-gray-300 mb-2">
                      Energy Type
                    </label>
                    <select
                      id="filterEnergyType"
                      value={filterEnergyType || ''}
                      onChange={(e) => setFilterEnergyType(e.target.value ? parseInt(e.target.value) : undefined)}
                      className="w-full px-3 py-2 bg-gray-700 border border-gray-600 rounded-md text-white focus:outline-none focus:ring-2 focus:ring-blue-500"
                    >
                      <option value="">All Types</option>
                      <option value={EnergyType.Gasoline}>Gasoline</option>
                      <option value={EnergyType.Diesel}>Diesel</option>
                      <option value={EnergyType.Electricity}>Electricity</option>
                    </select>
                  </div>

                  <DateRangeFilter
                    startDate={startDate}
                    endDate={endDate}
                    onStartDateChange={setStartDate}
                    onEndDateChange={setEndDate}
                  />

                  {hasActiveFilters && (
                    <div className="flex items-end">
                      <button
                        onClick={clearFilters}
                        className="flex items-center space-x-2 px-4 py-2 text-gray-300 hover:text-white transition-colors"
                      >
                        <X className="w-4 h-4" />
                        <span>Clear Filters</span>
                      </button>
                    </div>
                  )}
                </div>
              </div>
            </div>
          </div>

          {/* Fuel Entries Table */}
          {fuelEntries.length === 0 ? (
            <div className="text-center py-12 bg-gray-800 rounded-lg border border-gray-700">
              <Fuel className="w-16 h-16 text-gray-600 mx-auto mb-4" />
              <h3 className="text-xl font-semibold text-white mb-2">
                {hasActiveFilters ? 'No Matching Fuel Entries' : 'No Fuel Entries Yet'}
              </h3>
              <p className="text-gray-400 mb-4">
                {hasActiveFilters
                  ? 'Try adjusting your filters or add a new fuel entry.'
                  : 'Start tracking your fuel consumption by adding your first entry.'}
              </p>
              {!hasActiveFilters && (
                <button
                  onClick={() => setShowAddForm(true)}
                  className="inline-flex items-center space-x-2 px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition-colors"
                >
                  <Plus className="w-5 h-5" />
                  <span>Add Fuel Entry</span>
                </button>
              )}
            </div>
          ) : (
            <div className="bg-gray-800 rounded-lg border border-gray-700 overflow-hidden">
              <div className="overflow-x-auto">
                <table className="w-full">
                  <thead className="bg-gray-900 border-b border-gray-700">
                    <tr>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-400 uppercase tracking-wider">
                        Date
                      </th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-400 uppercase tracking-wider">
                        Vehicle
                      </th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-400 uppercase tracking-wider">
                        Type
                      </th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-400 uppercase tracking-wider">
                        Amount
                      </th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-400 uppercase tracking-wider">
                        Cost
                      </th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-400 uppercase tracking-wider">
                        Cost/Unit
                      </th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-400 uppercase tracking-wider">
                        Odometer
                      </th>
                      <th className="px-6 py-3 text-right text-xs font-medium text-gray-400 uppercase tracking-wider">
                        Actions
                      </th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-700">
                    {paginatedFuelEntries.map((entry) => (
                      <tr key={entry.id} className="hover:bg-gray-700/50 transition-colors">
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-white">
                          {formatDateOnly(entry.date)}
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap">
                          <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-900/50 text-blue-300 border border-blue-700">
                            {entry.vehicleMake} {entry.vehicleModel}
                          </span>
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap">
                          <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium border ${getEnergyTypeBadgeColor(entry.energyType)}`}>
                            {entry.energyTypeDisplay}
                          </span>
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-300">
                          {entry.amount.toFixed(2)} {entry.unit}
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-white font-medium">
                          {formatCurrency(entry.cost)}
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-400">
                          {formatCurrency(entry.costPerUnit)}/{entry.unit}
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-300">
                          {entry.odometer ? `${entry.odometer.toLocaleString()} km` : 'N/A'}

                        </td>
                        <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                          <button
                            onClick={() => setEditingEntry(entry)}
                            className="text-blue-400 hover:text-blue-300 mr-3"
                            title="Edit"
                          >
                            <Edit2 className="w-4 h-4" />
                          </button>
                          <button
                            onClick={() => setDeletingEntry(entry)}
                            className="text-red-400 hover:text-red-300"
                            title="Delete"
                          >
                            <Trash2 className="w-4 h-4" />
                          </button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>

              <Pagination
                currentPage={currentPage}
                totalPages={totalPages}
                totalItems={totalFuelEntries}
                itemsPerPage={itemsPerPage}
                onPageChange={(page) => {
                  setCurrentPage(page);
                  window.scrollTo({ top: 0, behavior: 'smooth' });
                }}
                onItemsPerPageChange={(newItemsPerPage) => {
                  setItemsPerPage(newItemsPerPage);
                  setCurrentPage(1);
                }}
                itemLabel="fuel entry"
                itemLabelPlural="fuel entries"
              />
            </div>
          )}
        </div>
      </div>

      {/* Modals */}
      {showAddForm && (
        <FuelForm
          vehicles={vehicles}
          preSelectedVehicleId={filterVehicleId}
          onSubmit={handleCreate}
          onClose={() => setShowAddForm(false)}
        />
      )}

      {editingEntry && (
        <FuelForm
          fuelEntry={editingEntry}
          vehicles={vehicles}
          preSelectedVehicleId={filterVehicleId}
          onSubmit={handleUpdate}
          onClose={() => setEditingEntry(null)}
        />
      )}

      {deletingEntry && (
        <DeleteFuelModal
          fuelEntry={deletingEntry}
          onConfirm={handleDelete}
          onCancel={() => setDeletingEntry(null)}
        />
      )}
    </div>
  );
}
