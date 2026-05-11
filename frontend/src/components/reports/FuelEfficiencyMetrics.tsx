import { formatCurrency } from '../../utils/helpers';
import type { FuelEfficiencyDto } from '../../types/Fuel';

interface FuelEfficiencyMetricsProps {
  efficiency: FuelEfficiencyDto;
}

export function FuelEfficiencyMetrics({ efficiency }: FuelEfficiencyMetricsProps) {
  return (
    <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
      {efficiency.averageLitersPer100Km && (
        <EfficiencyCard
          title="Fuel Consumption"
          value={`${efficiency.averageLitersPer100Km.toFixed(2)} L/100km`}
          subtitle={`${efficiency.totalAmount.toFixed(1)}L total`}
        />
      )}

      <EfficiencyCard
        title="Average Cost/km"
        value={formatCurrency(efficiency.averageCostPerKilometer || 0)}
        subtitle={`${efficiency.totalKilometers.toFixed(0)}km tracked`}
        highlight
      />

      <EfficiencyCard
        title="Total Cost"
        value={formatCurrency(efficiency.totalCost)}
        subtitle={`${efficiency.totalFillUps} fill-ups`}
      />

      <EfficiencyCard
        title="Average Cost per Fill-up"
        value={formatCurrency(efficiency.averageCostPerFillUp)}
        subtitle={`${efficiency.entriesWithOdometer} with odometer`}
      />
    </div>
  );
}

interface EfficiencyCardProps {
  title: string;
  value: string;
  subtitle: string;
  highlight?: boolean;
}

function EfficiencyCard({ title, value, subtitle, highlight }: EfficiencyCardProps) {
  return (
    <div className={`${highlight ? 'bg-green-600/20 border-green-500' : 'bg-gray-700/50'} border ${highlight ? '' : 'border-gray-600'} rounded-lg p-4`}>
      <p className="text-gray-300 text-sm">{title}</p>
      <p className={`${highlight ? 'text-green-400' : 'text-white'} text-lg font-bold mt-1`}>{value}</p>
      <p className="text-gray-400 text-xs mt-1">{subtitle}</p>
    </div>
  );
}
