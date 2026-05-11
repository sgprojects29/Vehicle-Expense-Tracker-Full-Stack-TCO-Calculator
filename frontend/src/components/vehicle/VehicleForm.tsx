import { useState } from 'react';
import type { FormEvent } from 'react';
import { X, Loader2 } from 'lucide-react';
import type { Vehicle, CreateVehicleDto, UpdateVehicleDto } from '../../types/Vehicle'
import type { AxiosError } from 'axios';
import { toDateInputValue } from '../../utils/helpers';

interface VehicleFormProps {
  vehicle?: Vehicle;
  onSubmit: (data: CreateVehicleDto | UpdateVehicleDto) => Promise<void>;
  onCancel: () => void;
}

export function VehicleForm({ vehicle, onSubmit, onCancel }: VehicleFormProps) {
  const isEditing = !!vehicle;
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [formData, setFormData] = useState({
    make: vehicle?.make || '',
    model: vehicle?.model || '',
    year: vehicle?.year.toString() || new Date().getFullYear().toString(),
    purchasePrice: vehicle?.purchasePrice.toString() || '',
    ownershipStart: vehicle?.ownershipStart ? toDateInputValue(vehicle.ownershipStart) : '',
    ownershipEnd: vehicle?.ownershipEnd ? toDateInputValue(vehicle.ownershipEnd) : '',
    vehicleType: vehicle?.vehicleType.toString() || '0',
  });

  const [validationErrors, setValidationErrors] = useState({
    make: '',
    model: '',
    year: '',
    purchasePrice: '',
    ownershipStart: '',
    ownershipEnd: '',
  });

  const vehicleTypeOptions = [
    { value: 0, label: 'Gasoline' },
    { value: 1, label: 'Diesel' },
    { value: 2, label: 'Electric' },
    { value: 3, label: 'Hybrid' },
    { value: 4, label: 'Plug-in Hybrid' },
  ];

  const validateForm = (): boolean => {
    const errors = {
      make: '',
      model: '',
      year: '',
      purchasePrice: '',
      ownershipStart: '',
      ownershipEnd: '',
    };

    if (!formData.make.trim()) {
      errors.make = 'Make is required';
    } else if (formData.make.length < 2) {
      errors.make = 'Make must be at least 2 characters';
    }

    if (!formData.model.trim()) {
      errors.model = 'Model is required';
    } else if (formData.model.length < 2) {
      errors.model = 'Model must be at least 2 characters';
    }

    const year = parseInt(formData.year);
    const currentYear = new Date().getFullYear();
    if (!formData.year) {
      errors.year = 'Year is required';
    } else if (isNaN(year) || year < 1900 || year > currentYear + 1) {
      errors.year = `Year must be between 1900 and ${currentYear + 1}`;
    }

    const price = parseFloat(formData.purchasePrice);
    if (!formData.purchasePrice) {
      errors.purchasePrice = 'Purchase price is required';
    } else if (isNaN(price) || price <= 0) {
      errors.purchasePrice = 'Purchase price must be greater than 0';
    }

    if (!formData.ownershipStart) {
      errors.ownershipStart = 'Ownership start date is required';
    }

    if (formData.ownershipEnd && formData.ownershipStart) {
      const startDate = new Date(formData.ownershipStart);
      const endDate = new Date(formData.ownershipEnd);
      if (endDate < startDate) {
        errors.ownershipEnd = 'End date cannot be before start date';
      }
    }

    setValidationErrors(errors);
    return !Object.values(errors).some(error => error !== '');
  };

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    
    if (!validateForm()) {
      return;
    }

    setIsSubmitting(true);
    setError(null);

    try {
      const submitData: CreateVehicleDto | UpdateVehicleDto = {
        make: formData.make.trim(),
        model: formData.model.trim(),
        year: parseInt(formData.year),
        purchasePrice: parseFloat(formData.purchasePrice),
        ownershipStart: formData.ownershipStart,
        ownershipEnd: formData.ownershipEnd || undefined,
        vehicleType: parseInt(formData.vehicleType),
      };

      await onSubmit(submitData);
    } catch (err) {
        console.error('Form submission error:', err);
        const axiosError = err as AxiosError<{ message?: string }>;
        setError(axiosError.response?.data?.message || 'Failed to save vehicle. Please try again.');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleChange = (field: string, value: string) => {
    setFormData(prev => ({ ...prev, [field]: value }));
    // Clear validation error for this field when user starts typing
    if (validationErrors[field as keyof typeof validationErrors]) {
      setValidationErrors(prev => ({ ...prev, [field]: '' }));
    }
  };

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
      <div className="bg-gray-800 rounded-lg shadow-xl max-w-2xl w-full max-h-[90vh] overflow-y-auto">
        <div className="flex items-center justify-between p-6 border-b border-gray-700">
          <h2 className="text-2xl font-bold text-white">
            {isEditing ? 'Edit Vehicle' : 'Add New Vehicle'}
          </h2>
          <button
            onClick={onCancel}
            className="text-gray-400 hover:text-white transition-colors"
            disabled={isSubmitting}
          >
            <X className="h-6 w-6" />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="p-6 space-y-4">
          {error && (
            <div className="bg-red-900/50 border border-red-700 rounded-lg p-4">
              <p className="text-sm text-red-200">{error}</p>
            </div>
          )}

          <div>
            <label htmlFor="make" className="block text-sm font-medium text-gray-300 mb-1">
              Make *
            </label>
            <input
              type="text"
              id="make"
              value={formData.make}
              onChange={(e) => handleChange('make', e.target.value)}
              className="w-full px-3 py-2 bg-gray-700 border border-gray-600 rounded-md text-white placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              placeholder="e.g., Toyota"
              disabled={isSubmitting}
              required
            />
            {validationErrors.make && (
              <p className="mt-1 text-sm text-red-400">{validationErrors.make}</p>
            )}
          </div>

          <div>
            <label htmlFor="model" className="block text-sm font-medium text-gray-300 mb-1">
              Model *
            </label>
            <input
              type="text"
              id="model"
              value={formData.model}
              onChange={(e) => handleChange('model', e.target.value)}
              className="w-full px-3 py-2 bg-gray-700 border border-gray-600 rounded-md text-white placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              placeholder="e.g., Camry"
              disabled={isSubmitting}
              required
            />
            {validationErrors.model && (
              <p className="mt-1 text-sm text-red-400">{validationErrors.model}</p>
            )}
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div>
              <label htmlFor="year" className="block text-sm font-medium text-gray-300 mb-1">
                Year *
              </label>
              <input
                type="number"
                id="year"
                value={formData.year}
                onChange={(e) => handleChange('year', e.target.value)}
                min="1900"
                max={new Date().getFullYear() + 1}
                className="w-full px-3 py-2 bg-gray-700 border border-gray-600 rounded-md text-white placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                disabled={isSubmitting}
                required
              />
              {validationErrors.year && (
                <p className="mt-1 text-sm text-red-400">{validationErrors.year}</p>
              )}
            </div>

            <div>
              <label htmlFor="vehicleType" className="block text-sm font-medium text-gray-300 mb-1">
                Vehicle Type *
              </label>
              <select
                id="vehicleType"
                value={formData.vehicleType}
                onChange={(e) => handleChange('vehicleType', e.target.value)}
                className="w-full px-3 py-2 bg-gray-700 border border-gray-600 rounded-md text-white focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                disabled={isSubmitting}
                required
              >
                {vehicleTypeOptions.map(option => (
                  <option key={option.value} value={option.value}>
                    {option.label}
                  </option>
                ))}
              </select>
            </div>
          </div>

          <div>
            <label htmlFor="purchasePrice" className="block text-sm font-medium text-gray-300 mb-1">
              Purchase Price ($) *
            </label>
            <input
              type="number"
              id="purchasePrice"
              value={formData.purchasePrice}
              onChange={(e) => handleChange('purchasePrice', e.target.value)}
              step="0.01"
              min="0"
              className="w-full px-3 py-2 bg-gray-700 border border-gray-600 rounded-md text-white placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              placeholder="e.g., 25000.00"
              disabled={isSubmitting}
              required
            />
            {validationErrors.purchasePrice && (
              <p className="mt-1 text-sm text-red-400">{validationErrors.purchasePrice}</p>
            )}
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div>
              <label htmlFor="ownershipStart" className="block text-sm font-medium text-gray-300 mb-1">
                Ownership Start Date *
              </label>
              <input
                type="date"
                id="ownershipStart"
                value={formData.ownershipStart}
                onChange={(e) => handleChange('ownershipStart', e.target.value)}
                className="w-full px-3 py-2 bg-gray-700 border border-gray-600 rounded-md text-white focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                disabled={isSubmitting}
                required
              />
              {validationErrors.ownershipStart && (
                <p className="mt-1 text-sm text-red-400">{validationErrors.ownershipStart}</p>
              )}
            </div>

            <div>
              <label htmlFor="ownershipEnd" className="block text-sm font-medium text-gray-300 mb-1">
                Ownership End Date
              </label>
              <input
                type="date"
                id="ownershipEnd"
                value={formData.ownershipEnd}
                onChange={(e) => handleChange('ownershipEnd', e.target.value)}
                className="w-full px-3 py-2 bg-gray-700 border border-gray-600 rounded-md text-white focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                disabled={isSubmitting}
              />
              {validationErrors.ownershipEnd && (
                <p className="mt-1 text-sm text-red-400">{validationErrors.ownershipEnd}</p>
              )}
            </div>
          </div>

          <div className="flex justify-end space-x-3 pt-4 border-t border-gray-700">
            <button
              type="button"
              onClick={onCancel}
              className="px-4 py-2 border border-gray-600 rounded-md text-gray-300 hover:bg-gray-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-gray-500 transition-colors"
              disabled={isSubmitting}
            >
              Cancel
            </button>
            <button
              type="submit"
              className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed transition-colors inline-flex items-center"
              disabled={isSubmitting}
            >
              {isSubmitting && <Loader2 className="h-4 w-4 mr-2 animate-spin" />}
              {isSubmitting ? 'Saving...' : (isEditing ? 'Update Vehicle' : 'Add Vehicle')}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
