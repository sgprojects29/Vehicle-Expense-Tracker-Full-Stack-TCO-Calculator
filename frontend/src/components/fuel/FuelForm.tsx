import { useState, useEffect, useMemo, type FormEvent } from 'react';
import { X, Loader2 } from 'lucide-react';
import type { FuelEntry, CreateFuelEntryDto, UpdateFuelEntryDto } from '../../types/Fuel';
import { EnergyType } from '../../types/Fuel';
import type { Vehicle } from '../../types/Vehicle';
import { toDateInputValue } from '../../utils/helpers';

interface FuelFormProps {
  fuelEntry?: FuelEntry;
  vehicles: Vehicle[];
  preSelectedVehicleId?: number;
  onSubmit: (data: CreateFuelEntryDto | UpdateFuelEntryDto) => Promise<void>;
  onClose: () => void;
}

const getAllowedEnergyTypes = (vehicleTypeId: number): number[] => {
  // VehicleType enum values from backend:
  // Gasoline = 0, Diesel = 1, Electric = 2, Hybrid = 3, PlugInHybrid = 4
  
  switch (vehicleTypeId) {
    case 0: // Gasoline
      return [EnergyType.Gasoline];
    case 1: // Diesel
      return [EnergyType.Diesel];
    case 2: // Electric
      return [EnergyType.Electricity];
    case 3: // Hybrid
    case 4: // Plug-in Hybrid
      return [EnergyType.Gasoline, EnergyType.Electricity];
    default:
      // Unknown vehicle type - allow all
      return [EnergyType.Gasoline, EnergyType.Diesel, EnergyType.Electricity];
  }
};

