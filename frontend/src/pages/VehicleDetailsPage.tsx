import { useState, useEffect } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { Car, Edit, Trash2, Loader2, AlertCircle, ArrowLeft, Calendar, DollarSign, TrendingUp } from 'lucide-react';
import { Navigation } from '../components/Navigation';
import { VehicleForm } from '../components/vehicle/VehicleForm';
import { DeleteVehicleModal } from '../components/vehicle/DeleteVehicleModal';
import { vehicleService } from '../services/vehicleService';
import { reportService } from '../services/reportService';
import { formatCurrency, formatDateOnly } from '../utils/helpers';
import type { Vehicle, UpdateVehicleDto } from '../types/Vehicle';
import type { TcoReportDto } from '../types/Report';

export function VehicleDetailsPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const [vehicle, setVehicle] = useState<Vehicle | null>(null);
  const [tcoReport, setTcoReport] = useState<TcoReportDto | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [showEditForm, setShowEditForm] = useState(false);
  const [showDeleteModal, setShowDeleteModal] = useState(false);

  const fetchVehicleDetails = async () => {
    if (!id) return;

    try {
      setIsLoading(true);
      setError(null);

      const [vehicleData, tcoData] = await Promise.all([
        vehicleService.getById(parseInt(id)),
        reportService.getTcoReport(parseInt(id)),
      ]);

      setVehicle(vehicleData);
      setTcoReport(tcoData);
    } catch (err) {
      console.error('Error fetching vehicle details:', err);
      setError('Failed to load vehicle details. Please try again.');
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchVehicleDetails();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id]);

  const handleEditClick = () => {
    setShowEditForm(true);
  };

  const handleDeleteClick = () => {
    setShowDeleteModal(true);
  };

  const handleFormSubmit = async (data: UpdateVehicleDto) => {
    if (!vehicle) return;

    await vehicleService.update(vehicle.id, data);
    setShowEditForm(false);
    await fetchVehicleDetails();
  };

  const handleFormCancel = () => {
    setShowEditForm(false);
  };

  const handleDeleteConfirm = async () => {
    if (!vehicle) return;
    
    await vehicleService.delete(vehicle.id);
    navigate('/vehicles');
  };

  const handleDeleteCancel = () => {
    setShowDeleteModal(false);
  };

  if (isLoading) {
    return (
      <div className="min-h-screen bg-gray-900">
        <Navigation />
        <div className="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
          <div className="flex justify-center items-center h-64">
            <div className="flex items-center space-x-2">
              <Loader2 className="h-8 w-8 text-blue-500 animate-spin" />
              <span className="text-gray-400 text-lg">Loading vehicle details...</span>
            </div>
          </div>
        </div>
      </div>
    );
  }

  if (error || !vehicle) {
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
                <p className="text-sm font-medium text-red-200">
                  {error || 'Vehicle not found'}
                </p>
              </div>
            </div>
          </div>
          <div className="mt-4">
            <Link
              to="/vehicles"
              className="inline-flex items-center text-sm text-blue-400 hover:text-blue-300"
            >
              <ArrowLeft className="h-4 w-4 mr-2" />
              Back to Vehicles
            </Link>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-900">
      <Navigation />

      <div className="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
        {/* Back Button */}
        <div className="px-4 sm:px-0 mb-4">
          <Link
            to="/vehicles"
            className="inline-flex items-center text-sm text-blue-400 hover:text-blue-300"
          >
            <ArrowLeft className="h-4 w-4 mr-2" />
            Back to Vehicles
          </Link>
        </div>

        {/* Header */}
        <div className="px-4 py-6 sm:px-0">
          <div className="flex justify-between items-start">
            <div className="flex items-center">
              <div className="p-3 bg-blue-900/50 rounded-lg">
                <Car className="h-8 w-8 text-blue-400" />
              </div>
              <div className="ml-4">
                <h1 className="text-3xl font-bold text-white">
                  {vehicle.year} {vehicle.make} {vehicle.model}
                </h1>
                <p className="mt-1 text-sm text-gray-400">
                  {vehicle.vehicleTypeDisplay}
                </p>
              </div>
            </div>
            <div className="flex space-x-3">
              <button
                onClick={handleEditClick}
                className="inline-flex items-center px-4 py-2 border border-gray-600 rounded-md text-sm font-medium text-gray-300 bg-gray-700 hover:bg-gray-600 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 transition-colors"
              >
                <Edit className="h-4 w-4 mr-2" />
                Edit
              </button>
              <button
                onClick={handleDeleteClick}
                className="inline-flex items-center px-4 py-2 border border-red-600 rounded-md text-sm font-medium text-red-400 hover:bg-red-900/50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500 transition-colors"
              >
                <Trash2 className="h-4 w-4 mr-2" />
                Delete
              </button>
            </div>
          </div>
        </div>

        <div className="px-4 sm:px-0 mt-8">
          <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
            {/* Basic Information */}
            <div className="bg-gray-800 shadow-lg rounded-lg border border-gray-700">
              <div className="px-6 py-5">
                <h2 className="text-lg font-semibold text-white mb-4">
                  Vehicle Information
                </h2>
                <div className="space-y-3">
                  <div className="flex justify-between">
                    <span className="text-sm text-gray-400">Make:</span>
                    <span className="text-sm font-medium text-white">{vehicle.make}</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-sm text-gray-400">Model:</span>
                    <span className="text-sm font-medium text-white">{vehicle.model}</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-sm text-gray-400">Year:</span>
                    <span className="text-sm font-medium text-white">{vehicle.year}</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-sm text-gray-400">Type:</span>
                    <span className="px-2 inline-flex text-xs leading-5 font-semibold rounded-full bg-blue-900 text-blue-200">
                      {vehicle.vehicleTypeDisplay}
                    </span>
                  </div>
                  <div className="flex justify-between pt-3 border-t border-gray-700">
                    <span className="text-sm text-gray-400">Purchase Price:</span>
                    <span className="text-sm font-semibold text-white">
                      {formatCurrency(vehicle.purchasePrice)}
                    </span>
                  </div>
                </div>
              </div>
            </div>

            <div className="bg-gray-800 shadow-lg rounded-lg border border-gray-700">
              <div className="px-6 py-5">
                <h2 className="text-lg font-semibold text-white mb-4 flex items-center">
                  <Calendar className="h-5 w-5 mr-2 text-blue-400" />
                  Ownership Period
                </h2>
                <div className="space-y-3">
                  <div className="flex justify-between">
                    <span className="text-sm text-gray-400">Start Date:</span>
                    <span className="text-sm font-medium text-white">
                      {formatDateOnly(vehicle.ownershipStart)}
                    </span>
                  </div>
                  {vehicle.ownershipEnd ? (
                    <>
                      <div className="flex justify-between">
                        <span className="text-sm text-gray-400">End Date:</span>
                        <span className="text-sm font-medium text-white">
                          {formatDateOnly(vehicle.ownershipEnd)}
                        </span>
                      </div>
                      <div className="flex justify-between pt-3 border-t border-gray-700">
                        <span className="text-sm text-gray-400">Status:</span>
                        <span className="px-2 inline-flex text-xs leading-5 font-semibold rounded-full bg-gray-700 text-gray-300">
                          Previously Owned
                        </span>
                      </div>
                    </>
                  ) : (
                    <div className="flex justify-between pt-3 border-t border-gray-700">
                      <span className="text-sm text-gray-400">Status:</span>
                      <span className="px-2 inline-flex text-xs leading-5 font-semibold rounded-full bg-green-900 text-green-200">
                        Currently Owned
                      </span>
                    </div>
                  )}
                  {tcoReport && (
                    <div className="flex justify-between">
                      <span className="text-sm text-gray-400">Days Owned:</span>
                      <span className="text-sm font-medium text-white">
                        {tcoReport.ownershipDays}
                      </span>
                    </div>
                  )}
                </div>
              </div>
            </div>
          </div>
        </div>

        {tcoReport && (
          <div className="px-4 sm:px-0 mt-8">
            <div className="bg-gray-800 shadow-lg rounded-lg border border-gray-700">
              <div className="px-6 py-5">
                <h2 className="text-lg font-semibold text-white mb-4 flex items-center">
                  <TrendingUp className="h-5 w-5 mr-2 text-blue-400" />
                  Total Cost of Ownership
                </h2>
                <div className="grid grid-cols-1 gap-6 md:grid-cols-2 lg:grid-cols-4">
                  <div className="bg-gray-700/50 rounded-lg p-4">
                    <div className="flex items-center">
                      <DollarSign className="h-5 w-5 text-green-400 mr-2" />
                      <p className="text-xs text-gray-400">Purchase Price</p>
                    </div>
                    <p className="text-xl font-semibold text-white mt-2">
                      {formatCurrency(tcoReport.purchasePrice)}
                    </p>
                  </div>
                  <div className="bg-gray-700/50 rounded-lg p-4">
                    <p className="text-xs text-gray-400">Total Fuel Cost</p>
                    <p className="text-xl font-semibold text-white mt-2">
                      {formatCurrency(tcoReport.totalFuelCost)}
                    </p>
                  </div>
                  <div className="bg-gray-700/50 rounded-lg p-4">
                    <p className="text-xs text-gray-400">Total Expenses</p>
                    <p className="text-xl font-semibold text-white mt-2">
                      {formatCurrency(tcoReport.totalExpensesCost)}
                    </p>
                  </div>
                  <div className="bg-blue-900/50 rounded-lg p-4">
                    <p className="text-xs text-blue-200">Total TCO</p>
                    <p className="text-xl font-bold text-white mt-2">
                      {formatCurrency(tcoReport.totalCost)}
                    </p>
                  </div>
                </div>

                <div className="grid grid-cols-1 gap-6 md:grid-cols-3 mt-6">
                  <div className="bg-gray-700/50 rounded-lg p-4">
                    <p className="text-xs text-gray-400">Cost per Day</p>
                    <p className="text-lg font-semibold text-white mt-2">
                      {formatCurrency(tcoReport.costPerDay)}
                    </p>
                  </div>
                  <div className="bg-gray-700/50 rounded-lg p-4">
                    <p className="text-xs text-gray-400">Cost per Month</p>
                    <p className="text-lg font-semibold text-white mt-2">
                      {formatCurrency(tcoReport.costPerMonth)}
                    </p>
                  </div>
                  <div className="bg-gray-700/50 rounded-lg p-4">
                    <p className="text-xs text-gray-400">Days Owned</p>
                    <p className="text-lg font-semibold text-white mt-2">
                      {tcoReport.ownershipDays}
                    </p>
                  </div>
                </div>

                <div className="mt-6 pt-6 border-t border-gray-700">
                  <Link
                    to={`/reports?vehicleId=${vehicle.id}`}
                    className="inline-flex items-center text-sm text-blue-400 hover:text-blue-300"
                  >
                    View Full Report
                    <TrendingUp className="h-4 w-4 ml-2" />
                  </Link>
                </div>
              </div>
            </div>
          </div>
        )}
      </div>

      {showEditForm && (
        <VehicleForm
          vehicle={vehicle}
          onSubmit={handleFormSubmit}
          onCancel={handleFormCancel}
        />
      )}

      {showDeleteModal && (
        <DeleteVehicleModal
          vehicle={vehicle}
          onConfirm={handleDeleteConfirm}
          onCancel={handleDeleteCancel}
        />
      )}
    </div>
  );
}
