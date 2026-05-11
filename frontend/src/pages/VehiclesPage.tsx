import { useState, useEffect } from 'react';
import { Car, Plus, Edit, Trash2, Loader2, AlertCircle } from 'lucide-react';
import { Navigation } from '../components/Navigation';
import { VehicleForm } from '../components/vehicle/VehicleForm';
import { DeleteVehicleModal } from '../components/vehicle/DeleteVehicleModal';
import { vehicleService } from '../services/vehicleService';
import { formatCurrency, formatDateOnly } from '../utils/helpers';
import type { Vehicle, CreateVehicleDto, UpdateVehicleDto } from '../types/Vehicle';

export function VehiclesPage() {
  const [vehicles, setVehicles] = useState<Vehicle[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  
  const [showForm, setShowForm] = useState(false);
  const [editingVehicle, setEditingVehicle] = useState<Vehicle | undefined>(undefined);
  
  const [showDeleteModal, setShowDeleteModal] = useState(false);
  const [deletingVehicle, setDeletingVehicle] = useState<Vehicle | undefined>(undefined);

  useEffect(() => {
    fetchVehicles();
  }, []);

  const fetchVehicles = async () => {
    try {
      setIsLoading(true);
      setError(null);
      const data = await vehicleService.getAll();
      setVehicles(data);
    } catch (err) {
      console.error('Error fetching vehicles:', err);
      setError('Failed to load vehicles. Please try again.');
    } finally {
      setIsLoading(false);
    }
  };

  const handleAddClick = () => {
    setEditingVehicle(undefined);
    setShowForm(true);
  };

  const handleEditClick = (vehicle: Vehicle) => {
    setEditingVehicle(vehicle);
    setShowForm(true);
  };

  const handleDeleteClick = (vehicle: Vehicle) => {
    setDeletingVehicle(vehicle);
    setShowDeleteModal(true);
  };

  const handleFormSubmit = async (data: CreateVehicleDto | UpdateVehicleDto) => {
    if (editingVehicle) {
      await vehicleService.update(editingVehicle.id, data as UpdateVehicleDto);
    } else {
      await vehicleService.create(data as CreateVehicleDto);
    }
    
    setShowForm(false);
    setEditingVehicle(undefined);
    await fetchVehicles();
  };

  const handleFormCancel = () => {
    setShowForm(false);
    setEditingVehicle(undefined);
  };

  const handleDeleteConfirm = async () => {
    if (!deletingVehicle) return;
    
    await vehicleService.delete(deletingVehicle.id);
    
    setShowDeleteModal(false);
    setDeletingVehicle(undefined);
    await fetchVehicles();
  };

  const handleDeleteCancel = () => {
    setShowDeleteModal(false);
    setDeletingVehicle(undefined);
  };

  if (isLoading) {
    return (
      <div className="min-h-screen bg-gray-900">
        <Navigation />
        <div className="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
          <div className="flex justify-center items-center h-64">
            <div className="flex items-center space-x-2">
              <Loader2 className="h-8 w-8 text-blue-500 animate-spin" />
              <span className="text-gray-400 text-lg">Loading vehicles...</span>
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
        <div className="px-4 py-6 sm:px-0">
          <div className="flex justify-between items-center">
            <div>
              <h1 className="text-3xl font-bold text-white">Vehicles</h1>
              <p className="mt-2 text-sm text-gray-400">
                Manage your vehicle fleet and track total cost of ownership
              </p>
            </div>
            <button
              onClick={handleAddClick}
              className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 transition-colors"
            >
              <Plus className="h-5 w-5 mr-2" />
              Add Vehicle
            </button>
          </div>
        </div>

        <div className="px-4 sm:px-0 mt-8">
          {vehicles.length === 0 ? (
            <div className="bg-gray-800 shadow-lg rounded-lg border border-gray-700">
              <div className="text-center py-12">
                <Car className="mx-auto h-12 w-12 text-gray-600" />
                <h3 className="mt-2 text-sm font-medium text-gray-300">No vehicles</h3>
                <p className="mt-1 text-sm text-gray-400">
                  Get started by adding your first vehicle.
                </p>
                <div className="mt-6">
                  <button
                    onClick={handleAddClick}
                    className="inline-flex items-center px-4 py-2 border border-transparent shadow-sm text-sm font-medium rounded-md text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 transition-colors"
                  >
                    <Plus className="h-4 w-4 mr-2" />
                    Add Vehicle
                  </button>
                </div>
              </div>
            </div>
          ) : (
            <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
              {vehicles.map((vehicle) => (
                <div
                  key={vehicle.id}
                  className="bg-gray-800 overflow-hidden shadow-lg rounded-lg border border-gray-700 hover:border-gray-600 transition-colors"
                >
                  <div className="p-6">
                    {/* Vehicle Header */}
                    <div className="flex items-center justify-between mb-4">
                      <div className="flex items-center">
                        <div className="p-2 bg-blue-900/50 rounded-lg">
                          <Car className="h-6 w-6 text-blue-400" />
                        </div>
                        <div className="ml-3">
                          <h3 className="text-lg font-semibold text-white">
                            {vehicle.make} {vehicle.model}
                          </h3>
                          <p className="text-sm text-gray-400">{vehicle.year}</p>
                        </div>
                      </div>
                    </div>

                    <div className="space-y-2 mb-4">
                      <div className="flex justify-between">
                        <span className="text-sm text-gray-400">Type:</span>
                        <span className="px-2 inline-flex text-xs leading-5 font-semibold rounded-full bg-blue-900 text-blue-200">
                          {vehicle.vehicleTypeDisplay}
                        </span>
                      </div>
                      <div className="flex justify-between">
                        <span className="text-sm text-gray-400">Purchase Price:</span>
                        <span className="text-sm font-medium text-white">
                          {formatCurrency(vehicle.purchasePrice)}
                        </span>
                      </div>
                      <div className="flex justify-between">
                        <span className="text-sm text-gray-400">Owned Since:</span>
                        <span className="text-sm text-gray-300">
                          {formatDateOnly(vehicle.ownershipStart)}
                        </span>
                      </div>
                      {vehicle.ownershipEnd && (
                        <div className="flex justify-between">
                          <span className="text-sm text-gray-400">Owned Until:</span>
                          <span className="text-sm text-gray-300">
                            {formatDateOnly(vehicle.ownershipEnd)}
                          </span>
                        </div>
                      )}
                    </div>

                    <div className="flex space-x-2 pt-4 border-t border-gray-700">
                      <button
                        onClick={() => handleEditClick(vehicle)}
                        className="flex-1 inline-flex justify-center items-center px-3 py-2 border border-gray-600 rounded-md text-sm font-medium text-gray-300 bg-gray-700 hover:bg-gray-600 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 transition-colors"
                      >
                        <Edit className="h-4 w-4 mr-2" />
                        Edit
                      </button>
                      <button
                        onClick={() => handleDeleteClick(vehicle)}
                        className="flex-1 inline-flex justify-center items-center px-3 py-2 border border-red-600 rounded-md text-sm font-medium text-red-400 hover:bg-red-900/50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500 transition-colors"
                      >
                        <Trash2 className="h-4 w-4 mr-2" />
                        Delete
                      </button>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>

      {showForm && (
        <VehicleForm
          vehicle={editingVehicle}
          onSubmit={handleFormSubmit}
          onCancel={handleFormCancel}
        />
      )}

      {showDeleteModal && deletingVehicle && (
        <DeleteVehicleModal
          vehicle={deletingVehicle}
          onConfirm={handleDeleteConfirm}
          onCancel={handleDeleteCancel}
        />
      )}
    </div>
  );
}
