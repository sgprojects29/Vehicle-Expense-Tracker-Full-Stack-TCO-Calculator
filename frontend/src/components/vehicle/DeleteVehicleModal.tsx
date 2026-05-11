import { useState } from 'react';
import { AlertTriangle, X, Loader2 } from 'lucide-react';
import { formatCurrency } from '../../utils/helpers';
import type { Vehicle } from '../../types/Vehicle';
import type { AxiosError } from 'axios';

interface DeleteVehicleModalProps {
  vehicle: Vehicle;
  onConfirm: () => Promise<void>;
  onCancel: () => void;
}

export function DeleteVehicleModal({ vehicle, onConfirm, onCancel }: DeleteVehicleModalProps) {
  const [isDeleting, setIsDeleting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleDelete = async () => {
    setIsDeleting(true);
    setError(null);

    try {
      await onConfirm();
    } catch (err) {
      console.error('Delete error:', err);
      const axiosError = err as AxiosError<{ message?: string }>;
      setError(axiosError.response?.data?.message || 'Failed to delete vehicle. Please try again.');
      setIsDeleting(false);
    }
  };

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
      <div className="bg-gray-800 rounded-lg shadow-xl max-w-md w-full">
        <div className="flex items-center justify-between p-6 border-b border-gray-700">
          <div className="flex items-center space-x-3">
            <div className="shrink-0">
              <AlertTriangle className="h-6 w-6 text-red-500" />
            </div>
            <h2 className="text-xl font-bold text-white">Delete Vehicle</h2>
          </div>
          <button
            onClick={onCancel}
            className="text-gray-400 hover:text-white transition-colors"
            disabled={isDeleting}
          >
            <X className="h-6 w-6" />
          </button>
        </div>

        <div className="p-6 space-y-4">
          {error && (
            <div className="bg-red-900/50 border border-red-700 rounded-lg p-4">
              <p className="text-sm text-red-200">{error}</p>
            </div>
          )}

          <div className="space-y-2">
            <p className="text-gray-300">
              Are you sure you want to delete this vehicle? This action cannot be undone.
            </p>
            
            <div className="bg-gray-700 rounded-lg p-4 mt-4">
              <div className="space-y-2">
                <div className="flex justify-between">
                  <span className="text-sm text-gray-400">Vehicle:</span>
                  <span className="text-sm font-medium text-white">
                    {vehicle.year} {vehicle.make} {vehicle.model}
                  </span>
                </div>
                <div className="flex justify-between">
                  <span className="text-sm text-gray-400">Type:</span>
                  <span className="text-sm text-gray-300">{vehicle.vehicleTypeDisplay}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-sm text-gray-400">Purchase Price:</span>
                  <span className="text-sm text-gray-300">
                    {formatCurrency(vehicle.purchasePrice)}
                  </span>
                </div>
              </div>
            </div>

            <div className="bg-yellow-900/30 border border-yellow-700/50 rounded-lg p-3 mt-4">
              <div className="flex items-start space-x-2">
                <AlertTriangle className="h-5 w-5 text-yellow-400 shrink-0 mt-0.5" />
                <p className="text-sm text-yellow-200">
                  All expenses, fuel entries, and receipts associated with this vehicle will also be deleted.
                </p>
              </div>
            </div>
          </div>
        </div>

        <div className="flex justify-end space-x-3 p-6 border-t border-gray-700">
          <button
            type="button"
            onClick={onCancel}
            className="px-4 py-2 border border-gray-600 rounded-md text-gray-300 hover:bg-gray-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-gray-500 transition-colors"
            disabled={isDeleting}
          >
            Cancel
          </button>
          <button
            type="button"
            onClick={handleDelete}
            className="px-4 py-2 bg-red-600 text-white rounded-md hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500 disabled:opacity-50 disabled:cursor-not-allowed transition-colors inline-flex items-center"
            disabled={isDeleting}
          >
            {isDeleting && <Loader2 className="h-4 w-4 mr-2 animate-spin" />}
            {isDeleting ? 'Deleting...' : 'Delete Vehicle'}
          </button>
        </div>
      </div>
    </div>
  );
}
