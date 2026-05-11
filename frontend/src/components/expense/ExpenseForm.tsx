import { useState, useEffect } from 'react';
import type { FormEvent } from 'react';
import { X, Loader2 } from 'lucide-react';
import type { Expense, CreateExpenseDto, UpdateExpenseDto } from '../../types/Expense';
import type { Vehicle } from '../../types/Vehicle';
import type { AxiosError } from 'axios';
import { toDateInputValue } from '../../utils/helpers';

interface ExpenseFormProps {
  expense?: Expense;
  vehicles: Vehicle[];
  onSubmit: (data: CreateExpenseDto | UpdateExpenseDto) => Promise<void>;
  onCancel: () => void;
}

export function ExpenseForm({ expense, vehicles, onSubmit, onCancel }: ExpenseFormProps) {
  const isEditing = !!expense;
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [formData, setFormData] = useState({
    amount: expense?.amount.toString() || '',
    category: expense?.category.toString() || '0',
    date: toDateInputValue(expense?.date),
    notes: expense?.notes || '',
    vehicleId: expense?.vehicleId.toString() || '',
  });

  const [validationErrors, setValidationErrors] = useState({
    amount: '',
    category: '',
    date: '',
    vehicleId: '',
  });

  const categoryOptions = [
    { value: 0, label: 'Fuel' },
    { value: 1, label: 'Maintenance' },
    { value: 2, label: 'Insurance' },
    { value: 3, label: 'Registration' },
    { value: 4, label: 'Repairs' },
    { value: 5, label: 'Parking' },
    { value: 6, label: 'Tolls' },
    { value: 7, label: 'Car Wash' },
    { value: 8, label: 'Modifications' },
    { value: 9, label: 'Other' },
  ];

  // Auto-select first vehicle if adding new expense and no vehicle selected
  useEffect(() => {
    if (!isEditing && !formData.vehicleId && vehicles.length > 0) {
      setFormData(prev => ({ ...prev, vehicleId: vehicles[0].id.toString() }));
    }
  }, [isEditing, formData.vehicleId, vehicles]);

  const validateForm = (): boolean => {
    const errors = {
      amount: '',
      category: '',
      date: '',
      vehicleId: '',
    };

    const amount = parseFloat(formData.amount);
    if (!formData.amount) {
      errors.amount = 'Amount is required';
    } else if (isNaN(amount) || amount <= 0) {
      errors.amount = 'Amount must be greater than 0';
    }

    const category = parseInt(formData.category);
    if (isNaN(category) || category < 0 || category > 9) {
      errors.category = 'Please select a valid category';
    }

    if (!formData.date) {
      errors.date = 'Date is required';
    }

    if (!formData.vehicleId) {
      errors.vehicleId = 'Please select a vehicle';
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
      const baseData = {
        amount: parseFloat(formData.amount),
        category: parseInt(formData.category),
        date: formData.date,
        notes: formData.notes.trim() || undefined,
      };

    const submitData: CreateExpenseDto | UpdateExpenseDto = {
      ...baseData,
      vehicleId: parseInt(formData.vehicleId)
    };

      await onSubmit(submitData);
    } catch (err) {
      console.error('Form submission error:', err);
      const axiosError = err as AxiosError<{ message?: string }>;
      setError(axiosError.response?.data?.message || 'Failed to save expense. Please try again.');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleChange = (field: string, value: string) => {
    setFormData(prev => ({ ...prev, [field]: value }));
    if (validationErrors[field as keyof typeof validationErrors]) {
      setValidationErrors(prev => ({ ...prev, [field]: '' }));
    }
  };

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
      <div className="bg-gray-800 rounded-lg shadow-xl max-w-2xl w-full max-h-[90vh] overflow-y-auto">
        <div className="flex items-center justify-between p-6 border-b border-gray-700">
          <h2 className="text-2xl font-bold text-white">
            {isEditing ? 'Edit Expense' : 'Add New Expense'}
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

          {!isEditing && (
            <div>
              <label htmlFor="vehicleId" className="block text-sm font-medium text-gray-300 mb-1">
                Vehicle *
              </label>
              <select
                id="vehicleId"
                value={formData.vehicleId}
                onChange={(e) => handleChange('vehicleId', e.target.value)}
                className="w-full px-3 py-2 bg-gray-700 border border-gray-600 rounded-md text-white focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                disabled={isSubmitting}
                required
              >
                <option value="">Select a vehicle</option>
                {vehicles.map(vehicle => (
                  <option key={vehicle.id} value={vehicle.id}>
                    {vehicle.year} {vehicle.make} {vehicle.model}
                  </option>
                ))}
              </select>
              {validationErrors.vehicleId && (
                <p className="mt-1 text-sm text-red-400">{validationErrors.vehicleId}</p>
              )}
            </div>
          )}

          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div>
              <label htmlFor="amount" className="block text-sm font-medium text-gray-300 mb-1">
                Amount ($) *
              </label>
              <input
                type="number"
                id="amount"
                value={formData.amount}
                onChange={(e) => handleChange('amount', e.target.value)}
                step="0.01"
                min="0"
                className="w-full px-3 py-2 bg-gray-700 border border-gray-600 rounded-md text-white placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                placeholder="e.g., 50.00"
                disabled={isSubmitting}
                required
              />
              {validationErrors.amount && (
                <p className="mt-1 text-sm text-red-400">{validationErrors.amount}</p>
              )}
            </div>

            <div>
              <label htmlFor="category" className="block text-sm font-medium text-gray-300 mb-1">
                Category *
              </label>
              <select
                id="category"
                value={formData.category}
                onChange={(e) => handleChange('category', e.target.value)}
                className="w-full px-3 py-2 bg-gray-700 border border-gray-600 rounded-md text-white focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                disabled={isSubmitting}
                required
              >
                {categoryOptions.map(option => (
                  <option key={option.value} value={option.value}>
                    {option.label}
                  </option>
                ))}
              </select>
              {validationErrors.category && (
                <p className="mt-1 text-sm text-red-400">{validationErrors.category}</p>
              )}
            </div>
          </div>

          <div>
            <label htmlFor="date" className="block text-sm font-medium text-gray-300 mb-1">
              Date *
            </label>
            <input
              type="date"
              id="date"
              value={formData.date}
              onChange={(e) => handleChange('date', e.target.value)}
              className="w-full px-3 py-2 bg-gray-700 border border-gray-600 rounded-md text-white focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              disabled={isSubmitting}
              required
            />
            {validationErrors.date && (
              <p className="mt-1 text-sm text-red-400">{validationErrors.date}</p>
            )}
          </div>

          <div>
            <label htmlFor="notes" className="block text-sm font-medium text-gray-300 mb-1">
              Notes
            </label>
            <textarea
              id="notes"
              value={formData.notes}
              onChange={(e) => handleChange('notes', e.target.value)}
              rows={3}
              className="w-full px-3 py-2 bg-gray-700 border border-gray-600 rounded-md text-white placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              placeholder="Optional notes about this expense..."
              disabled={isSubmitting}
            />
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
              {isSubmitting ? 'Saving...' : (isEditing ? 'Update Expense' : 'Add Expense')}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