export default function FuelForm({ fuelEntry, vehicles, preSelectedVehicleId, onSubmit, onClose }: FuelFormProps) {
  const isEdit = !!fuelEntry;

  const [vehicleId, setVehicleId] = useState<number>(fuelEntry?.vehicleId || preSelectedVehicleId || 0);
  const [energyType, setEnergyType] = useState<number>(fuelEntry?.energyType ?? EnergyType.Gasoline);
  const [amount, setAmount] = useState(fuelEntry?.amount.toString() || '');
  const [cost, setCost] = useState(fuelEntry?.cost.toString() || '');
  const [odometer, setOdometer] = useState(fuelEntry?.odometer?.toString() || '');
  const [date, setDate] = useState(toDateInputValue(fuelEntry?.date) || '');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const selectedVehicle = vehicles.find(v => v.id === vehicleId);
  const allowedEnergyTypes = useMemo(() => {
    return selectedVehicle 
      ? getAllowedEnergyTypes(selectedVehicle.vehicleType) 
      : [EnergyType.Gasoline, EnergyType.Diesel, EnergyType.Electricity];
  }, [selectedVehicle]);

  // Auto-select first vehicle when adding new entry if no vehicle selected
  useEffect(() => {
    if (!isEdit && vehicles.length > 0 && vehicleId === 0) {
      setVehicleId(vehicles[0].id);
    }
  }, [isEdit, vehicles, vehicleId]);

  // Auto-select first allowed energy type when vehicle changes
  useEffect(() => {
    if (selectedVehicle && !allowedEnergyTypes.includes(energyType)) {
      setEnergyType(allowedEnergyTypes[0]);
    }
  }, [vehicleId, allowedEnergyTypes, energyType, selectedVehicle]);


  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError('');

    // Validation
    if (!isEdit && vehicleId === 0) {
      setError('Please select a vehicle');
      return;
    }

    const amountNum = parseFloat(amount);
    if (isNaN(amountNum) || amountNum <= 0) {
      setError('Amount must be greater than 0');
      return;
    }

    const costNum = parseFloat(cost);
    if (isNaN(costNum) || costNum <= 0) {
      setError('Cost must be greater than 0');
      return;
    }

   const odometerNum = odometer ? parseInt(odometer) : null;
    if (odometerNum !== null && (isNaN(odometerNum) || odometerNum < 0)) {
      setError('Odometer reading must be a positive number or left blank');
      return;
    }

    if (!date) {
      setError('Date is required');
      return;
    }

    setLoading(true);

    try {
      const data: CreateFuelEntryDto | UpdateFuelEntryDto = {
        vehicleId: vehicleId, // Always include it
        energyType,
        amount: amountNum,
        cost: costNum,
        odometer: odometerNum,
        date,
      };

      await onSubmit(data);
      onClose();
    } catch (err: unknown) {
      const errorMessage = err instanceof Error ? err.message : 'Failed to save fuel entry';
      setError(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  const getEnergyTypeLabel = (type: EnergyType): string => {
    switch (type) {
      case EnergyType.Gasoline:
        return 'Gasoline';
      case EnergyType.Diesel:
        return 'Diesel';
      case EnergyType.Electricity:
        return 'Electricity';
      default:
        return 'Unknown';
    }
  };

  const getUnitLabel = (type: number): string => {
    return type === EnergyType.Electricity ? 'kWh' : 'Liters';
  };

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
      <div className="bg-gray-800 rounded-lg max-w-md w-full p-6 border border-gray-700">
        <div className="flex justify-between items-center mb-4">
          <h2 className="text-xl font-semibold text-white">
            {isEdit ? 'Edit Fuel Entry' : 'Add Fuel Entry'}
          </h2>
          <button
            onClick={onClose}
            className="text-gray-400 hover:text-white"
            disabled={loading}
          >
            <X className="w-6 h-6" />
          </button>
        </div>

        {error && (
          <div className="mb-4 p-3 bg-red-900/50 border border-red-700 rounded text-red-200">
            {error}
          </div>
        )}

        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label htmlFor="vehicle" className="block text-sm font-medium text-gray-300 mb-1">
              Vehicle *
            </label>
            <select
              id="vehicle"
              value={vehicleId}
              onChange={(e) => setVehicleId(parseInt(e.target.value))}
              required
              disabled={loading}
              className="w-full px-3 py-2 bg-gray-700 border border-gray-600 rounded-md text-white focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              <option value={0}>Select a vehicle</option>
              {vehicles.map((vehicle) => (
                <option key={vehicle.id} value={vehicle.id}>
                  {vehicle.year} {vehicle.make} {vehicle.model}
                </option>
              ))}
            </select>
          </div>

         <div>
            <label htmlFor="energyType" className="block text-sm font-medium text-gray-300 mb-1">
              Energy Type *
            </label>
            <select
              id="energyType"
              value={energyType}
              onChange={(e) => setEnergyType(parseInt(e.target.value))}
              required
              disabled={loading}
              className="w-full px-3 py-2 bg-gray-700 border border-gray-600 rounded-md text-white focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              <option 
                value={EnergyType.Gasoline}
                disabled={!allowedEnergyTypes.includes(EnergyType.Gasoline)}
              >
                {getEnergyTypeLabel(EnergyType.Gasoline)}
                {!allowedEnergyTypes.includes(EnergyType.Gasoline) && ' (Not compatible)'}
              </option>
              <option 
                value={EnergyType.Diesel}
                disabled={!allowedEnergyTypes.includes(EnergyType.Diesel)}
              >
                {getEnergyTypeLabel(EnergyType.Diesel)}
                {!allowedEnergyTypes.includes(EnergyType.Diesel) && ' (Not compatible)'}
              </option>
              <option 
                value={EnergyType.Electricity}
                disabled={!allowedEnergyTypes.includes(EnergyType.Electricity)}
              >
                {getEnergyTypeLabel(EnergyType.Electricity)}
                {!allowedEnergyTypes.includes(EnergyType.Electricity) && ' (Not compatible)'}
              </option>
            </select>
          </div>

          <div>
            <label htmlFor="amount" className="block text-sm font-medium text-gray-300 mb-1">
              Amount ({getUnitLabel(energyType)}) *
            </label>
            <input
              type="number"
              id="amount"
              value={amount}
              onChange={(e) => setAmount(e.target.value)}
              step="0.01"
              min="0"
              required
              disabled={loading}
              className="w-full px-3 py-2 bg-gray-700 border border-gray-600 rounded-md text-white focus:outline-none focus:ring-2 focus:ring-blue-500"
              placeholder={energyType === EnergyType.Electricity ? '45.50' : '50.25'}
            />
          </div>

          <div>
            <label htmlFor="cost" className="block text-sm font-medium text-gray-300 mb-1">
              Cost (CAD) *
            </label>
            <input
              type="number"
              id="cost"
              value={cost}
              onChange={(e) => setCost(e.target.value)}
              step="0.01"
              min="0"
              required
              disabled={loading}
              className="w-full px-3 py-2 bg-gray-700 border border-gray-600 rounded-md text-white focus:outline-none focus:ring-2 focus:ring-blue-500"
              placeholder="75.50"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-300 mb-2">
              Odometer (km) - Optional
            </label>
            <input
              type="number"
              value={odometer || ''}
              onChange={(e) => setOdometer(e.target.value)}
              className="w-full px-3 py-2 bg-gray-700 border border-gray-600 rounded-lg text-white focus:outline-none focus:ring-2 focus:ring-blue-500"
              placeholder="Leave blank if unknown (can add later)"
              min="0"
            />
            <p className="text-xs text-gray-400 mt-1">
              Add odometer reading to track fuel efficiency. You can add this later if you don't have it now.
            </p>
          </div>

          <div>
            <label htmlFor="date" className="block text-sm font-medium text-gray-300 mb-1">
              Date *
            </label>
            <input
              type="date"
              id="date"
              value={date}
              onChange={(e) => setDate(e.target.value)}
              required
              disabled={loading}
              className="w-full px-3 py-2 bg-gray-700 border border-gray-600 rounded-md text-white focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>

          <div className="flex justify-end space-x-3 pt-4">
            <button
              type="button"
              onClick={onClose}
              disabled={loading}
              className="px-4 py-2 text-gray-300 hover:text-white transition-colors disabled:opacity-50"
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={loading}
              className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition-colors disabled:opacity-50 flex items-center space-x-2"
            >
              {loading && <Loader2 className="w-4 h-4 animate-spin" />}
              <span>{isEdit ? 'Update' : 'Add'} Entry</span>
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
