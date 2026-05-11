import { BarChart3, DollarSign, TrendingUp, PieChart } from 'lucide-react';
import { formatCurrency } from '../../utils/helpers';
import type { VehicleSummaryDto } from '../../types/Report';

interface SummaryCardsProps {
  summary: VehicleSummaryDto;
}

export function SummaryCards({ summary }: SummaryCardsProps) {
  return (
    <>
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <SummaryCard
          icon={<BarChart3 className="h-6 w-6" />}
          title="Total Vehicles"
          value={summary.totalVehicles.toString()}
          bgColor="bg-blue-500/10"
          iconColor="text-blue-500"
        />
        <SummaryCard
          icon={<DollarSign className="h-6 w-6" />}
          title="Total Investment"
          value={formatCurrency(summary.totalInvestment)}
          bgColor="bg-green-500/10"
          iconColor="text-green-500"
        />
        <SummaryCard
          icon={<TrendingUp className="h-6 w-6" />}
          title="Total Fuel Cost"
          value={formatCurrency(summary.totalFuelCost)}
          bgColor="bg-orange-500/10"
          iconColor="text-orange-500"
        />
        <SummaryCard
          icon={<PieChart className="h-6 w-6" />}
          title="Total Expenses"
          value={formatCurrency(summary.totalExpensesCost)}
          bgColor="bg-purple-500/10"
          iconColor="text-purple-500"
        />
      </div>
      
      <div className="mt-4 bg-linear-to-br from-blue-600 to-purple-600 rounded-lg p-6 text-white">
        <div className="flex items-center justify-between">
          <div>
            <p className="text-blue-100 text-sm font-medium">Grand Total TCO</p>
            <p className="text-3xl font-bold mt-1">{formatCurrency(summary.grandTotalCost)}</p>
          </div>
          <DollarSign className="h-12 w-12 text-white/80" />
        </div>
      </div>
    </>
  );
}

interface SummaryCardProps {
  icon: React.ReactNode;
  title: string;
  value: string;
  bgColor: string;
  iconColor: string;
}

function SummaryCard({ icon, title, value, bgColor, iconColor }: SummaryCardProps) {
  return (
    <div className="bg-gray-800 rounded-lg p-6">
      <div className="flex items-center justify-between">
        <div>
          <p className="text-gray-400 text-sm">{title}</p>
          <p className="text-white text-2xl font-bold mt-1">{value}</p>
        </div>
        <div className={`${bgColor} ${iconColor} rounded-lg p-3`}>
          {icon}
        </div>
      </div>
    </div>
  );
}
