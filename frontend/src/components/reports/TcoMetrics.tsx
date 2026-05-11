import { formatCurrency } from '../../utils/helpers';
import type { TcoReportDto } from '../../types/Report';

interface TcoMetricsProps {
  tcoReport: TcoReportDto;
}

export function TcoMetrics({ tcoReport }: TcoMetricsProps) {
  return (
    <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
      <MetricCard
        title="Purchase Price"
        value={formatCurrency(tcoReport.purchasePrice)}
        subtitle="Initial investment"
      />
      <MetricCard
        title="Total Fuel Cost"
        value={formatCurrency(tcoReport.totalFuelCost)}
        subtitle={`${tcoReport.totalFuelEntries} entries`}
      />
      <MetricCard
        title="Total Expenses"
        value={formatCurrency(tcoReport.totalExpensesCost)}
        subtitle={`${tcoReport.totalExpenseEntries} entries`}
      />
      <MetricCard
        title="Total TCO"
        value={formatCurrency(tcoReport.totalCost)}
        subtitle={`${tcoReport.ownershipDays} days owned`}
        highlight
      />
      <MetricCard
        title="Cost Per Day"
        value={formatCurrency(tcoReport.costPerDay)}
        subtitle="Daily average"
      />
      <MetricCard
        title="Cost Per Month"
        value={formatCurrency(tcoReport.costPerMonth)}
        subtitle="Monthly average"
      />
    </div>
  );
}

interface MetricCardProps {
  title: string;
  value: string;
  subtitle: string;
  highlight?: boolean;
}

function MetricCard({ title, value, subtitle, highlight }: MetricCardProps) {
  return (
    <div className={`${highlight ? 'bg-blue-600/20 border-blue-500' : 'bg-gray-800'} border ${highlight ? '' : 'border-gray-700'} rounded-lg p-4`}>
      <p className="text-gray-400 text-sm">{title}</p>
      <p className={`${highlight ? 'text-blue-400' : 'text-white'} text-xl font-bold mt-1`}>{value}</p>
      <p className="text-gray-500 text-xs mt-1">{subtitle}</p>
    </div>
  );
}
