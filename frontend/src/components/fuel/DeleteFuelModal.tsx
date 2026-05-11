import { useState } from 'react';
import { AlertTriangle, Loader2 } from 'lucide-react';
import type { FuelEntry } from '../../types/Fuel';
import { formatCurrency, formatDateOnly } from '../../utils/helpers';

interface DeleteFuelModalProps {
  fuelEntry: FuelEntry;
  onConfirm: () => Promise<void>;
  onCancel: () => void;
}

export default function DeleteFuelModal({ fuelEntry, onConfirm, onCancel }: DeleteFuelModalProps) {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const handleConfirm = async () => {
    setLoading(true);
    setError('');
    try {
      await onConfirm();
    } catch (err: unknown) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to delete fuel entry';
      setError(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
      <div className="bg-gray-800 rounded-lg max-w-md w-full p-6 border border-gray-700">
        <div className="flex items-start space-x-4">
          <div className="shrink-0">
            <AlertTriangle className="w-6 h-6 text-red-400" />
          </div>
          <div className="flex-1">
            <h3 className="text-lg font-semibold text-white mb-2">Delete Fuel Entry</h3>
            <p className="text-gray-300 mb-4">
              Are you sure you want to delete this fuel entry? This action cannot be undone.
            </p>

            <div className="bg-gray-900 rounded-lg p-4 mb-4 space-y-2">
              <p className="text-sm text-gray-400">
                <span className="font-medium text-gray-300">Vehicle:</span>{' '}
                {fuelEntry.vehicleMake} {fuelEntry.vehicleModel}
              </p>
              <p className="text-sm text-gray-400">
                <span className="font-medium text-gray-300">Date:</span>{' '}
                {formatDateOnly(fuelEntry.date)}
              </p>
              <p className="text-sm text-gray-400">
                <span className="font-medium text-gray-300">Type:</span>{' '}
                {fuelEntry.energyTypeDisplay}
              </p>
              <p className="text-sm text-gray-400">
                <span className="font-medium text-gray-300">Amount:</span>{' '}
                {fuelEntry.amount.toFixed(2)} {fuelEntry.unit}
              </p>
              <p className="text-sm text-gray-400">
                <span className="font-medium text-gray-300">Cost:</span>{' '}
                {formatCurrency(fuelEntry.cost)}
              </p>
              {fuelEntry.odometer && (
                <p className="text-sm text-gray-400">
                  <span className="font-medium text-gray-300">Odometer:</span>{' '}
                  {fuelEntry.odometer.toLocaleString()} km
                </p>
              )}
            </div>

            {error && (
              <div className="mb-4 p-3 bg-red-900/50 border border-red-700 rounded text-red-200 text-sm">
                {error}
              </div>
            )}

            <div className="flex justify-end space-x-3">
              <button
                onClick={onCancel}
                disabled={loading}
                className="px-4 py-2 text-gray-300 hover:text-white transition-colors disabled:opacity-50"
              >
                Cancel
              </button>
              <button
                onClick={handleConfirm}
                disabled={loading}
                className="px-4 py-2 bg-red-600 text-white rounded-md hover:bg-red-700 transition-colors disabled:opacity-50 flex items-center space-x-2"
              >
                {loading && <Loader2 className="w-4 h-4 animate-spin" />}
                <span>Delete</span>
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
